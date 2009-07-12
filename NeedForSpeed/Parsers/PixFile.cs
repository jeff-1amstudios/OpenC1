using System;
using System.Collections.Generic;
using System.Linq;
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

        private static byte[] _palette;

        List<PixMap> _pixMaps = new List<PixMap>();

        public PixFile(string filename)
		{
            if (_palette == null)
            {
                PaletteFile paletteFile = new PaletteFile("c:\\games\\carma1\\data\\reg\\palettes\\drrender.pal");
                _palette = paletteFile.Palette;
            }

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

                        Texture2D texture = new Texture2D(Engine.Instance.Device, currentPix.Width, currentPix.Height, 1, TextureUsage.None, SurfaceFormat.Color);
                        texture.SetData<byte>(GetBytesForImage(pixels, currentPix.Width, currentPix.Height));
                        //texture.Save("c:\\temp\\" + currentPix.Name + ".png", ImageFileFormat.Png);
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

        private byte[] GetBytesForImage(byte[] pixels, int width, int height)
        {
            int overhang = 0;// (4 - ((width * 4) % 4));
            int stride = (width * 4) + overhang;

            byte[] imgData = new byte[stride * height];
            int curPosition = 0;
            for (int i = 0; i < height; i++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixel = pixels[width * i + x];

                    if (pixel == 0)
                    {
                        imgData[curPosition] = 0;
                        imgData[curPosition + 1] = 0;
                        imgData[curPosition + 2] = 0;
                        imgData[curPosition + 3] = 0;
                    }
                    else
                    {
                        byte[] rgb = GetRGBForPixel(pixel);
                        imgData[curPosition] = rgb[2];
                        imgData[curPosition + 1] = rgb[1];
                        imgData[curPosition + 2] = rgb[0];
                        imgData[curPosition + 3] = 0xFF;
                    }
                    curPosition += 4;
                }
                curPosition += overhang;
            }
            return imgData;
        }

        private byte[] GetRGBForPixel(int pixel)
        {
            byte[] rgb = new byte[3];
            rgb[0] = _palette[pixel * 4 + 1];
            rgb[1] = _palette[pixel * 4 + 2];
            rgb[2] = _palette[pixel * 4 + 3];
            return rgb;
        }

        public PixMap GetPixelMap(string name)
        {
            return _pixMaps.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
	}
}
