using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos;
using System.Security.Claims;

namespace REST_VECINDAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventosController : ControllerBase
    {
        private readonly cn_Eventos _eventosService;

        public EventosController(cn_Eventos eventosService)
        {
            _eventosService = eventosService;
        }

        [HttpPost]
        [Authorize(Roles = "Directiva")]
        public IActionResult CrearEvento([FromBody] Evento evento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    mensaje = "Datos del evento inválidos",
                    errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var (exito, eventoCreado, mensaje) = _eventosService.CrearEvento(evento);

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Evento creado exitosamente",
                    evento = eventoCreado
                });
            }

            return BadRequest(new { mensaje });
        }

        [HttpPost("asistencia")]
        public IActionResult RegistrarAsistencia([FromBody] AsistenciaRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    mensaje = "Datos de asistencia inválidos",
                    errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");
                Console.WriteLine($"Intentando registrar asistencia - Código: {request.CodigoNumerico}, RUT: {rut}");
                
                var (exito, mensaje) = _eventosService.RegistrarAsistencia(request.CodigoQr, request.CodigoNumerico, rut);

                if (exito)
                {
                    return Ok(new { mensaje });
                }

                return BadRequest(new { mensaje });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar asistencia: {ex.Message}");
                return BadRequest(new { mensaje = $"Error al registrar asistencia: {ex.Message}" });
            }
        }

        [HttpGet("{eventoId}/asistentes")]
        [Authorize(Roles = "Directiva")]
        public IActionResult ConsultarAsistentes(int eventoId)
        {
            var (exito, asistentes, mensaje) = _eventosService.ConsultarAsistentes(eventoId);

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Asistentes consultados exitosamente",
                    asistentes
                });
            }

            return BadRequest(new { mensaje });
        }

        [HttpGet]
        public IActionResult ConsultarEventos()
        {
            var rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");
            var (exito, eventos, mensaje) = _eventosService.ConsultarEventos(rut);

            if (exito)
            {
                return Ok(eventos);
            }

            return BadRequest(new { mensaje });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Directiva")]
        public IActionResult ActualizarEvento(int id, [FromBody] Evento evento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    mensaje = "Datos del evento inválidos",
                    errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var (exito, eventoActualizado, mensaje) = _eventosService.ActualizarEvento(id, evento);

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Evento actualizado exitosamente",
                    evento = eventoActualizado
                });
            }

            return BadRequest(new { mensaje });
        }

        [HttpGet("historial")]
        public IActionResult ConsultarHistorialAsistencia()
        {
            var rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");
            var (exito, asistencias, mensaje) = _eventosService.ConsultarHistorialAsistencia(rut);

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Historial de asistencia consultado exitosamente",
                    asistencias
                });
            }

            return BadRequest(new { mensaje });
        }

        [HttpGet("reporte")]
        [Authorize(Roles = "Directiva")]
        public IActionResult GenerarReporteAsistencia([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            var (exito, eventos, mensaje) = _eventosService.GenerarReporteAsistencia(fechaInicio, fechaFin);

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Reporte generado exitosamente",
                    eventos
                });
            }

            return BadRequest(new { mensaje });
        }

        [HttpGet("{id}")]
        public IActionResult ConsultarEvento(int id)
        {
            var (exito, evento, mensaje) = _eventosService.ConsultarEvento(id);

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Evento consultado exitosamente",
                    evento
                });
            }

            return BadRequest(new { mensaje });
        }

        [HttpPost("{id}/cancelar")]
        [Authorize(Roles = "Directiva")]
        public IActionResult CancelarEvento(int id)
        {
            var (exito, mensaje) = _eventosService.CancelarEvento(id);

            if (exito)
            {
                return Ok(new { mensaje });
            }

            return BadRequest(new { mensaje });
        }

        [HttpPost("{id}/eliminar")]
        [Authorize(Roles = "Directiva")]
        public IActionResult EliminarEvento(int id)
        {
            var (exito, mensaje) = _eventosService.EliminarEvento(id);

            if (exito)
            {
                return Ok(new { mensaje = "Evento eliminado exitosamente" });
            }

            return BadRequest(new { mensaje });
        }
    }

    public class AsistenciaRequest
    {
        public string? CodigoQr { get; set; }
        public string? CodigoNumerico { get; set; }
    }
}
