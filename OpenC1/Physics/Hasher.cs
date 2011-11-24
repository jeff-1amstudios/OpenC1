namespace Carmageddon.Physics
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public sealed class Hasher
    {
        private Hasher()
        {
        }

        private static byte[] ConvertStringToByteArray(string data)
        {
            return new ASCIIEncoding().GetBytes(data);
        }

        private static FileStream GetFileStream(string pathName)
        {
            return new FileStream(pathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public static string GetMD5HashFromFile(string pathName)
        {
            string str = "";
            FileStream inputStream = null;
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            try
            {
                inputStream = GetFileStream(pathName);
                byte[] buffer = provider.ComputeHash(inputStream);
                inputStream.Close();
                str = BitConverter.ToString(buffer).Replace("-", "");
            }
            catch
            {
            }
            return str;
        }

        public static string GetMD5HashFromString(string stringData)
        {
            string str = "";
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            try
            {
                byte[] buffer = ConvertStringToByteArray(stringData);
                str = BitConverter.ToString(provider.ComputeHash(buffer)).Replace("-", "");
            }
            catch
            {
            }
            return str;
        }
    }
}

