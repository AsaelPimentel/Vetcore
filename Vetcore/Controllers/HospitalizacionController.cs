using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vetcore.Filters;
using Vetcore.Models;
using Vetcore.Helpers;
using Vetcore.Services;

namespace Vetcore.Controllers
{
    [Authorize] // Buena práctica: Asegura que haya una sesión activa (Claims) antes de verificar el rol
    [AuthorizeRol(RolesSistema.Admin, RolesSistema.Medico)]
    public class HospitalizacionController : Controller
    {
        private readonly HospitalizacionService _hospitalizacionService = new HospitalizacionService();

        // ELIMINADO: Se borró el método ObtenerIdEmpleadoActual() que dependía de Session

        public async Task<ActionResult> Index()
        {
            var jaulas = await _hospitalizacionService.ObtenerTodasLasJaulasAsync();

            ViewBag.HospitalizacionesActivas = await _hospitalizacionService.ObtenerHospitalizacionesActivasAsync();

            return View(jaulas);
        }

        public async Task<ActionResult> Ingresar()
        {
            var expedientes = await _hospitalizacionService.ObtenerExpedientesParaIngresoAsync();
            ViewBag.IdExpediente = new SelectList(expedientes, "IdExpediente", "Mascotas.Nombre");

            var jaulasLibres = await _hospitalizacionService.ObtenerJaulasLibresAsync();
            ViewBag.IdJaula = new SelectList(jaulasLibres, "IdJaula", "Codigo");

            return PartialView("_IngresarModal", new Hospitalizaciones());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Ingresar(Hospitalizaciones modelo)
        {
            if (ModelState.IsValid)
            {
                // MIGRACIÓN A CLAIMS: Usamos la extensión HttpContext para leer la cookie segura
                modelo.IdEmpleado = HttpContext.ObtenerIdEmpleado();

                await _hospitalizacionService.IngresarPacienteAsync(modelo);

                return Json(new { success = true });
            }

            var expedientes = await _hospitalizacionService.ObtenerExpedientesParaIngresoAsync();
            ViewBag.IdExpediente = new SelectList(expedientes, "IdExpediente", "Mascotas.Nombre", modelo.IdExpediente);

            var jaulasLibres = await _hospitalizacionService.ObtenerJaulasLibresAsync();
            ViewBag.IdJaula = new SelectList(jaulasLibres, "IdJaula", "Codigo", modelo.IdJaula);

            return PartialView("_IngresarModal", modelo);
        }

        public async Task<ActionResult> DetallesEstancia(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var hospitalizacion = await _hospitalizacionService.ObtenerHospitalizacionConDetallesAsync(id.Value);
            if (hospitalizacion == null) return HttpNotFound();

            ViewBag.NotasMedicas = await _hospitalizacionService.ObtenerNotasMedicasAsync(id.Value);

            var productosActivos = await _hospitalizacionService.ObtenerProductosActivosAsync();
            ViewBag.ListaProductos = new SelectList(productosActivos, "IdProducto", "Nombre");

            return View(hospitalizacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AgregarNotaOConsumo(int IdHospitalizacion, string NotasMedicas, int? IdProductoConsumo, decimal? CantidadConsumo)
        {
            try
            {
                // MIGRACIÓN A CLAIMS: Registramos la nota a nombre de quien tiene la sesión
                int idEmpleadoActual = HttpContext.ObtenerIdEmpleado();

                bool resultado = await _hospitalizacionService.AgregarNotaYConsumoAsync(
                    IdHospitalizacion,
                    NotasMedicas,
                    IdProductoConsumo,
                    CantidadConsumo,
                    idEmpleadoActual
                );

                if (!resultado)
                {
                    return Json(new { success = false, message = "Estancia no encontrada." });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DarAlta(int IdHospitalizacion, string DiagnosticoSalida)
        {
            // MIGRACIÓN A CLAIMS: Leemos el ID del empleado desde el ticket cifrado
            int idEmpleadoActual = HttpContext.ObtenerIdEmpleado();

            bool resultado = await _hospitalizacionService.DarAltaAsync(IdHospitalizacion, DiagnosticoSalida, idEmpleadoActual);

            if (!resultado) return HttpNotFound();

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}