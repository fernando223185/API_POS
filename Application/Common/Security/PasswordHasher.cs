using System.Security.Cryptography;
using System.Text;

namespace Application.Common.Security
{
    public static class PasswordHasher
    {
        public static byte[] HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static bool VerifyPassword(string password, byte[] hashedPassword)
        {
            var hashToVerify = HashPassword(password);
            return CompareHashes(hashToVerify, hashedPassword);
        }

        private static bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }
            return true;
        }
    }
}