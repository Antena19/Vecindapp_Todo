using MySqlConnector;
using REST_VECINDAPP.Modelos;
using REST_VECINDAPP.Modelos.DTOs;
using System.Data;

namespace REST_VECINDAPP.CapaNegocios
{
    public class cn_Administrador
    {
        private readonly string _connectionString;

        public cn_Administrador(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        public (bool Exito, string Mensaje) AsignarRolUsuario(int rut, string rol)
        {
            string[] rolesValidos = { "vecino", "socio", "directiva" };
            if (!rolesValidos.Contains(rol.ToLower()))
            {
                return (false, "Rol inválido. Debe ser vecino, socio o directiva.");
            }

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_ASIGNAR_ROL_USUARIO", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_rut", rut);
                        cmd.Parameters.AddWithValue("@p_rol", rol.ToLower());

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
                    return (false, $"Error al asignar rol: {ex.Message}");
                }
            }
        }

        public List<Usuario> ListarUsuariosPorRol(string rol)
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_LISTAR_USUARIOS_POR_ROL", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_rol", rol);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Usuario usuario = new Usuario
                                {
                                    rut = Convert.ToInt32(reader["rut"]),
                                    dv_rut = reader["dv_rut"].ToString(),
                                    nombre = reader["nombre"].ToString(),
                                    apellido_paterno = reader["apellido_paterno"].ToString(),
                                    apellido_materno = reader["apellido_materno"] != DBNull.Value ?
                                        reader["apellido_materno"].ToString() : null,
                                    correo_electronico = reader["correo_electronico"].ToString(),
                                    telefono = reader["telefono"] != DBNull.Value ?
                                        reader["telefono"].ToString() : null,
                                    direccion = reader["direccion"] != DBNull.Value ?
                                        reader["direccion"].ToString() : null,
                                    fecha_registro = Convert.ToDateTime(reader["fecha_registro"]),
                                    estado = Convert.ToInt32(reader["estado"]),
                                    tipo_usuario = reader["tipo_usuario"].ToString()
                                };

                                usuarios.Add(usuario);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al listar usuarios por rol: {ex.Message}");
                }
            }

            return usuarios;
        }

        public (bool Exito, string Mensaje) ConfigurarDirectiva(int rut, bool esDirectiva)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_CONFIGURAR_DIRECTIVA", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_rut", rut);
                        cmd.Parameters.AddWithValue("@p_es_directiva", esDirectiva);

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
                    return (false, $"Error al configurar directiva: {ex.Message}");
                }
            }
        }

        public List<ConfiguracionDTO> ObtenerConfiguracionesJJVV()
        {
            List<ConfiguracionDTO> configuraciones = new List<ConfiguracionDTO>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM configuraciones", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ConfiguracionDTO config = new ConfiguracionDTO
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Clave = reader["clave"].ToString(),
                                    Valor = reader["valor"].ToString(),
                                    Descripcion = reader["descripcion"]?.ToString(),
                                    FechaModificacion = Convert.ToDateTime(reader["fecha_modificacion"])
                                };

                                configuraciones.Add(config);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al obtener configuraciones: {ex.Message}");
                }
            }

            return configuraciones;
        }

        public (bool Exito, string Mensaje) ActualizarConfiguracion(int id, string valor)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_ACTUALIZAR_CONFIGURACION", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_id", id);
                        cmd.Parameters.AddWithValue("@p_valor", valor);

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
                    return (false, $"Error al actualizar configuración: {ex.Message}");
                }
            }
        }

        public RespuestaRegistroDTO GenerarReporteRegistros(DateTime fechaInicio, DateTime fechaFin, string tipoActividad = null)
        {
            RespuestaRegistroDTO respuesta = new RespuestaRegistroDTO
            {
                TotalRegistros = 0,
                Registros = new List<RegistroActividadDTO>()
            };

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_GENERAR_REPORTE_REGISTROS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_fecha_inicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@p_fecha_fin", fechaFin);
                        cmd.Parameters.AddWithValue("@p_tipo_actividad", tipoActividad ?? (object)DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var registro = new RegistroActividadDTO
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    UsuarioRut = reader["usuario_rut"] != DBNull.Value ?
                                        Convert.ToInt32(reader["usuario_rut"]) : null,
                                    NombreUsuario = reader["nombre_usuario"]?.ToString(),
                                    Accion = reader["accion"].ToString(),
                                    Entidad = reader["entidad"].ToString(),
                                    EntidadId = reader["entidad_id"] != DBNull.Value ?
                                        Convert.ToInt32(reader["entidad_id"]) : null,
                                    Detalles = reader["detalles"]?.ToString(),
                                    IP = reader["ip"]?.ToString(),
                                    FechaHora = Convert.ToDateTime(reader["fecha_hora"])
                                };

                                respuesta.Registros.Add(registro);
                            }
                        }
                    }

                    respuesta.TotalRegistros = respuesta.Registros.Count;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al generar reporte de registros: {ex.Message}");
                }
            }

            return respuesta;
        }
    }

}