using MySqlConnector;
using REST_VECINDAPP.Modelos;
using System.Data;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using REST_VECINDAPP.Servicios;


namespace REST_VECINDAPP.CapaNegocios
{
    public class cn_Usuarios
    {
        private readonly string _connectionString;
        private readonly IEmailService _emailService;

        public cn_Usuarios(IConfiguration configuration, IEmailService emailService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("La cadena de conexión 'DefaultConnection' no está configurada.");
            _emailService = emailService;
        }

        public List<Usuario> ListarUsuarios(int rut)
        {
            List<Usuario> Usuarios = new List<Usuario>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SP_SELECT_USUARIOS", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                if (rut != -1)
                    cmd.Parameters.AddWithValue("@p_rut", rut);
                else
                    cmd.Parameters.AddWithValue("@p_rut", null);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Usuario usuarioTemp = new Usuario();

                        usuarioTemp.rut = Convert.ToInt32(reader["rut"]);
                        usuarioTemp.dv_rut = Convert.ToString(reader["dv_rut"]);
                        usuarioTemp.nombre = Convert.ToString(reader["nombre"]);
                        usuarioTemp.apellido_paterno = Convert.ToString(reader["apellido_paterno"]);
                        usuarioTemp.apellido_materno = Convert.ToString(reader["apellido_materno"]);
                        usuarioTemp.correo_electronico = Convert.ToString(reader["correo_electronico"]);
                        usuarioTemp.telefono = Convert.ToString(reader["telefono"]);
                        usuarioTemp.direccion = Convert.ToString(reader["direccion"]);
                        usuarioTemp.password = Convert.ToString(reader["password"]);
                        usuarioTemp.fecha_registro = Convert.ToDateTime(reader["fecha_registro"]);
                        usuarioTemp.estado = Convert.ToInt32(reader["estado"]);
                        usuarioTemp.tipo_usuario = Convert.ToString(reader["tipo_usuario"]);

                        if (reader["token_recuperacion"] != DBNull.Value)
                            usuarioTemp.token_recuperacion = Convert.ToString(reader["token_recuperacion"]);

                        if (reader["fecha_token_recuperacion"] != DBNull.Value)
                            usuarioTemp.fecha_token_recuperacion = Convert.ToDateTime(reader["fecha_token_recuperacion"]);

                        Usuarios.Add(usuarioTemp);
                    }
                }

