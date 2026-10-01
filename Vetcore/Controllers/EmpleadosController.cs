using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vetcore.Filters;
using Vetcore.Helpers;
using Vetcore.Models;
using Vetcore.Services;

namespace Vetcore.Controllers
{
    [AuthorizeRol(RolesSistema.Admin)]
    public class EmpleadosController : Controller
    {
        private readonly EmpleadoService _empleadoService = new EmpleadoService();

<<<<<<< HEAD
        // ELIMINADO: Se borró el método ObtenerIdEmpleadoActual() que dependía de Session
=======
        private int ObtenerIdEmpleadoActual()
        {
            return Session["IdEmpleado"] != null ? (int)Session["IdEmpleado"] : 1;
        }
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba

        public async Task<ActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 8;

            var empleadosList = await _empleadoService.ObtenerEmpleadosAsync(search);

            int totalRecords = empleadosList.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var empleadosPaginated = empleadosList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages == 0 ? 1 : totalPages;
            ViewBag.SearchTerm = search;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_TablaEmpleados", empleadosPaginated);
            }

            return View(empleadosPaginated);
        }

        public async Task<ActionResult> Create()
        {
            var puestos = await _empleadoService.ObtenerPuestosAsync();
            var roles = await _empleadoService.ObtenerRolesAsync();

            ViewBag.IdPuesto = new SelectList(puestos, "IdPuesto", "Nombre");
            ViewBag.IdRol = new SelectList(roles, "IdRol", "Nombre");

            return PartialView("_CreateModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Empleados empleado, string Username, string PasswordHash, int? IdRol)
        {
            if (ModelState.IsValid)
            {
                await _empleadoService.CrearEmpleadoYUsuarioAsync(empleado, Username, PasswordHash, IdRol);

<<<<<<< HEAD
                // MIGRACIÓN A CLAIMS: Usamos HttpContext en lugar de Session
                int idAdmin = HttpContext.ObtenerIdEmpleado();
=======
                int idAdmin = ObtenerIdEmpleadoActual();
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
                await AuditoriaService.RegistrarAccionAsync(
                    idAdmin,
                    "REGISTRAR_EMPLEADO",
                    $"Se registró al nuevo empleado: {empleado.Nombre} {empleado.Apellidos} (ID: {empleado.IdEmpleado})"
                );

                return Json(new { success = true });
            }

            var puestos = await _empleadoService.ObtenerPuestosAsync();
            var roles = await _empleadoService.ObtenerRolesAsync();

            ViewBag.IdPuesto = new SelectList(puestos, "IdPuesto", "Nombre", empleado.IdPuesto);
            ViewBag.IdRol = new SelectList(roles, "IdRol", "Nombre", IdRol);

            return PartialView("_CreateModal", empleado);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var empleado = await _empleadoService.ObtenerPorIdAsync(id.Value);
            if (empleado == null) return HttpNotFound();

            var usuario = await _empleadoService.ObtenerUsuarioPorIdEmpleadoAsync(empleado.IdEmpleado);
            var puestos = await _empleadoService.ObtenerPuestosAsync();
            var roles = await _empleadoService.ObtenerRolesAsync();

            ViewBag.IdPuesto = new SelectList(puestos, "IdPuesto", "Nombre", empleado.IdPuesto);
            ViewBag.IdRol = new SelectList(roles, "IdRol", "Nombre", usuario?.IdRol);
            ViewBag.Username = usuario?.Username;

            return PartialView("_EditModal", empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Empleados empleado, string Username, string PasswordHash, int? IdRol)
        {
            if (ModelState.IsValid)
            {
                await _empleadoService.ActualizarEmpleadoYUsuarioAsync(empleado, Username, PasswordHash, IdRol);

<<<<<<< HEAD
                // MIGRACIÓN A CLAIMS
                int idAdmin = HttpContext.ObtenerIdEmpleado();
=======
                int idAdmin = ObtenerIdEmpleadoActual();
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
                await AuditoriaService.RegistrarAccionAsync(
                    idAdmin,
                    "MODIFICAR_EMPLEADO",
                    $"Se actualizaron los datos del empleado: {empleado.Nombre} {empleado.Apellidos} (ID: {empleado.IdEmpleado})"
                );

                return Json(new { success = true });
            }

            var puestos = await _empleadoService.ObtenerPuestosAsync();
            var roles = await _empleadoService.ObtenerRolesAsync();

            ViewBag.IdPuesto = new SelectList(puestos, "IdPuesto", "Nombre", empleado.IdPuesto);
            ViewBag.IdRol = new SelectList(roles, "IdRol", "Nombre", IdRol);

            return PartialView("_EditModal", empleado);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var empleado = await _empleadoService.ObtenerPorIdAsync(id.Value);
            if (empleado == null) return HttpNotFound();

            return PartialView("_DetailsModal", empleado);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var empleado = await _empleadoService.ObtenerPorIdAsync(id.Value);
            if (empleado == null) return HttpNotFound();

            return PartialView("_DeleteModal", empleado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var empleadoEliminado = await _empleadoService.EliminarEmpleadoYUsuarioAsync(id);

            if (empleadoEliminado != null)
            {
                string nombreCompleto = $"{empleadoEliminado.Nombre} {empleadoEliminado.Apellidos}";

<<<<<<< HEAD
                // MIGRACIÓN A CLAIMS
                int idAdmin = HttpContext.ObtenerIdEmpleado();
=======
                int idAdmin = ObtenerIdEmpleadoActual();
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
                await AuditoriaService.RegistrarAccionAsync(
                    idAdmin,
                    "ELIMINAR_EMPLEADO",
                    $"Se eliminó permanentemente al empleado: {nombreCompleto} (ID: {id})"
                );
            }

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}