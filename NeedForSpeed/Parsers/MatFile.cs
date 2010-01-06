using System;
using System.Collections.Generic;

using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace Carmageddon.Parsers
{
    
    class MatFile : BaseDataFile
    {
        enum MaterialBlockType
        {
            Null = 0,
            TextureName = 28,
            TabName = 31,
            Attributes = 4
        }


        List<CMaterial> _materials = new List<CMaterial>();

        internal List<CMaterial> Materials
        {
            get { return _materials; }
        }

        public MatFile(string filename)
        {
            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, File.Open(filename, FileMode.Open));

            CMaterial currentMaterial = null;

            while (true)
            {
                int blockLength = 0;
                MaterialBlockType blockType = (MaterialBlockType)reader.ReadInt32();
                blockLength = reader.ReadInt32();

                switch (blockType)
                {
                    case MaterialBlockType.Attributes:
                        currentMaterial = new CMaterial();
                        _materials.Add(currentMaterial);

                        byte[] color = reader.ReadBytes(4);
                        byte[] otherColors = reader.ReadBytes(16);
                        byte[] flags = reader.ReadBytes(2);
                        byte[] transform = reader.ReadBytes(24);
                        currentMaterial.BasePixel = reader.ReadByte();
                        reader.ReadByte(); //unk
                        currentMaterial.DoubleSided = flags[0] == 0x10;
                        currentMaterial.Name = ReadNullTerminatedString(reader);
                        
                        break;

                    case MaterialBlockType.TextureName:
                        currentMaterial.PixName = ReadNullTerminatedString(reader);
                        break;

                    case MaterialBlockType.TabName:
                        string tabName = ReadNullTerminatedString(reader);
                        break;

                    case MaterialBlockType.Null:
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

        public CMaterial GetMaterial(string name)
        {
            return _materials.Find(m => m.Name == name); 
        }

    }
}
