using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Vetcore.Models;
using Vetcore.Helpers;

namespace Vetcore.Services
{
    public class EmpleadoService
    {
        public async Task<List<Empleados>> ObtenerEmpleadosAsync(string search = "")
        {
            using (var db = new VetcoreEntities())
            {
                var query = db.Empleados.Include(e => e.Puestos).AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(e => e.Nombre.Contains(search) ||
                                             e.Apellidos.Contains(search) ||
                                             e.Especialidad.Contains(search) ||
                                             e.Puestos.Nombre.Contains(search));
                }

                return await query.OrderByDescending(e => e.IdEmpleado).ToListAsync();
            }
        }

        public async Task<Empleados> ObtenerPorIdAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Empleados
                    .Include(e => e.Puestos)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.IdEmpleado == id);
            }
        }

        public async Task<Usuarios> ObtenerUsuarioPorIdEmpleadoAsync(int idEmpleado)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.IdEmpleado == idEmpleado);
            }
        }

        public async Task<List<Puestos>> ObtenerPuestosAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Puestos.AsNoTracking().ToListAsync();
            }
        }

        public async Task<List<Roles>> ObtenerRolesAsync()
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Roles.AsNoTracking().OrderBy(r => r.Nombre).ToListAsync();
            }
        }

        public async Task CrearEmpleadoYUsuarioAsync(Empleados empleado, string username, string passwordHash, int? idRol)
        {
            using (var db = new VetcoreEntities())
            {
                empleado.Activo = true;
                db.Empleados.Add(empleado);
                await db.SaveChangesAsync();

                if (!string.IsNullOrEmpty(username) && idRol.HasValue)
                {
                    string passwordTemporal = !string.IsNullOrEmpty(passwordHash) ? passwordHash : "123456";

                    var usuario = new Usuarios
                    {
                        IdEmpleado = empleado.IdEmpleado,
                        IdRol = idRol.Value,
                        Username = username,
                        PasswordHash = PasswordHelper.HashPassword(passwordTemporal),
                        Activo = true
                    };
                    db.Usuarios.Add(usuario);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task ActualizarEmpleadoYUsuarioAsync(Empleados empleado, string username, string passwordHash, int? idRol)
        {
            using (var db = new VetcoreEntities())
            {
                db.Entry(empleado).State = EntityState.Modified;

                var usuarioExistente = await db.Usuarios.FirstOrDefaultAsync(u => u.IdEmpleado == empleado.IdEmpleado);

                if (!string.IsNullOrEmpty(username))
                {
                    if (usuarioExistente != null)
                    {
                        usuarioExistente.Username = username;
                        usuarioExistente.IdRol = idRol ?? usuarioExistente.IdRol;

                        if (!string.IsNullOrEmpty(passwordHash))
                        {
                            usuarioExistente.PasswordHash = PasswordHelper.HashPassword(passwordHash);
                        }
                        db.Entry(usuarioExistente).State = EntityState.Modified;
                    }
                    else if (idRol.HasValue)
                    {
                        string passwordTemporal = !string.IsNullOrEmpty(passwordHash) ? passwordHash : "123456";

                        var nuevoUsuario = new Usuarios
                        {
                            IdEmpleado = empleado.IdEmpleado,
                            IdRol = idRol.Value,
                            Username = username,
                            PasswordHash = PasswordHelper.HashPassword(passwordTemporal),
                            Activo = true
                        };
                        db.Usuarios.Add(nuevoUsuario);
                    }
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task<Empleados> EliminarEmpleadoYUsuarioAsync(int id)
        {
            using (var db = new VetcoreEntities())
            {
                var empleado = await db.Empleados.FindAsync(id);
                if (empleado != null)
                {
                    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.IdEmpleado == id);
                    if (usuario != null)
                    {
                        db.Usuarios.Remove(usuario);
                    }

                    db.Empleados.Remove(empleado);
                    await db.SaveChangesAsync();

                    return empleado;
                }
                return null;
            }
        }
    }
}