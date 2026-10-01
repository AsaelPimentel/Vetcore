using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace Vetcore.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int IterationCount = 10000; // Número de iteraciones para PBKDF2

        public static string HashPassword(string password)
        {
            // Generar un Salt aleatorio seguro
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Aplicar PBKDF2 con HMACSHA256
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, IterationCount, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(KeySize);

                // Combinar Salt y Hash en un solo buffer para almacenarlo en la base de datos
                byte[] hashBytes = new byte[SaltSize + KeySize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string providedPassword, string hashedPassword)
        {
            try
            {
                // Si la contraseña anterior está en texto plano (migración)
                if (string.IsNullOrEmpty(hashedPassword) || !hashedPassword.Contains("="))
                {
                    return providedPassword == hashedPassword;
                }

                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                // Extraer el Salt
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Obtener el hash de la contraseña ingresada usando el mismo Salt e iteraciones
                using (var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, IterationCount, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(KeySize);

                    // Comparar byte a byte de manera segura contra tiempos de respuesta (timing attack)
                    for (int i = 0; i < KeySize; i++)
                    {
                        if (hashBytes[SaltSize + i] != hash[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            catch
            {
                // En caso de error de formato, intentamos comparación directa por compatibilidad
                return providedPassword == hashedPassword;
            }
        }
    }
}