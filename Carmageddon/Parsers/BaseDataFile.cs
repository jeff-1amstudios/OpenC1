using System;
using System.Collections.Generic;

using System.Text;
using MiscUtil.IO;
using System.IO;
using System.Diagnostics;

namespace OpenC1.Parsers
{
    class BaseDataFile
    {
        static List<string> _pixPaths, _matPaths, _fliPaths;
        public bool Exists { get; private set; }

        static BaseDataFile()
        {
            _pixPaths = new List<string>();
            _pixPaths.Add(GameVars.BasePath + "pixelmap\\");
            _pixPaths.Add(GameVars.BasePath + "reg\\pixelmap\\");
            if (GameVars.Emulation != EmulationMode.Demo)
                _pixPaths.Add(GameVars.BasePath + "64X48X8\\pixelmap\\");  //demo doesnt have 64x48x8 folder
            _pixPaths.Add(GameVars.BasePath + "32X20X8\\pixelmap\\");

            _matPaths = new List<string>();
            _matPaths.Add(GameVars.BasePath + "material\\");
            _matPaths.Add(GameVars.BasePath + "reg\\material\\");

            _fliPaths = new List<string>();
            _fliPaths.Add(GameVars.BasePath + "anim\\");
            _fliPaths.Add(GameVars.BasePath + "32x20x8\\anim\\");
        }

        protected Stream OpenDataFile(string filename)
        {
            Exists = true;
            string fullname="";
            if (this is PixFile)
            {
                foreach (string path in _pixPaths)
                {
                    fullname = path + filename;
                    if (File.Exists(fullname))
                        return File.Open(fullname, FileMode.Open);
                }
                Debug.WriteLine("File not found: " + filename);
                Exists = false;
                return null;
            }
            else if (this is MatFile)
            {
                foreach (string path in _matPaths)
                {
                    fullname = path + filename;
                    if (File.Exists(fullname))
                        return File.Open(fullname, FileMode.Open);
                }
                Debug.WriteLine("File not found: " + filename);
                Exists = false;
                return null;
            }
            else if (this is FliFile)
            {
                foreach (string path in _fliPaths)
                {
                    fullname = path + filename;
                    if (File.Exists(fullname))
                        return File.Open(fullname, FileMode.Open);
                }
                Debug.WriteLine("File not found: " + filename);
                Exists = false;
                return null;
            }
            else if (this is ActFile)
            {
                fullname = GameVars.BasePath + "actors\\" + filename;
            }
            else if (this is DatFile)
            {
                fullname = GameVars.BasePath + "models\\" + filename;
            }
            if (File.Exists(fullname))
                return File.Open(fullname, FileMode.Open);
            else
            {
                Debug.WriteLine("File not found: " + filename);
                Exists = false;
                return null;
            }
        }

        protected string ReadNullTerminatedString(EndianBinaryReader reader)
        {
            List<byte> bytes = new List<byte>(20);
            while (true)
            {
                byte chr = reader.ReadByte();
                if (chr == 0) break;
                bytes.Add(chr);
            }
            return Encoding.ASCII.GetString(bytes.ToArray());
        }
    }
}
