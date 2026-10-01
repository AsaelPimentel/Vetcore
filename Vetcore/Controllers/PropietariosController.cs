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
    [Authorize]
    public class PropietariosController : Controller
    {
        private readonly PropietarioService _propietarioService = new PropietarioService();

        public async Task<ActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 8;

            // 1. Pedimos la lista al servicio
            var propietariosList = await _propietarioService.ObtenerPropietariosAsync(search);

            // 2. Realizamos la paginación de la lista obtenida
            int totalRecords = propietariosList.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var propietariosPaginated = propietariosList
                .Skip((page - 1) * pageSize) // CORRECCIÓN: Error de compilación en el cálculo de paginación
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages == 0 ? 1 : totalPages;
            ViewBag.SearchTerm = search;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_TablaPropietarios", propietariosPaginated);
            }

            return View(propietariosPaginated);
        }

        public ActionResult Create()
        {
            return PartialView("_CreateModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Propietarios propietario)
        {
            if (ModelState.IsValid)
            {
                await _propietarioService.CrearPropietarioAsync(propietario);
                return Json(new { success = true });
            }

            return PartialView("_CreateModal", propietario);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var propietario = await _propietarioService.ObtenerPorIdAsync(id.Value);
            if (propietario == null) return HttpNotFound();

            return PartialView("_EditModal", propietario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Propietarios propietario)
        {
            if (ModelState.IsValid)
            {
                await _propietarioService.ActualizarPropietarioAsync(propietario);
                return Json(new { success = true });
            }
            return PartialView("_EditModal", propietario);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var propietario = await _propietarioService.ObtenerPorIdAsync(id.Value);
            if (propietario == null) return HttpNotFound();

            return PartialView("_DetailsModal", propietario);
        }

        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var propietario = await _propietarioService.ObtenerPorIdAsync(id.Value);
            if (propietario == null) return HttpNotFound();

            return PartialView("_DeleteModal", propietario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            // Primero consultamos al propietario para registrar su nombre en auditoría
            var propietario = await _propietarioService.ObtenerPorIdAsync(id);
            if (propietario != null)
            {
                string nombreCliente = $"{propietario.Nombre} {propietario.Apellidos}";

                // Procedemos a eliminar
                await _propietarioService.EliminarPropietarioAsync(id);

                // MIGRACIÓN A CLAIMS: Leer ID del empleado autenticado de forma segura
                int idEmpleadoActual = HttpContext.ObtenerIdEmpleado();
                await AuditoriaService.RegistrarAccionAsync(
                    idEmpleadoActual,
                    "ELIMINAR_CLIENTE",
                    $"Se eliminó permanentemente al cliente: {nombreCliente} (ID: {id})"
                );
            }
            return Json(new { success = true });
        }
    }
}