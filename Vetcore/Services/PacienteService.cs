using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class PacienteService
    {
        public async Task<List<Mascotas>> ObtenerMascotasAsync(string search = "")
        {
            using (var db = new VetcoreEntities())
            {
                var query = db.Mascotas
                              .Include(m => m.Propietarios)
                              .Include(m => m.Razas)
                              .AsNoTracking()
                              .Where(m => m.Activo == true)
                              .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(m => m.Nombre.Contains(search) ||
                                             m.Propietarios.Nombre.Contains(search) ||
                                             m.Propietarios.Apellidos.Contains(search) ||
                                             m.Razas.Nombre.Contains(search));
                }

                return await query.OrderByDescending(m => m.IdMascota).ToListAsync();
            }
        }

        public async Task<Mascotas> ObtenerPorIdAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Mascotas
                    .Include(m => m.Propietarios)
                    .Include(m => m.Razas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.IdMascota == id);
            }
        }

        public async Task CrearMascotaAsync(Mascotas mascota)
        {
            using (var db = new VetcoreEntities())
            {
                mascota.Fallecido = false;
                db.Mascotas.Add(mascota);
                await db.SaveChangesAsync();
            }
        }

        public async Task ActualizarMascotaAsync(Mascotas mascota)
        {
            using (var db = new VetcoreEntities())
            {
                db.Entry(mascota).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        public async Task<Mascotas> DesactivarMascotaAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                var mascota = await db.Mascotas.FindAsync(id);
                if (mascota != null)
                {
                    mascota.Activo = false;
                    db.Entry(mascota).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return mascota; // Retornamos la mascota para usar su nombre en la auditoría
                }
                return null;
            }
        }

        // ====================================================
        // MÉTODOS PARA LLENAR LOS COMBOS (DROPDOWNS)
        // ====================================================

        public async Task<List<Propietarios>> ObtenerPropietariosAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Propietarios.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
            }
        }

        public async Task<List<Razas>> ObtenerRazasAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Razas.AsNoTracking().OrderBy(r => r.Nombre).ToListAsync();
            }
        }
    }
}