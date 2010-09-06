using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon.Parsers
{
    abstract class BaseTextFile
    {
        protected StreamReader _file;

        public BaseTextFile(string filename)
        {
            _file = new StreamReader(filename);
        }

        public void CloseFile()
        {
            _file.Close();
        }

        public void SkipLines(int skip)
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

        public string SkipLinesTillComment(string comment)
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
        public string ReadLine()
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

        public int ReadLineAsInt()
        {
            string line = ReadLine();
            return int.Parse(line);
        }

        public float ReadLineAsFloat()
        {
            return ReadLineAsFloat(true);
        }
        public float ReadLineAsFloat(bool scale)
        {
            string line = ReadLine();
            return float.Parse(line) * (scale ? GameVars.Scale.X : 1); 
        }

        public Color ReadLineAsColor()
        {
            Vector3 v3 = ReadLineAsVector3(false);
            return new Color((byte)v3.X, (byte)v3.Y, (byte)v3.Z, 255);
        }

        public Vector3 ReadLineAsVector3()
        {
            return ReadLineAsVector3(true);
        }

        public Vector3 ReadLineAsVector3(bool scale)
        {
            string line = ReadLine();
            string[] tokens = line.Split(new char[] {',', '\t', ' '}, StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(tokens.Length == 3);
            Vector3 vec = new Vector3(float.Parse(tokens[0]), float.Parse(tokens[1]), float.Parse(tokens[2]));
            if (scale) vec *= GameVars.Scale;
            return vec;
        }

        public Rectangle ReadLineAsRect()
        {
            string line = ReadLine();
            string[] tokens = line.Split(new char[] { ',', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(tokens.Length == 4);
            Rectangle rectangle = new Rectangle(int.Parse(tokens[0]), int.Parse(tokens[1]), 0, 0);
            rectangle.Width = int.Parse(tokens[2]) - rectangle.X;
            rectangle.Height = int.Parse(tokens[3]) - rectangle.Y;
            
            return rectangle;
        }

        public Vector2 ReadLineAsVector2(bool scale)
        {
            string line = ReadLine();
            string[] tokens = line.Split(new char[] { ',', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(tokens.Length == 2);
            Vector2 vec = new Vector2(float.Parse(tokens[0]), float.Parse(tokens[1]));
            if (scale) vec *= new Vector2(GameVars.Scale.X, GameVars.Scale.Y);
            return vec;
        }

        public Matrix ReadMatrix()
        {
            Matrix m = new Matrix();
            Vector3 v = ReadLineAsVector3(false);
            m.M11 = v.X;
            m.M12 = v.Y;
            m.M13 = v.Z;
            v = ReadLineAsVector3(false);
            m.M21 = v.X;
            m.M22 = v.Y;
            m.M23 = v.Z;
            v = ReadLineAsVector3(false);
            m.M31 = v.X;
            m.M32 = v.Y;
            m.M33 = v.Z;
            v = ReadLineAsVector3(false);
            m.M41 = v.X;
            m.M42 = v.Y;
            m.M43 = v.Z;
            m.M44 = 1;
            
            return m;
        }
    }
}
