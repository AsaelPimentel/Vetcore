using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vetcore.Helpers; // Necesario para acceder a Context.ObtenerRolEmpleado()

namespace Vetcore.Filters
{
    public class AuthorizeRolAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedroles;

        public AuthorizeRolAttribute(params string[] roles)
        {
            this.allowedroles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // 1. Verificamos si el usuario tiene una sesión/cookie válida
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            // 2. Extraemos el rol desde los Claims en lugar de usar Session
            string rolActual = httpContext.ObtenerRolEmpleado();

            if (string.IsNullOrEmpty(rolActual))
            {
                return false;
            }

            // 3. Validamos si su rol actual está en la lista de roles permitidos
            return allowedroles.Contains(rolActual);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Si no está logueado, lo mandamos al login
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                // Si está logueado pero intenta entrar a un módulo que no le corresponde (ej. Recepción intentando entrar a Expedientes), 
                // lo redirigimos al Dashboard en lugar del Login para evitar confusión.
                filterContext.Result = new RedirectResult("~/Home/Index");
            }
        }
    }
}