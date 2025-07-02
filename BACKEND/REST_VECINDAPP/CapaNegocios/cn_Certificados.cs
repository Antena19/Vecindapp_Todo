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
using iText.Kernel.Colors;

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
            
            // Usar consulta SQL directa en lugar del stored procedure
            var query = @"
                INSERT INTO solicitudes_certificado 
                (usuario_rut, tipo_certificado_id, motivo, documentos_adjuntos, precio, observaciones)
                VALUES (@usuario_rut, @tipo_certificado_id, @motivo, @documentos_adjuntos, @precio, @observaciones);
                SELECT LAST_INSERT_ID();";
                
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@usuario_rut", usuarioId);
            command.Parameters.AddWithValue("@tipo_certificado_id", solicitud.TipoCertificadoId);
            command.Parameters.AddWithValue("@motivo", solicitud.Motivo ?? string.Empty);
            command.Parameters.AddWithValue("@documentos_adjuntos", solicitud.DocumentosAdjuntos ?? string.Empty);
            command.Parameters.AddWithValue("@precio", solicitud.Precio);
            command.Parameters.AddWithValue("@observaciones", solicitud.Observaciones ?? string.Empty);

            var solicitudId = Convert.ToInt32(await command.ExecuteScalarAsync());
            return solicitudId;
        }

        public async Task<bool> AprobarCertificado(int solicitudId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Usar consulta SQL directa en lugar del stored procedure
            var query = @"
                UPDATE solicitudes_certificado 
                SET estado = 'aprobado', 
                    fecha_aprobacion = NOW() 
                WHERE id = @solicitud_id";
                
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@solicitud_id", solicitudId);
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> RechazarCertificado(int solicitudId, string motivoRechazo)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Usar consulta SQL directa en lugar del stored procedure
            var query = @"
                UPDATE solicitudes_certificado 
                SET estado = 'rechazado', 
                    observaciones = @motivo_rechazo 
                WHERE id = @solicitud_id";
                
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@solicitud_id", solicitudId);
            command.Parameters.AddWithValue("@motivo_rechazo", motivoRechazo);
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
            
            // Consulta SQL modificada para incluir información del certificado
            var query = @"
                SELECT 
                    sc.id,
                    sc.usuario_rut,
                    sc.tipo_certificado_id,
                    sc.fecha_solicitud,
                    sc.estado,
                    sc.motivo,
                    sc.documentos_adjuntos,
                    sc.fecha_aprobacion,
                    sc.directiva_rut,
                    sc.precio,
                    sc.observaciones,
                    tc.nombre AS tipo_certificado_nombre,
                    c.codigo_verificacion,
                    c.fecha_emision,
                    c.solicitud_id AS certificado_id
                FROM solicitudes_certificado sc
                LEFT JOIN tipos_certificado tc ON sc.tipo_certificado_id = tc.id
                LEFT JOIN certificados c ON sc.id = c.solicitud_id
                WHERE sc.usuario_rut = @usuario_rut
                ORDER BY sc.fecha_solicitud DESC";
                
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@usuario_rut", usuarioRut);
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
                    Observaciones = reader.IsDBNull("observaciones") ? null : reader.GetString("observaciones"),
                    CodigoVerificacion = reader.IsDBNull("codigo_verificacion") ? null : reader.GetString("codigo_verificacion"),
                    FechaEmision = reader.IsDBNull("fecha_emision") ? null : reader.GetDateTime("fecha_emision"),
                    CertificadoId = reader.IsDBNull("certificado_id") ? null : reader.GetInt32("certificado_id")
                });
            }
            return solicitudes;
        }

        public async Task<List<SolicitudCertificadoDTO>> ObtenerSolicitudesPendientes()
        {
            var solicitudes = new List<SolicitudCertificadoDTO>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Usar consulta SQL directa en lugar del stored procedure
            var query = @"
                SELECT 
                    sc.id,
                    sc.usuario_rut,
                    sc.tipo_certificado_id,
                    sc.fecha_solicitud,
                    sc.estado,
                    sc.motivo,
                    sc.documentos_adjuntos,
                    sc.fecha_aprobacion,
                    sc.directiva_rut,
                    sc.precio,
                    sc.observaciones,
                    tc.nombre AS tipo_certificado_nombre
                FROM solicitudes_certificado sc
                LEFT JOIN tipos_certificado tc ON sc.tipo_certificado_id = tc.id
                WHERE sc.estado = 'pendiente'
                ORDER BY sc.fecha_solicitud DESC";
                
            using var command = new MySqlCommand(query, connection);
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
            
            // Usar consulta SQL directa en lugar del stored procedure
            var query = @"
                SELECT 
                    sc.id,
                    sc.usuario_rut,
                    sc.tipo_certificado_id,
                    sc.fecha_solicitud,
                    sc.estado,
                    sc.motivo,
                    sc.documentos_adjuntos,
                    sc.fecha_aprobacion,
                    sc.directiva_rut,
                    sc.precio,
                    sc.observaciones,
                    tc.nombre AS tipo_certificado_nombre
                FROM solicitudes_certificado sc
                LEFT JOIN tipos_certificado tc ON sc.tipo_certificado_id = tc.id
                WHERE sc.id = @solicitud_id";
                
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@solicitud_id", solicitudId);
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
            
            // Usar consulta SQL directa en lugar del stored procedure
            var query = @"
                SELECT estado 
                FROM pagos 
                WHERE token_webpay = @preferencia_id";
                
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@preferencia_id", preferenciaId);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<(bool Exito, string Mensaje)> ProcesarPagoCertificado(int solicitudId, string preferenciaId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Usar consulta SQL directa en lugar del stored procedure
                var query = @"
                    UPDATE solicitudes_certificado 
                    SET estado = 'aprobado', 
                        fecha_aprobacion = NOW() 
                    WHERE id = @solicitud_id";
                    
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@solicitud_id", solicitudId);
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
            
            // Usar consulta SQL directa y aliasar solicitud_id como id
            var query = @"
                SELECT 
                    c.solicitud_id AS id,
                    c.solicitud_id,
                    c.codigo_verificacion,
                    c.fecha_emision,
                    c.fecha_vencimiento,
                    c.archivo_pdf,
                    c.estado,
                    sc.usuario_rut,
                    sc.tipo_certificado_id,
                    sc.fecha_solicitud,
                    sc.estado AS solicitud_estado,
                    sc.motivo,
                    sc.documentos_adjuntos
                FROM certificados c
                INNER JOIN solicitudes_certificado sc ON c.solicitud_id = sc.id
                WHERE c.solicitud_id = @solicitud_id";
                
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@solicitud_id", solicitudId);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Certificado
                {
                    id = reader.GetInt32("id"),
                    solicitud_id = reader.GetInt32("solicitud_id"),
                    codigo_verificacion = reader.IsDBNull("codigo_verificacion") ? null : reader.GetString("codigo_verificacion"),
                    fecha_emision = reader.GetDateTime("fecha_emision"),
                    fecha_vencimiento = reader.GetDateTime("fecha_vencimiento"),
                    archivo_pdf = reader.IsDBNull("archivo_pdf") ? null : reader.GetString("archivo_pdf"),
                    estado = reader.GetString("estado"),
                    solicitud = new SolicitudCertificado
                    {
                        id = reader.GetInt32("solicitud_id"),
                        usuario_rut = reader.GetInt32("usuario_rut"),
                        tipo_certificado_id = reader.GetInt32("tipo_certificado_id"),
                        fecha_solicitud = reader.GetDateTime("fecha_solicitud"),
                        estado = reader.GetString("solicitud_estado"),
                        motivo = reader.IsDBNull("motivo") ? null : reader.GetString("motivo"),
                        documentos_adjuntos = reader.IsDBNull("documentos_adjuntos") ? null : reader.GetString("documentos_adjuntos")
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
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // 1. Obtener toda la información necesaria con un solo query
                var query = @"
                    SELECT 
                        s.id AS solicitud_id,
                        s.motivo,
                        c.codigo_verificacion,
                        c.fecha_emision,
                        u.nombre,
                        u.apellido_paterno,
                        u.apellido_materno,
                        u.rut,
                        u.direccion
                    FROM solicitudes_certificado s
                    JOIN certificados c ON s.id = c.solicitud_id
                    JOIN usuarios u ON s.usuario_rut = u.rut
                    WHERE s.id = @solicitud_id";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@solicitud_id", solicitudId);

                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return (false, "No se encontró la información del certificado y usuario.");
                }

                var data = new
                {
                    CodigoVerificacion = reader.IsDBNull("codigo_verificacion") ? "No disponible" : reader.GetString("codigo_verificacion"),
                    FechaEmision = reader.GetDateTime("fecha_emision"),
                    Nombres = reader.GetString("nombre"),
                    Apellidos = $"{reader.GetString("apellido_paterno")} {reader.GetString("apellido_materno")}".Trim(),
                    Rut = reader.GetInt32("rut"),
                    Motivo = reader.IsDBNull("motivo") ? "No especificado" : reader.GetString("motivo"),
                    Direccion = reader.IsDBNull("direccion") ? "" : reader.GetString("direccion")
                };
                await reader.CloseAsync();


                // 2. Definir rutas a las imágenes (ajusta si es necesario)
                string basePath = Directory.GetCurrentDirectory();
                string logoPath = Path.Combine(basePath, "wwwroot", "logo-junta.png");
                string firmaPath = Path.Combine(basePath, "wwwroot", "firma-presidente.jpg");

                if (!File.Exists(logoPath))
                {
                    return (false, $"No se encontró la imagen del logo en la ruta: {logoPath}");
                }
                
                if (!File.Exists(logoPath) || !File.Exists(firmaPath))
                {
                    return (false, "No se encontraron las imágenes del logo o la firma en la ruta esperada.");
                }
                
                string directorioCertificados = Path.Combine(basePath, "Certificados");
                if (!Directory.Exists(directorioCertificados))
                {
                    Directory.CreateDirectory(directorioCertificados);
                }
                rutaArchivo = Path.Combine(directorioCertificados, $"certificado_{solicitudId}.pdf");

                // 3. Crear el PDF con iText
                using var writer = new PdfWriter(new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write));
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);
                document.SetMargins(40, 40, 40, 40);

                // Logo
                var logo = new Image(iText.IO.Image.ImageDataFactory.Create(logoPath))
                    .ScaleToFit(120, 120)
                    .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(logo);

                // Título (mayúsculas, color teal)
                var titulo = new Paragraph("CERTIFICADO DE RESIDENCIA")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(22)
                    .SetBold()
                    .SetFontColor(new DeviceRgb(0, 137, 123))
                    .SetMarginBottom(10);
                document.Add(titulo);

                // Subtítulo con ciudad y fecha
                var fechaFormateada = data.FechaEmision.ToString("d 'de' MMMM yyyy", new System.Globalization.CultureInfo("es-CL"));
                var subtitulo = new Paragraph($"PUERTO MONTT, {fechaFormateada}.")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(13)
                    .SetMarginBottom(18);
                document.Add(subtitulo);

                // Cuerpo principal (formato normal, datos en negrita)
                var cuerpo = new Paragraph()
                    .Add("La Junta de Vecinos Portal de Puerto Montt, RUT: 65.066.453 – 1, Personalidad Juridica N°3849 con foja 3850, constituida el 13 de marzo 2013 en Puerto Montt, perteneciente a la unidad N° 20 con facultad que otorga la Ley N°19.418 en el artículo 43 certifica que:\n\n")
                    .Add("Sr.(a): ").Add(new Text($"{data.Nombres} {data.Apellidos}").SetBold()).Add("\n")
                    .Add("RUT: ").Add(new Text($"{data.Rut}").SetBold()).Add("\n")
                    .Add("Reside en: ").Add(new Text($"{data.Direccion}").SetBold()).Add("\n");
                if (!string.IsNullOrEmpty(data.Motivo))
                {
                    cuerpo.Add("Motivo: ").Add(new Text(data.Motivo).SetBold()).Add("\n\n");
                }
                cuerpo.Add("Se extiende el presente documento para ser presentado en la institución que lo requiera.\n\n")
                    .Add(new Text("Ley 20.718 de 02 de enero 2014").SetBold())
                    .Add(" faculta a las juntas de vecinos a emitir certificados de residencia, siéndole aplicable al requirente que faltare a la verdad cuanto a los datos proporcionados al efecto, las sanciones contempladas en el artículo 212 del código penal.")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.JUSTIFIED)
                    .SetFontSize(12)
                    .SetMarginBottom(40);
                document.Add(cuerpo);

                // Firma del presidente
                var firma = new Image(iText.IO.Image.ImageDataFactory.Create(firmaPath))
                    .ScaleToFit(80, 40)
                    .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER)
                    .SetMarginBottom(5);
                document.Add(firma);

                var nombrePresidente = new Paragraph("Presidente Junta de Vecinos")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(11)
                    .SetFontColor(new DeviceRgb(0, 137, 123))
                    .SetBold();
                document.Add(nombrePresidente);

                // Pie de página con fecha y código de verificación
                var pie = new Paragraph($"Fecha: {data.FechaEmision:dd-MM-yyyy}        Código: {data.CodigoVerificacion}")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFontSize(10)
                    .SetMarginTop(30);
                document.Add(pie);

                document.Close();
                
                return (true, "Certificado generado correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Falló la generación del PDF: {ex.Message}\n{ex.StackTrace}");
                return (false, $"Error al generar el certificado: {ex.Message}");
            }
        }

        public async Task<List<Certificado>> ObtenerHistorialCertificados(int usuarioRut)
        {
            var certificados = new List<Certificado>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new MySqlCommand(
                @"SELECT 
                    c.solicitud_id,
                    c.codigo_verificacion,
                    c.fecha_emision,
                    c.archivo_pdf,
                    c.estado,
                    sc.usuario_rut,
                    sc.tipo_certificado_id,
                    sc.fecha_solicitud,
                    sc.estado as solicitud_estado,
                    sc.motivo,
                    sc.documentos_adjuntos
                FROM certificados c
                INNER JOIN solicitudes_certificado sc ON c.solicitud_id = sc.id
                WHERE sc.usuario_rut = @usuario_rut
                ORDER BY c.fecha_emision DESC", 
                connection);
            command.Parameters.AddWithValue("@usuario_rut", usuarioRut);
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                certificados.Add(new Certificado
                {
                    id = reader.GetInt32("solicitud_id"),
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
            
            // Usar consulta SQL directa en lugar del stored procedure
            var query = @"
                SELECT COUNT(*) as total
                FROM certificados 
                WHERE codigo_verificacion = @codigo_verificacion 
                AND estado = 'vigente'";
                
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@codigo_verificacion", codigoVerificacion);
            var result = await command.ExecuteScalarAsync();
            
            if (result != null && Convert.ToInt32(result) > 0)
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
                
                // Usar consulta SQL directa en lugar del stored procedure
                var query = @"
                    UPDATE solicitudes_certificado 
                    SET token_webpay = @token 
                    WHERE id = @solicitud_id";
                    
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@solicitud_id", solicitudId);
                command.Parameters.AddWithValue("@token", token);
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

                // Actualizamos el pago_id en la solicitud de certificado
                using var commandUpdateSolicitud = new MySqlCommand(
                    "UPDATE solicitudes_certificado SET pago_id = @pago_id WHERE id = @solicitud_id",
                    connection);
                commandUpdateSolicitud.Parameters.AddWithValue("@pago_id", pagoId);
                commandUpdateSolicitud.Parameters.AddWithValue("@solicitud_id", solicitudId);
                var updateSolicitudResult = await commandUpdateSolicitud.ExecuteNonQueryAsync();
                Console.WriteLine($"[LOG] Actualización de pago_id en solicitud. Filas afectadas: {updateSolicitudResult}");

                {
                    Console.WriteLine("[LOG] Estados actualizados (o ya estaban actualizados), procediendo con la aprobación del certificado");
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
                        // No retornamos false aquí porque el certificado ya se aprobó
                    }

                    Console.WriteLine("[LOG] Proceso de confirmación de pago completado exitosamente");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en ConfirmarPago: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool Exito, string Mensaje)> RegistrarPagoCertificadoDirecto(int usuarioRut, int solicitudId, decimal monto, string metodoPago, string tokenWebpay, string urlPago)
        {
            try
            {
                Console.WriteLine($"[LOG] Registrando pago directo - Usuario: {usuarioRut}, Solicitud: {solicitudId}, Monto: {monto}");
                
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Registrar pago directamente
                using var commandPago = new MySqlCommand(
                    @"INSERT INTO pagos 
                    (usuario_rut, tipo, referencia_id, monto, metodo_pago, estado, token_webpay, url_pago_webpay)
                    VALUES (@usuario_rut, 'certificado', @referencia_id, @monto, @metodo_pago, 'procesando', @token_webpay, @url_pago)",
                    connection);
                
                commandPago.Parameters.AddWithValue("@usuario_rut", usuarioRut);
                commandPago.Parameters.AddWithValue("@referencia_id", solicitudId);
                commandPago.Parameters.AddWithValue("@monto", monto);
                commandPago.Parameters.AddWithValue("@metodo_pago", metodoPago);
                commandPago.Parameters.AddWithValue("@token_webpay", tokenWebpay);
                commandPago.Parameters.AddWithValue("@url_pago", urlPago);
                
                await commandPago.ExecuteNonQueryAsync();
                var pagoId = await GetLastInsertId(connection);
                
                // Registrar transacción WebPay
                using var commandTransaccion = new MySqlCommand(
                    @"INSERT INTO transacciones_webpay 
                    (pago_id, token_webpay, monto, estado, tipo_transaccion, usuario_rut)
                    VALUES (@pago_id, @token_webpay, @monto, 'iniciada', 'certificado', @usuario_rut)",
                    connection);
                
                commandTransaccion.Parameters.AddWithValue("@pago_id", pagoId);
                commandTransaccion.Parameters.AddWithValue("@token_webpay", tokenWebpay);
                commandTransaccion.Parameters.AddWithValue("@monto", monto);
                commandTransaccion.Parameters.AddWithValue("@usuario_rut", usuarioRut);
                
                await commandTransaccion.ExecuteNonQueryAsync();
                
                Console.WriteLine($"[LOG] Pago registrado exitosamente - ID: {pagoId}, Token: {tokenWebpay}");
                return (true, "Pago registrado correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error al registrar el pago directo: {ex.Message}");
                return (false, $"Error al registrar el pago: {ex.Message}");
            }
        }

        private async Task<int> GetLastInsertId(MySqlConnection connection)
        {
            using var command = new MySqlCommand("SELECT LAST_INSERT_ID()", connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
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

                // Iniciar transacción
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // 1. Actualizar solicitud de certificado
                    using var commandUpdateSolicitud = new MySqlCommand(
                        @"UPDATE solicitudes_certificado 
                        SET estado = 'aprobado', 
                            fecha_aprobacion = NOW(),
                            directiva_rut = @directiva_rut,
                            observaciones = @observaciones
                        WHERE id = @solicitud_id",
                        connection, transaction);
                    
                    commandUpdateSolicitud.Parameters.AddWithValue("@solicitud_id", solicitudId);
                    commandUpdateSolicitud.Parameters.AddWithValue("@directiva_rut", directivaRut);
                    commandUpdateSolicitud.Parameters.AddWithValue("@observaciones", "Aprobación automática por pago");
                    
                    var filasActualizadas = await commandUpdateSolicitud.ExecuteNonQueryAsync();
                    Console.WriteLine($"[LOG] Filas actualizadas en solicitud: {filasActualizadas}");

                    // 2. Generar código de verificación único
                    var codigoVerificacion = $"CERT-{DateTime.Now:yyyyMMdd}-{solicitudId}-{new Random().Next(1000, 9999)}";
                    var archivoPdf = $"/certificados/{codigoVerificacion}.pdf";
                    
                    Console.WriteLine($"[LOG] Código de verificación generado: {codigoVerificacion}");

                    // 3. Insertar certificado directamente
                    using var commandInsertCertificado = new MySqlCommand(
                        @"INSERT INTO certificados 
                        (solicitud_id, codigo_verificacion, fecha_emision, fecha_vencimiento, archivo_pdf, estado)
                        VALUES 
                        (@solicitud_id, @codigo_verificacion, NOW(), DATE_ADD(NOW(), INTERVAL 3 MONTH), @archivo_pdf, 'vigente')",
                        connection, transaction);
                    
                    commandInsertCertificado.Parameters.AddWithValue("@solicitud_id", solicitudId);
                    commandInsertCertificado.Parameters.AddWithValue("@codigo_verificacion", codigoVerificacion);
                    commandInsertCertificado.Parameters.AddWithValue("@archivo_pdf", archivoPdf);
                    
                    var filasInsertadas = await commandInsertCertificado.ExecuteNonQueryAsync();
                    Console.WriteLine($"[LOG] Filas insertadas en certificados: {filasInsertadas}");

                    // Confirmar transacción
                    await transaction.CommitAsync();
                    Console.WriteLine($"[LOG] Transacción confirmada exitosamente");

                    Console.WriteLine($"[LOG] Certificado aprobado automáticamente. Código: {codigoVerificacion}");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error durante la transacción: {ex.Message}");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en AprobarCertificadoAutomatico: {ex.Message}");
                return false;
            }
        }

        // Aprobar certificado sin pago cuando no hay conectividad con Transbank
        public async Task<bool> AprobarCertificadoSinPago(int solicitudId, string motivo, string observaciones)
        {
            try
            {
                Console.WriteLine($"[LOG] ===== INICIANDO APROBACIÓN DE CERTIFICADO SIN PAGO =====");
                Console.WriteLine($"[LOG] Solicitud ID: {solicitudId}");
                Console.WriteLine($"[LOG] Motivo: {motivo}");
                Console.WriteLine($"[LOG] Observaciones: {observaciones}");

                using var connection = new MySqlConnection(_connectionString);
                Console.WriteLine($"[LOG] Abriendo conexión a la base de datos...");
                await connection.OpenAsync();
                Console.WriteLine($"[LOG] Conexión abierta exitosamente");

                // Verificar que la solicitud existe y está pendiente
                Console.WriteLine($"[LOG] Verificando estado de la solicitud...");
                using var commandVerificar = new MySqlCommand(
                    @"SELECT estado FROM solicitudes_certificado WHERE id = @solicitud_id",
                    connection);
                commandVerificar.Parameters.AddWithValue("@solicitud_id", solicitudId);
                
                var estadoActual = await commandVerificar.ExecuteScalarAsync();
                Console.WriteLine($"[LOG] Estado actual de la solicitud: {estadoActual}");
                
                if (estadoActual == null)
                {
                    Console.WriteLine($"[ERROR] No se encontró la solicitud {solicitudId}");
                    return false;
                }

                // Verificar que la solicitud esté pendiente
                if (estadoActual.ToString() != "pendiente")
                {
                    Console.WriteLine($"[ERROR] La solicitud {solicitudId} no está pendiente. Estado actual: {estadoActual}");
                    
                    // Si ya está aprobada, considerarlo como éxito
                    if (estadoActual.ToString() == "aprobado")
                    {
                        Console.WriteLine($"[LOG] La solicitud {solicitudId} ya está aprobada. Retornando éxito.");
                        return true;
                    }
                    
                    return false;
                }

                // Obtener un RUT válido de la directiva
                Console.WriteLine($"[LOG] Buscando RUT de directiva activo...");
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
                Console.WriteLine($"[LOG] Aprobando certificado sin pago - Solicitud: {solicitudId}, Directiva: {directivaRut}");

                // Iniciar transacción
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // 1. Actualizar solicitud de certificado
                    Console.WriteLine($"[LOG] Actualizando solicitud de certificado...");
                    using var commandUpdateSolicitud = new MySqlCommand(
                        @"UPDATE solicitudes_certificado 
                        SET estado = 'aprobado', 
                            fecha_aprobacion = NOW(),
                            directiva_rut = @directiva_rut,
                            observaciones = @observaciones
                        WHERE id = @solicitud_id",
                        connection, transaction);
                    
                    commandUpdateSolicitud.Parameters.AddWithValue("@solicitud_id", solicitudId);
                    commandUpdateSolicitud.Parameters.AddWithValue("@directiva_rut", directivaRut);
                    commandUpdateSolicitud.Parameters.AddWithValue("@observaciones", $"Aprobación sin pago - Motivo: {motivo}. {observaciones}");
                    
                    var filasActualizadas = await commandUpdateSolicitud.ExecuteNonQueryAsync();
                    Console.WriteLine($"[LOG] Filas actualizadas en solicitud: {filasActualizadas}");

                    // 2. Generar código de verificación único
                    var codigoVerificacion = $"CERT-{DateTime.Now:yyyyMMdd}-{solicitudId}-{new Random().Next(1000, 9999)}";
                    var archivoPdf = $"/certificados/{codigoVerificacion}.pdf";
                    
                    Console.WriteLine($"[LOG] Código de verificación generado: {codigoVerificacion}");

                    // 3. Verificar si ya existe un certificado para esta solicitud
                    Console.WriteLine($"[LOG] Verificando si ya existe un certificado para la solicitud...");
                    using var commandVerificarCertificado = new MySqlCommand(
                        @"SELECT COUNT(*) FROM certificados WHERE solicitud_id = @solicitud_id",
                        connection, transaction);
                    commandVerificarCertificado.Parameters.AddWithValue("@solicitud_id", solicitudId);
                    
                    var certificadoExistente = await commandVerificarCertificado.ExecuteScalarAsync();
                    if (Convert.ToInt32(certificadoExistente) > 0)
                    {
                        Console.WriteLine($"[LOG] Ya existe un certificado para la solicitud {solicitudId}. Actualizando...");
                        
                        // Actualizar certificado existente
                        using var commandUpdateCertificado = new MySqlCommand(
                            @"UPDATE certificados 
                            SET codigo_verificacion = @codigo_verificacion,
                                fecha_emision = NOW(),
                                fecha_vencimiento = DATE_ADD(NOW(), INTERVAL 3 MONTH),
                                archivo_pdf = @archivo_pdf,
                                estado = 'vigente'
                            WHERE solicitud_id = @solicitud_id",
                            connection, transaction);
                        
                        commandUpdateCertificado.Parameters.AddWithValue("@solicitud_id", solicitudId);
                        commandUpdateCertificado.Parameters.AddWithValue("@codigo_verificacion", codigoVerificacion);
                        commandUpdateCertificado.Parameters.AddWithValue("@archivo_pdf", archivoPdf);
                        
                        var filasActualizadasCert = await commandUpdateCertificado.ExecuteNonQueryAsync();
                        Console.WriteLine($"[LOG] Certificado actualizado. Filas afectadas: {filasActualizadasCert}");
                    }
                    else
                    {
                        // 4. Insertar certificado nuevo
                        Console.WriteLine($"[LOG] Insertando nuevo certificado en la tabla...");
                        
                        // Intentar con la estructura más simple posible
                        using var commandInsertCertificado = new MySqlCommand(
                            @"INSERT INTO certificados 
                            (solicitud_id, codigo_verificacion, fecha_emision, fecha_vencimiento, archivo_pdf, estado)
                            VALUES 
                            (@solicitud_id, @codigo_verificacion, NOW(), DATE_ADD(NOW(), INTERVAL 3 MONTH), @archivo_pdf, 'vigente')",
                            connection, transaction);
                        
                        commandInsertCertificado.Parameters.AddWithValue("@solicitud_id", solicitudId);
                        commandInsertCertificado.Parameters.AddWithValue("@codigo_verificacion", codigoVerificacion);
                        commandInsertCertificado.Parameters.AddWithValue("@archivo_pdf", archivoPdf);
                        
                        var filasInsertadas = await commandInsertCertificado.ExecuteNonQueryAsync();
                        Console.WriteLine($"[LOG] Filas insertadas en certificados: {filasInsertadas}");
                    }

                    // Confirmar transacción
                    await transaction.CommitAsync();
                    Console.WriteLine($"[LOG] Transacción confirmada exitosamente");

                    // Generar el PDF del certificado
                    Console.WriteLine($"[LOG] Generando PDF del certificado...");
                    var (exitoGeneracion, mensajeGeneracion) = await GenerarPDFCertificado(solicitudId);
                    if (!exitoGeneracion)
                    {
                        Console.WriteLine($"[WARNING] Error al generar PDF del certificado: {mensajeGeneracion}");
                        // No retornamos false porque el certificado ya se aprobó
                    }
                    else
                    {
                        Console.WriteLine($"[LOG] PDF generado exitosamente: {mensajeGeneracion}");
                    }
                    
                    Console.WriteLine($"[LOG] ===== APROBACIÓN DE CERTIFICADO COMPLETADA =====");
                    Console.WriteLine($"[LOG] Código de verificación: {codigoVerificacion}");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error durante la transacción: {ex.Message}");
                    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ===== EXCEPCIÓN EN APROBAR CERTIFICADO SIN PAGO =====");
                Console.WriteLine($"[ERROR] Mensaje: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"[ERROR] Inner Exception: {ex.InnerException?.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, int>> ObtenerResumenCertificados()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var resumen = new Dictionary<string, int>
            {
                { "solicitados", 0 },
                { "aprobados", 0 },
                { "rechazados", 0 },
                { "emitidos", 0 }
            };

            // Contar por estado en solicitudes_certificado
            var queryEstados = @"SELECT estado, COUNT(*) as cantidad FROM solicitudes_certificado GROUP BY estado";
            using (var command = new MySqlCommand(queryEstados, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var estado = reader.GetString("estado").ToLower();
                    var cantidad = reader.GetInt32("cantidad");
                    if (estado == "solicitado" || estado == "pendiente")
                        resumen["solicitados"] += cantidad;
                    else if (estado == "aprobado")
                        resumen["aprobados"] += cantidad;
                    else if (estado == "rechazado")
                        resumen["rechazados"] += cantidad;
                }
            }

            // Contar emitidos en certificados
            var queryEmitidos = @"SELECT COUNT(*) as emitidos FROM certificados WHERE estado = 'vigente'";
            using (var command = new MySqlCommand(queryEmitidos, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    resumen["emitidos"] = reader.GetInt32("emitidos");
                }
            }

            return resumen;
        }
    }
} 