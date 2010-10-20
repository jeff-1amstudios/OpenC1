using System;
using System.Collections.Generic;
using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OneAmEngine;

namespace OpenC1.Parsers
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
            Stream file = OpenDataFile(filename);
            if (!Exists)
                return;

            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, file);
            PixMap currentPix=null;

			while (true)
			{
				int blockLength = 0;
				PixBlockType blockType = (PixBlockType)reader.ReadInt32();
                
                if (blockType == PixBlockType.Null && reader.BaseStream.Position + 3 >= reader.BaseStream.Length)
                    break;

				blockLength = reader.ReadInt32();

				switch (blockType)
				{
					case PixBlockType.Attributes:

                        currentPix = new PixMap();
                                                
						int type = reader.ReadByte();
                        currentPix.Width = reader.ReadInt16();
						int width2 = reader.ReadInt16();
                        currentPix.Height = reader.ReadInt16();
						                        
						byte[] unk2 = reader.ReadBytes(4);
                        currentPix.Name = ReadNullTerminatedString(reader);

                        _pixMaps.Add(currentPix);
						break;

					case PixBlockType.PixelData:
						int pixelCount = reader.ReadInt32();
						int bytesPerPixel = reader.ReadInt32();
                        byte[] pixels = reader.ReadBytes(pixelCount * bytesPerPixel);

                        Texture2D texture = new Texture2D(Engine.Device, currentPix.Width, currentPix.Height,1, TextureUsage.None, SurfaceFormat.Color);
                        texture.SetData<byte>(Helpers.GetBytesForImage(pixels, currentPix.Width, currentPix.Height, GameVars.Palette));
                        
                        currentPix.Texture = texture;
						break;

					case PixBlockType.Null:
                        if (reader.BaseStream.Position >= 135350)
                        {
                        }
						break;

					default:
						reader.Seek(blockLength, SeekOrigin.Current);
						break;
				}
				if (reader.BaseStream.Position+3 >= reader.BaseStream.Length)
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
