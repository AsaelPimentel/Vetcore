using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class PropietarioService
    {
        public async Task<List<Propietarios>> ObtenerPropietariosAsync(string search = "")
        {
            using (var db = new VetcoreEntities())
            {
                var query = db.Propietarios.AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Nombre.Contains(search) ||
                                             p.Apellidos.Contains(search) ||
                                             p.Telefono.Contains(search) ||
                                             p.Email.Contains(search));
                }

                return await query.OrderByDescending(p => p.IdPropietario).ToListAsync();
            }
        }

        public async Task<Propietarios> ObtenerPorIdAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Propietarios
                    .Include(p => p.Mascotas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdPropietario == id);
            }
        }

        public async Task CrearPropietarioAsync(Propietarios propietario)
        {
            using (var db = new VetcoreEntities())
            {
                propietario.FechaRegistro = DateTime.Now;
                db.Propietarios.Add(propietario);
                await db.SaveChangesAsync();
            }
        }

        public async Task ActualizarPropietarioAsync(Propietarios propietario)
        {
            using (var db = new VetcoreEntities())
            {
                db.Entry(propietario).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        public async Task EliminarPropietarioAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                var propietario = await db.Propietarios.FindAsync(id);
                if (propietario != null)
                {
                    db.Propietarios.Remove(propietario);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}