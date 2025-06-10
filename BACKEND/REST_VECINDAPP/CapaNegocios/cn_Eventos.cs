using MySqlConnector;
using REST_VECINDAPP.Modelos;
using System.Data;

namespace REST_VECINDAPP.CapaNegocios
{
    // CapaNegocios/cn_Eventos.cs
    public class cn_Eventos
    {
        private readonly string _connectionString;

        public cn_Eventos(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        public (bool Exito, Evento? Evento, string Mensaje) CrearEvento(Evento evento)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_CREAR_EVENTO", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_titulo", evento.Titulo);
                        cmd.Parameters.AddWithValue("@p_descripcion", evento.Descripcion);
                        cmd.Parameters.AddWithValue("@p_fecha_evento", evento.FechaEvento);
                        cmd.Parameters.AddWithValue("@p_hora_evento", evento.HoraEvento);
                        cmd.Parameters.AddWithValue("@p_lugar", evento.Lugar);
                        cmd.Parameters.AddWithValue("@p_directiva_rut", evento.DirectivaRut);
                        cmd.Parameters.AddWithValue("@p_notas", evento.Notas ?? (object)DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                evento.Id = Convert.ToInt32(reader["id_evento"]);
                                evento.CodigoQr = Convert.ToString(reader["codigo_qr"]);
                                return (true, evento, "Evento creado exitosamente");
                            }
                        }
                    }
                }
                return (false, null, "Error al crear el evento");
            }
            catch (MySqlException ex)
            {
                return (false, null, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public (bool Exito, string Mensaje) RegistrarAsistencia(string codigoQr, int usuarioRut)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_REGISTRAR_ASISTENCIA", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_codigo_qr", codigoQr);
                        cmd.Parameters.AddWithValue("@p_usuario_rut", usuarioRut);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (true, Convert.ToString(reader["mensaje"]));
                            }
                        }
                    }
                }
                return (false, "Error al registrar la asistencia");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public (bool Exito, List<AsistenciaEvento>? Asistentes, string Mensaje) ConsultarAsistentes(int eventoId)
        {
            try
            {
                var asistentes = new List<AsistenciaEvento>();
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_ASISTENTES", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_evento_id", eventoId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var asistente = new AsistenciaEvento
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    EventoId = Convert.ToInt32(reader["evento_id"]),
                                    UsuarioRut = Convert.ToInt32(reader["usuario_rut"]),
                                    Nombre = Convert.ToString(reader["nombre"]) ?? string.Empty,
                                    Apellido = Convert.ToString(reader["apellido"]) ?? string.Empty,
                                    FechaAsistencia = Convert.ToDateTime(reader["fecha_asistencia"])
                                };
                                asistentes.Add(asistente);
                            }
                        }
                    }
                }
                return (true, asistentes, "Asistentes consultados exitosamente");
            }
            catch (MySqlException ex)
            {
                return (false, null, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public (bool Exito, List<Evento>? Eventos, string Mensaje) ConsultarEventos(int usuarioRut)
        {
            try
            {
                var eventos = new List<Evento>();
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_EVENTOS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_usuario_rut", usuarioRut);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var evento = new Evento
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Titulo = Convert.ToString(reader["titulo"]),
                                    Descripcion = Convert.ToString(reader["descripcion"]),
                                    FechaEvento = Convert.ToDateTime(reader["fecha_evento"]),
                                    HoraEvento = ((TimeSpan)reader["hora_evento"]),
                                    Lugar = Convert.ToString(reader["lugar"]),
                                    DirectivaRut = Convert.ToInt32(reader["directiva_rut"]),
                                    Estado = Convert.ToString(reader["estado"]),
                                    CodigoQr = Convert.ToString(reader["codigo_qr"]),
                                    FechaCreacion = Convert.ToDateTime(reader["fecha_creacion"]),
                                    Notas = reader["notas"] != DBNull.Value ? Convert.ToString(reader["notas"]) : null
                                };
                                eventos.Add(evento);
                            }
                        }
                    }
                }
                return (true, eventos, "Eventos consultados exitosamente");
            }
            catch (MySqlException ex)
            {
                return (false, null, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public (bool Exito, Evento? Evento, string Mensaje) ActualizarEvento(int id, Evento evento)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_ACTUALIZAR_EVENTO", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_evento_id", id);
                        cmd.Parameters.AddWithValue("@p_titulo", evento.Titulo);
                        cmd.Parameters.AddWithValue("@p_descripcion", evento.Descripcion);
                        cmd.Parameters.AddWithValue("@p_fecha_evento", evento.FechaEvento);
                        cmd.Parameters.AddWithValue("@p_hora_evento", evento.HoraEvento);
                        cmd.Parameters.AddWithValue("@p_lugar", evento.Lugar);
                        cmd.Parameters.AddWithValue("@p_notas", evento.Notas ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@p_estado", evento.Estado);

                        int filasAfectadas = cmd.ExecuteNonQuery();
                        if (filasAfectadas >= 0)
                        {
                            evento.Id = id;
                            return (true, evento, "Evento actualizado exitosamente");
                        }
                    }
                }
                return (false, null, "No se pudo actualizar el evento");
            }
            catch (MySqlException ex)
            {
                return (false, null, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public (bool Exito, List<AsistenciaEvento>? Asistencias, string Mensaje) ConsultarHistorialAsistencia(int usuarioRut)
        {
            try
            {
                var asistencias = new List<AsistenciaEvento>();
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_HISTORIAL_ASISTENCIA", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_usuario_rut", usuarioRut);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var asistencia = new AsistenciaEvento
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    EventoId = Convert.ToInt32(reader["evento_id"]),
                                    UsuarioRut = Convert.ToInt32(reader["usuario_rut"]),
                                    FechaAsistencia = Convert.ToDateTime(reader["fecha_asistencia"])
                                };
                                asistencias.Add(asistencia);
                            }
                        }
                    }
                }
                return (true, asistencias, "Historial de asistencia consultado exitosamente");
            }
            catch (MySqlException ex)
            {
                return (false, null, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public (bool Exito, List<Evento>? Eventos, string Mensaje) GenerarReporteAsistencia(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var eventos = new List<Evento>();
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_GENERAR_REPORTE_ASISTENCIA", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_fecha_inicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@p_fecha_fin", fechaFin);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var evento = new Evento
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Titulo = Convert.ToString(reader["titulo"]),
                                    FechaEvento = Convert.ToDateTime(reader["fecha_evento"]),
                                    HoraEvento = ((TimeSpan)reader["hora_evento"]),
                                    Lugar = Convert.ToString(reader["lugar"])
                                };
                                eventos.Add(evento);
                            }
                        }
                    }
                }
                return (true, eventos, "Reporte generado exitosamente");
            }
            catch (MySqlException ex)
            {
                return (false, null, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public (bool Exito, Evento? Evento, string Mensaje) ConsultarEvento(int eventoId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_EVENTO", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_evento_id", eventoId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var evento = new Evento
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Titulo = Convert.ToString(reader["titulo"]),
                                    Descripcion = Convert.ToString(reader["descripcion"]),
                                    FechaEvento = Convert.ToDateTime(reader["fecha_evento"]),
                                    HoraEvento = ((TimeSpan)reader["hora_evento"]),
                                    Lugar = Convert.ToString(reader["lugar"]),
                                    DirectivaRut = Convert.ToInt32(reader["directiva_rut"]),
                                    Estado = Convert.ToString(reader["estado"]),
                                    CodigoQr = Convert.ToString(reader["codigo_qr"]),
                                    FechaCreacion = Convert.ToDateTime(reader["fecha_creacion"]),
                                    Notas = reader["notas"] != DBNull.Value ? Convert.ToString(reader["notas"]) : null
                                };
                                return (true, evento, "Evento consultado exitosamente");
                            }
                        }
                    }
                }
                return (false, null, "Evento no encontrado");
            }
            catch (MySqlException ex)
            {
                return (false, null, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public (bool Exito, string Mensaje) CancelarEvento(int eventoId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_CANCELAR_EVENTO", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_evento_id", eventoId);

                        int filasAfectadas = cmd.ExecuteNonQuery();
                        if (filasAfectadas > 0)
                        {
                            return (true, "Evento cancelado exitosamente");
                        }
                    }
                }
                return (false, "No se pudo cancelar el evento");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }
    }
}
