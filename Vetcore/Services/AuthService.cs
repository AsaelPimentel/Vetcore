using System.Data.Entity;
using System.Threading.Tasks;
using Vetcore.Models;

namespace Vetcore.Services
{
    public class AuthService
    {
        public async Task<Usuarios> ObtenerUsuarioPorUsernameAsync(string username)
        {
            using (var db = new VetcoreEntities())
            {
                return await db.Usuarios
                    .Include(u => u.Empleados)
                    .Include(u => u.Roles)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == username && u.Activo == true);
            }
        }
    }
}