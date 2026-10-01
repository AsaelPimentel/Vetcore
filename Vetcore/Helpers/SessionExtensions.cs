using System.Security.Claims;
using System.Web;

namespace Vetcore.Helpers
{
    public static class SessionExtensions // Mantenemos el nombre para no quebrar tus Vistas
    {
        // Extensiones para Vistas (@Context.ObtenerNombreEmpleado()) y Controladores
        public static int ObtenerIdEmpleado(this HttpContextBase context)
        {
            var identity = context.User.Identity as ClaimsIdentity;
            var claim = identity?.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 1;
        }

        public static string ObtenerNombreEmpleado(this HttpContextBase context)
        {
            var identity = context.User.Identity as ClaimsIdentity;
            return identity?.FindFirst(ClaimTypes.Name)?.Value ?? "Invitado";
        }

        public static string ObtenerRolEmpleado(this HttpContextBase context)
        {
            var identity = context.User.Identity as ClaimsIdentity;
            return identity?.FindFirst(ClaimTypes.Role)?.Value ?? "Sin Rol";
        }

        // Estas ya las tenías, las redirigimos a Claims
        public static bool IsAdmin(this HttpContextBase context)
        {
            return context.User.IsInRole(RolesSistema.Admin);
        }

        public static bool IsMedicoOrAdmin(this HttpContextBase context)
        {
            return context.User.IsInRole(RolesSistema.Admin) ||
                   context.User.IsInRole(RolesSistema.Medico);
        }
    }
}