using Microsoft.AspNetCore.Mvc;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos;
using REST_VECINDAPP.Modelos.DTOs;
using REST_VECINDAPP.Seguridad;

namespace REST_VECINDAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SociosController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SociosController(IConfiguration configuration)
        {
            _config = configuration;
        }

        [HttpPost("solicitar")]
        public async Task<IActionResult> SolicitarMembresia([FromForm] SolicitudMembresia datos)
        {
            try
            {
                int rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");
                if (rut == 0)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "archivos");
                Directory.CreateDirectory(rootPath);

                string rutaIdentidad = null;
                string rutaDomicilio = null;

                if (datos.DocumentoIdentidad != null)
                {
                    string fileName = $"identidad_{rut}_{DateTime.Now.Ticks}{Path.GetExtension(datos.DocumentoIdentidad.FileName)}";
                    string fullPath = Path.Combine(rootPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await datos.DocumentoIdentidad.CopyToAsync(stream);
                    }
                    rutaIdentidad = $"/archivos/{fileName}";
                }

                if (datos.DocumentoDomicilio != null)
                {
                    string fileName = $"domicilio_{rut}_{DateTime.Now.Ticks}{Path.GetExtension(datos.DocumentoDomicilio.FileName)}";
                    string fullPath = Path.Combine(rootPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await datos.DocumentoDomicilio.CopyToAsync(stream);
                    }
                    rutaDomicilio = $"/archivos/{fileName}";
                }

                cn_Socios cnSocios = new cn_Socios(_config);
                string resultado = cnSocios.SolicitarMembresia(rut, rutaIdentidad, rutaDomicilio);

                return Ok(new { mensaje = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al solicitar membresía", error = ex.Message });
            }
        }

        [HttpGet("solicitud/estado")]
        public IActionResult ConsultarEstadoSolicitud()
        {
            try
            {
                // Obtener el RUT del usuario desde el token
                int rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");

                if (rut == 0)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                cn_Socios cnSocios = new cn_Socios(_config);
                var solicitud = cnSocios.ConsultarEstadoSolicitud(rut);

                if (solicitud == null)
                    return NotFound(new { mensaje = "No se encontró solicitud para este usuario" });

                return Ok(solicitud);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al consultar estado de solicitud", error = ex.Message });
            }
        }

        [HttpGet("perfil")]
        public IActionResult ObtenerPerfil()
        {
            try
            {
                // Obtener el RUT del usuario desde el token
                int rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");

                if (rut == 0)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                cn_Socios cnSocios = new cn_Socios(_config);
                var perfil = cnSocios.ObtenerPerfil(rut);

                if (perfil == null)
                    return NotFound(new { mensaje = "No se encontró información de socio para este usuario" });

                return Ok(perfil);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener perfil de socio", error = ex.Message });
            }
        }

        [HttpGet("verificar")]
        public IActionResult EsSocio()
        {
            try
            {
                // Obtener el RUT del usuario desde el token
                int rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");

                if (rut == 0)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                cn_Socios cnSocios = new cn_Socios(_config);
                bool esSocio = cnSocios.EsSocio(rut);

                return Ok(new { esSocio = esSocio });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al verificar estado de socio", error = ex.Message });
            }
        }

        /*[HttpGet("cuotas")]
        public IActionResult ConsultarCuotas()
        {
            try
            {
                // Obtener el RUT del usuario desde el token
                int rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");

                if (rut == 0)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                cn_Socios cnSocios = new cn_Socios(_config);
                var cuotas = cnSocios.ConsultarCuotas(rut);

                return Ok(cuotas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al consultar cuotas", error = ex.Message });
            }
        }*/

        /*[HttpPost("cuotas/{cuotaId}/pagar")]
        public IActionResult PagarCuota(int cuotaId, [FromBody] PagoCuotaDTO datos)
        {
            try
            {
                // Obtener el RUT del usuario desde el token
                int rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");

                if (rut == 0)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                cn_Socios cnSocios = new cn_Socios(_config);
                string resultado = cnSocios.PagarCuota(
                    rut,
                    cuotaId,
                    datos.Monto,
                    datos.MetodoPago);

                return Ok(new { mensaje = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar pago de cuota", error = ex.Message });
            }
        }*/

        /*[HttpGet("certificados")]
        public IActionResult ConsultarCertificados()
        {
            try
            {
                // Obtener el RUT del usuario desde el token
                int rut = int.Parse(User.FindFirst("rut")?.Value ?? "0");

                if (rut == 0)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                cn_Socios cnSocios = new cn_Socios(_config);
                var certificados = cnSocios.ConsultarCertificados(rut);

                return Ok(certificados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al consultar certificados", error = ex.Message });
            }
        }*/
    }
}