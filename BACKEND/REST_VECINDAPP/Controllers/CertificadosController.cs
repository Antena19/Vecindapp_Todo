using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos.DTOs;
using REST_VECINDAPP.Seguridad;
using REST_VECINDAPP.Servicios;
using System.Security.Claims;
using Transbank.Webpay.WebpayPlus;
using Transbank.Webpay.Common;

namespace REST_VECINDAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CertificadosController : ControllerBase
    {
        private readonly cn_Certificados _certificadosService;
        private readonly VerificadorRoles _verificadorRoles;
        private readonly TransbankService _transbankService;

        public CertificadosController(
            cn_Certificados certificadosService,
            VerificadorRoles verificadorRoles,
            TransbankService transbankService)
        {
            _certificadosService = certificadosService;
            _verificadorRoles = verificadorRoles;
            _transbankService = transbankService;
        }

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

        [HttpPost("aprobar")]
        [Authorize(Roles = "Directiva")]
        public async Task<IActionResult> AprobarCertificado([FromBody] AprobarCertificadoRequest request)
        {
            try
            {
                var resultado = await _certificadosService.AprobarCertificado(request.SolicitudId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("rechazar")]
        [Authorize(Roles = "Directiva")]
        public async Task<IActionResult> RechazarCertificado([FromBody] RechazarCertificadoRequest request)
        {
            try
            {
                var resultado = await _certificadosService.RechazarCertificado(request.SolicitudId, request.MotivoRechazo);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("tipos")]
        public async Task<IActionResult> ObtenerTiposCertificado()
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

        [HttpGet("mis-solicitudes")]
        public async Task<IActionResult> ObtenerMisSolicitudes()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
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

        [HttpPost("pago/iniciar")]
        public async Task<IActionResult> IniciarPago([FromBody] PagoTransbankRequest request)
        {
            try
            {
                var solicitud = await _certificadosService.ObtenerSolicitud(request.SolicitudId);
                if (solicitud == null)
                    return NotFound(new { mensaje = "Solicitud no encontrada" });

                var buyOrder = $"cert-{request.SolicitudId}-{DateTime.Now.Ticks}";
                var sessionId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString();

                var response = await _transbankService.CreateTransaction(
                    solicitud.Precio,
                    buyOrder,
                    sessionId
                );

                // Registrar el pago en la base de datos
                int usuarioRut = solicitud.UsuarioRut;
                var (exito, mensaje) = await _certificadosService.RegistrarPagoCertificado(
                    usuarioRut,
                    request.SolicitudId,
                    solicitud.Precio,
                    "webpay",
                    response.Token,
                    response.Url
                );
                if (!exito)
                {
                    return BadRequest(new { mensaje });
                }

                // Guardar el token en la base de datos para su posterior verificación
                await _certificadosService.GuardarTokenPago(request.SolicitudId, response.Token);

                return Ok(new
                {
                    url = response.Url,
                    token = response.Token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("pago/confirmar")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarPago([FromBody] ConfirmarPagoRequest request)
        {
            try
            {
                Console.WriteLine($"[LOG] Iniciando confirmación de pago para token: {request.Token}");
                var response = await _transbankService.CommitTransaction(request.Token);
                Console.WriteLine($"[LOG] Respuesta de Transbank: Status={response.Status}, BuyOrder={response.BuyOrder}, SessionId={response.SessionId}, Amount={response.Amount}, ResponseCode={response.ResponseCode}, AuthorizationCode={response.AuthorizationCode}, CardDetail={response.CardDetail?.CardNumber}");

                if (response.Status == "AUTHORIZED")
                {
                    // Primero actualizamos el estado del pago
                    var pagoOk = await _certificadosService.ConfirmarPago(request.Token, "aprobada");
                    Console.WriteLine($"[LOG] ConfirmarPago ejecutado, resultado: {pagoOk}");

                    if (pagoOk)
                    {
                        // Obtenemos la solicitud para verificar que todo está correcto
                        var solicitud = await _certificadosService.ObtenerSolicitudPorTokenWebpay(request.Token);
                        if (solicitud == null)
                        {
                            Console.WriteLine($"[ERROR] No se encontró la solicitud para el token: {request.Token}");
                            return BadRequest(new { mensaje = "Error al procesar la solicitud" });
                        }

                        return Ok(new { 
                            mensaje = "Pago confirmado y certificado generado correctamente",
                            solicitudId = solicitud.Id
                        });
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] Error al confirmar el pago para el token: {request.Token}");
                        return BadRequest(new { mensaje = "Error al confirmar el pago" });
                    }
                }
                else
                {
                    Console.WriteLine($"[LOG] Pago no autorizado. Status: {response.Status}");
                    return BadRequest(new { mensaje = "Pago no autorizado" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en ConfirmarPago: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
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

        [HttpPost("pagar/{solicitudId}")]
        public async Task<IActionResult> PagarCertificado(int solicitudId, [FromBody] PagoTransbankRequest pago)
        {
            try
            {
                var resultado = await _certificadosService.ProcesarPagoCertificado(solicitudId, pago.Token);
                return Ok(resultado);
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
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var certificado = await _certificadosService.ObtenerCertificado(certificadoId);
                
                if (certificado == null)
                    return NotFound(new { mensaje = "Certificado no encontrado" });

                if (certificado.solicitud.usuario_rut != userId && !_verificadorRoles.EsDirectiva())
                    return Forbid();

                var resultado = await _certificadosService.GenerarPDFCertificado(certificadoId);
                if (!resultado.Exito)
                    return BadRequest(new { mensaje = resultado.Mensaje });
                return Ok(new { mensaje = "Certificado generado correctamente" });
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
    }

    public class WebhookNotification
    {
        public string Type { get; set; }
        public string Action { get; set; }
        public WebhookData Data { get; set; }
    }

    public class WebhookData
    {
        public string Token { get; set; }
        public string Status { get; set; }
    }

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
}

