using System.Security.Cryptography;
using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;

namespace PortLog.Helpers
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int Parallelism = 4;
        private const int Threads = 4;
        private const int Iterations = 4;
        private const int MemorySize = 1024 * 64; // 64 MB

        public static string Hash(string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            var config = new Argon2Config
            {
                Type = Argon2Type.HybridAddressing,
                Version = Argon2Version.Nineteen,
                TimeCost = Iterations,
                MemoryCost = MemorySize,
                Lanes = Parallelism,
                Threads = Threads,
                Password = passwordBytes,
                Salt = GenerateSalt(SaltSize),
                HashLength = HashSize
            };

            var argon2 = new Argon2(config);
            using SecureArray<byte> hash = argon2.Hash();

            return config.EncodeString(hash.Buffer);
        }

        private static byte[] GenerateSalt(int saltSize)
        {
            byte[] salt = new byte[saltSize];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        public static bool Verify(string password, string hash)
        {
            try
            {
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                return Argon2.Verify(hash, passwordBytes, threads: Threads);
            }
            catch
            {
                return false;
            }
        }
    }
}
