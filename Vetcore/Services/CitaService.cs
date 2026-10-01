using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class CitaService
    {
        public async Task<List<Citas>> ObtenerCitasAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Citas
                    .Include(c => c.Mascotas)
                    .Include(c => c.Empleados)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<Citas> ObtenerPorIdAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Citas
                    .Include(c => c.Mascotas)
                    .Include(c => c.Empleados)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdCita == id);
            }
        }

        public async Task CrearCitaAsync(Citas cita)
        {
            using (var db = new VetcoreEntities())
            {
                if (string.IsNullOrEmpty(cita.Estado)) cita.Estado = "Pendiente";

                db.Citas.Add(cita);
                await db.SaveChangesAsync();
            }
        }

        public async Task ActualizarCitaAsync(Citas cita)
        {
            using (var db = new VetcoreEntities())
            {
                db.Entry(cita).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        public async Task EliminarCitaAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                var cita = await db.Citas.FindAsync(id);
                if (cita != null)
                {
                    db.Citas.Remove(cita);
                    await db.SaveChangesAsync();
                }
            }
        }

        // ====================================================
        // MÉTODOS PARA LLENAR LOS COMBOS (DROPDOWNS)
        // ====================================================

        public async Task<List<Mascotas>> ObtenerMascotasDropdownAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Mascotas
                    .Include(m => m.Propietarios)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<List<Empleados>> ObtenerEmpleadosActivosAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Empleados
                    .Where(e => e.Activo == true)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }
    }
}