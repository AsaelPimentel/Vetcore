using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vetcore.Models;
using Vetcore.Filters;
using Vetcore.Services;
using Vetcore.Helpers;

namespace Vetcore.Controllers
{
    [AuthorizeRol(RolesSistema.Admin)]
    public class CatalogosController : Controller
    {
        private readonly CatalogoService _catalogoService = new CatalogoService();

<<<<<<< HEAD
=======
        private int ObtenerIdEmpleadoActual()
        {
            return Session["IdEmpleado"] != null ? (int)Session["IdEmpleado"] : 1;
        }

>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        public async Task<ActionResult> Index()
        {
            ViewBag.Puestos = await _catalogoService.ObtenerPuestosAsync();
            ViewBag.Roles = await _catalogoService.ObtenerRolesAsync();
            ViewBag.Especies = await _catalogoService.ObtenerEspeciesAsync();
            ViewBag.Razas = await _catalogoService.ObtenerRazasAsync();
            ViewBag.Categorias = await _catalogoService.ObtenerCategoriasAsync();
            ViewBag.Jaulas = await _catalogoService.ObtenerJaulasAsync();

            return View();
        }

        public async Task<ActionResult> Crear(string entidad)
        {
            ViewBag.Entidad = entidad;

            if (entidad == "Razas")
            {
                var especies = await _catalogoService.ObtenerEspeciesAsync();
                ViewBag.IdEspecie = new SelectList(especies, "IdEspecie", "Nombre");
            }

            return PartialView("_CrearCatalogoModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(string entidad, FormCollection form)
        {
            try
            {
                string nombreItem = form["Nombre"] ?? form["Codigo"] ?? "Desconocido";

                switch (entidad)
                {
                    case "Puestos":
                        await _catalogoService.CrearPuestoAsync(form["Nombre"], int.Parse(form["NivelAcceso"] ?? "1"));
                        break;
                    case "Roles":
                        await _catalogoService.CrearRolAsync(form["Nombre"], form["Descripcion"]);
                        break;
                    case "Especies":
                        await _catalogoService.CrearEspecieAsync(form["Nombre"]);
                        break;
                    case "Razas":
                        await _catalogoService.CrearRazaAsync(form["Nombre"], int.Parse(form["IdEspecie"]));
                        break;
                    case "Categorias":
                        await _catalogoService.CrearCategoriaAsync(form["Nombre"], form["EsServicio"] != null);
                        break;
                    case "Jaulas":
                        await _catalogoService.CrearJaulaAsync(form["Codigo"], form["Tamano"]);
                        break;
                    default:
                        return Json(new { success = false, message = "Entidad no válida." });
                }

<<<<<<< HEAD
                // MIGRACIÓN A CLAIMS: Usar la extensión HttpContext en lugar de Session
                int idAdmin = HttpContext.ObtenerIdEmpleado();
=======
                int idAdmin = ObtenerIdEmpleadoActual();
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
                await AuditoriaService.RegistrarAccionAsync(
                    idAdmin,
                    "CREAR_CATALOGO",
                    $"Se creó un nuevo registro en el catálogo '{entidad}': {nombreItem}"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar: " + ex.Message });
            }
        }

        public async Task<ActionResult> Editar(string entidad, int id)
        {
            ViewBag.Entidad = entidad;

            object modelo = await _catalogoService.ObtenerRegistroPorIdAsync(entidad, id);

            if (modelo == null) return HttpNotFound();

            if (entidad == "Razas")
            {
                var especies = await _catalogoService.ObtenerEspeciesAsync();
                ViewBag.IdEspecie = new SelectList(especies, "IdEspecie", "Nombre", ((Razas)modelo).IdEspecie);
            }

            return PartialView("_EditarCatalogoModal", modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(string entidad, int id, FormCollection form)
        {
            try
            {
                string nombreItem = form["Nombre"] ?? form["Codigo"] ?? "ID " + id;

                switch (entidad)
                {
                    case "Puestos":
                        await _catalogoService.ActualizarPuestoAsync(id, form["Nombre"], int.Parse(form["NivelAcceso"] ?? "1"));
                        break;
                    case "Roles":
                        await _catalogoService.ActualizarRolAsync(id, form["Nombre"], form["Descripcion"]);
                        break;
                    case "Especies":
                        await _catalogoService.ActualizarEspecieAsync(id, form["Nombre"]);
                        break;
                    case "Razas":
                        await _catalogoService.ActualizarRazaAsync(id, form["Nombre"], int.Parse(form["IdEspecie"]));
                        break;
                    case "Categorias":
                        await _catalogoService.ActualizarCategoriaAsync(id, form["Nombre"], form["EsServicio"] != null);
                        break;
                    case "Jaulas":
                        await _catalogoService.ActualizarJaulaAsync(id, form["Codigo"], form["Tamano"]);
                        break;
                    default:
                        return Json(new { success = false, message = "Entidad no válida." });
                }

<<<<<<< HEAD
                // MIGRACIÓN A CLAIMS: Usar la extensión HttpContext en lugar de Session
                int idAdmin = HttpContext.ObtenerIdEmpleado();
=======
                int idAdmin = ObtenerIdEmpleadoActual();
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
                await AuditoriaService.RegistrarAccionAsync(
                    idAdmin,
                    "MODIFICAR_CATALOGO",
                    $"Se actualizó el registro en el catálogo '{entidad}' (ID: {id}): {nombreItem}"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar: " + ex.Message });
            }
        }
    }
}