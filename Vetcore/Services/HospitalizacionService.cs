using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class HospitalizacionService
    {
        public async Task<List<Jaulas>> ObtenerTodasLasJaulasAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Jaulas.AsNoTracking().ToListAsync();
            }
        }

        public async Task<List<Hospitalizaciones>> ObtenerHospitalizacionesActivasAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Hospitalizaciones
                    .Include(h => h.Expedientes.Mascotas.Propietarios)
                    .Include(h => h.Jaulas)
                    .Include(h => h.Empleados)
                    .AsNoTracking()
                    .Where(h => h.Estado == "Internado")
                    .ToListAsync();
            }
        }

        public async Task<List<Expedientes>> ObtenerExpedientesParaIngresoAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Expedientes
                    .Include(e => e.Mascotas)
                    .AsNoTracking()
                    .Where(e => e.Mascotas.Fallecido != true)
                    .ToListAsync();
            }
        }

        public async Task<List<Jaulas>> ObtenerJaulasLibresAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Jaulas
                    .AsNoTracking()
                    .Where(j => j.Ocupada != true)
                    .ToListAsync();
            }
        }

        public async Task IngresarPacienteAsync(Hospitalizaciones modelo)
        {
            using (var db = new VetcoreEntities())
            {
                modelo.FechaIngreso = DateTime.Now;
                modelo.Estado = "Internado";

                db.Hospitalizaciones.Add(modelo);

                var jaula = await db.Jaulas.FindAsync(modelo.IdJaula);
                if (jaula != null)
                {
                    jaula.Ocupada = true;
                    db.Entry(jaula).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task<Hospitalizaciones> ObtenerHospitalizacionConDetallesAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Hospitalizaciones
                    .Include(h => h.Expedientes.Mascotas.Propietarios)
                    .Include(h => h.Expedientes.Mascotas.Razas)
                    .Include(h => h.Jaulas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.IdHospitalizacion == id);
            }
        }

        public async Task<List<HospitalizacionNotas>> ObtenerNotasMedicasAsync(int idHospitalizacion)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.HospitalizacionNotas
                    .Include(n => n.Empleados)
                    .AsNoTracking()
                    .Where(n => n.IdHospitalizacion == idHospitalizacion)
                    .OrderByDescending(n => n.FechaHora)
                    .ToListAsync();
            }
        }

        public async Task<List<Productos>> ObtenerProductosActivosAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Productos
                    .AsNoTracking()
                    .Where(p => p.Activo == true)
                    .ToListAsync();
            }
        }

        public async Task<bool> AgregarNotaYConsumoAsync(int idHospitalizacion, string notasMedicas, int? idProductoConsumo, decimal? cantidadConsumo, int idEmpleadoActual)
        {
            using (var db = new VetcoreEntities())
            {
                var hosp = await db.Hospitalizaciones
                    .Include(h => h.Expedientes.Mascotas)
                    .FirstOrDefaultAsync(h => h.IdHospitalizacion == idHospitalizacion);

                if (hosp == null) return false;

                var nuevaNota = new HospitalizacionNotas
                {
                    IdHospitalizacion = idHospitalizacion,
                    IdEmpleado = idEmpleadoActual,
                    FechaHora = DateTime.Now,
                    NotasMedicas = notasMedicas ?? "Sin notas adicionales."
                };

                if (idProductoConsumo.HasValue && cantidadConsumo.HasValue && cantidadConsumo.Value > 0)
                {
                    var producto = await db.Productos.FindAsync(idProductoConsumo.Value);
                    if (producto != null)
                    {
                        decimal subtotalConsumo = producto.PrecioVenta * cantidadConsumo.Value;
                        nuevaNota.ConceptoConsumo = $"{cantidadConsumo.Value}x {producto.Nombre}";
                        nuevaNota.CostoConsumo = subtotalConsumo;

                        if (producto.StockActual != null)
                        {
                            producto.StockActual -= (int)cantidadConsumo.Value;
                            db.Entry(producto).State = EntityState.Modified;

                            db.KardexInventario.Add(new KardexInventario
                            {
                                IdProducto = producto.IdProducto,
                                IdEmpleado = idEmpleadoActual,
                                FechaHora = DateTime.Now,
                                TipoMovimiento = "UsoClinico",
                                Cantidad = (int)cantidadConsumo.Value,
                                StockResultante = producto.StockActual.Value,
                                Motivo = $"Consumo en Hospitalización - Paciente ID {hosp.Expedientes.IdMascota}"
                            });
                        }
                    }
                }

                db.HospitalizacionNotas.Add(nuevaNota);
                await db.SaveChangesAsync();

                return true;
            }
        }

        public async Task<bool> DarAltaAsync(int idHospitalizacion, string diagnosticoSalida, int idEmpleadoActual)
        {
            using (var db = new VetcoreEntities())
            {
                var hosp = await db.Hospitalizaciones.FindAsync(idHospitalizacion);
                if (hosp == null) return false;

                hosp.Estado = "Alta";
                hosp.FechaAlta = DateTime.Now;

                db.HospitalizacionNotas.Add(new HospitalizacionNotas
                {
                    IdHospitalizacion = idHospitalizacion,
                    IdEmpleado = idEmpleadoActual,
                    FechaHora = DateTime.Now,
                    NotasMedicas = "ALTA MÉDICA: " + diagnosticoSalida
                });

                db.Entry(hosp).State = EntityState.Modified;

                var jaula = await db.Jaulas.FindAsync(hosp.IdJaula);
                if (jaula != null)
                {
                    jaula.Ocupada = false;
                    db.Entry(jaula).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                return true;
            }
        }
    }
}