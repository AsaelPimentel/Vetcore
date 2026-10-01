using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class CatalogoService
    {
        // ==========================================
        // LECTURAS (INDEX Y DROPDOWNS)
        // ==========================================
        public async Task<List<Puestos>> ObtenerPuestosAsync()
        {
            using (var db = new VetcoreEntities())
                return await db.Puestos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
        }

        public async Task<List<Roles>> ObtenerRolesAsync()
        {
            using (var db = new VetcoreEntities())
                return await db.Roles.AsNoTracking().OrderBy(r => r.Nombre).ToListAsync();
        }

        public async Task<List<Especies>> ObtenerEspeciesAsync()
        {
            using (var db = new VetcoreEntities())
                return await db.Especies.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync();
        }

        public async Task<List<Razas>> ObtenerRazasAsync()
        {
            using (var db = new VetcoreEntities())
                return await db.Razas.Include(r => r.Especies).AsNoTracking().OrderBy(r => r.Especies.Nombre).ThenBy(r => r.Nombre).ToListAsync();
        }

        public async Task<List<Categorias>> ObtenerCategoriasAsync()
        {
            using (var db = new VetcoreEntities())
                return await db.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync();
        }

        public async Task<List<Jaulas>> ObtenerJaulasAsync()
        {
            using (var db = new VetcoreEntities())
                return await db.Jaulas.AsNoTracking().OrderBy(j => j.Codigo).ToListAsync();
        }

        // ==========================================
        // OBTENER POR ID (PARA MODAL EDITAR)
        // ==========================================
        public async Task<object> ObtenerRegistroPorIdAsync(string entidad, int id)
        {
            using (var db = new VetcoreEntities())
            {
                switch (entidad)
                {
                    case "Puestos": return await db.Puestos.FindAsync(id);
                    case "Roles": return await db.Roles.FindAsync(id);
                    case "Especies": return await db.Especies.FindAsync(id);
                    case "Razas": return await db.Razas.FindAsync(id);
                    case "Categorias": return await db.Categorias.FindAsync(id);
                    case "Jaulas": return await db.Jaulas.FindAsync(id);
                    default: return null;
                }
            }
        }

        // ==========================================
        // CREACIONES
        // ==========================================
        public async Task CrearPuestoAsync(string nombre, int nivelAcceso)
        {
            using (var db = new VetcoreEntities())
            {
                db.Puestos.Add(new Puestos { Nombre = nombre, NivelAcceso = nivelAcceso });
                await db.SaveChangesAsync();
            }
        }

        public async Task CrearRolAsync(string nombre, string descripcion)
        {
            using (var db = new VetcoreEntities())
            {
                db.Roles.Add(new Roles { Nombre = nombre, Descripcion = descripcion });
                await db.SaveChangesAsync();
            }
        }

        public async Task CrearEspecieAsync(string nombre)
        {
            using (var db = new VetcoreEntities())
            {
                db.Especies.Add(new Especies { Nombre = nombre });
                await db.SaveChangesAsync();
            }
        }

        public async Task CrearRazaAsync(string nombre, int idEspecie)
        {
            using (var db = new VetcoreEntities())
            {
                db.Razas.Add(new Razas { Nombre = nombre, IdEspecie = idEspecie });
                await db.SaveChangesAsync();
            }
        }

        public async Task CrearCategoriaAsync(string nombre, bool esServicio)
        {
            using (var db = new VetcoreEntities())
            {
                db.Categorias.Add(new Categorias { Nombre = nombre, EsServicio = esServicio });
                await db.SaveChangesAsync();
            }
        }

        public async Task CrearJaulaAsync(string codigo, string tamano)
        {
            using (var db = new VetcoreEntities())
            {
                db.Jaulas.Add(new Jaulas { Codigo = codigo, Tamano = tamano, Ocupada = false });
                await db.SaveChangesAsync();
            }
        }

        // ==========================================
        // ACTUALIZACIONES
        // ==========================================
        public async Task ActualizarPuestoAsync(int id, string nombre, int nivelAcceso)
        {
            using (var db = new VetcoreEntities())
            {
                var item = await db.Puestos.FindAsync(id);
                if (item != null) { item.Nombre = nombre; item.NivelAcceso = nivelAcceso; await db.SaveChangesAsync(); }
            }
        }

        public async Task ActualizarRolAsync(int id, string nombre, string descripcion)
        {
            using (var db = new VetcoreEntities())
            {
                var item = await db.Roles.FindAsync(id);
                if (item != null) { item.Nombre = nombre; item.Descripcion = descripcion; await db.SaveChangesAsync(); }
            }
        }

        public async Task ActualizarEspecieAsync(int id, string nombre)
        {
            using (var db = new VetcoreEntities())
            {
                var item = await db.Especies.FindAsync(id);
                if (item != null) { item.Nombre = nombre; await db.SaveChangesAsync(); }
            }
        }

        public async Task ActualizarRazaAsync(int id, string nombre, int idEspecie)
        {
            using (var db = new VetcoreEntities())
            {
                var item = await db.Razas.FindAsync(id);
                if (item != null) { item.Nombre = nombre; item.IdEspecie = idEspecie; await db.SaveChangesAsync(); }
            }
        }

        public async Task ActualizarCategoriaAsync(int id, string nombre, bool esServicio)
        {
            using (var db = new VetcoreEntities())
            {
                var item = await db.Categorias.FindAsync(id);
                if (item != null) { item.Nombre = nombre; item.EsServicio = esServicio; await db.SaveChangesAsync(); }
            }
        }

        public async Task ActualizarJaulaAsync(int id, string codigo, string tamano)
        {
            using (var db = new VetcoreEntities())
            {
                var item = await db.Jaulas.FindAsync(id);
                if (item != null) { item.Codigo = codigo; item.Tamano = tamano; await db.SaveChangesAsync(); }
            }
        }
    }
}