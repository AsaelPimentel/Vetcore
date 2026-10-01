using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Vetcore.Helpers;
using Vetcore.Models;
using Vetcore.Services;

namespace Vetcore.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService = new AuthService();

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string username, string password, string returnUrl)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Por favor, ingresa usuario y contraseña.");
                return View();
            }

            var usuarioDb = await _authService.ObtenerUsuarioPorUsernameAsync(username);

            if (usuarioDb == null || !PasswordHelper.VerifyPassword(password, usuarioDb.PasswordHash))
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View();
            }

            // 1. Armamos el modelo con los Claims
            var userModel = new UserDataModel
            {
                IdEmpleado = usuarioDb.IdEmpleado,
                Nombre = $"{usuarioDb.Empleados.Nombre} {usuarioDb.Empleados.Apellidos}",
                Rol = usuarioDb.Roles != null ? usuarioDb.Roles.Nombre : "Sin Rol"
            };

            // 2. Serializamos a JSON
            string userDataJson = JsonConvert.SerializeObject(userModel);

            // 3. Creamos el Ticket de Autenticación
            var authTicket = new FormsAuthenticationTicket(
                1,                              // Versión
                usuarioDb.Username,             // Nombre de usuario
                DateTime.Now,                   // Fecha de creación
                DateTime.Now.AddHours(12),      // Fecha de expiración (12 horas)
                false,                          // Persistente (se puede vincular a un checkbox "Recordarme")
                userDataJson                    // Datos serializados
            );

            // 4. Ciframos el ticket y lo añadimos a las Cookies HTTP
            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true // Seguridad adicional contra ataques XSS
            };
            Response.Cookies.Add(authCookie);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            // Ya no hay sesión que limpiar, la cookie se destruye con SignOut()
            return RedirectToAction("Login", "Auth");
        }
    }
}