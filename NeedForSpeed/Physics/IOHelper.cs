namespace Carmageddon.Physics
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    internal class IOHelper
    {
        public static bool DebugLog;
        private static FileStream myFileStream;
        private static XmlSerializer mySerializer;
        private static StreamWriter myStreamWriter;

        public static void WriteReport(string file_name, string message)
        {
            myStreamWriter = new StreamWriter(file_name, false);
            myStreamWriter.WriteLine("Time:  " + DateTime.Now.ToShortDateString() + ", " + DateTime.Now.ToLongTimeString());
            myStreamWriter.Write(message);
            myStreamWriter.Close();
        }

        public static void WriteToDebugLog(string message)
        {
            if (DebugLog)
            {
                myStreamWriter = new StreamWriter(@".\ErrorLog.txt", true);
                myStreamWriter.WriteLine(DateTime.Now.ToLongTimeString() + " NOTICE:  " + message);
                myStreamWriter.Close();
            }
        }

        public static void WriteToErrorLog(string message)
        {
            myStreamWriter = new StreamWriter(@".\ErrorLog.txt", true);
            myStreamWriter.WriteLine(DateTime.Now.ToLongTimeString() + " ERROR:  " + message);
            myStreamWriter.Close();
        }

        public static T XMLDeserialize<T>(string file_name)
        {
            T local2;
            mySerializer = new XmlSerializer(typeof(T));
            try
            {
                myFileStream = new FileStream(file_name, FileMode.Open);
                T local = (T)mySerializer.Deserialize(myFileStream);
                myFileStream.Close();
                local2 = local;
            }
            catch (Exception exception)
            {
                WriteToErrorLog(exception.Message);
                throw;
            }
            return local2;
        }

        public static void XMLSerialize<T>(string file_name, T data)
        {
            mySerializer = new XmlSerializer(typeof(T));
            try
            {
                myStreamWriter = new StreamWriter(file_name, false);
                mySerializer.Serialize((TextWriter)myStreamWriter, data);
                myStreamWriter.Close();
            }
            catch (Exception exception)
            {
                WriteToErrorLog(exception.Message);
                throw;
            }
        }
    }
}

