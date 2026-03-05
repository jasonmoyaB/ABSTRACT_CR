using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Abstract_CR.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string[] _allowedRoles;

        public SessionAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? Array.Empty<string>();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var session = context.HttpContext.Session;
            var usuarioId = session.GetInt32("UsuarioID");

            if (usuarioId is null)
            {
                context.Result = new RedirectToActionResult("Login", "Autenticacion", null);
                return;
            }

            if (_allowedRoles.Length > 0)
            {
                var rol = session.GetString("Rol");
                var hasRole = !string.IsNullOrWhiteSpace(rol) &&
                              _allowedRoles.Any(allowed => string.Equals(allowed, rol, StringComparison.OrdinalIgnoreCase));

                if (!hasRole)
                {
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                    return;
                }
            }

            await next();
        }
    }
}
