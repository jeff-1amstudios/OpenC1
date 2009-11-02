using System;
using System.Collections.Generic;

using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PlatformEngine;

namespace Carmageddon.Parsers
{
    class PixMap
    {
        public string Name { get; set; }
        public int Width, Height;
        public Texture2D Texture { get; set; }
    }

    class PixFile : BaseDataFile
	{
		enum PixBlockType
		{
			Null = 0,
			PixelData = 33,
			Attributes = 3
		}

        List<PixMap> _pixMaps = new List<PixMap>();

        internal List<PixMap> PixMaps
        {
            get { return _pixMaps; }
        }

        public PixFile(string filename)
		{
			EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, File.Open(filename, FileMode.Open));
            PixMap currentPix=null;

			while (true)
			{
				int blockLength = 0;
				PixBlockType blockType = (PixBlockType)reader.ReadInt32();
				blockLength = reader.ReadInt32();

				switch (blockType)
				{
					case PixBlockType.Attributes:

                        currentPix = new PixMap();
                                                
						int type = reader.ReadByte();
						byte[] unk = reader.ReadBytes(2);
						currentPix.Width = reader.ReadInt16();
                        currentPix.Height = reader.ReadInt16();
						                        
						byte[] unk2 = reader.ReadBytes(4);
                        currentPix.Name = ReadNullTerminatedString(reader);

                        _pixMaps.Add(currentPix);
						break;

					case PixBlockType.PixelData:
						int pixelCount = reader.ReadInt32();
						int bytesPerPixel = reader.ReadInt32();
                        byte[] pixels = reader.ReadBytes(pixelCount * bytesPerPixel);

                        Texture2D texture = new Texture2D(Engine.Instance.Device, currentPix.Width, currentPix.Height, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
                        texture.SetData<byte>(Helpers.GetBytesForImage(pixels, currentPix.Width, currentPix.Height, GameVariables.Palette));
                        
                        currentPix.Texture = texture;
						break;

					case PixBlockType.Null:
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
        

        public PixMap GetPixelMap(string name)
        {
            return _pixMaps.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
	}
}
