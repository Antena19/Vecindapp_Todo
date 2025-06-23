using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos;
using REST_VECINDAPP.Modelos.DTOs;
using REST_VECINDAPP.Seguridad;

namespace REST_VECINDAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComunicacionController : ControllerBase
    {
        private readonly cn_Comunicacion _comunicacionService;

        public ComunicacionController(cn_Comunicacion comunicacionService)
        {
            _comunicacionService = comunicacionService;
        }

        #region NOTICIAS

        [HttpGet("noticias")]
        public async Task<ActionResult<List<Noticia>>> GetNoticias(
            [FromQuery] string? categoria = null,
            [FromQuery] string? alcance = null,
            [FromQuery] string? estado = null,
            [FromQuery] int? autorRut = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var noticias = await _comunicacionService.ObtenerNoticiasAsync(
                    categoria, alcance, estado, autorRut, fechaDesde, fechaHasta);
                return Ok(noticias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpGet("noticias/{id}")]
        public async Task<ActionResult<Noticia>> GetNoticia(int id)
        {
            try
            {
                var noticia = await _comunicacionService.ObtenerNoticiaAsync(id);
                if (noticia == null)
                    return NotFound(new { message = "Noticia no encontrada" });

                return Ok(noticia);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpPost("noticias")]
        [VerificarRol("ADMINISTRADOR", "DIRECTIVA")]
        public async Task<ActionResult<Noticia>> CrearNoticia([FromBody] CrearNoticiaDTO noticiaDto)
        {
            try
            {
                var noticia = await _comunicacionService.CrearNoticiaAsync(noticiaDto);
                return CreatedAtAction(nameof(GetNoticia), new { id = noticia.Id }, noticia);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpPut("noticias/{id}")]
        [VerificarRol("ADMINISTRADOR", "DIRECTIVA")]
        public async Task<ActionResult<Noticia>> ActualizarNoticia(int id, [FromBody] ActualizarNoticiaDTO noticiaDto)
        {
            try
            {
                var noticia = await _comunicacionService.ActualizarNoticiaAsync(id, noticiaDto);
                if (noticia == null)
                    return NotFound(new { message = "Noticia no encontrada" });

                return Ok(noticia);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpDelete("noticias/{id}")]
        [VerificarRol("ADMINISTRADOR", "DIRECTIVA")]
        public async Task<ActionResult> EliminarNoticia(int id)
        {
            try
            {
                var resultado = await _comunicacionService.EliminarNoticiaAsync(id);
                if (!resultado)
                    return NotFound(new { message = "Noticia no encontrada" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpPatch("noticias/{id}/publicar")]
        [VerificarRol("ADMINISTRADOR", "DIRECTIVA")]
        public async Task<ActionResult<Noticia>> PublicarNoticia(int id)
        {
            try
            {
                var noticia = await _comunicacionService.PublicarNoticiaAsync(id);
                if (noticia == null)
                    return NotFound(new { message = "Noticia no encontrada" });

                return Ok(noticia);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpGet("noticias/destacadas")]
        public async Task<ActionResult<List<Noticia>>> GetNoticiasDestacadas()
        {
            try
            {
                var noticias = await _comunicacionService.ObtenerNoticiasDestacadasAsync();
                return Ok(noticias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpGet("noticias/buscar")]
        public async Task<ActionResult<List<Noticia>>> BuscarNoticias([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { message = "El término de búsqueda es requerido" });

                var noticias = await _comunicacionService.BuscarNoticiasAsync(q);
                return Ok(noticias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        #endregion

        #region COMENTARIOS

        [HttpGet("noticias/{noticiaId}/comentarios")]
        public async Task<ActionResult<List<ComentarioNoticia>>> GetComentariosNoticia(int noticiaId)
        {
            try
            {
                var comentarios = await _comunicacionService.ObtenerComentariosNoticiaAsync(noticiaId);
                return Ok(comentarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpPost("noticias/{noticiaId}/comentarios")]
        [VerificarRol("ADMINISTRADOR", "DIRECTIVA", "SOCIO")]
        public async Task<ActionResult<ComentarioNoticia>> AgregarComentario(int noticiaId, [FromBody] CrearComentarioDTO comentarioDto)
        {
            try
            {
                var comentario = await _comunicacionService.AgregarComentarioAsync(noticiaId, comentarioDto);
                return CreatedAtAction(nameof(GetComentariosNoticia), new { noticiaId }, comentario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpDelete("comentarios/{id}")]
        [VerificarRol("ADMINISTRADOR", "DIRECTIVA")]
        public async Task<ActionResult> EliminarComentario(int id)
        {
            try
            {
                var resultado = await _comunicacionService.EliminarComentarioAsync(id);
                if (!resultado)
                    return NotFound(new { message = "Comentario no encontrado" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        #endregion

        #region NOTIFICACIONES

        [HttpGet("notificaciones/usuario/{rut}")]
        public async Task<ActionResult<List<NotificacionUsuario>>> GetNotificacionesUsuario(int rut)
        {
            try
            {
                var notificaciones = await _comunicacionService.ObtenerNotificacionesUsuarioAsync(rut);
                return Ok(notificaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpPost("notificaciones")]
        [VerificarRol("ADMINISTRADOR", "DIRECTIVA")]
        public async Task<ActionResult<Notificacion>> CrearNotificacion([FromBody] CrearNotificacionDTO notificacionDto)
        {
            try
            {
                var notificacion = await _comunicacionService.CrearNotificacionAsync(notificacionDto);
                return CreatedAtAction(nameof(GetNotificacionesUsuario), new { rut = notificacionDto.Destinatarios.FirstOrDefault() }, notificacion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpPatch("notificaciones/{id}/marcar-leida")]
        public async Task<ActionResult> MarcarNotificacionLeida(int id)
        {
            try
            {
                var resultado = await _comunicacionService.MarcarNotificacionLeidaAsync(id);
                if (!resultado)
                    return NotFound(new { message = "Notificación no encontrada" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        #endregion

        #region ESTADÍSTICAS

        [HttpGet("estadisticas")]
        [VerificarRol("Administrador", "Directiva")]
        public async Task<ActionResult<EstadisticasComunicacion>> GetEstadisticas()
        {
            try
            {
                var estadisticas = await _comunicacionService.ObtenerEstadisticasComunicacionAsync();
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpGet("estadisticas/test")]
        [AllowAnonymous]
        public async Task<ActionResult<EstadisticasComunicacion>> GetEstadisticasTest()
        {
            try
            {
                var estadisticas = await _comunicacionService.ObtenerEstadisticasComunicacionAsync();
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        #endregion

        #region FUNCIONALIDADES ESPECIALES

        [HttpPost("noticias/{noticiaId}/notificar-importante")]
        [VerificarRol("ADMINISTRADOR", "DIRECTIVA")]
        public async Task<ActionResult<Notificacion>> NotificarAvisoImportante(int noticiaId)
        {
            try
            {
                var notificacion = await _comunicacionService.NotificarAvisoImportanteAsync(noticiaId);
                return Ok(notificacion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        [HttpPost("noticias/{noticiaId}/imagen")]
        [VerificarRol("ADMINISTRADOR", "DIRECTIVA")]
        public async Task<ActionResult<object>> SubirImagenNoticia(int noticiaId, IFormFile imagen)
        {
            try
            {
                if (imagen == null || imagen.Length == 0)
                    return BadRequest(new { message = "No se proporcionó ninguna imagen" });

                var imagenUrl = await _comunicacionService.SubirImagenNoticiaAsync(noticiaId, imagen);
                return Ok(new { imagenUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.ToString() });
            }
        }

        #endregion
    }
} 