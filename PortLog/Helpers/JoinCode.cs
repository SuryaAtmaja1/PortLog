using System.Security.Cryptography;
using System.Text;

namespace PortLog.Helpers
{
    public static class JoinCode
    {
        private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        private const int CodeLength = 7;

        public static string CreateFromGuid(Guid companyId)
        {
            // Convert GUID to bytes and hash
            byte[] guidBytes = companyId.ToByteArray();
            byte[] hash = SHA256.HashData(guidBytes);

            // Convert hash to join code
            var result = new char[CodeLength];
            for (int i = 0; i < CodeLength; i++)
            {
                result[i] = Characters[hash[i] % Characters.Length];
            }

            return new string(result);
        }

        [Obsolete("Use CreateFromGuid instead to ensure uniqueness")]
        public static string CreateRandom()
        {
            var data = new byte[CodeLength];
            RandomNumberGenerator.Fill(data);

            var result = new char[CodeLength];
            for (int i = 0; i < CodeLength; i++)
            {
                result[i] = Characters[data[i] % Characters.Length];
            }

            return new string(result);
        }
    }
}