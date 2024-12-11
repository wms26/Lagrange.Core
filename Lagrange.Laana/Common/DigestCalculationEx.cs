using System.Security.Cryptography;
using System.Text;

namespace Lagrange.Laana.Common
{
    public static class DigestCalculationEx
    {
        public static string CalculateMd5(this string input)
        {
            byte[] md5 = MD5.HashData(
                Encoding.ASCII.GetBytes(input));
            return Convert.ToHexString(md5);
        }

        public static string CalculateMd5(this byte[] input)
        {
            byte[] md5 = MD5.HashData(input);
            return Convert.ToHexString(md5);
        }
    }
}