using System.Text;

namespace MongoDB_Web.Server.Helpers
{
    public class OTPManagement
    {
        public string? DecryptUserData(string? authCookieKey, string? randData)
        {
            if (authCookieKey is null || randData is null)
                return null;

            string DecryptedData = BinaryStringToText(XorBinary(authCookieKey, randData));

            if (!DecryptedData.Contains("Data:"))
                return null;

            return DecryptedData;
        }

        public (string, string) GetUserData(string inputData)
        {
            StringBuilder builder = new StringBuilder(inputData);
            builder.Replace("Data:", "");
            string[] dataArray = builder.ToString().Split("@");

            if (dataArray.Length is not 2)
                return ("", "");

            return (dataArray[0], dataArray[1]);
        }

        static string XorBinary(string bin1, string bin2)
        {
            int len = Math.Max(bin1.Length, bin2.Length);
            string res = "";
            bin1 = bin1.PadLeft(len, '0');
            bin2 = bin2.PadLeft(len, '0');
            for (int i = 0; i < len; i++)
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

        static string BinaryStringToText(string binary)
        {
            string[] binaryArray = binary.Split(' ');
            string text = "";
            foreach (string s in binaryArray)
            {
                int i = Convert.ToInt32(s, 2);
                char c = (char)i;
                text += c.ToString();
            }
            return text;
        }
    }
}
