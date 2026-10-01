using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vetcore.Services;

namespace Vetcore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly DashboardService _dashboardService = new DashboardService();

        public async Task<ActionResult> Index()
        {
            DateTime hoy = DateTime.Today;
            DateTime ahora = DateTime.Now;

            ViewBag.TotalClientes = await _dashboardService.ObtenerTotalClientesAsync();

            ViewBag.TotalPacientes = await _dashboardService.ObtenerTotalPacientesActivosAsync();

            ViewBag.CitasHoy = await _dashboardService.ObtenerTotalCitasHoyAsync(hoy);

            var proximasCitas = await _dashboardService.ObtenerProximasCitasHoyAsync(hoy, ahora);

            ViewBag.ProductosStockBajoLista = await _dashboardService.ObtenerProductosStockBajoAsync();

            return View(proximasCitas);
        }
    }
}