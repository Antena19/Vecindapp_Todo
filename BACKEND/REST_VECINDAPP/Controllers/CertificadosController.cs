using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos.DTOs;
using REST_VECINDAPP.Seguridad;
using System.Security.Claims;

namespace REST_VECINDAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CertificadosController : ControllerBase
    {
        private readonly cn_Certificados _certificadosService;
        private readonly cn_MercadoPago _mercadoPagoService;
        private readonly VerificadorRoles _verificadorRoles;

        public CertificadosController(
            cn_Certificados certificadosService,
            cn_MercadoPago mercadoPagoService,
            VerificadorRoles verificadorRoles)
        {
            _certificadosService = certificadosService;
            _mercadoPagoService = mercadoPagoService;
            _verificadorRoles = verificadorRoles;
        }

        [HttpPost("solicitar")]
        public async Task<IActionResult> SolicitarCertificado([FromBody] SolicitudCertificadoDTO solicitud)
        {
            try
            {
                var resultado = await _certificadosService.SolicitarCertificado(solicitud.UsuarioRut, solicitud);
                return Ok(resultado);
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
        public async Task<IActionResult> IniciarPago([FromBody] PagoCertificadoDTO pago)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var solicitud = await _certificadosService.ObtenerDetalleSolicitud(pago.SolicitudId);

                if (solicitud == null || solicitud.UsuarioRut != userId)
                {
                    return BadRequest(new { mensaje = "Solicitud no encontrada o no autorizada" });
                }

                var preferencia = await _mercadoPagoService.CrearPreferenciaPago(pago);
                return Ok(preferencia);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("pago/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> RecibirWebhook([FromBody] WebhookNotification notification)
        {
            try
            {
                if (notification.Type == "payment" && notification.Action == "payment.updated")
                {
                    var preferenciaId = notification.Data.Id;
                    var estado = notification.Data.Status;

                    _mercadoPagoService.ActualizarEstadoPago(preferenciaId, estado);
                    return Ok();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("pago/estado/{preferenciaId}")]
        public async Task<IActionResult> VerificarEstadoPago(string preferenciaId)
        {
            try
            {
                var estado = await _certificadosService.ObtenerEstadoPago(preferenciaId);
                return Ok(new { estado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("pagar/{solicitudId}")]
        public async Task<IActionResult> PagarCertificado(int solicitudId, [FromBody] PagoCertificadoDTO pago)
        {
            try
            {
                var resultado = await _certificadosService.ProcesarPagoCertificado(solicitudId, pago.PreferenciaId);
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
        public string Id { get; set; }
        public string Status { get; set; }
    }
}

