using System;
using System.Security.Cryptography;

namespace Auth
{
    static class Utils
    {
        internal static string Hash(string passw) {
            byte[] hashBytes = new byte[36];
            byte[] salt = new byte[16];

            using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider()) {
                cryptoServiceProvider.GetBytes(salt);

                using (Rfc2898DeriveBytes bytesDeriver = new Rfc2898DeriveBytes(passw, salt, 40000)) {
                    byte[] hash = bytesDeriver.GetBytes(20);

                    Array.Copy(salt, 0, hashBytes, 0, 16);
                    Array.Copy(hash, 0, hashBytes, 16, 20);
                }
            }

            return Convert.ToBase64String(hashBytes);
        }

        internal static bool CompHash(string passw, string savedPassw) {
            byte[] hashBytes = Convert.FromBase64String(savedPassw);
            byte[] salt = new byte[16];

            Array.Copy(hashBytes, 0, salt, 0, 16);

            using (Rfc2898DeriveBytes bytesDeriver = new Rfc2898DeriveBytes(passw, salt, 40000)) {
                byte[] hash = bytesDeriver.GetBytes(20);

                for (int i = 0; i < 20; i++) {
                    if (hashBytes[i + 16] != hash[i])
                        return false;
                }
            }

            return true;
        }
    }
}
