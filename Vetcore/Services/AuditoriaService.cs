using System;
using System.Threading.Tasks;
using System.Web;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class AuditoriaService
    {
        public static async Task RegistrarAccionAsync(int idEmpleado, string accion, string detalle)
        {
            try
            {
                using (var db = new VetcoreEntities())
                {
                    var ip = HttpContext.Current?.Request?.UserHostAddress ?? "Desconocida";

                    var log = new AuditoriaLogs
                    {
                        IdEmpleado = idEmpleado,
                        Accion = accion,
                        Detalle = detalle,
                        IpAddress = ip,
                        FechaHora = DateTime.Now
                    };

                    db.AuditoriaLogs.Add(log);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // El registro de auditoría no debe romper la aplicación principal si falla, 
                // pero puedes registrar el error en NLog si lo deseas.
                System.Diagnostics.Debug.WriteLine("Error al registrar auditoría: " + ex.Message);
            }
        }
    }
}