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
                    solicitud_id = reader.GetInt32("solicitud_id"),
                    codigo_verificacion = reader.GetString("codigo_verificacion"),
                    fecha_emision = reader.GetDateTime("fecha_emision"),
                    fecha_vencimiento = reader.GetDateTime("fecha_vencimiento"),
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
            string rutaArchivo = string.Empty;
            try
            {
                var certificado = await ObtenerCertificado(solicitudId);
                if (certificado == null)
                {
                    return (false, "No se encontró el certificado");
                }

                // Asegurarnos de que el directorio existe
                string directorioCertificados = Path.Combine(Directory.GetCurrentDirectory(), "Certificados");
                if (!Directory.Exists(directorioCertificados))
                {
                    Directory.CreateDirectory(directorioCertificados);
                }

                rutaArchivo = Path.Combine(directorioCertificados, $"certificado_{certificado.id}.pdf");

                // Crear el documento PDF con configuración básica
                using var writer = new PdfWriter(new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write));
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                // Configuración básica del documento
                document.SetMargins(50, 50, 50, 50);

                // Título
                var titulo = new Paragraph("CERTIFICADO DE RESIDENCIA")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetBold();
                document.Add(titulo);

                // Espacio
                document.Add(new Paragraph("\n"));

                // Información del certificado
                var infoCertificado = new Paragraph()
                    .Add(new Text($"Código de Verificación: {certificado.codigo_verificacion}\n"))
                    .Add(new Text($"Fecha de Emisión: {certificado.fecha_emision:dd/MM/yyyy}\n"))
                    .Add(new Text($"Estado: {certificado.estado}\n"));
                document.Add(infoCertificado);

                // Información del solicitante
                if (certificado.solicitud != null)
                {
                    document.Add(new Paragraph("\n"));
                    var infoSolicitante = new Paragraph()
                        .Add(new Text($"Solicitante: {certificado.solicitud.usuario_rut}\n"))
                        .Add(new Text($"Motivo: {certificado.solicitud.motivo}\n"));
                    document.Add(infoSolicitante);
                }

                // Pie de página
                document.Add(new Paragraph("\n\n"));
                var piePagina = new Paragraph("Este certificado es válido por 3 meses desde la fecha de emisión.")
                    .SetFontSize(10)
                    .SetItalic()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(piePagina);

                // Cerrar el documento
                document.Close();

                // Actualizar la ruta del archivo en la base de datos
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
                // Limpiar el archivo si existe
                if (!string.IsNullOrEmpty(rutaArchivo) && File.Exists(rutaArchivo))
                {
                    try
                    {
                        File.Delete(rutaArchivo);
                    }
                    catch { }
                }

                // Registrar el error detallado
                Console.WriteLine($"[ERROR] Detalles del error al generar PDF: {ex}");
                
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
        public async Task<bool> ConfirmarPago(string token, string estadoPago)
        {
            try
            {
                Console.WriteLine($"[LOG] Iniciando proceso de confirmación de pago para token: {token}");
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Primero obtenemos el solicitud_id y pago_id
                using var commandGetInfo = new MySqlCommand(
                    @"SELECT p.referencia_id, p.id as pago_id 
                    FROM pagos p 
                    WHERE p.token_webpay = @token 
                    LIMIT 1", 
                    connection);
                commandGetInfo.Parameters.AddWithValue("@token", token);
                
                using var reader = await commandGetInfo.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    Console.WriteLine($"[ERROR] No se encontró información para el token: {token}");
                    return false;
                }

                var solicitudId = reader.GetInt32("referencia_id");
                var pagoId = reader.GetInt32("pago_id");
                await reader.CloseAsync();
                Console.WriteLine($"[LOG] Información encontrada - Solicitud ID: {solicitudId}, Pago ID: {pagoId}");

                // Actualizamos el estado del pago
                using var commandUpdatePago = new MySqlCommand(
                    "UPDATE pagos SET estado = @estado WHERE id = @pago_id",
                    connection);
                commandUpdatePago.Parameters.AddWithValue("@estado", estadoPago);
                commandUpdatePago.Parameters.AddWithValue("@pago_id", pagoId);
                var updatePagoResult = await commandUpdatePago.ExecuteNonQueryAsync();
                Console.WriteLine($"[LOG] Actualización de estado de pago. Filas afectadas: {updatePagoResult}");

                // Actualizamos el estado de la transacción WebPay
                using var commandUpdateTransaccion = new MySqlCommand(
                    "UPDATE transacciones_webpay SET estado = @estado WHERE pago_id = @pago_id",
                    connection);
                commandUpdateTransaccion.Parameters.AddWithValue("@estado", estadoPago);
                commandUpdateTransaccion.Parameters.AddWithValue("@pago_id", pagoId);
                var updateTransaccionResult = await commandUpdateTransaccion.ExecuteNonQueryAsync();
                Console.WriteLine($"[LOG] Actualización de estado de transacción WebPay. Filas afectadas: {updateTransaccionResult}");

                if (updatePagoResult > 0 && updateTransaccionResult > 0)
                {
                    Console.WriteLine("[LOG] Estados actualizados correctamente, procediendo con la aprobación del certificado");
                    
                    // Si el pago se confirmó exitosamente, aprobamos el certificado automáticamente
                    var aprobacionExitosa = await AprobarCertificadoAutomatico(solicitudId);
                    if (!aprobacionExitosa)
                    {
                        Console.WriteLine($"[ERROR] Error al aprobar certificado automáticamente para solicitud {solicitudId}");
                        return false;
                    }

                    Console.WriteLine("[LOG] Certificado aprobado, procediendo con la generación del PDF");
                    
                    // Generamos el PDF del certificado
                    var (exitoGeneracion, mensajeGeneracion) = await GenerarPDFCertificado(solicitudId);
                    if (!exitoGeneracion)
                    {
                        Console.WriteLine($"[ERROR] Error al generar PDF del certificado: {mensajeGeneracion}");
                        return false;
                    }

                    Console.WriteLine("[LOG] Proceso de confirmación de pago completado exitosamente");
                    return true;
                }

                Console.WriteLine("[ERROR] No se pudieron actualizar los estados del pago");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en ConfirmarPago: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool Exito, string Mensaje)> RegistrarPagoCertificado(int usuarioRut, int solicitudId, decimal monto, string metodoPago, string tokenWebpay, string urlPago)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new MySqlCommand("SP_REGISTRAR_PAGO_CERTIFICADO", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_usuario_rut", usuarioRut);
                command.Parameters.AddWithValue("@p_solicitud_certificado_id", solicitudId);
                command.Parameters.AddWithValue("@p_monto", monto);
                command.Parameters.AddWithValue("@p_metodo_pago", metodoPago);
                command.Parameters.AddWithValue("@p_token_webpay", tokenWebpay);
                command.Parameters.AddWithValue("@p_url_pago", urlPago);
                await command.ExecuteNonQueryAsync();
                return (true, "Pago registrado correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar el pago: {ex.Message}");
            }
        }

        public async Task<SolicitudCertificadoDTO?> ObtenerSolicitudPorTokenWebpay(string tokenWebpay)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SELECT referencia_id FROM pagos WHERE token_webpay = @token LIMIT 1", connection);
            command.Parameters.AddWithValue("@token", tokenWebpay);
            var result = await command.ExecuteScalarAsync();
            if (result != null && int.TryParse(result.ToString(), out int solicitudId))
            {
                return await ObtenerDetalleSolicitud(solicitudId);
            }
            return null;
        }

        // Genera el certificado automáticamente al pagar, sin intervención de la directiva
        public async Task<bool> AprobarCertificadoAutomatico(int solicitudId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Primero obtenemos un RUT válido de la directiva
                using var commandGetDirectiva = new MySqlCommand(
                    @"SELECT rut FROM usuarios 
                    WHERE tipo_usuario = 'Directiva' 
                    AND estado = 1 
                    LIMIT 1", 
                    connection);
                
                var directivaRut = await commandGetDirectiva.ExecuteScalarAsync();
                if (directivaRut == null)
                {
                    Console.WriteLine("[ERROR] No se encontró ningún miembro de la directiva activo");
                    return false;
                }

                Console.WriteLine($"[LOG] RUT de directiva encontrado: {directivaRut}");

                // Ahora aprobamos el certificado con el RUT de la directiva
                using var command = new MySqlCommand("SP_APROBAR_CERTIFICADO", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_solicitud_id", solicitudId);
                command.Parameters.AddWithValue("@p_directiva_rut", directivaRut);
                command.Parameters.AddWithValue("@p_observaciones", "Aprobación automática por pago");

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var mensaje = reader["mensaje"]?.ToString() ?? string.Empty;
                    var codigoVerificacion = reader["codigo_verificacion"]?.ToString() ?? string.Empty;
                    Console.WriteLine($"[LOG] Certificado aprobado. Mensaje: {mensaje}, Código: {codigoVerificacion}");
                    return true;
                }
                
                Console.WriteLine("[LOG] No se pudo aprobar el certificado");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en AprobarCertificadoAutomatico: {ex.Message}");
                return false;
            }
        }
    }
} 