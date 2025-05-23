using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Modelos;
using REST_VECINDAPP.Modelos.DTOs;
using REST_VECINDAPP.Servicios;
using System.Security.Claims;

namespace REST_VECINDAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly cn_Usuarios _cnUsuarios;
        private readonly IEmailService _emailService;

        public UsuariosController(IConfiguration configuration, cn_Usuarios cnUsuarios, IEmailService emailService)
        {
            _config = configuration;
            _emailService = emailService;
            _cnUsuarios = cnUsuarios;
        }

        [Authorize]
        [HttpGet]
        public ActionResult<List<Usuario>> Get([FromQuery] int rut = -1)
        {
            var cnUsuarios = _cnUsuarios;
            return Ok(cnUsuarios.ListarUsuarios(rut));
        }

        [HttpPost("registrar")]
        [AllowAnonymous]
        public IActionResult Registrar([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    mensaje = "Datos de registro inválidos",
                    errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var cnUsuarios = _cnUsuarios;

            (bool exito, string mensaje) = cnUsuarios.RegistrarUsuario(usuario);

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Usuario registrado exitosamente",
                    rut = usuario.rut
                });
            }
            else
            {
                if (mensaje.Contains("correo electrónico") || mensaje.Contains("RUT"))
                {
                    return Conflict(new { mensaje });
                }

                return BadRequest(new { mensaje });
            }
        }

        [HttpPut("{rut}")]
        [Authorize]
        public IActionResult ActualizarUsuario(int rut, [FromBody] ActualizacionUsuarioDTO datosActualizacion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    mensaje = "Datos de actualización inválidos",
                    errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            if (rut <= 0)
            {
                return BadRequest(new { mensaje = "RUT inválido" });
            }

            var cnUsuarios = _cnUsuarios;
            (bool exito, string mensaje) = cnUsuarios.ActualizarDatosUsuario(
                rut,
                datosActualizacion.Nombres,
                datosActualizacion.Apellidos,  // Esto se asigna a apellidoPaterno
                datosActualizacion.CorreoElectronico,
                datosActualizacion.Telefono,
                datosActualizacion.Direccion,
                datosActualizacion.ApellidoMaterno  // Agregar este parámetro
            );
            Console.WriteLine($"Resultado actualización: exito={exito}, mensaje='{mensaje}'");

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Datos de usuario actualizados exitosamente"
                });
            }
            else
            {
                if (mensaje.Contains("no encontrado"))
                {
                    return NotFound(new { mensaje });
                }

                if (mensaje.Contains("correo ya registrado"))
                {
                    return Conflict(new { mensaje });
                }

                return BadRequest(new { mensaje });
            }
        }

        [HttpDelete("{rut}")]
        [Authorize]
        public IActionResult EliminarUsuario(int rut)
        {
            if (rut <= 0)
            {
                return BadRequest(new { mensaje = "RUT inválido" });
            }

            // Obtener el RUT del usuario autenticado desde el token JWT
            int rutSolicitante;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("rut") ?? User.FindFirst("sub");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out rutSolicitante))
            {
                // Como alternativa, podríamos usar el mismo RUT que se está intentando eliminar
                // Esto permitiría al usuario eliminar su propia cuenta
                rutSolicitante = rut;
            }

            var cnUsuarios = _cnUsuarios;
            (bool exito, string mensaje) = cnUsuarios.EliminarUsuario(rut, rutSolicitante);

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Usuario eliminado exitosamente"
                });
            }
            else
            {
                if (mensaje.Contains("no encontrado"))
                {
                    return NotFound(new { mensaje });
                }
                if (mensaje.Contains("No autorizado"))
                {
                    return Unauthorized(new { mensaje });
                }
                if (mensaje.Contains("No se puede eliminar"))
                {
                    return Conflict(new { mensaje });
                }
                return BadRequest(new { mensaje });
            }
        }

        [HttpGet("autenticado")]
        [Authorize]
        public IActionResult ObtenerUsuarioAutenticado()
        {
            var cnUsuarios = _cnUsuarios;
            int rutUsuario = ObtenerRutUsuarioAutenticado();

            (bool exito, Usuario? usuario, string mensaje) = cnUsuarios.ObtenerDatosUsuario(rutUsuario);

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Información de usuario obtenida exitosamente",
                    usuario = new
                    {
                        rut = usuario.rut,
                        nombre = usuario.nombre,
                        apellido_paterno = usuario.apellido_paterno,
                        apellido_materno = usuario.apellido_materno,
                        correo_electronico = usuario.correo_electronico,
                        telefono = usuario.telefono,
                        direccion = usuario.direccion,
                        tipo_usuario = usuario.tipo_usuario
                    }
                });
            }
            else
            {
                if (mensaje.Contains("no encontrado"))
                {
                    return NotFound(new { mensaje });
                }

                return BadRequest(new { mensaje });
            }
        }

        [HttpPost("cambiar-contrasena")]
        [Authorize]
        public IActionResult CambiarContrasena([FromBody] CambioContrasenaDTO cambioDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    mensaje = "Datos de cambio de contraseña inválidos",
                    errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            int rut = ObtenerRutUsuarioAutenticado();

            (bool exito, string mensaje) = _cnUsuarios.CambiarContrasena(
                rut,
                cambioDto.ContrasenaActual,
                cambioDto.NuevaContrasena
            );

            if (exito)
            {
                return Ok(new
                {
                    mensaje = "Contraseña cambiada exitosamente"
                });
            }
            else
            {
                return BadRequest(new { mensaje });
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
}