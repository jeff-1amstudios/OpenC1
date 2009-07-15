using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;

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
            if (skip == 0) return;
            int count = 0;
            while (true)
            {
                string line = _file.ReadLine();
                if (!line.StartsWith("//") && line != "") count++; //ignore comment lines

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
                if (!line.StartsWith("//") && line != "")
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

        protected Vector3 ReadLineAsVector3()
        {
            return ReadLineAsVector3(true);
        }

        protected Vector3 ReadLineAsVector3(bool scale)
        {
            string line = ReadLine();
            string[] tokens = line.Split(new char[] {',', '\t', ' '}, StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(tokens.Length == 3);
            return new Vector3(float.Parse(tokens[0]), float.Parse(tokens[1]), float.Parse(tokens[2])) * GameVariables.Scale;
        }
    }
}
