using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Carmageddon.Parsers
{
    abstract class BaseTextFile
    {
        StreamReader _file;

        public BaseTextFile(string filename)
        {
            _file = new StreamReader(filename);
        }

        public void CloseFile()
        {
            _file.Close();
        }

        protected void SkipLines(int skip)
        {
            int count = 0;
            while (true)
            {
                string line = _file.ReadLine();
                if (!line.StartsWith("//")) count++; //ignore comment lines

                if (count == skip)
                    break;
            }
        }

        protected string SkipLinesTillComment(string comment)
        {
            while (true)
            {
                string line = _file.ReadLine();
                if (line.Contains(comment))
                    return line;
            }
        }

        /// <summary>
        /// Return the next non-comment line, with comment stripped out
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected string ReadLine()
        {
            while (true)
            {
                string line = _file.ReadLine();
                if (!line.StartsWith("//"))
                {
                    return line.Split(new string[] { "//" }, StringSplitOptions.None)[0].Trim();
                }
            }
        }

        protected int ReadLineAsInt()
        {
            string line = ReadLine();
            return int.Parse(line);
        }
    }
}
