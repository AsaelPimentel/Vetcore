using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vetcore.Filters;
using Vetcore.Helpers;
using Vetcore.Models;
using Vetcore.Services;

namespace Vetcore.Controllers
{
    [Authorize] // Asegura que solo personal autenticado (con cookie válida) pueda operar el POS
    public class VentasController : Controller
    {
        // Inyectamos todos los servicios que necesitamos
        private readonly VentaService _ventaService = new VentaService();
        private readonly ProductoService _productoService = new ProductoService();
        private readonly PropietarioService _propietarioService = new PropietarioService();

        // ELIMINADO: Método ObtenerIdEmpleadoActual() que dependía de Session

        public async Task<ActionResult> Index()
        {
            // MIGRACIÓN A CLAIMS: Lectura segura de la cookie
            int idEmpleado = HttpContext.ObtenerIdEmpleado();

            var cajaAbierta = await _ventaService.ObtenerCajaAbiertaAsync(idEmpleado);

            if (cajaAbierta != null)
            {
                ViewBag.CajaAbierta = true;
                ViewBag.IdSesion = cajaAbierta.IdSesion;
                ViewBag.MontoApertura = cajaAbierta.MontoApertura;
                ViewBag.FechaApertura = cajaAbierta.FechaApertura;

                // Reutilizamos la lógica limpia del ProductoService
                ViewBag.CatalogoProductos = await _productoService.ObtenerInventarioCompletoAsync();
                ViewBag.ListaCategorias = await _productoService.ObtenerCategoriasAsync();

                // Reutilizamos la lógica limpia del PropietarioService
                var propietarios = await _propietarioService.ObtenerPropietariosAsync("");
                ViewBag.ListaClientes = propietarios.OrderBy(p => p.Nombre).ToList();
            }
            else
            {
                ViewBag.CajaAbierta = false;
            }

            return View();
        }

        public ActionResult AbrirCaja()
        {
            return PartialView("_AbrirCajaModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CORRECTO: Es un formulario tradicional HTML de 1 campo
        public async Task<ActionResult> AbrirCaja(decimal? MontoApertura)
        {
            try
            {
                int idEmpleado = HttpContext.ObtenerIdEmpleado();

                var cajaExistente = await _ventaService.ObtenerCajaAbiertaAsync(idEmpleado);
                if (cajaExistente != null)
                {
                    return Json(new { success = false, message = "Ya tienes una caja abierta." });
                }

                await _ventaService.AbrirCajaAsync(idEmpleado, MontoApertura ?? 0);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al abrir la caja: " + ex.Message });
            }
        }

        [HttpPost]
        // ATENCIÓN: SIN ValidateAntiForgeryToken porque recibe JSON (application/json) desde JavaScript
        public async Task<ActionResult> ProcesarVenta(decimal subtotal, decimal total, string metodoPago, int? idPropietario, List<VentasDetalle> carrito)
        {
            try
            {
                int idEmpleado = HttpContext.ObtenerIdEmpleado();

                int idVentaGenerada = await _ventaService.ProcesarVentaCompletaAsync(idEmpleado, subtotal, total, metodoPago, idPropietario, carrito);

                return Json(new { success = true, idVenta = idVentaGenerada });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al procesar la venta: " + ex.Message });
            }
        }

        public async Task<ActionResult> ImprimirTicket(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var venta = await _ventaService.ObtenerVentaConDetallesAsync(id.Value);

            if (venta == null) return HttpNotFound();

            return View(venta);
        }

        public async Task<ActionResult> CorteCaja()
        {
            int idEmpleado = HttpContext.ObtenerIdEmpleado();

            var cajaAbierta = await _ventaService.ObtenerCajaAbiertaAsync(idEmpleado);
            if (cajaAbierta == null) return HttpNotFound();

            var ventasSesion = await _ventaService.ObtenerVentasPorSesionAsync(cajaAbierta.IdSesion);

            decimal totalVentas = ventasSesion.Sum(v => v.Total);
            decimal ventasEfectivo = ventasSesion.Where(v => v.MetodoPago == "Efectivo").Sum(v => v.Total);
            decimal ventasTarjeta = ventasSesion.Where(v => v.MetodoPago == "Tarjeta").Sum(v => v.Total);

            decimal fondoInicial = cajaAbierta.MontoApertura;
            decimal efectivoEsperado = fondoInicial + ventasEfectivo;

            ViewBag.FondoInicial = fondoInicial;
            ViewBag.VentasEfectivo = ventasEfectivo;
            ViewBag.VentasTarjeta = ventasTarjeta;
            ViewBag.TotalVentas = totalVentas;
            ViewBag.EfectivoEsperado = efectivoEsperado;

            return PartialView("_CorteCajaModal", cajaAbierta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CORRECTO: Es un formulario HTML con form.serialize()
        public async Task<ActionResult> CerrarCaja(int IdSesion, decimal MontoCierre)
        {
            try
            {
                await _ventaService.CerrarCajaAsync(IdSesion, MontoCierre);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cerrar la caja: " + ex.Message });
            }
        }
    }
}