                conn.Close();
            }

            return Usuarios;
        }

        private bool ValidarRut(int rut, string? dv)
        {
            if (rut < 1)
                return false;

            if (string.IsNullOrWhiteSpace(dv))
                return false;

            string rutString = rut.ToString();

            int suma = 0;
            int multiplicador = 2;

            for (int i = rutString.Length - 1; i >= 0; i--)
            {
                suma += int.Parse(rutString[i].ToString()) * multiplicador;
                multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
            }

            int dvCalculado = 11 - (suma % 11);
            string dvEsperado = dvCalculado == 11 ? "0" :
                                dvCalculado == 10 ? "K" :
                                dvCalculado.ToString();

            return dv.ToUpper() == dvEsperado;
        }

        private bool ValidarCorreoElectronico(string correo)
        {
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(correo, patron);
        }

        private bool ValidarComplejidadContraseña(string contraseña)
        {
            var patron = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
            return patron.IsMatch(contraseña);
        }

        private bool ValidarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return true; // El teléfono es opcional

            var patron = new Regex(@"^(\+?56|0)?[9]\d{8}$");
            return patron.IsMatch(telefono.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", ""));
        }

        public string HashearContraseña(string contraseña)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contraseña));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public (bool Exito, string Mensaje) RegistrarUsuario(Usuario usuario)
        {
            if (!ValidarRut(usuario.rut, usuario.dv_rut))
            {
                return (false, "El RUT ingresado no es válido.");
            }

            if (string.IsNullOrWhiteSpace(usuario.correo_electronico) ||
                !ValidarCorreoElectronico(usuario.correo_electronico))
            {
                return (false, "El correo electrónico no tiene un formato válido.");
            }

            if (string.IsNullOrWhiteSpace(usuario.nombre) ||
                string.IsNullOrWhiteSpace(usuario.apellido_paterno))
            {
                return (false, "El nombre y apellido son obligatorios.");
            }

            if (string.IsNullOrWhiteSpace(usuario.password))
            {
                return (false, "La contraseña no puede estar vacía.");
            }

            if (!ValidarComplejidadContraseña(usuario.password))
            {
                return (false, "La contraseña debe tener al menos 8 caracteres, " +
                               "incluyendo mayúsculas, minúsculas, números y un carácter especial.");
            }

            if (!string.IsNullOrWhiteSpace(usuario.telefono) &&
                !ValidarTelefono(usuario.telefono))
            {
                return (false, "El número de teléfono no tiene un formato válido.");
            }

            string passwordHash = HashearContraseña(usuario.password);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_REGISTRAR_USUARIOS", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_rut", usuario.rut);
                        cmd.Parameters.AddWithValue("@p_dv_rut", usuario.dv_rut);
                        cmd.Parameters.AddWithValue("@p_nombre", usuario.nombre);
                        cmd.Parameters.AddWithValue("@p_apellido_paterno", usuario.apellido_paterno);
                        cmd.Parameters.AddWithValue("@p_apellido_materno", usuario.apellido_materno ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@p_correo_electronico", usuario.correo_electronico);
                        cmd.Parameters.AddWithValue("@p_telefono", usuario.telefono ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@p_direccion", usuario.direccion ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@p_password", passwordHash);

                        cmd.ExecuteNonQuery();
                    }

                    return (true, "Usuario registrado exitosamente");
                }
                catch (MySqlException ex)
                {
                    return (false, ex.Message);
                }
                catch (Exception ex)
                {
                    return (false, $"Error inesperado: {ex.Message}");
                }
            }
        }

        public (bool Exito, string Mensaje) IniciarSesion(int rut, string contrasena)
        {
            string passwordHash = HashearContraseña(contrasena);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_INICIAR_SESION", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_rut", rut);
                        cmd.Parameters.AddWithValue("@p_password", passwordHash);

                        MySqlParameter msgParam = new MySqlParameter("@p_mensaje", MySqlDbType.VarChar, 255);
                        msgParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(msgParam);

                        cmd.ExecuteNonQuery();

                        string mensaje = msgParam.Value?.ToString() ?? "";

                        return (mensaje == "OK", mensaje);
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Error al iniciar sesión: {ex.Message}");
                }
            }
        }

        public async Task<(bool Exito, string Mensaje, int Rut)> SolicitarRecuperacionClave(string correoElectronico)
        {
            if (!ValidarCorreoElectronico(correoElectronico))
            {
                return (false, "El correo electrónico no tiene un formato válido.", 0);
            }

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    int rut = 0;
                    using (MySqlCommand cmdBuscarRut = new MySqlCommand("SELECT rut FROM usuarios WHERE correo_electronico = @p_correo_electronico", conn))
                    {
                        cmdBuscarRut.Parameters.AddWithValue("@p_correo_electronico", correoElectronico);
                        object resultado = cmdBuscarRut.ExecuteScalar();

                        if (resultado == null || resultado == DBNull.Value)
                        {
                            return (false, "Correo electrónico no encontrado.", 0);
                        }

                        rut = Convert.ToInt32(resultado);
                    }

                    using (MySqlCommand cmd = new MySqlCommand("SP_GENERAR_TOKEN_RECUPERACION", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_rut", rut);
                        cmd.Parameters.AddWithValue("@p_correo_electronico", correoElectronico);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string token = reader["token"].ToString();

                                string subject = "Recuperación de contraseña - VecindApp";
                                string htmlBody = $@"
                                <html>
                                <body>
                                    <h2>Recuperación de contraseña</h2>
                                    <p>Hola,</p>
                                    <p>Has solicitado un código para restablecer tu contraseña en VecindApp.</p>
                                    <p>Tu código de recuperación es: <strong>{token}</strong></p>
                                    <p>Este código expirará en 1 hora.</p>
                                    <p>Si no has solicitado este cambio, puedes ignorar este correo.</p>
                                    <p>Saludos,<br>El equipo de VecindApp</p>
                                </body>
                                </html>";

                                bool emailSent = await _emailService.SendEmailAsync(correoElectronico, subject, htmlBody);

                                if (emailSent)
                                    return (true, "Se ha enviado un correo con el código de recuperación", rut);
                                else
                                    return (false, "Error al enviar correo de recuperación", 0);
                            }
                            else
                            {
                                return (false, "Error al generar token de recuperación.", 0);
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    if (ex.Message.Contains("Correo electrónico no coincide"))
                    {
                        return (false, "El correo electrónico no coincide con el RUT registrado.", 0);
                    }
                    return (false, $"Error al solicitar recuperación de clave: {ex.Message}", 0);
                }
                catch (Exception ex)
                {
                    return (false, $"Error al solicitar recuperación de clave: {ex.Message}", 0);
                }
            }
        }

        public (bool Exito, string Mensaje) ConfirmarRecuperacionClave(int rut, string token, string nuevaContrasena)
        {
            if (!ValidarComplejidadContraseña(nuevaContrasena))
            {
                return (false, "La nueva contraseña no cumple con los requisitos de complejidad.");
            }

            string nuevaContrasenaHash = HashearContraseña(nuevaContrasena);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_RESTABLECER_CONTRASENA", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_rut", rut);
                        cmd.Parameters.AddWithValue("@p_token", token);
                        cmd.Parameters.AddWithValue("@p_nueva_password", nuevaContrasenaHash);

                        cmd.ExecuteNonQuery();

                        return (true, "Contraseña restablecida exitosamente");
                    }
                }
                catch (MySqlException ex)
                {
                    if (ex.Message.Contains("Token inválido") || ex.Message.Contains("expirado"))
                    {
                        return (false, "El token de recuperación es inválido o ha expirado.");
                    }
                    return (false, $"Error al confirmar recuperación de clave: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return (false, $"Error al confirmar recuperación de clave: {ex.Message}");
                }
            }
        }

        public (bool Exito, string Mensaje) CambiarContrasena(int rut, string contrasenaActual, string nuevaContrasena)
        {
            if (!ValidarComplejidadContraseña(nuevaContrasena))
            {
                return (false, "La nueva contraseña no cumple con los requisitos de complejidad.");
            }

            string contrasenaActualHash = HashearContraseña(contrasenaActual);
            string nuevaContrasenaHash = HashearContraseña(nuevaContrasena);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_CAMBIAR_CONTRASENA", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_rut", rut);
                        cmd.Parameters.AddWithValue("@p_contrasena_actual", contrasenaActualHash);
                        cmd.Parameters.AddWithValue("@p_nueva_contrasena", nuevaContrasenaHash);

                        MySqlParameter msgParam = new MySqlParameter("@p_mensaje", MySqlDbType.VarChar, 255);
                        msgParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(msgParam);

                        cmd.ExecuteNonQuery();

                        string mensaje = msgParam.Value?.ToString() ?? "";

                        return (mensaje == "OK", mensaje);
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Error al cambiar contraseña: {ex.Message}");
                }
            }
        }

        public (bool Exito, string Mensaje) ActualizarDatosUsuario(
            int rut,
            string nombres,
            string apellidoPaterno,
            string? correoElectronico = null,
            string? telefono = null,
            string? direccion = null,
            string? apellidoMaterno = null)
        {
            if (string.IsNullOrWhiteSpace(nombres))
            {
                return (false, "Los nombres son obligatorios.");
            }

            if (string.IsNullOrWhiteSpace(apellidoPaterno))
            {
                return (false, "El apellido paterno es obligatorio.");
            }

            if (!string.IsNullOrWhiteSpace(correoElectronico) &&
                !ValidarCorreoElectronico(correoElectronico))
            {
                return (false, "El correo electrónico no tiene un formato válido.");
            }

            if (!string.IsNullOrWhiteSpace(telefono) &&
                !ValidarTelefono(telefono))
            {
                return (false, "El número de teléfono no tiene un formato válido.");
            }

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_ACTUALIZAR_DATOS_USUARIO", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_rut", rut);
                        cmd.Parameters.AddWithValue("@p_nombres", nombres);
                        cmd.Parameters.AddWithValue("@p_apellido_paterno", apellidoPaterno);
                        cmd.Parameters.AddWithValue("@p_apellido_materno", apellidoMaterno ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@p_correo_electronico", correoElectronico ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@p_telefono", telefono ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@p_direccion", direccion ?? (object)DBNull.Value);

                        MySqlParameter msgParam = new MySqlParameter("@p_mensaje", MySqlDbType.VarChar, 255);
                        msgParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(msgParam);

                        cmd.ExecuteNonQuery();

                        string mensaje = msgParam.Value?.ToString() ?? "";

                        return (mensaje == "OK", mensaje);
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Error al actualizar datos de usuario: {ex.Message}");
                }
            }
        }

        public (bool Exito, Usuario? Usuario, string Mensaje) ObtenerDatosUsuario(int rut)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_SELECT_USUARIOS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_rut", rut);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Usuario usuario = new Usuario
                                {
                                    rut = Convert.ToInt32(reader["rut"]),
                                    dv_rut = Convert.ToString(reader["dv_rut"]),
                                    nombre = Convert.ToString(reader["nombre"]),
                                    apellido_paterno = Convert.ToString(reader["apellido_paterno"]),
                                    apellido_materno = reader["apellido_materno"] != DBNull.Value
                                        ? Convert.ToString(reader["apellido_materno"])
                                        : null,
                                    correo_electronico = Convert.ToString(reader["correo_electronico"]),
                                    telefono = reader["telefono"] != DBNull.Value
                                        ? Convert.ToString(reader["telefono"])
                                        : null,
                                    direccion = reader["direccion"] != DBNull.Value
                                        ? Convert.ToString(reader["direccion"])
                                        : null,
                                    tipo_usuario = Convert.ToString(reader["tipo_usuario"])
                                };

                                return (true, usuario, "Usuario encontrado");
                            }
                            else
                            {
                                return (false, null, "Usuario no encontrado");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return (false, null, $"Error al obtener datos de usuario: {ex.Message}");
                }
            }
        }

        public (bool Exito, string Mensaje) EliminarUsuario(int rut, int rutSolicitante)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Verificar si el solicitante es Directiva o Administrador
                    MySqlCommand checkRoleCmd = new MySqlCommand("SELECT tipo_usuario FROM usuarios WHERE rut = @rut", conn);
                    checkRoleCmd.Parameters.AddWithValue("@rut", rutSolicitante);
                    string role = Convert.ToString(checkRoleCmd.ExecuteScalar());

                    if (role != "Directiva" && role != "Administrador")
                    {
                        return (false, "No tienes permiso para realizar esta acción.");
                    }

                    // Proceder con la eliminación
                    MySqlCommand cmd = new MySqlCommand("SP_ELIMINAR_USUARIO", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_rut", rut);
                    cmd.ExecuteNonQuery();

                    return (true, "Usuario eliminado correctamente");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar el usuario: {ex.Message}");
            }
        }

        public (bool Exito, string Mensaje) RecuperarClaveSimple(string rutCompleto, string nombreCompleto, string nuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                return (false, "La nueva contraseña no puede estar vacía.");
            }
            if (!ValidarComplejidadContraseña(nuevaContrasena))
            {
                return (false, "La nueva contraseña no cumple con los requisitos de complejidad.");
            }

            var rutParts = rutCompleto.Split('-');
            if (rutParts.Length != 2 || !int.TryParse(rutParts[0], out int rut) || string.IsNullOrEmpty(rutParts[1]))
            {
                return (false, "El formato del RUT no es válido. Debe ser '12345678-9'.");
            }
            string dv = rutParts[1];

            if (!ValidarRut(rut, dv))
            {
                return (false, "El RUT ingresado no es válido.");
            }

            string passwordHash = HashearContraseña(nuevaContrasena);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_RECUPERAR_CLAVE_SIMPLE", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_rut", rut);
                        cmd.Parameters.AddWithValue("@p_nombre_completo", nombreCompleto);
                        cmd.Parameters.AddWithValue("@p_nueva_contrasena_hash", passwordHash);
                        
                        MySqlParameter msgParam = new MySqlParameter("@p_mensaje", MySqlDbType.VarChar, 255);
                        msgParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(msgParam);

                        cmd.ExecuteNonQuery();

                        string mensaje = msgParam.Value?.ToString() ?? "";
                        if (mensaje == "OK")
                        {
                            return (true, "Contraseña actualizada exitosamente.");
                        }
                        return (false, mensaje);
                    }
                }
                catch (Exception ex)
                {
                    // Deberías registrar este error en un sistema de logging.
                    return (false, $"Ocurrió un error al intentar recuperar la contraseña. Detalle: {ex.Message}");
                }
            }
        }
    }
}