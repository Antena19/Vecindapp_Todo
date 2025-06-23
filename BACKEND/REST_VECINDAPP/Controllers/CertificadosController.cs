using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos.DTOs;
using REST_VECINDAPP.Seguridad;
using REST_VECINDAPP.Servicios;
using System.Security.Claims;
using Transbank.Webpay.WebpayPlus;
using Transbank.Webpay.Common;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System;

namespace REST_VECINDAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CertificadosController : ControllerBase
    {
        private readonly cn_Certificados _certificadosService;
        private readonly VerificadorRoles _verificadorRoles;
        private readonly TransbankServiceV2 _transbankService;
        private readonly IConfiguration _configuration;

        public CertificadosController(
            cn_Certificados certificadosService,
            VerificadorRoles verificadorRoles,
            TransbankServiceV2 transbankService,
            IConfiguration configuration)
        {
            _certificadosService = certificadosService;
            _verificadorRoles = verificadorRoles;
            _transbankService = transbankService;
            _configuration = configuration;
        }

        // ===== ENDPOINTS PRINCIPALES =====

        [HttpPost("solicitar")]
        public async Task<IActionResult> SolicitarCertificado([FromBody] SolicitudCertificadoDTO solicitud)
        {
            try
            {
                var solicitudId = await _certificadosService.SolicitarCertificado(solicitud.UsuarioRut, solicitud);
                return Ok(new { id = solicitudId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("mis-solicitudes")]
        public async Task<IActionResult> ObtenerMisSolicitudes()
        {
            try
            {
                var userId = ObtenerRutUsuarioAutenticado();
                var solicitudes = await _certificadosService.ObtenerSolicitudesUsuario(userId);
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("pendientes")]
        [Authorize(Roles = "Directiva")]
        public async Task<IActionResult> ObtenerSolicitudesPendientes()
        {
            try
            {
                var solicitudes = await _certificadosService.ObtenerSolicitudesPendientes();
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // ===== FLUJO DE PAGO TRANSBANK =====

        [HttpPost("pago/iniciar")]
        public async Task<IActionResult> IniciarPago([FromBody] PagoTransbankRequest request)
        {
            try
            {
                Console.WriteLine($"[LOG] Iniciando proceso de pago - Solicitud: {request.SolicitudId}");
                
                var solicitud = await _certificadosService.ObtenerSolicitud(request.SolicitudId);
                if (solicitud == null)
                {
                    Console.WriteLine($"[ERROR] Solicitud no encontrada: {request.SolicitudId}");
                    return NotFound(new { mensaje = "Solicitud no encontrada" });
                }

                var buyOrder = $"cert{request.SolicitudId}{DateTimeOffset.Now.ToUnixTimeSeconds()}";
                var sessionId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString();

                var montoPago = solicitud.Precio;
                Console.WriteLine($"[LOG] Iniciando pago - Solicitud: {request.SolicitudId}, Monto: {montoPago}, BuyOrder: {buyOrder}");

                // Verificar que el monto sea válido
                if (montoPago <= 0)
                {
                    Console.WriteLine($"[ERROR] Monto inválido: {montoPago}");
                    return BadRequest(new { mensaje = "El monto del pago debe ser mayor a 0" });
                }

                // Crear transacción con el nuevo servicio simplificado
                var response = await _transbankService.CreateTransaction(
                    montoPago,
                    buyOrder,
                    sessionId
                );

                Console.WriteLine($"[LOG] Transacción creada exitosamente - Token: {response.Token}");

                // Registrar el pago en la base de datos
                int usuarioRut = solicitud.UsuarioRut;
                var (exito, mensaje) = await _certificadosService.RegistrarPagoCertificadoDirecto(
                    usuarioRut,
                    request.SolicitudId,
                    montoPago,
                    "webpay",
                    response.Token,
                    response.Url
                );
                if (!exito)
                {
                    Console.WriteLine($"[ERROR] Error al registrar pago en BD: {mensaje}");
                    return BadRequest(new { mensaje });
                }

                // Guardar el token en la base de datos para su posterior verificación
                await _certificadosService.GuardarTokenPago(request.SolicitudId, response.Token);

                Console.WriteLine($"[LOG] Pago registrado exitosamente - URL: {response.Url}");

                return Ok(new
                {
                    url = response.Url,
                    token = response.Token,
                    monto = montoPago,
                    conectividad = true,
                    simulada = false
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en IniciarPago: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor al procesar el pago" });
            }
        }

        [HttpPost("pago/confirmar")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarPago([FromBody] ConfirmarPagoRequest request)
        {
            try
            {
                Console.WriteLine($"[LOG] ===== INICIANDO CONFIRMACIÓN DE PAGO =====");
                Console.WriteLine($"[LOG] Token recibido: {request.Token}");
                
                // Confirmar la transacción con Transbank
                Console.WriteLine($"[LOG] Llamando a TransbankService.CommitTransaction...");
                var response = await _transbankService.CommitTransaction(request.Token);
                
                Console.WriteLine($"[LOG] Respuesta de Transbank - Status: {response.Status}, Amount: {response.Amount}");
                
                // Buscar la solicitud asociada al token
                Console.WriteLine($"[LOG] Buscando solicitud por token: {request.Token}");
                var solicitud = await _certificadosService.ObtenerSolicitudPorTokenWebpay(request.Token);
                
                if (solicitud == null)
                {
                    Console.WriteLine($"[ERROR] No se encontró solicitud para el token: {request.Token}");
                    return NotFound(new { mensaje = "Solicitud no encontrada" });
                }
                
                Console.WriteLine($"[LOG] Solicitud encontrada - ID: {solicitud.Id}, Usuario: {solicitud.UsuarioRut}, Estado: {solicitud.Estado}");
                
                // Procesar el resultado del pago
                if (response.Status == "AUTHORIZED")
                {
                    Console.WriteLine($"[LOG] Pago autorizado, procediendo a aprobar certificado...");
                    
                    // Pago exitoso
                    var exito = await _certificadosService.AprobarCertificadoSinPago(
                        solicitud.Id,
                        "Pago confirmado exitosamente",
                        $"Pago procesado por Transbank. Monto: {response.Amount}"
                    );
                    
                    Console.WriteLine($"[LOG] Resultado de AprobarCertificadoSinPago: {exito}");
                    
                    if (exito)
                    {
                        Console.WriteLine($"[LOG] Certificado aprobado exitosamente - Solicitud: {solicitud.Id}");
                        return Ok(new { 
                            mensaje = "Pago confirmado y certificado aprobado",
                            estado = "AUTHORIZED",
                            solicitudId = solicitud.Id,
                            redirectUrl = "/certificados/solicitar?descargar=1"
                        });
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] Error al aprobar certificado");
                        return BadRequest(new { mensaje = "Error al aprobar el certificado" });
                    }
                }
                else
                {
                    // Pago fallido
                    Console.WriteLine($"[LOG] Pago fallido - Status: {response.Status}");
                    return BadRequest(new { 
                        mensaje = "El pago no pudo ser procesado",
                        estado = response.Status,
                        solicitudId = solicitud.Id
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ===== EXCEPCIÓN EN CONFIRMAR PAGO =====");
                Console.WriteLine($"[ERROR] Mensaje: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"[ERROR] Inner Exception: {ex.InnerException?.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor al confirmar el pago" });
            }
        }

        [HttpGet("pago/estado/{token}")]
        public async Task<IActionResult> VerificarEstadoPago(string token)
        {
            try
            {
                var response = await _transbankService.GetTransactionStatus(token);
                return Ok(new { 
                    estado = response.Status,
                    monto = response.Amount,
                    fecha = response.TransactionDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // ===== ENDPOINTS DE APROBACIÓN MANUAL =====

        [HttpPost("aprobar-sin-pago")]
        [Authorize(Roles = "Directiva")]
        public async Task<IActionResult> AprobarCertificadoSinPago([FromBody] AprobarCertificadoSinPagoRequest request)
        {
            try
            {
                Console.WriteLine($"[LOG] Aprobando certificado sin pago - Solicitud ID: {request.SolicitudId}, Motivo: {request.Motivo}");
                
                // Aprobar el certificado directamente
                var exito = await _certificadosService.AprobarCertificadoSinPago(
                    request.SolicitudId, 
                    request.Motivo,
                    request.Observaciones
                );
                
                if (exito)
                {
                    return Ok(new { 
                        mensaje = "Certificado aprobado exitosamente sin pago",
                        solicitudId = request.SolicitudId
                    });
                }
                else
                {
                    return BadRequest(new { mensaje = "Error al aprobar el certificado" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en AprobarCertificadoSinPago: {ex.Message}");
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // ===== ENDPOINTS DE GESTIÓN =====

        [HttpGet("tipos")]
        public async Task<IActionResult> ObtenerTiposCertificados()
        {
            try
            {
                var tipos = await _certificadosService.ObtenerTiposCertificado();
                return Ok(tipos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("descargar/{certificadoId}")]
        public async Task<IActionResult> DescargarCertificado(int certificadoId)
        {
            try
            {
                var userId = ObtenerRutUsuarioAutenticado();
                var certificado = await _certificadosService.ObtenerCertificado(certificadoId);

                if (certificado == null)
                    return NotFound(new { mensaje = "Certificado no encontrado" });

                if (certificado.solicitud.usuario_rut != userId && !_verificadorRoles.EsDirectiva())
                    return Forbid();

                var resultado = await _certificadosService.GenerarPDFCertificado(certificadoId);
                if (!resultado.Exito)
                    return BadRequest(new { mensaje = resultado.Mensaje });

                var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "Certificados", $"certificado_{certificadoId}.pdf");

                if (!System.IO.File.Exists(rutaArchivo))
                    return NotFound(new { mensaje = "Archivo PDF no encontrado" });

                var bytes = await System.IO.File.ReadAllBytesAsync(rutaArchivo);
                var base64 = Convert.ToBase64String(bytes);

                return Ok(new
                {
                    file = base64,
                    fileName = $"certificado_{certificadoId}.pdf",
                    codigoVerificacion = certificado.codigo_verificacion,
                    fechaAprobacion = certificado.fecha_emision.ToString("dd-MM-yyyy")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("historial")]
        [Authorize(Roles = "Directiva")]
        public async Task<IActionResult> ObtenerHistorialCertificados([FromQuery] int? usuarioId)
        {
            try
            {
                var historial = await _certificadosService.ObtenerHistorialCertificados(usuarioId ?? 0);
                return Ok(historial);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("verificar/{codigoVerificacion}")]
        public async Task<IActionResult> VerificarCertificado(string codigoVerificacion)
        {
            try
            {
                var resultado = await _certificadosService.VerificarCertificado(codigoVerificacion);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("resumen")]
        [Authorize(Roles = "Directiva")]
        public async Task<IActionResult> ObtenerResumenCertificados()
        {
            try
            {
                var resumen = await _certificadosService.ObtenerResumenCertificados();
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        private int ObtenerRutUsuarioAutenticado()
        {
            var rut = User.Claims.FirstOrDefault(c => c.Type == "Rut")?.Value;

            if (string.IsNullOrEmpty(rut))
            {
                throw new UnauthorizedAccessException("No se pudo obtener el RUT del usuario autenticado");
            }

            return int.Parse(rut);
        }
    }

    // ===== DTOs =====

    public class PagoTransbankRequest
    {
        public int SolicitudId { get; set; }
        public string Token { get; set; }
        public decimal Monto { get; set; }
        public string RutUsuario { get; set; }
    }

    public class ConfirmarPagoRequest
    {
        public string Token { get; set; }
    }

    public class AprobarCertificadoSinPagoRequest
    {
        public int SolicitudId { get; set; }
        public string Motivo { get; set; }
        public string Observaciones { get; set; }
    }
}

