using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.IO;

namespace Carmageddon.Parsers
{
    class BaseDataFile
    {

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
