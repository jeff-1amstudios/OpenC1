using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.IO;
using System.Diagnostics;

namespace NeedForSpeed.Parsers
{
    class Material
    {
        public string Name { get; set; }
        public string PixName { get; set; }
        public bool DoubleSided { get; set; }
    }

    class MatFileParser : BaseParser
    {
        enum MaterialBlockType
        {
            Null = 0,
            TextureName = 28,
            TabName = 31,
            Attributes = 4
        }


        public List<Material> _materials = new List<Material>();

        public void Parse(string filename)
        {
            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, File.Open(filename, FileMode.Open));

            Material currentMaterial = null;

            while (true)
            {
                int blockLength = 0;
                MaterialBlockType blockType = (MaterialBlockType)reader.ReadInt32();
                blockLength = reader.ReadInt32();

                switch (blockType)
                {
                    case MaterialBlockType.Attributes:
                        currentMaterial = new Material();
                        _materials.Add(currentMaterial);

                        byte[] color = reader.ReadBytes(4);
                        byte[] otherColors = reader.ReadBytes(16);
                        byte[] flags = reader.ReadBytes(2);
                        byte[] transform = reader.ReadBytes(26);
                        currentMaterial.DoubleSided = flags[0] == 0x10;
                        currentMaterial.Name = ReadNullTerminatedString(reader);
                        
                        //Debug.WriteLine("Name: " + name + ", flags: " + flags[0] + " " + flags[1]);
                        break;

                    case MaterialBlockType.TextureName:
                        currentMaterial.PixName = ReadNullTerminatedString(reader);
                        //Debug.WriteLine("TextureName: " + modeName);
                        break;

                    case MaterialBlockType.TabName:
                        string tabName = ReadNullTerminatedString(reader);
                        Debug.WriteLine("TabName: " + tabName);
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

        public Material GetMaterial(string name)
        {
            return _materials.Find(m => m.Name == name); 
        }
    }
}
