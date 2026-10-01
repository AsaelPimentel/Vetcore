using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class ExpedienteService
    {
        public async Task<Mascotas> ObtenerMascotaConDetallesAsync(int idMascota)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Mascotas
                    .Include(m => m.Propietarios)
                    .Include(m => m.Razas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.IdMascota == idMascota);
            }
        }

        public async Task<Expedientes> ObtenerOAsignarExpedienteAsync(int idMascota)
        {
            using (var db = new VetcoreEntities())
            {
                var expediente = await db.Expedientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.IdMascota == idMascota);

                if (expediente == null)
                {
                    expediente = new Expedientes
                    {
                        IdMascota = idMascota,
                        FechaCreacion = DateTime.Now
                    };
                    db.Expedientes.Add(expediente);
                    await db.SaveChangesAsync();
                }

                return expediente;
            }
        }

        public async Task<List<Consultas>> ObtenerHistorialConsultasAsync(int idExpediente)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Consultas
                    .Include(c => c.Empleados)
                    .AsNoTracking()
                    .Where(c => c.IdExpediente == idExpediente)
                    .OrderByDescending(c => c.Fecha)
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

        public async Task RegistrarConsultaAsync(Consultas consulta)
        {
            using (var db = new VetcoreEntities())
            {
                if (!consulta.Fecha.HasValue) consulta.Fecha = DateTime.Now;

                db.Consultas.Add(consulta);

                var expediente = await db.Expedientes
                       .Include(e => e.Mascotas)
                       .FirstOrDefaultAsync(e => e.IdExpediente == consulta.IdExpediente);

                if (expediente != null && consulta.PesoKg.HasValue)
                {
                    expediente.Mascotas.PesoActual = consulta.PesoKg.Value;
                }

                if (consulta.IdCita.HasValue)
                {
                    var citaAnclada = await db.Citas.FindAsync(consulta.IdCita.Value);
                    if (citaAnclada != null)
                    {
                        citaAnclada.Estado = "Atendida";
                    }
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Consultas>> ObtenerConsultasParaEvolucionAsync(int idMascota)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Consultas
                    .Include(c => c.Expedientes)
                    .AsNoTracking()
                    .Where(c => c.Expedientes.IdMascota == idMascota && c.Fecha != null && (c.PesoKg > 0 || c.Temperatura > 0))
                    .OrderBy(c => c.Fecha)
                    .ToListAsync();
            }
        }

        public async Task<Consultas> ObtenerConsultaConDetallesAsync(int idConsulta)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Consultas
                    .Include(c => c.Expedientes.Mascotas.Propietarios)
                    .Include(c => c.Empleados)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdConsulta == idConsulta);
            }
        }

        public async Task<Consultas> ObtenerConsultaPorIdAsync(int idConsulta)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Consultas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdConsulta == idConsulta);
            }
        }

        public async Task<bool> ActualizarConsultaAsync(Consultas modelo)
        {
            using (var db = new VetcoreEntities())
            {
                var consultaDb = await db.Consultas.FindAsync(modelo.IdConsulta);
                if (consultaDb == null) return false;

                if (consultaDb.Fecha.HasValue && consultaDb.Fecha.Value.Date != DateTime.Today)
                {
                    throw new InvalidOperationException("Esta consulta ya está bloqueada y puede ser editada.");
                }

                consultaDb.PesoKg = modelo.PesoKg;
                consultaDb.Temperatura = modelo.Temperatura;
                consultaDb.Sintomas = modelo.Sintomas;
                consultaDb.Diagnostico = modelo.Diagnostico;
                consultaDb.RecetaMedica = modelo.RecetaMedica;
                consultaDb.NotasAdicionales = modelo.NotasAdicionales;

                if (modelo.PesoKg.HasValue)
                {
                    var expediente = await db.Expedientes.Include(e => e.Mascotas).FirstOrDefaultAsync(e => e.IdExpediente == consultaDb.IdExpediente);
                    if (expediente != null) expediente.Mascotas.PesoActual = modelo.PesoKg.Value;
                }

                await db.SaveChangesAsync();
                return true;
            }
        }
    }
}