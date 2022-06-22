using System.Security.Cryptography;
using System.Text;

namespace _Scripts.Utils
{
    public static class CryptoUtils
    {
        public static byte[] Sha256(string input)
        {
            using SHA256 mySHA256 = SHA256.Create();
            var inputArr = Encoding.UTF8.GetBytes(input);
            return mySHA256.ComputeHash(inputArr);
        }

        public static string Base64(byte[] input)
        {
            return System.Convert.ToBase64String(input);
        }
    }
}