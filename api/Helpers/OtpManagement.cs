using System.Security.Cryptography;
using System.Text;

namespace api.Helpers
{
    public static class OtpManagement
    {
        public static string? DecryptUserData(string? authCookieKey, string? randData)
        {
            if (authCookieKey is null || randData is null)
                return null;

            var decryptedData = BinaryStringToText(XorBinary(authCookieKey, randData));

            return !decryptedData.Contains("Data:") ? null : decryptedData;
        }

        public static string EncryptUserData(string inputData, string randData)
        {
            var inputBinary = StringToBinary(inputData);
            var encryptedData = Encoding.UTF8.GetBytes(XorBinary(inputBinary, randData));
            return Convert.ToBase64String(encryptedData);
        }

        public static string GenerateRandomBinaryData(int length)
        {
            var result = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var randomNumber = RandomNumberGenerator.GetInt32(0, 2);
                result.Append(randomNumber);

                if ((i + 1) % 8 == 0 && i != length - 1)
                    result.Append(' ');
            }

            return result.ToString();
        }

        public static (string, string) GetUserData(string inputData)
        {
            var builder = new StringBuilder(inputData);
            builder.Replace("Data:", "");
            var dataArray = builder.ToString().Split("@");

            return dataArray.Length is not 2 ? ("", "") : (dataArray[0], dataArray[1]);
        }

        private static string XorBinary(string bin1, string bin2)
        {
            var len = Math.Max(bin1.Length, bin2.Length);
            var res = "";
            bin1 = bin1.PadLeft(len, '0');
            bin2 = bin2.PadLeft(len, '0');
            for (var i = 0; i < len; i++)
            {
                if (bin1[i] == ' ')
                {
                    res += ' ';
                    continue;
                }

                res += bin1[i] == bin2[i] ? '0' : '1';
            }
            return res;
        }

        private static string BinaryStringToText(string binary)
        {
            var binaryArray = binary.Split(' ');
            return binaryArray.Select(s => Convert.ToInt32(s, 2)).Select(i => (char)i).Aggregate("", (current, c) => current + c.ToString());
        }

        private static string StringToBinary(string input)
        {
            return string.Join(" ", input.Select(c => Convert.ToString(c, 2).PadLeft(8, '0')));
        }

    }
}
