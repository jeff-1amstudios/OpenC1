using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.IO;

namespace NeedForSpeed.Parsers
{
    class PaletteFileParser : BaseParser
    {

        enum PaletteBlockType
        {
            Null = 0,
            PixelData = 33,
            Attributes = 3
        }

        public byte[] Palette { get; private set; }

        public void Parse(string filename)
        {
            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, File.Open(filename, FileMode.Open));

            while (true)
            {
                int blockLength = 0;
                PaletteBlockType blockType = (PaletteBlockType)reader.ReadInt32();
                blockLength = reader.ReadInt32();

                switch (blockType)
                {
                    case PaletteBlockType.Attributes:

                        //contains name of palette and some attributes                        
                        //we dont care about this
                        reader.Seek(blockLength, SeekOrigin.Current);
                        break;

                    case PaletteBlockType.PixelData:
                        int entryCount = reader.ReadInt32();
                        int bytesPerEntry = reader.ReadInt32();
                        Palette = reader.ReadBytes(entryCount * bytesPerEntry);

                        break;

                    case PaletteBlockType.Null:
                        break;

                    default:
                        reader.Seek(blockLength, SeekOrigin.Current);
                        break;
                }
                if (reader.BaseStream.Position == reader.BaseStream.Length)
                    break;
            }

            reader.Close();
        }
    }
}
