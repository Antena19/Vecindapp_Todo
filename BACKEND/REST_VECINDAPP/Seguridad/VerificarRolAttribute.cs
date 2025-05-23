using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace REST_VECINDAPP.Seguridad
{
    public class VerificarRolAttribute : TypeFilterAttribute
    {
        public VerificarRolAttribute(params string[] roles) : base(typeof(VerificarRolFilter))
        {
            Arguments = new object[] { roles };
        }
    }

    public class VerificarRolFilter : IAuthorizationFilter
    {
        private readonly string[] _roles;
        private readonly VerificadorRoles _verificador;

        public VerificarRolFilter(string[] roles, VerificadorRoles verificador)
        {
            _roles = roles;
            _verificador = verificador;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Verificar si el usuario está autenticado
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Verificar si tiene el rol adecuado
            if (!_verificador.TieneRol(_roles))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}