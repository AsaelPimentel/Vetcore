using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class DashboardService
    {
        public async Task<int> ObtenerTotalClientesAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Propietarios.AsNoTracking().CountAsync();
            }
        }

        public async Task<int> ObtenerTotalPacientesActivosAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Mascotas.AsNoTracking().CountAsync(m => m.Fallecido != true);
            }
        }

        public async Task<int> ObtenerTotalCitasHoyAsync(DateTime hoy)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Citas
                    .AsNoTracking()
                    .CountAsync(c => DbFunctions.TruncateTime(c.FechaHora) == hoy);
            }
        }

        public async Task<List<Citas>> ObtenerProximasCitasHoyAsync(DateTime hoy, DateTime ahora)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Citas
                    .Include(c => c.Mascotas)
                    .Include(c => c.Mascotas.Propietarios)
                    .Include(c => c.Empleados)
                    .AsNoTracking()
                    .Where(c => DbFunctions.TruncateTime(c.FechaHora) == hoy
                             && c.FechaHora >= ahora
                             && c.Estado == "Pendiente")
                    .OrderBy(c => c.FechaHora)
                    .Take(5)
                    .ToListAsync();
            }
        }

        public async Task<List<Productos>> ObtenerProductosStockBajoAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Productos
                    .AsNoTracking()
                    .Where(p => p.Activo == true && p.StockActual != null && p.StockMinimo != null && p.StockActual <= p.StockMinimo)
                    .OrderBy(p => p.StockActual)
                    .ToListAsync();
            }
        }
    }
}