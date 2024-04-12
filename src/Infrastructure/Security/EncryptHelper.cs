using System.Security.Cryptography;
using System.Text;

namespace Roller.Infrastructure.Security
{
    public static class EncryptHelper
    {
        private static string GenerateMd5(byte[] bytes)
        {
            var buffer = MD5.HashData(bytes);
            var strBuilder = new StringBuilder();
            foreach (var item in buffer)
            {
                strBuilder.Append(item.ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public static string Md5Encrypt32(this string plainText, string salt)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentException($"{nameof(plainText)} is null or empty。", nameof(plainText));
            }

            if (string.IsNullOrEmpty(salt))
            {
                throw new ArgumentException($"{nameof(salt)} is null or empty。", nameof(salt));
            }

            var contentBytes = Encoding.UTF8.GetBytes(plainText + salt);
            return GenerateMd5(contentBytes);
        }
    }
}
