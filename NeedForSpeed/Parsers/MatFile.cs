using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace Carmageddon.Parsers
{
    class Material
    {
        public string Name { get; set; }
        public string PixName { get; set; }
        public bool DoubleSided { get; set; }
        public int BaseColor { get; set; }
        Texture2D _baseTexture;

        public Texture2D BaseTexture
        {
            get
            {
                if (_baseTexture == null)
                {
                    _baseTexture = new Texture2D(Engine.Instance.Device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
                    _baseTexture.SetData<Color>(new Color[] { GameVariables.Palette.GetRGBColorForPixel(BaseColor) });
                }
                return _baseTexture;
            }
        }

        public bool IsSimpMat
        {
            get { return PixName == null; }
        }
    }

    class MatFile : BaseDataFile
    {
        enum MaterialBlockType
        {
            Null = 0,
            TextureName = 28,
            TabName = 31,
            Attributes = 4
        }


        List<Material> _materials = new List<Material>();

        internal List<Material> Materials
        {
            get { return _materials; }
        }

        public MatFile(string filename)
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
                        byte[] transform = reader.ReadBytes(24);
                        currentMaterial.BaseColor = reader.ReadByte();
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

        public Material GetMaterial(string name)
        {
            return _materials.Find(m => m.Name == name); 
        }

    }
}
