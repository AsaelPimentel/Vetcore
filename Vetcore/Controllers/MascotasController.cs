using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vetcore.Filters;
using Vetcore.Models;
using Vetcore.Services;
using Vetcore.Helpers;

namespace Vetcore.Controllers
{
    [Authorize]
    public class MascotasController : Controller
    {
        private readonly PacienteService _pacienteService = new PacienteService();

        public async Task<ActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 8;

            var mascotasList = await _pacienteService.ObtenerMascotasAsync(search);

            int totalRecords = mascotasList.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var mascotasPaginated = mascotasList
                .Skip((page - 1) * pageSize) // CORRECCIÓN: Se arregló el error de sintaxis de (page1)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages == 0 ? 1 : totalPages;
            ViewBag.SearchTerm = search;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_TablaMascotas", mascotasPaginated);
            }

            return View(mascotasPaginated);
        }

        public async Task<ActionResult> Create()
        {
            var propietarios = await _pacienteService.ObtenerPropietariosAsync();
            var razas = await _pacienteService.ObtenerRazasAsync();

            ViewBag.IdPropietario = new SelectList(propietarios, "IdPropietario", "Nombre");
            ViewBag.IdRaza = new SelectList(razas, "IdRaza", "Nombre");

            return PartialView("_CreateModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Mascotas mascota)
        {
            if (ModelState.IsValid)
            {
                await _pacienteService.CrearMascotaAsync(mascota);
                return Json(new { success = true });
            }

            var propietarios = await _pacienteService.ObtenerPropietariosAsync();
            var razas = await _pacienteService.ObtenerRazasAsync();

            ViewBag.IdPropietario = new SelectList(propietarios, "IdPropietario", "Nombre", mascota.IdPropietario);
            ViewBag.IdRaza = new SelectList(razas, "IdRaza", "Nombre", mascota.IdRaza);

            return PartialView("_CreateModal", mascota);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var mascota = await _pacienteService.ObtenerPorIdAsync(id.Value);
            if (mascota == null) return HttpNotFound();

            var propietarios = await _pacienteService.ObtenerPropietariosAsync();
            var razas = await _pacienteService.ObtenerRazasAsync();

            ViewBag.IdPropietario = new SelectList(propietarios, "IdPropietario", "Nombre", mascota.IdPropietario);
            ViewBag.IdRaza = new SelectList(razas, "IdRaza", "Nombre", mascota.IdRaza);

            return PartialView("_EditModal", mascota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Mascotas mascota)
        {
            if (ModelState.IsValid)
            {
                await _pacienteService.ActualizarMascotaAsync(mascota);
                return Json(new { success = true });
            }

            var propietarios = await _pacienteService.ObtenerPropietariosAsync();
            var razas = await _pacienteService.ObtenerRazasAsync();

            ViewBag.IdPropietario = new SelectList(propietarios, "IdPropietario", "Nombre", mascota.IdPropietario);
            ViewBag.IdRaza = new SelectList(razas, "IdRaza", "Nombre", mascota.IdRaza);

            return PartialView("_EditModal", mascota);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var mascota = await _pacienteService.ObtenerPorIdAsync(id.Value);
            if (mascota == null) return HttpNotFound();

            return PartialView("_DetailsModal", mascota);
        }

        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var mascota = await _pacienteService.ObtenerPorIdAsync(id.Value);
            if (mascota == null) return HttpNotFound();

            return PartialView("_DeleteModal", mascota);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var mascota = await _pacienteService.DesactivarMascotaAsync(id);

            if (mascota != null)
            {
                // MIGRACIÓN A CLAIMS: Leemos la identidad cifrada de la sesión activa
                int idEmpleadoActual = HttpContext.ObtenerIdEmpleado();
                await AuditoriaService.RegistrarAccionAsync(
                    idEmpleadoActual,
                    "DESACTIVAR_PACIENTE",
                    $"Se desactivó (borrado lógico) al paciente: {mascota.Nombre} (ID de mascota: {id})"
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