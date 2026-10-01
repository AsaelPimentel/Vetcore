using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vetcore.Models;
using Vetcore.Filters;
using Vetcore.Services;
<<<<<<< HEAD
using Vetcore.Helpers; // Importante para acceder a RolesSistema
=======
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba

namespace Vetcore.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly CitaService _citaService = new CitaService();

        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetCitas()
        {
            var citas = await _citaService.ObtenerCitasAsync();

            var eventos = citas.Select(c => new {
                id = c.IdCita,
<<<<<<< HEAD
                // CORRECCIÓN: Salto de línea seguro usando \n para evitar errores de compilación en C#
                title = c.Mascotas != null ? $"{c.Mascotas.Nombre}\n{c.Tipo}" : c.Tipo,
=======
                // Corregido el salto de línea para evitar errores
                title = c.Mascotas != null ? $"{c.Mascotas.Nombre} - {c.Tipo}" : c.Tipo,
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
                start = c.FechaHora.ToString("yyyy-MM-ddTHH:mm:ss"),
                description = c.Motivo,
                veterinario = c.Empleados != null ? c.Empleados.Nombre : "Sin asignar",
                color = ObtenerColorPorTipo(c.Tipo)
            });

            return Json(eventos, JsonRequestBehavior.AllowGet);
        }

        private string ObtenerColorPorTipo(string tipo)
        {
            switch (tipo)
            {
                case "Medica":
                case "Consulta":
                    return "#0EA5E9";
                case "Estetica":
                    return "#10B981";
                case "Vacunacion":
                    return "#F59E0B";
                case "Cirugia":
                    return "#EF4444";
                default:
                    return "#8B5CF6";
            }
        }

        public async Task<ActionResult> Create(string fecha = null)
        {
            var mascotas = await _citaService.ObtenerMascotasDropdownAsync();
            var selectMascotas = mascotas.Select(m => new {
                IdMascota = m.IdMascota,
                DisplayNombre = m.Nombre + " (Dueño: " + (m.Propietarios != null ? m.Propietarios.Nombre : "N/D") + ")"
            }).ToList();

            ViewBag.IdMascota = new SelectList(selectMascotas, "IdMascota", "DisplayNombre");

            var empleados = await _citaService.ObtenerEmpleadosActivosAsync();
            ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre");
            ViewBag.FechaSeleccionada = fecha;

            return PartialView("_CreateModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Citas cita)
        {
            if (cita.FechaHora < DateTime.Now)
            {
                ModelState.AddModelError("FechaHora", "La fecha y hora no puede ser en el pasado.");
            }

            if (ModelState.IsValid)
            {
                await _citaService.CrearCitaAsync(cita);
                return Json(new { success = true });
            }

            var mascotas = await _citaService.ObtenerMascotasDropdownAsync();
            var empleados = await _citaService.ObtenerEmpleadosActivosAsync();

            ViewBag.IdMascota = new SelectList(mascotas, "IdMascota", "Nombre", cita.IdMascota);
            ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre", cita.IdEmpleado);

            return PartialView("_CreateModal", cita);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            Citas cita = await _citaService.ObtenerPorIdAsync(id.Value);

            if (cita == null) return HttpNotFound();

            return PartialView("_DetailsModal", cita);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            Citas cita = await _citaService.ObtenerPorIdAsync(id.Value);
            if (cita == null) return HttpNotFound();

            var mascotas = await _citaService.ObtenerMascotasDropdownAsync();
            var empleados = await _citaService.ObtenerEmpleadosActivosAsync();

            ViewBag.IdMascota = new SelectList(mascotas, "IdMascota", "Nombre", cita.IdMascota);
            ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre", cita.IdEmpleado);

            return PartialView("_EditModal", cita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Citas cita)
        {
            if (ModelState.IsValid)
            {
                await _citaService.ActualizarCitaAsync(cita);
                return Json(new { success = true });
            }

            var mascotas = await _citaService.ObtenerMascotasDropdownAsync();
            var empleados = await _citaService.ObtenerEmpleadosActivosAsync();

            ViewBag.IdMascota = new SelectList(mascotas, "IdMascota", "Nombre", cita.IdMascota);
            ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre", cita.IdEmpleado);

            return PartialView("_EditModal", cita);
        }

<<<<<<< HEAD
        // CORRECCIÓN: Uso de la constante fuertemente tipada en lugar de string "Administrador"
        [AuthorizeRol(RolesSistema.Admin)]
=======
        [AuthorizeRol("Administrador")]
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            Citas cita = await _citaService.ObtenerPorIdAsync(id.Value);

            if (cita == null) return HttpNotFound();

            return PartialView("_DeleteModal", cita);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
<<<<<<< HEAD
        [AuthorizeRol(RolesSistema.Admin)] // CORRECCIÓN
=======
        [AuthorizeRol("Administrador")]
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _citaService.EliminarCitaAsync(id);
            return Json(new { success = true });
        }
    }
}