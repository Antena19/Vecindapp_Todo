using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using REST_VECINDAPP.Modelos.DTOs;
using Microsoft.Extensions.Configuration;
using REST_VECINDAPP.Modelos;

namespace REST_VECINDAPP.CapaNegocios
{
    public class cn_Directiva
    {
        private readonly string _connectionString;

        public cn_Directiva(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        public List<Socio> ListarSocios(int idsocio)
        {
            List<Socio> Socios = new List<Socio>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("SP_SELECT_SOCIOS", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                if (idsocio != -1)
                    cmd.Parameters.AddWithValue("@p_idsocio", idsocio);
                else
                    cmd.Parameters.AddWithValue("@p_idsocio", DBNull.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Socio socioTemp = new Socio();

                        socioTemp.idsocio = Convert.ToInt32(reader["idsocio"]);
                        socioTemp.num_socio = reader["num_socio"] != DBNull.Value ? Convert.ToInt32(reader["num_socio"]) : 0;
                        socioTemp.rut = Convert.ToInt32(reader["rut"]);
                        socioTemp.fecha_solicitud = reader["fecha_solicitud"] != DBNull.Value ?
                            Convert.ToDateTime(reader["fecha_solicitud"]) : DateTime.MinValue;
                        socioTemp.fecha_aprobacion = reader["fecha_aprobacion"] != DBNull.Value ?
                            Convert.ToDateTime(reader["fecha_aprobacion"]) : (DateTime?)null;
                        socioTemp.estado_solicitud = Convert.ToString(reader["estado_solicitud"]);
                        socioTemp.motivo_rechazo = Convert.ToString(reader["motivo_rechazo"]);
                        socioTemp.documento_identidad = reader["documento_identidad"] != DBNull.Value ?
                            reader["documento_identidad"].ToString() : null;
                        socioTemp.documento_domicilio = reader["documento_domicilio"] != DBNull.Value ?
                            reader["documento_domicilio"].ToString() : null;
                        socioTemp.estado = Convert.ToInt32(reader["estado"]);
                        

                        Socios.Add(socioTemp);
                    }
                }

                conn.Close();
            }

            return Socios;
        }

