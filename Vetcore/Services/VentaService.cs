using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class VentaService
    {
        public async Task<int> ProcesarVentaCompletaAsync(int idEmpleado, decimal subtotal, decimal total, string metodoPago, int? idPropietario, List<VentasDetalle> carrito)
        {
            // TransactionScope garantiza que toda la transacción sea atómica (Todo o Nada)
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var db = new VetcoreEntities())
                {
                    // 1. Obtener la sesión abierta del empleado actual
                    var cajaAbierta = await db.CajasSesion
                        .FirstOrDefaultAsync(c => c.IdEmpleado == idEmpleado && c.Estado == "Abierta");

                    if (cajaAbierta == null)
                    {
                        throw new InvalidOperationException("No tienes una sesión de caja abierta.");
                    }

                    // 2. Crear la cabecera de la venta
                    var nuevaVenta = new Ventas
                    {
                        IdSesion = cajaAbierta.IdSesion,
                        IdPropietario = idPropietario,
                        FechaHora = DateTime.Now,
                        Subtotal = subtotal,
                        Impuestos = 0,
                        Total = total,
                        MetodoPago = metodoPago
                    };

                    db.Ventas.Add(nuevaVenta);
                    await db.SaveChangesAsync(); // Genera el IdVenta

                    // 3. Procesar el detalle de la venta y el inventario
                    foreach (var item in carrito)
                    {
                        var nuevoDetalle = new VentasDetalle
                        {
                            IdVenta = nuevaVenta.IdVenta,
                            IdProducto = item.IdProducto,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.PrecioUnitario,
                            Subtotal = item.Subtotal
                        };
                        db.VentasDetalle.Add(nuevoDetalle);

                        // Descontar inventario físico y registrar en Kardex
                        var productoBD = await db.Productos.FindAsync(item.IdProducto);

                        if (productoBD != null && productoBD.StockActual.HasValue)
                        {
                            // --- INICIO CONTROL DE CONCURRENCIA ---
                            await db.Entry(productoBD).ReloadAsync();

                            if (productoBD.StockActual.Value < item.Cantidad)
                            {
                                throw new InvalidOperationException($"El producto '{productoBD.Nombre}' se acaba de agotar o no tiene stock suficiente. Disponible actual: {productoBD.StockActual.Value}");
                            }
                            // --- FIN CONTROL DE CONCURRENCIA ---

                            int stockAnterior = productoBD.StockActual.Value;

                            // SINTAXIS CORREGIDA: Se agregó el operador de resta (-)
                            int stockResultante = stockAnterior - (int)item.Cantidad;

                            productoBD.StockActual = stockResultante;
                            db.Entry(productoBD).State = EntityState.Modified;

                            var nuevoKardex = new KardexInventario
                            {
                                IdProducto = item.IdProducto,
                                IdEmpleado = idEmpleado,
                                TipoMovimiento = "VentaPOS",
                                Cantidad = (int)item.Cantidad,
                                StockResultante = stockResultante,
                                FechaHora = DateTime.Now,
                                Motivo = $"Venta de mostrador (Ticket #{nuevaVenta.IdVenta})"
                            };
                            db.KardexInventario.Add(nuevoKardex);
                        }
                    }

                    await db.SaveChangesAsync();
                    scope.Complete(); // Confirmamos la transacción global

                    return nuevaVenta.IdVenta;
                }
            }
        }

        // ====================================================
        // NUEVOS MÉTODOS PARA DESACOPLAR EL CONTROLADOR
        // ====================================================

        public async Task<CajasSesion> ObtenerCajaAbiertaAsync(int idEmpleado)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.CajasSesion
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdEmpleado == idEmpleado && c.Estado == "Abierta");
            }
        }

        public async Task AbrirCajaAsync(int idEmpleado, decimal montoApertura)
        {
            using (var db = new VetcoreEntities())
            {
                var nuevaCaja = new CajasSesion
                {
                    IdEmpleado = idEmpleado,
                    FechaApertura = DateTime.Now,
                    MontoApertura = montoApertura,
                    Estado = "Abierta"
                };

                db.CajasSesion.Add(nuevaCaja);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Ventas> ObtenerVentaConDetallesAsync(int idVenta)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Ventas
                    .Include(v => v.VentasDetalle.Select(d => d.Productos))
                    .Include(v => v.CajasSesion.Empleados)
                    .Include(v => v.Propietarios)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
            }
        }

        public async Task<List<Ventas>> ObtenerVentasPorSesionAsync(int idSesion)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Ventas
                    .AsNoTracking()
                    .Where(v => v.IdSesion == idSesion)
                    .ToListAsync();
            }
        }

        public async Task CerrarCajaAsync(int idSesion, decimal montoCierre)
        {
            using (var db = new VetcoreEntities())
            {
                var caja = await db.CajasSesion.FindAsync(idSesion);

                if (caja == null || caja.Estado != "Abierta")
                    throw new Exception("La sesión de caja no es válida o ya fue cerrada.");

                caja.FechaCierre = DateTime.Now;
                caja.MontoCierre = montoCierre;
                caja.Estado = "Cerrada";

                db.Entry(caja).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }
    }
}