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
            Attributes = 4,
            AttributesV2 = 60
        }


        List<CMaterial> _materials = new List<CMaterial>();

        internal List<CMaterial> Materials
        {
            get { return _materials; }
        }

        public MatFile(string filename)
        {
            Stream file = OpenDataFile(filename);
            if (!Exists)
                return;

            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, file);

            CMaterial currentMaterial = null;

            while (true)
            {
                int blockLength = 0;
                MaterialBlockType blockType = (MaterialBlockType)reader.ReadInt32();
                blockLength = reader.ReadInt32();

                byte[] flags;

                switch (blockType)
                {
                    case MaterialBlockType.Attributes:
                        currentMaterial = new CMaterial();
                        _materials.Add(currentMaterial);

                        byte[] color = reader.ReadBytes(4);
                        byte[] otherColors = reader.ReadBytes(16);
                        flags = reader.ReadBytes(2);
                        byte[] transform = reader.ReadBytes(24);
                        currentMaterial.SimpMatPixelIndex = reader.ReadByte();
                        currentMaterial.SimpMatGradientCount = reader.ReadByte();
                        
                        currentMaterial.DoubleSided = flags[0] == 0x10;
                        currentMaterial.Name = ReadNullTerminatedString(reader);

                        break;

                    case MaterialBlockType.AttributesV2:
                        currentMaterial = new CMaterial();
                        _materials.Add(currentMaterial);

                        reader.ReadBytes(4); //color
                        reader.ReadBytes(16); //othercolors
                        flags = reader.ReadBytes(4); // flags
                        reader.ReadBytes(24); //transform
                        reader.ReadBytes(4); //unk
                        currentMaterial.DoubleSided = flags[0] == 0x10;
                        reader.BaseStream.Position += 13;                        
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
