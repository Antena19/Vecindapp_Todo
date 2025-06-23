using MySqlConnector;
using REST_VECINDAPP.Modelos;
using System.Data;
using REST_VECINDAPP.Modelos.DTOs;
using System;
using System.Threading.Tasks;

namespace REST_VECINDAPP.CapaNegocios
{
    public class cn_SolicitudesCertificado
    {
        private readonly string _connectionString;

        public cn_SolicitudesCertificado(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        public string SolicitarCertificado(int usuarioRut, int tipoCertificadoId, string motivo, string documentosAdjuntos)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_SOLICITAR_CERTIFICADO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_usuario_rut", usuarioRut);
                    cmd.Parameters.AddWithValue("@p_tipo_certificado_id", tipoCertificadoId);
                    cmd.Parameters.AddWithValue("@p_motivo", motivo);
                    cmd.Parameters.AddWithValue("@p_documentos_adjuntos", documentosAdjuntos);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mensaje = reader["mensaje"].ToString();
                        }
                    }
                }

                conn.Close();
            }

            return mensaje;
        }

        public SolicitudCertificadoDTO ConsultarEstadoSolicitud(int solicitudId)
        {
            SolicitudCertificadoDTO solicitud = null;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_ESTADO_SOLICITUD_CERTIFICADO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_solicitud_id", solicitudId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            solicitud = new SolicitudCertificadoDTO
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UsuarioRut = Convert.ToInt32(reader["usuario_rut"]),
                                TipoCertificadoId = Convert.ToInt32(reader["tipo_certificado_id"]),
                                FechaSolicitud = Convert.ToDateTime(reader["fecha_solicitud"]),
                                Estado = reader["estado"].ToString(),
                                Motivo = reader["motivo"].ToString(),
                                Precio = Convert.ToDecimal(reader["precio"]),
                                Observaciones = reader["observaciones"].ToString()
                            };
                        }
                    }
                }

                conn.Close();
            }

            return solicitud;
        }

        public List<SolicitudCertificadoDTO> ObtenerSolicitudesPendientes()
        {
            List<SolicitudCertificadoDTO> solicitudes = new List<SolicitudCertificadoDTO>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_SOLICITUDES_PENDIENTES", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            solicitudes.Add(new SolicitudCertificadoDTO
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UsuarioRut = Convert.ToInt32(reader["usuario_rut"]),
                                TipoCertificadoId = Convert.ToInt32(reader["tipo_certificado_id"]),
                                FechaSolicitud = Convert.ToDateTime(reader["fecha_solicitud"]),
                                Estado = reader["estado"].ToString(),
                                Motivo = reader["motivo"].ToString(),
                                Precio = Convert.ToDecimal(reader["precio"])
                            });
                        }
                    }
                }

                conn.Close();
            }

            return solicitudes;
        }

        public string AprobarSolicitud(int solicitudId, int directivaRut, string observaciones)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_APROBAR_CERTIFICADO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
                    cmd.Parameters.AddWithValue("@p_directiva_rut", directivaRut);
                    cmd.Parameters.AddWithValue("@p_observaciones", observaciones);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mensaje = reader["mensaje"].ToString();
                        }
                    }
                }

                conn.Close();
            }

            return mensaje;
        }

        public string RechazarSolicitud(int solicitudId, int directivaRut, string motivo)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_RECHAZAR_CERTIFICADO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
                    cmd.Parameters.AddWithValue("@p_directiva_rut", directivaRut);
                    cmd.Parameters.AddWithValue("@p_motivo", motivo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mensaje = reader["mensaje"].ToString();
                        }
                    }
                }

                conn.Close();
            }

            return mensaje;
        }

        public List<SolicitudCertificadoDTO> ObtenerHistorialSolicitudes(int usuarioRut)
        {
            List<SolicitudCertificadoDTO> solicitudes = new List<SolicitudCertificadoDTO>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_HISTORIAL_CERTIFICADOS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_usuario_rut", usuarioRut);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            solicitudes.Add(new SolicitudCertificadoDTO
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UsuarioRut = Convert.ToInt32(reader["usuario_rut"]),
                                TipoCertificadoId = Convert.ToInt32(reader["tipo_certificado_id"]),
                                FechaSolicitud = Convert.ToDateTime(reader["fecha_solicitud"]),
                                Estado = reader["estado"].ToString(),
                                Motivo = reader["motivo"].ToString(),
                                Precio = Convert.ToDecimal(reader["precio"]),
                                Observaciones = reader["observaciones"].ToString()
                            });
                        }
                    }
                }

                conn.Close();
            }

            return solicitudes;
        }

        public async Task<bool> ActualizarEstadoPagoAsync(int solicitudId, string estado, string tokenWebpay, int monto)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand("SP_ACTUALIZAR_ESTADO_PAGO", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
                        cmd.Parameters.AddWithValue("@p_estado", estado);
                        cmd.Parameters.AddWithValue("@p_token_webpay", tokenWebpay);
                        cmd.Parameters.AddWithValue("@p_monto", monto);
                        cmd.Parameters.AddWithValue("@p_fecha_pago", DateTime.Now);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    await conn.CloseAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log del error
                return false;
            }
        }

        public async Task<bool> GuardarPagoEnHistorialAsync(int solicitudId, string tokenWebpay, int monto, string estado)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand("SP_GUARDAR_PAGO_HISTORIAL", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
                        cmd.Parameters.AddWithValue("@p_token_webpay", tokenWebpay);
                        cmd.Parameters.AddWithValue("@p_monto", monto);
                        cmd.Parameters.AddWithValue("@p_estado", estado);
                        cmd.Parameters.AddWithValue("@p_fecha_pago", DateTime.Now);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    await conn.CloseAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log del error
                return false;
            }
        }

        public async Task<SolicitudCertificado> FindAsync(int solicitudId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_SOLICITUD_POR_ID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_solicitud_id", solicitudId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new SolicitudCertificado
                                {
                                    id = Convert.ToInt32(reader["id"]),
                                    usuario_rut = Convert.ToInt32(reader["usuario_rut"]),
                                    tipo_certificado_id = Convert.ToInt32(reader["tipo_certificado_id"]),
                                    fecha_solicitud = Convert.ToDateTime(reader["fecha_solicitud"]),
                                    estado = reader["estado"].ToString(),
                                    motivo = reader["motivo"].ToString(),
                                    documentos_adjuntos = reader["documentos_adjuntos"].ToString(),
                                    fecha_aprobacion = reader["fecha_aprobacion"] != DBNull.Value ? Convert.ToDateTime(reader["fecha_aprobacion"]) : null,
                                    directiva_rut = reader["directiva_rut"] != DBNull.Value ? Convert.ToInt32(reader["directiva_rut"]) : null,
                                    precio = Convert.ToDecimal(reader["precio"]),
                                    observaciones = reader["observaciones"].ToString(),
                                    token_webpay = reader["token_webpay"]?.ToString(),
                                    monto = reader["monto"] != DBNull.Value ? Convert.ToInt32(reader["monto"]) : null,
                                    fecha_pago = reader["fecha_pago"] != DBNull.Value ? Convert.ToDateTime(reader["fecha_pago"]) : null
                                };
                            }
                        }
                    }

                    await conn.CloseAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log del error
                return null;
            }
        }
    }
}
