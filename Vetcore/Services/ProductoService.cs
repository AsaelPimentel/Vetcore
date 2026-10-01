using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class ProductoService
    {
        public async Task<List<Productos>> ObtenerProductosAsync(string search = "")
        {
            using (var db = new VetcoreEntities())
            {
                var query = db.Productos.Include(p => p.Categorias).AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Nombre.Contains(search) ||
                                             p.Categorias.Nombre.Contains(search) ||
                                             (p.CodigoBarras != null && p.CodigoBarras.Contains(search)));
                }

                return await query.OrderBy(p => p.Nombre).ToListAsync();
            }
        }

        public async Task<Productos> ObtenerPorIdAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == id);
            }
        }

        public async Task CrearProductoAsync(Productos producto)
        {
            using (var db = new VetcoreEntities())
            {
                db.Productos.Add(producto);
                await db.SaveChangesAsync();
            }
        }

        public async Task ActualizarProductoAsync(Productos producto)
        {
            using (var db = new VetcoreEntities())
            {
                db.Entry(producto).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        // --- NUEVOS MÉTODOS AÑADIDOS ---

        public async Task<List<Categorias>> ObtenerCategoriasAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Categorias.AsNoTracking().ToListAsync();
            }
        }

        public async Task<List<Productos>> ObtenerInventarioCompletoAsync()
        {
            using (var db = new VetcoreEntities())
            {
                var productos = await db.Productos
                    .Include(p => p.Categorias)
                    .AsNoTracking()
                    .ToListAsync();

                return productos
                    .OrderBy(p => p.Categorias != null ? p.Categorias.Nombre : "Sin Categoría")
                    .ThenBy(p => p.Nombre)
                    .ToList();
            }
        }

        public async Task<bool> RegistrarMovimientoKardexAsync(int idProducto, int idEmpleado, string tipoMovimiento, int cantidad, string motivo)
        {
            using (var db = new VetcoreEntities())
            {
                var producto = await db.Productos.FindAsync(idProducto);
                if (producto == null) return false;

                bool esEntrada = tipoMovimiento == "CompraProveedor" ||
                                 tipoMovimiento == "DevolucionCliente" ||
                                 tipoMovimiento == "AjusteEntrada";

                int stockAnterior = producto.StockActual ?? 0;
                int stockNuevo = stockAnterior;

                if (esEntrada)
                {
                    stockNuevo += cantidad;
                }
                else
                {
                    stockNuevo -= cantidad;
                    if (stockNuevo < 0) stockNuevo = 0;
                }

                producto.StockActual = stockNuevo;

                var kardex = new KardexInventario
                {
                    IdProducto = idProducto,
                    IdEmpleado = idEmpleado,
                    TipoMovimiento = tipoMovimiento,
                    Cantidad = cantidad,
                    StockResultante = stockNuevo,
                    FechaHora = DateTime.Now,
                    Motivo = string.IsNullOrEmpty(motivo) ? $"Movimiento tipo: {tipoMovimiento}" : motivo
                };

                db.KardexInventario.Add(kardex);
                await db.SaveChangesAsync();

                await AuditoriaService.RegistrarAccionAsync(
                    idEmpleado,
                    "MOVIMIENTO_KARDEX",
                    $"Movimiento '{tipoMovimiento}' de {cantidad} unidades en producto ID: {idProducto}. Stock resultante: {stockNuevo}"
                );

                return true;
            }
        }

        public async Task<List<KardexInventario>> ObtenerKardexHistorialAsync(int? idCategoria, string tipoMovimiento, DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var db = new VetcoreEntities())
            {
                var kardexQuery = db.KardexInventario
                    .Include(k => k.Productos.Categorias)
                    .Include(k => k.Empleados)
                    .AsNoTracking()
                    .AsQueryable();

                if (idCategoria.HasValue)
                {
                    kardexQuery = kardexQuery.Where(k => k.Productos.IdCategoria == idCategoria.Value);
                }

                if (!string.IsNullOrEmpty(tipoMovimiento))
                {
                    kardexQuery = kardexQuery.Where(k => k.TipoMovimiento == tipoMovimiento);
                }

                if (fechaInicio.HasValue)
                {
                    kardexQuery = kardexQuery.Where(k => k.FechaHora >= fechaInicio.Value);
                }
                if (fechaFin.HasValue)
                {
                    var finDia = fechaFin.Value.Date.AddDays(1).AddSeconds(-1);
                    kardexQuery = kardexQuery.Where(k => k.FechaHora <= finDia);
                }

                return await kardexQuery
                    .OrderByDescending(k => k.FechaHora)
                    .ToListAsync();
            }
        }
    }
}