        public List<SolicitudSocioDTO> ConsultarSolicitudes(string estadoSolicitud = null)
        {
            List<SolicitudSocioDTO> solicitudes = new List<SolicitudSocioDTO>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_SOLICITUDES_SOCIOS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_estado_solicitud", estadoSolicitud ?? "");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SolicitudSocioDTO solicitud = new SolicitudSocioDTO
                            {
                                Rut = Convert.ToInt32(reader["rut"]),
                                Nombre = reader["nombre"].ToString(),
                                ApellidoPaterno = reader["apellido_paterno"].ToString(),
                                ApellidoMaterno = reader["apellido_materno"] != DBNull.Value ? reader["apellido_materno"].ToString() : string.Empty,
                                FechaSolicitud = Convert.ToDateTime(reader["fecha_solicitud"]),
                                EstadoSolicitud = reader["estado_solicitud"].ToString(),
                                MotivoRechazo = reader["motivo_rechazo"] != DBNull.Value ? reader["motivo_rechazo"].ToString() : string.Empty,
                                DocumentoIdentidad = reader["documento_identidad"] != DBNull.Value ? reader["documento_identidad"].ToString() : string.Empty,
                                DocumentoDomicilio = reader["documento_domicilio"] != DBNull.Value ? reader["documento_domicilio"].ToString() : string.Empty
                            };

                            solicitudes.Add(solicitud);
                        }
                    }
                }

                conn.Close();
            }

            return solicitudes;
        }

        public string AprobarSolicitud(int rut)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_APROBAR_SOLICITUD_SOCIO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_rut", rut);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mensaje = reader["mensaje"]?.ToString() ?? string.Empty;
                        }
                    }
                }

                conn.Close();
            }

            return mensaje;
        }

        public string RechazarSolicitud(int rut, string motivoRechazo)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_RECHAZAR_SOLICITUD_SOCIO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_rut", rut);
                    cmd.Parameters.AddWithValue("@p_motivo_rechazo", motivoRechazo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mensaje = reader["mensaje"]?.ToString() ?? string.Empty;
                        }
                    }
                }

                conn.Close();
            }

            return mensaje;
        }

        public EstadisticasResponse ObtenerEstadisticas()
        {
            var response = new EstadisticasResponse();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_obtener_estadisticas_socios", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    response.Estadisticas = new EstadisticasSocios
                                    {
                                        TotalSocios = 0,
                                        SolicitudesPendientes = 0,
                                        SociosActivos = 0,
                                        SociosInactivos = 0
                                    };
                                }
                                else if (reader.Read())
                                {
                                    response.Estadisticas = new EstadisticasSocios
                                    {
                                        TotalSocios = reader["total_socios"] != DBNull.Value ? Convert.ToInt32(reader["total_socios"]) : 0,
                                        SolicitudesPendientes = reader["solicitudes_pendientes"] != DBNull.Value ? Convert.ToInt32(reader["solicitudes_pendientes"]) : 0,
                                        SociosActivos = reader["socios_activos"] != DBNull.Value ? Convert.ToInt32(reader["socios_activos"]) : 0,
                                        SociosInactivos = reader["socios_inactivos"] != DBNull.Value ? Convert.ToInt32(reader["socios_inactivos"]) : 0
                                    };
                                }

                                if (reader.NextResult())
                                {
                                    response.UltimasActividades = new List<Actividad>();
                                    while (reader.Read())
                                    {
                                        response.UltimasActividades.Add(new Actividad
                                        {
                                            Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                            Titulo = reader["titulo"] != DBNull.Value ? reader["titulo"].ToString() : string.Empty,
                                            Descripcion = reader["descripcion"] != DBNull.Value ? reader["descripcion"].ToString() : string.Empty,
                                            Fecha = reader["fecha"] != DBNull.Value ? Convert.ToDateTime(reader["fecha"]) : DateTime.Now,
                                            Icono = reader["icono"] != DBNull.Value ? reader["icono"].ToString() : string.Empty,
                                            Color = reader["color"] != DBNull.Value ? reader["color"].ToString() : string.Empty
                                        });
                                    }
                                }
                                else
                                {
                                    response.UltimasActividades = new List<Actividad>();
                                }
                            }
                        }
                    }
                    catch (MySqlException sqlEx)
                    {
                        Console.WriteLine($"Error de MySQL: {sqlEx.Message}, Número: {sqlEx.Number}");
                        throw;
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                throw;
            }
        }

        public List<SocioActivoDTO> ObtenerSociosActivos()
        {
            List<SocioActivoDTO> socios = new List<SocioActivoDTO>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_OBTENER_SOCIOS_ACTIVOS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nombre = reader["nombre"].ToString();
                                string apellidoPaterno = reader["apellido_paterno"].ToString();
                                string apellidoMaterno = reader["apellido_materno"] != DBNull.Value ? reader["apellido_materno"].ToString() : "";

                                socios.Add(new SocioActivoDTO
                                {
                                    IdSocio = Convert.ToInt32(reader["idsocio"]),
                                    Rut = Convert.ToInt32(reader["rut"]),
                                    DvRut = reader["dv_rut"].ToString(),
                                    NombreCompleto = $"{nombre} {apellidoPaterno} {apellidoMaterno}".Trim(),
                                    Correo = reader["correo_electronico"].ToString(),
                                    Telefono = reader["telefono"].ToString(),
                                    Direccion = reader["direccion"].ToString(),
                                    FechaRegistro = Convert.ToDateTime(reader["fecha_registro"]),
                                    FechaAprobacion = reader["fecha_aprobacion"] != DBNull.Value ? Convert.ToDateTime(reader["fecha_aprobacion"]) : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener socios activos: {ex.Message}");
                throw;
            }

            return socios;
        }

        public string ActualizarEstadoSocio(int idSocio, int estado, string? motivo)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand("SP_ACTUALIZAR_ESTADO_SOCIO", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_idsocio", idSocio);
                        cmd.Parameters.AddWithValue("@p_estado", estado);
                        cmd.Parameters.AddWithValue("@p_motivo", motivo ?? (object)DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                mensaje = reader["mensaje"]?.ToString() ?? string.Empty;
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Error de MySQL al actualizar estado del socio: {ex.Message}");
                    throw;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            return mensaje;
        }

        public List<SocioHistorialDTO> ObtenerHistorialSocios()
        {
            List<SocioHistorialDTO> socios = new List<SocioHistorialDTO>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_OBTENER_HISTORIAL_SOCIOS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nombre = reader["nombre"].ToString();
                                string apellidoPaterno = reader["apellido_paterno"].ToString();
                                string apellidoMaterno = reader["apellido_materno"] != DBNull.Value ?
                                    reader["apellido_materno"].ToString() : "";

                                socios.Add(new SocioHistorialDTO
                                {
                                    IdSocio = Convert.ToInt32(reader["idsocio"]),
                                    Rut = Convert.ToInt32(reader["rut"]),
                                    DvRut = reader["dv_rut"].ToString(),
                                    NombreCompleto = $"{nombre} {apellidoPaterno} {apellidoMaterno}".Trim(),
                                    Correo = reader["correo_electronico"].ToString(),
                                    Telefono = reader["telefono"].ToString(),
                                    Direccion = reader["direccion"].ToString(),
                                    FechaRegistro = Convert.ToDateTime(reader["fecha_registro"]),
                                    FechaAprobacion = reader["fecha_aprobacion"] != DBNull.Value ?
                                        Convert.ToDateTime(reader["fecha_aprobacion"]) : null,
                                    Estado = Convert.ToInt32(reader["estado"]),
                                    MotivoDesactivacion = reader["motivo_desactivacion"]?.ToString(),
                                    FechaDesactivacion = reader["fecha_desactivacion"] != DBNull.Value ?
                                        Convert.ToDateTime(reader["fecha_desactivacion"]) : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener historial de socios: {ex.Message}");
                throw;
            }

            return socios;
        }

        public List<SocioActivoDTO> ObtenerTodosSocios()
        {
            List<SocioActivoDTO> socios = new List<SocioActivoDTO>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SP_OBTENER_TODOS_SOCIOS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nombre = reader["nombre"].ToString();
                                string apellidoPaterno = reader["apellido_paterno"].ToString();
                                string apellidoMaterno = reader["apellido_materno"] != DBNull.Value ?
                                    reader["apellido_materno"].ToString() : "";

                                socios.Add(new SocioActivoDTO
                                {
                                    IdSocio = Convert.ToInt32(reader["idsocio"]),
                                    num_socio = reader["num_socio"] != DBNull.Value ? Convert.ToInt32(reader["num_socio"]) : 0,
                                    Rut = Convert.ToInt32(reader["rut"]),
                                    DvRut = reader["dv_rut"].ToString(),
                                    NombreCompleto = $"{nombre} {apellidoPaterno} {apellidoMaterno}".Trim(),
                                    Correo = reader["correo_electronico"].ToString(),
                                    Telefono = reader["telefono"].ToString(),
                                    Direccion = reader["direccion"].ToString(),
                                    FechaRegistro = Convert.ToDateTime(reader["fecha_registro"]),
                                    FechaAprobacion = reader["fecha_aprobacion"] != DBNull.Value ?
                                        Convert.ToDateTime(reader["fecha_aprobacion"]) : null,
                                    Estado = Convert.ToInt32(reader["estado"]),
                                    MotivoDesactivacion = reader["motivo_desactivacion"]?.ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener todos los socios: {ex.Message}");
                throw;
            }

            return socios;
        }

        public string RevocarMembresia(int rut, string motivoRevocacion)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_REVOCAR_MEMBRESIA_SOCIO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_rut", rut);
                    cmd.Parameters.AddWithValue("@p_motivo_revocacion", motivoRevocacion);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mensaje = reader["mensaje"]?.ToString() ?? string.Empty;
                        }
                    }
                }

                conn.Close();
            }

            return mensaje;
        }

        /*public List<CertificadoAdminDTO> ConsultarSolicitudesCertificados(string estado = null)
        {
            List<CertificadoAdminDTO> solicitudes = new List<CertificadoAdminDTO>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_CONSULTAR_SOLICITUDES_CERTIFICADOS_ADMIN", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_estado", estado ?? "");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            solicitudes.Add(new CertificadoAdminDTO
                            {
                                IdSolicitud = Convert.ToInt32(reader["id"]),
                                Rut = Convert.ToInt32(reader["usuario_rut"]),
                                NombreCompleto = reader["nombre_completo"].ToString(),
                                TipoCertificado = reader["tipo_certificado"].ToString(),
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
        }*/

        public string AprobarCertificado(int solicitudId, int directivaRut, string observaciones)
        {
            string mensaje = string.Empty;
            string codigoVerificacion = string.Empty;

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
                            codigoVerificacion = reader["codigo_verificacion"]?.ToString() ?? string.Empty;
                            mensaje = reader["mensaje"]?.ToString() ?? string.Empty;
                        }
                    }
                }

                conn.Close();
            }

            return $"Certificado aprobado. Código de verificación: {codigoVerificacion}";
        }

        public string RechazarCertificado(int solicitudId, int directivaRut, string motivoRechazo)
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
                    cmd.Parameters.AddWithValue("@p_motivo_rechazo", motivoRechazo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mensaje = reader["mensaje"]?.ToString() ?? string.Empty;
                        }
                    }
                }

                conn.Close();
            }

            return mensaje;
        }

        /*public ReporteFinancieroDTO GenerarReporteFinanciero(DateTime fechaInicio, DateTime fechaFin)
        {
            ReporteFinancieroDTO reporte = new ReporteFinancieroDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                DetalleIngresos = new List<DetalleIngresoDTO>()
            };

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_GENERAR_REPORTE_FINANCIERO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_fecha_inicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@p_fecha_fin", fechaFin);

                    using (var reader = cmd.ExecuteReader())
                    {
                        decimal totalIngresos = 0;

                        while (reader.Read())
                        {
                            var detalle = new DetalleIngresoDTO
                            {
                                Concepto = reader["concepto"].ToString(),
                                Cantidad = Convert.ToInt32(reader["cantidad"]),
                                TotalIngresos = Convert.ToDecimal(reader["total_ingresos"]),
                                TotalIngresosConfirmados = Convert.ToDecimal(reader["total_ingresos_confirmados"])
                            };

                            reporte.DetalleIngresos.Add(detalle);
                            totalIngresos += detalle.TotalIngresosConfirmados;
                        }

                        reporte.TotalIngresos = totalIngresos;
                    }
                }

                conn.Close();
            }

            return reporte;
        }*/

        /*public ConfiguracionTarifasDTO ObtenerConfiguracionTarifas()
        {
            ConfiguracionTarifasDTO configuracion = new ConfiguracionTarifasDTO
            {
                TiposCertificado = new List<TipoCertificadoDTO>(),
                TiposCuota = new List<TipoCuotaDTO>()
            };

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                // Obtener tipos de certificado
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM tipos_certificado WHERE activo = 1", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            configuracion.TiposCertificado.Add(new TipoCertificadoDTO
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Nombre = reader["nombre"].ToString(),
                                Descripcion = reader["descripcion"].ToString(),
                                PrecioSocio = Convert.ToDecimal(reader["precio_socio"]),
                                PrecioVecino = Convert.ToDecimal(reader["precio_vecino"]),
                                DocumentosRequeridos = reader["documentos_requeridos"].ToString(),
                                MediosPagoHabilitados = reader["medios_pago_habilitados"].ToString()
                            });
                        }
                    }
                }

                // Obtener tipos de cuota
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM tipos_cuota WHERE activo = 1", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            configuracion.TiposCuota.Add(new TipoCuotaDTO
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Nombre = reader["nombre"].ToString(),
                                Descripcion = reader["descripcion"].ToString(),
                                Monto = Convert.ToDecimal(reader["monto"]),
                                Periodicidad = reader["periodicidad"].ToString(),
                                MediosPagoHabilitados = reader["medios_pago_habilitados"].ToString()
                            });
                        }
                    }
                }

                conn.Close();
            }

            return configuracion;
        }*/

        public string ConfigurarTarifaCertificado(int tipoCertificadoId, decimal precioSocio, decimal precioVecino, string mediosPago)
        {
            string mensaje = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("SP_CONFIGURAR_TARIFA_CERTIFICADO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_tipo_certificado_id", tipoCertificadoId);
                    cmd.Parameters.AddWithValue("@p_precio_socio", precioSocio);
                    cmd.Parameters.AddWithValue("@p_precio_vecino", precioVecino);
                    cmd.Parameters.AddWithValue("@p_medios_pago", mediosPago);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mensaje = reader["mensaje"]?.ToString() ?? string.Empty;
                        }
                    }
                }

                conn.Close();
            }

            return mensaje;
        }
    }
}