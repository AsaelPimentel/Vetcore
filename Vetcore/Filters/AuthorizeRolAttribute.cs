using System;
<<<<<<< HEAD
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
=======
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Vetcore.Filters
{
    // AGREGADO: ": AuthorizeAttribute" para que permita sobrescribir los métodos de seguridad
    public class AuthorizeRolAttribute : AuthorizeAttribute
    {
        private readonly string[] _rolesPermitidos;

        public AuthorizeRolAttribute(params string[] roles)
        {
            _rolesPermitidos = roles;
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
<<<<<<< HEAD
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
=======
            bool baseAuthorized = base.AuthorizeCore(httpContext);
            if (!baseAuthorized) return false;

            string rolUsuario = httpContext.Session["RolEmpleado"] as string;
            if (string.IsNullOrEmpty(rolUsuario)) return false;

            foreach (var rol in _rolesPermitidos)
            {
                if (rolUsuario.Equals(rol, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
<<<<<<< HEAD
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
=======
            filterContext.Result = new HttpUnauthorizedResult("No cuentas con los permisos necesarios para realizar esta acción.");
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        }
    }
}