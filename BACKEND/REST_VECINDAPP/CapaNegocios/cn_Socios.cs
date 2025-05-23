using MySqlConnector;
using REST_VECINDAPP.Modelos;
using System.Data;
using REST_VECINDAPP.Modelos.DTOs;

namespace REST_VECINDAPP.CapaNegocios
{
    public class cn_Socios
    {
        private readonly string _connectionString;

        public cn_Socios(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        public string SolicitarMembresia(int rut, string rutaDocumentoIdentidad, string rutaDocumentoDomicilio)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_SOLICITAR_MEMBRESIA_SOCIO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_rut", rut);
                    cmd.Parameters.AddWithValue("@p_ruta_documento_identidad", rutaDocumentoIdentidad);
                    cmd.Parameters.AddWithValue("@p_ruta_documento_domicilio", rutaDocumentoDomicilio);

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

        public SolicitudSocioDTO ConsultarEstadoSolicitud(int rut)
        {
            SolicitudSocioDTO solicitud = null;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_ESTADO_SOLICITUD", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_rut", rut);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            solicitud = new SolicitudSocioDTO
                            {
                                Rut = Convert.ToInt32(reader["rut"]),
                                Nombre = reader["nombre"].ToString(),
                                ApellidoPaterno = reader["apellido_paterno"].ToString(),
                                ApellidoMaterno = reader["apellido_materno"].ToString(),
                                FechaSolicitud = Convert.ToDateTime(reader["fecha_solicitud"]),
                                EstadoSolicitud = reader["estado_solicitud"].ToString(),
                                MotivoRechazo = reader["motivo_rechazo"].ToString()
                            };
                        }
                    }
                }

                conn.Close();
            }

            return solicitud;
        }

        public Socio ObtenerPerfil(int rut)
        {
            Socio socio = null;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_OBTENER_PERFIL_SOCIO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_rut", rut);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            socio = new Socio
                            {
                                idsocio = Convert.ToInt32(reader["idsocio"]),
                                num_socio = reader["num_socio"] != DBNull.Value ? Convert.ToInt32(reader["num_socio"]) : 0,
                                rut = Convert.ToInt32(reader["rut"]),
                                fecha_solicitud = reader["fecha_solicitud"] != DBNull.Value ?
                                    Convert.ToDateTime(reader["fecha_solicitud"]) : DateTime.MinValue,
                                fecha_aprobacion = reader["fecha_aprobacion"] != DBNull.Value ?
                                    Convert.ToDateTime(reader["fecha_aprobacion"]) : (DateTime?)null,
                                estado_solicitud = Convert.ToString(reader["estado_solicitud"]),
                                estado = Convert.ToInt32(reader["estado"])
                            };
                        }
                    }
                }

                conn.Close();
            }

            return socio;
        }

        public bool EsSocio(int rut)
        {
            bool esSocio = false;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_VERIFICAR_ES_SOCIO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_rut", rut);

                    var result = cmd.ExecuteScalar();
                    esSocio = result != null && Convert.ToBoolean(result);
                }

                conn.Close();
            }

            return esSocio;
        }

        /*public List<CuotaSocioDTO> ConsultarCuotas(int rut)
        {
            List<CuotaSocioDTO> cuotas = new List<CuotaSocioDTO>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_CUOTAS_SOCIO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_rut", rut);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CuotaSocioDTO cuota = new CuotaSocioDTO
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                FechaGeneracion = Convert.ToDateTime(reader["fecha_generacion"]),
                                FechaVencimiento = Convert.ToDateTime(reader["fecha_vencimiento"]),
                                Monto = Convert.ToDecimal(reader["monto"]),
                                Estado = reader["estado"].ToString(),
                                TipoCuota = reader["nombre_tipo_cuota"].ToString()
                            };

                            cuotas.Add(cuota);
                        }
                    }
                }

                conn.Close();
            }

            return cuotas;
        }*/

        public string PagarCuota(int rut, int cuotaId, decimal monto, string metodoPago)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_REGISTRAR_PAGO_CUOTA", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_usuario_rut", rut);
                    cmd.Parameters.AddWithValue("@p_cuota_id", cuotaId);
                    cmd.Parameters.AddWithValue("@p_monto", monto);
                    cmd.Parameters.AddWithValue("@p_metodo_pago", metodoPago);
                    cmd.Parameters.AddWithValue("@p_token_webpay", DBNull.Value);
                    cmd.Parameters.AddWithValue("@p_url_pago", DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mensaje = "Pago registrado correctamente";
                        }
                    }
                }

                conn.Close();
            }

            return mensaje;
        }

        /* public List<CertificadoSocioDTO> ConsultarCertificados(int rut)
        {
            List<CertificadoSocioDTO> certificados = new List<CertificadoSocioDTO>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_HISTORIAL_CERTIFICADOS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_usuario_rut", rut);
                    cmd.Parameters.AddWithValue("@p_estado", DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CertificadoSocioDTO certificado = new CertificadoSocioDTO
                            {
                                IdSolicitud = Convert.ToInt32(reader["id_solicitud"]),
                                TipoCertificado = reader["tipo_certificado"].ToString(),
                                FechaSolicitud = Convert.ToDateTime(reader["fecha_solicitud"]),
                                Estado = reader["estado"].ToString(),
                                CodigoVerificacion = reader["codigo_verificacion"]?.ToString(),
                                FechaEmision = reader["fecha_emision"] != DBNull.Value ?
                                    Convert.ToDateTime(reader["fecha_emision"]) : (DateTime?)null,
                                FechaVencimiento = reader["fecha_vencimiento"] != DBNull.Value ?
                                    Convert.ToDateTime(reader["fecha_vencimiento"]) : (DateTime?)null
                            };

                            certificados.Add(certificado);
                        }
                    }
                }

                conn.Close();
            }

            return certificados;
        }*/
    }
}