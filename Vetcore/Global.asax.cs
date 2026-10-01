using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Newtonsoft.Json;
using Vetcore.Models;

namespace Vetcore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // NUEVA LÍNEA: Indicamos que el ClaimTypes.NameIdentifier es el identificador único para el AntiForgery
            System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;
        }

        // Este método se ejecuta en cada petición HTTP
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
            {
                try
                {
                    var authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                    if (authTicket != null && !authTicket.Expired)
                    {
                        // Extraemos la información almacenada (Id, Nombre, Rol)
                        var userModel = JsonConvert.DeserializeObject<UserDataModel>(authTicket.UserData);

                        // Construimos los Claims (Identificadores seguros de ASP.NET)
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, userModel.IdEmpleado.ToString()),
                            new Claim(ClaimTypes.Name, userModel.Nombre),
                            new Claim(ClaimTypes.Role, userModel.Rol),
                            new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity")
                        };

                        // Establecemos la Identidad y el Principal
                        var identity = new ClaimsIdentity(claims, "Forms");
                        var principal = new ClaimsPrincipal(identity);

                        // Inyectamos el usuario en el contexto actual
                        HttpContext.Current.User = principal;
                        Thread.CurrentPrincipal = principal;
                    }
                }
                catch (Exception)
                {
                    // Si la cookie está corrupta o fue manipulada, se ignora de forma segura.
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            var logger = NLog.LogManager.GetCurrentClassLogger();

            // Como ya no usamos Session, leemos el nombre de los Claims
            var identity = HttpContext.Current.User?.Identity as ClaimsIdentity;
            string usuario = identity != null && identity.IsAuthenticated
                             ? identity.Name
                             : "Anónimo";

            logger.Error(exception, $"Error crítico en ruta por el usuario: {usuario}");
        }
    }
}