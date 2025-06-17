using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos;
using REST_VECINDAPP.Modelos.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace REST_VECINDAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Directiva")]
    public class DirectivaController : ControllerBase
    {
        private readonly cn_Directiva _directivaService;

        public DirectivaController(cn_Directiva directivaService)
        {
            _directivaService = directivaService;
        }

        [HttpGet("socios")]
        public ActionResult<List<Socio>> ListarSocios([FromQuery] int idsocio = -1)
        {
            try
            {
                var socios = _directivaService.ListarSocios(idsocio);
                return Ok(socios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("solicitudes")]
        public ActionResult<List<SolicitudSocioDTO>> ConsultarSolicitudes([FromQuery] string? estadoSolicitud = null)
        {
            try
            {
                var solicitudes = _directivaService.ConsultarSolicitudes(estadoSolicitud ?? string.Empty);
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("solicitudes/{rut}/aprobar")]
        public ActionResult<string> AprobarSolicitud(int rut)
        {
            try
            {
                var mensaje = _directivaService.AprobarSolicitud(rut);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("solicitudes/{rut}/rechazar")]
        public ActionResult<string> RechazarSolicitud(int rut, [FromBody] RechazoDTO rechazo)
        {
            try
            {
                var mensaje = _directivaService.RechazarSolicitud(rut, rechazo.MotivoRechazo);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("estadisticas")]
        public ActionResult<EstadisticasResponse> ObtenerEstadisticas()
        {
            try
            {
                var estadisticas = _directivaService.ObtenerEstadisticas();
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("socios/activos")]
        public ActionResult<List<SocioActivoDTO>> ObtenerSociosActivos()
        {
            try
            {
                var socios = _directivaService.ObtenerSociosActivos();
                return Ok(socios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("socios/{idSocio}/estado")]
        public ActionResult<string> ActualizarEstadoSocio(int idSocio, [FromBody] ActualizarEstadoSocioDTO request)
        {
            try
            {
                var mensaje = _directivaService.ActualizarEstadoSocio(idSocio, request.Estado, request.Motivo);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("socios/historial")]
        public ActionResult<List<SocioHistorialDTO>> ObtenerHistorialSocios()
        {
            try
            {
                var historial = _directivaService.ObtenerHistorialSocios();
                return Ok(historial);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("socios/todos")]
        public ActionResult<List<SocioActivoDTO>> ObtenerTodosSocios()
        {
            try
            {
                var socios = _directivaService.ObtenerTodosSocios();
                return Ok(socios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("socios/{rut}/revocar")]
        public ActionResult<string> RevocarMembresia(int rut, [FromBody] RechazoDTO rechazo)
        {
            try
            {
                var mensaje = _directivaService.RevocarMembresia(rut, rechazo.MotivoRechazo);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("certificados/{solicitudId}/aprobar")]
        public ActionResult<string> AprobarCertificado(int solicitudId, [FromBody] AprobarCertificadoRequest request)
        {
            try
            {
                if (request.SolicitudId != solicitudId)
                {
                    return BadRequest("El ID de la solicitud en la URL no coincide con el del cuerpo de la solicitud");
                }

                var mensaje = _directivaService.AprobarCertificado(solicitudId, int.Parse(request.DirectivaRut), request.Observaciones);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("certificados/{solicitudId}/rechazar")]
        public ActionResult<string> RechazarCertificado(int solicitudId, [FromBody] RechazarCertificadoRequest request)
        {
            try
            {
                var mensaje = _directivaService.RechazarCertificado(solicitudId, int.Parse(request.DirectivaRut), request.MotivoRechazo);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("tarifas/certificados/{tipoCertificadoId}")]
        public ActionResult<string> ConfigurarTarifaCertificado(int tipoCertificadoId, [FromBody] ConfigurarTarifaRequest request)
        {
            try
            {
                var mensaje = _directivaService.ConfigurarTarifaCertificado(
                    tipoCertificadoId,
                    Convert.ToDecimal(request.PrecioSocio),
                    Convert.ToDecimal(request.PrecioVecino),
                    request.MediosPago
                );
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
