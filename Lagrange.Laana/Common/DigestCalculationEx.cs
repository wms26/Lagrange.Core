namespace Lagrange.Laana.Common
{
    public static class DigestCalculationEx
    {
        public static string CalculateMd5(this string input)
        {
            byte[] md5 = System.Security.Cryptography.MD5.HashData(
                System.Text.Encoding.ASCII.GetBytes(input));
            return Convert.ToHexString(md5);
        }
        
        public static string CalculateMd5(this byte[] input)
        {
            byte[] md5 = System.Security.Cryptography.MD5.HashData(input);
            return Convert.ToHexString(md5);
        }
    }
}