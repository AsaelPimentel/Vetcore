using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Vetcore.Filters;
using Vetcore.Models;
using Vetcore.Helpers;
using Vetcore.Services;

namespace Vetcore.Controllers
{
    [Authorize] // Asegura que el usuario esté autenticado antes de evaluar los roles
    [AuthorizeRol(RolesSistema.Admin, RolesSistema.Medico)]
    public class ExpedientesController : Controller
    {
        private readonly ExpedienteService _expedienteService = new ExpedienteService();

        public async Task<ActionResult> Historial(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var mascota = await _expedienteService.ObtenerMascotaConDetallesAsync(id.Value);
            if (mascota == null) return HttpNotFound();

            var expediente = await _expedienteService.ObtenerOAsignarExpedienteAsync(id.Value);
            ViewBag.IdExpediente = expediente.IdExpediente;

            ViewBag.HistorialConsultas = await _expedienteService.ObtenerHistorialConsultasAsync(expediente.IdExpediente);

            return View(mascota);
        }

        [HttpGet]
        public async Task<ActionResult> RegistrarConsulta(int idExpediente)
        {
            var consulta = new Consultas
            {
                IdExpediente = idExpediente,
                Fecha = DateTime.Now
            };

            var empleados = await _expedienteService.ObtenerEmpleadosActivosAsync();
            ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre");

            return PartialView("_CreateConsultaModal", consulta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegistrarConsulta(Consultas consulta, HttpPostedFileBase EstudioFile)
        {
            ModelState.Remove("Sintomas");
            ModelState.Remove("Diagnostico");
            ModelState.Remove("RecetaMedica");
            ModelState.Remove("NotasAdicionales");

            if (ModelState.IsValid)
            {
                try
                {
                    // MIGRACIÓN A CLAIMS: Obtenemos el ID del empleado usando la extensión
                    consulta.IdEmpleado = HttpContext.ObtenerIdEmpleado();

                    if (EstudioFile != null && EstudioFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(EstudioFile.FileName).ToLower();

                        // VALIDACIÓN DE SEGURIDAD: Evitar subida de archivos maliciosos (.exe, .bat, etc)
                        var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                        if (!extensionesPermitidas.Contains(extension))
                        {
                            return Json(new { success = false, message = "Formato de archivo no permitido. Solo se aceptan imágenes o PDF." });
                        }

                        string folderPath = Server.MapPath("~/Content/Estudios");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        string fileName = Guid.NewGuid().ToString() + extension;
                        string path = Path.Combine(folderPath, fileName);

                        EstudioFile.SaveAs(path);
                        consulta.ArchivoEstudio = fileName;
                    }

                    await _expedienteService.RegistrarConsultaAsync(consulta);
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            var empleados = await _expedienteService.ObtenerEmpleadosActivosAsync();
            ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre", consulta.IdEmpleado);

            return PartialView("_CreateConsultaModal", consulta);
        }

        [HttpGet]
        public async Task<ActionResult> DatosEvolucion(int idMascota)
        {
            var consultas = await _expedienteService.ObtenerConsultasParaEvolucionAsync(idMascota);

            var labels = consultas.Select(c => c.Fecha.Value.ToString("dd/MMM")).ToList();
            var pesos = consultas.Select(c => c.PesoKg ?? 0).ToList();
            var temperaturas = consultas.Select(c => c.Temperatura ?? 0).ToList();

            return Json(new { labels, pesos, temperaturas }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ImprimirReceta(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var consulta = await _expedienteService.ObtenerConsultaConDetallesAsync(id.Value);
            if (consulta == null) return HttpNotFound();

            return View(consulta);
        }

        public async Task<ActionResult> EditarConsulta(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var consulta = await _expedienteService.ObtenerConsultaPorIdAsync(id.Value);
            if (consulta == null) return HttpNotFound();

            if (consulta.Fecha.HasValue && consulta.Fecha.Value.Date != DateTime.Today)
            {
                return Content("<div class='alert alert-danger m-3'>Por seguridad, solo se pueden editar las consultas creadas el día de hoy.</div>");
            }

            var empleados = await _expedienteService.ObtenerEmpleadosActivosAsync();
            ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre", consulta.IdEmpleado);

            return PartialView("_EditConsultaModal", consulta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarConsulta(Consultas modelo)
        {
            ModelState.Remove("Sintomas");
            ModelState.Remove("Diagnostico");
            ModelState.Remove("RecetaMedica");
            ModelState.Remove("NotasAdicionales");

            if (ModelState.IsValid)
            {
                try
                {
                    bool actualizado = await _expedienteService.ActualizarConsultaAsync(modelo);
                    if (!actualizado) return HttpNotFound();

                    return Json(new { success = true });
                }
                catch (InvalidOperationException ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                catch (Exception)
                {
                    return Json(new { success = false, message = "Ocurrió un error al actualizar los datos." });
                }
            }

            var empleados = await _expedienteService.ObtenerEmpleadosActivosAsync();
            ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre", modelo.IdEmpleado);

            return PartialView("_EditConsultaModal", modelo);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}