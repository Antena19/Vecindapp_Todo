﻿using Microsoft.AspNetCore.Http;
using REST_VECINDAPP.CapaNegocios;
using System.Security.Claims;

namespace REST_VECINDAPP.Seguridad
{
    public class VerificadorRoles
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly cn_Usuarios _cnUsuarios;

        public VerificadorRoles(IHttpContextAccessor httpContextAccessor, cn_Usuarios cnUsuarios)
        {
            _httpContextAccessor = httpContextAccessor;
            _cnUsuarios = cnUsuarios;
        }

        public bool TieneRol(string[] rolesPermitidos)
        {
            // Obtener el RUT del usuario desde el token JWT
            var rutClaim = _httpContextAccessor.HttpContext?.User?.Claims?
                .FirstOrDefault(c => c.Type == "Rut")?.Value;

            if (string.IsNullOrEmpty(rutClaim) || !int.TryParse(rutClaim, out int rut))
            {
                return false;
            }

            // Usar cn_Usuarios para obtener el tipo de usuario
            var (exito, usuario, _) = _cnUsuarios.ObtenerDatosUsuario(rut);

            if (!exito || usuario == null)
            {
                return false;
            }

            // Verificar si el tipo de usuario está en los roles permitidos
            // Convertimos a mayúsculas para una comparación sin distinción de mayúsculas/minúsculas
            return rolesPermitidos.Any(rol => rol.Equals(usuario.tipo_usuario, StringComparison.OrdinalIgnoreCase));
        }

        public bool EsDirectiva()
        {
            // Obtener el RUT del usuario desde el token JWT
            var rutClaim = _httpContextAccessor.HttpContext?.User?.Claims?
                .FirstOrDefault(c => c.Type == "Rut")?.Value;

            if (string.IsNullOrEmpty(rutClaim) || !int.TryParse(rutClaim, out int rut))
            {
                return false;
            }

            // Usar cn_Usuarios para obtener el tipo de usuario
            var (exito, usuario, _) = _cnUsuarios.ObtenerDatosUsuario(rut);

            if (!exito || usuario == null)
            {
                return false;
            }

            // Verificar si el usuario es de tipo directiva
            return usuario.tipo_usuario.Equals("Directiva", StringComparison.OrdinalIgnoreCase);
        }
    }
}