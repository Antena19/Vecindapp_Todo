using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using REST_VECINDAPP.Modelos;
using System.Data;
using REST_VECINDAPP.Modelos.DTOs;
using Microsoft.Extensions.Configuration;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace REST_VECINDAPP.CapaNegocios
{
    public class cn_Certificados
    {
        private readonly string _connectionString;

        public cn_Certificados(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        public async Task<int> SolicitarCertificado(int usuarioId, SolicitudCertificadoDTO solicitud)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_SOLICITAR_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_usuario_rut", usuarioId);
            command.Parameters.AddWithValue("@p_tipo_certificado_id", solicitud.TipoCertificadoId);
            command.Parameters.AddWithValue("@p_motivo", solicitud.Motivo);
            command.Parameters.AddWithValue("@p_documentos_adjuntos", solicitud.DocumentosAdjuntos ?? string.Empty);
            command.Parameters.AddWithValue("@p_precio", solicitud.Precio);
            command.Parameters.AddWithValue("@p_observaciones", solicitud.Observaciones ?? string.Empty);
            command.Parameters.AddWithValue("@p_nombre_solicitante", solicitud.NombreSolicitante ?? string.Empty);
            command.Parameters.AddWithValue("@p_rut_solicitante", solicitud.RutSolicitante ?? string.Empty);
            command.Parameters.AddWithValue("@p_telefono", solicitud.Telefono ?? string.Empty);
            command.Parameters.AddWithValue("@p_direccion", solicitud.Direccion ?? string.Empty);
            command.Parameters.AddWithValue("@p_firma_digital", solicitud.FirmaDigital ?? string.Empty);
            command.Parameters.AddWithValue("@p_hash_verificacion", solicitud.HashVerificacion ?? string.Empty);
            command.Parameters.AddWithValue("@p_timestamp_firma", solicitud.TimestampFirma.ToString());
            command.Parameters.AddWithValue("@p_usuario_firmante", solicitud.UsuarioFirmante ?? string.Empty);

            var solicitudId = Convert.ToInt32(await command.ExecuteScalarAsync());
            return solicitudId;
        }

        public async Task<bool> AprobarCertificado(int solicitudId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_APROBAR_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> RechazarCertificado(int solicitudId, string motivoRechazo)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_RECHAZAR_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
            command.Parameters.AddWithValue("@p_motivo_rechazo", motivoRechazo);
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<List<TipoCertificado>> ObtenerTiposCertificado()
        {
            var tipos = new List<TipoCertificado>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_TIPOS_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tipos.Add(new TipoCertificado
                {
                    id = reader.GetInt32("id"),
                    nombre = reader.GetString("nombre"),
                    descripcion = reader.GetString("descripcion"),
                    precio_socio = reader.GetDecimal("precio_socio"),
                    precio_vecino = reader.GetDecimal("precio_vecino"),
                    documentos_requeridos = reader.GetString("documentos_requeridos"),
                    activo = reader.GetBoolean("activo"),
                    medios_pago_habilitados = reader.GetString("medios_pago_habilitados")
                });
            }
            return tipos;
        }

        public async Task<List<SolicitudCertificadoDTO>> ObtenerSolicitudesUsuario(int usuarioRut)
        {
            var solicitudes = new List<SolicitudCertificadoDTO>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_SOLICITUDES_USUARIO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_usuario_rut", usuarioRut);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                solicitudes.Add(new SolicitudCertificadoDTO
                {
                    Id = reader.GetInt32("id"),
                    UsuarioRut = reader.GetInt32("usuario_rut"),
                    TipoCertificadoId = reader.GetInt32("tipo_certificado_id"),
                    FechaSolicitud = reader.GetDateTime("fecha_solicitud"),
                    Estado = reader.GetString("estado"),
                    Motivo = reader.GetString("motivo"),
                    DocumentosAdjuntos = reader.GetString("documentos_adjuntos"),
                    FechaAprobacion = reader.IsDBNull("fecha_aprobacion") ? null : reader.GetDateTime("fecha_aprobacion"),
                    DirectivaRut = reader.IsDBNull("directiva_rut") ? null : reader.GetInt32("directiva_rut"),
                    Precio = reader.GetDecimal("precio"),
                    Observaciones = reader.GetString("observaciones")
                });
            }
            return solicitudes;
        }

        public async Task<List<SolicitudCertificadoDTO>> ObtenerSolicitudesPendientes()
        {
            var solicitudes = new List<SolicitudCertificadoDTO>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_SOLICITUDES_PENDIENTES", connection);
            command.CommandType = CommandType.StoredProcedure;
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                solicitudes.Add(new SolicitudCertificadoDTO
                {
                    Id = reader.GetInt32("id"),
                    UsuarioRut = reader.GetInt32("usuario_rut"),
                    TipoCertificadoId = reader.GetInt32("tipo_certificado_id"),
                    FechaSolicitud = reader.GetDateTime("fecha_solicitud"),
                    Estado = reader.GetString("estado"),
                    Motivo = reader.IsDBNull("motivo") ? null : reader.GetString("motivo"),
                    DocumentosAdjuntos = reader.IsDBNull("documentos_adjuntos") ? null : reader.GetString("documentos_adjuntos"),
                    FechaAprobacion = reader.IsDBNull("fecha_aprobacion") ? null : reader.GetDateTime("fecha_aprobacion"),
                    DirectivaRut = reader.IsDBNull("directiva_rut") ? null : reader.GetInt32("directiva_rut"),
                    Precio = reader.GetDecimal("precio"),
                    Observaciones = reader.IsDBNull("observaciones") ? null : reader.GetString("observaciones")
                });
            }
            return solicitudes;
        }

        public async Task<SolicitudCertificadoDTO> ObtenerDetalleSolicitud(int solicitudId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_DETALLE_SOLICITUD", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new SolicitudCertificadoDTO
                {
                    Id = reader.GetInt32("id"),
                    UsuarioRut = reader.GetInt32("usuario_rut"),
                    TipoCertificadoId = reader.GetInt32("tipo_certificado_id"),
                    FechaSolicitud = reader.GetDateTime("fecha_solicitud"),
                    Estado = reader.GetString("estado"),
                    Motivo = reader.IsDBNull("motivo") ? null : reader.GetString("motivo"),
                    DocumentosAdjuntos = reader.IsDBNull("documentos_adjuntos") ? null : reader.GetString("documentos_adjuntos"),
                    FechaAprobacion = reader.IsDBNull("fecha_aprobacion") ? null : reader.GetDateTime("fecha_aprobacion"),
                    DirectivaRut = reader.IsDBNull("directiva_rut") ? null : reader.GetInt32("directiva_rut"),
                    Precio = reader.GetDecimal("precio"),
                    Observaciones = reader.IsDBNull("observaciones") ? null : reader.GetString("observaciones")
                };
            }
            return null;
        }

        public async Task<string> ObtenerEstadoPago(string preferenciaId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_ESTADO_PAGO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_preferencia_id", preferenciaId);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<(bool Exito, string Mensaje)> ProcesarPagoCertificado(int solicitudId, string preferenciaId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new MySqlCommand("SP_PROCESAR_PAGO_CERTIFICADO", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
                command.Parameters.AddWithValue("@p_preferencia_id", preferenciaId);
                var result = await command.ExecuteNonQueryAsync();
                return (result > 0, "Pago procesado correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al procesar el pago: {ex.Message}");
            }
        }

        public async Task<Certificado> ObtenerCertificado(int solicitudId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Certificado
                {
                    id = reader.GetInt32("id"),
                    codigo_verificacion = reader.GetString("codigo_verificacion"),
                    fecha_emision = reader.GetDateTime("fecha_emision"),
                    archivo_pdf = reader.GetString("archivo_pdf"),
                    estado = reader.GetString("estado"),
                    solicitud = new SolicitudCertificado
                    {
                        id = reader.GetInt32("solicitud_id"),
                        usuario_rut = reader.GetInt32("usuario_rut"),
                        tipo_certificado_id = reader.GetInt32("tipo_certificado_id"),
                        fecha_solicitud = reader.GetDateTime("fecha_solicitud"),
                        estado = reader.GetString("solicitud_estado"),
                        motivo = reader.GetString("motivo"),
                        documentos_adjuntos = reader.GetString("documentos_adjuntos")
                    }
                };
            }
            return null;
        }

        public async Task<(bool Exito, string Mensaje)> GenerarPDFCertificado(int solicitudId)
        {
            try
            {
                var certificado = await ObtenerCertificado(solicitudId);
                if (certificado == null)
                {
                    return (false, "No se encontró el certificado");
                }

                string rutaArchivo = Path.Combine("Certificados", $"certificado_{certificado.id}.pdf");
                Directory.CreateDirectory("Certificados");

                using var writer = new PdfWriter(rutaArchivo);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                document.Add(new Paragraph($"Certificado #{certificado.id}"));
                document.Add(new Paragraph($"Código de Verificación: {certificado.codigo_verificacion}"));
                document.Add(new Paragraph($"Fecha de Emisión: {certificado.fecha_emision:dd/MM/yyyy}"));
                document.Add(new Paragraph($"Estado: {certificado.estado}"));

                document.Close();

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new MySqlCommand("SP_ACTUALIZAR_RUTA_CERTIFICADO", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_certificado_id", certificado.id);
                command.Parameters.AddWithValue("@p_ruta_archivo", rutaArchivo);
                await command.ExecuteNonQueryAsync();

                return (true, "Certificado generado correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al generar el certificado: {ex.Message}");
            }
        }

        public async Task<List<Certificado>> ObtenerHistorialCertificados(int usuarioRut)
        {
            var certificados = new List<Certificado>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_OBTENER_HISTORIAL_CERTIFICADOS", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_usuario_rut", usuarioRut);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                certificados.Add(new Certificado
                {
                    id = reader.GetInt32("id"),
                    codigo_verificacion = reader.GetString("codigo_verificacion"),
                    fecha_emision = reader.GetDateTime("fecha_emision"),
                    archivo_pdf = reader.GetString("archivo_pdf"),
                    estado = reader.GetString("estado"),
                    solicitud = new SolicitudCertificado
                    {
                        id = reader.GetInt32("solicitud_id"),
                        usuario_rut = reader.GetInt32("usuario_rut"),
                        tipo_certificado_id = reader.GetInt32("tipo_certificado_id"),
                        fecha_solicitud = reader.GetDateTime("fecha_solicitud"),
                        estado = reader.GetString("solicitud_estado"),
                        motivo = reader.GetString("motivo"),
                        documentos_adjuntos = reader.GetString("documentos_adjuntos")
                    }
                });
            }
            return certificados;
        }

        public async Task<(bool Exito, string Mensaje)> VerificarCertificado(string codigoVerificacion)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_VERIFICAR_CERTIFICADO", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_codigo_verificacion", codigoVerificacion);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (true, "Certificado válido");
            }
            return (false, "Certificado no encontrado o inválido");
        }

        public async Task GuardarTokenPago(int solicitudId, string token)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("SP_GUARDAR_TOKEN_PAGO", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
                command.Parameters.AddWithValue("@p_token", token);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Alias para compatibilidad con el controlador
        public async Task<SolicitudCertificadoDTO> ObtenerSolicitud(int solicitudId)
        {
            return await ObtenerDetalleSolicitud(solicitudId);
        }

        // Confirmar pago usando el SP_CONFIRMAR_PAGOS
        public async Task<bool> ConfirmarPago(string token)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SP_CONFIRMAR_PAGOS", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_token_webpay", token);
            command.Parameters.AddWithValue("@p_estado_pago", "aprobado"); // Puedes parametrizar si lo necesitas
            command.Parameters.AddWithValue("@p_respuesta_webpay", ""); // Puedes guardar la respuesta real si la tienes
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
    }
} 