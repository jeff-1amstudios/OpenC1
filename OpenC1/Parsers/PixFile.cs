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
using System.Collections;

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

		bool _skipNextPixelSection;

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
                        
						int type = reader.ReadByte();
						if (type == 7) 
						{
							/* bmp palette data? - seen in some splat pack textures. Jump over it, and its pixel data */
							reader.Seek(blockLength-1, SeekOrigin.Current);
							_skipNextPixelSection = true;
							break;
						}
						currentPix = new PixMap();
                        currentPix.Width = reader.ReadInt16();
						int width2 = reader.ReadInt16();
                        currentPix.Height = reader.ReadInt16();
						                        
						byte[] unk2 = reader.ReadBytes(4);
                        currentPix.Name = ReadNullTerminatedString(reader);
                        _pixMaps.Add(currentPix);
						break;

					case PixBlockType.PixelData:
						
						if (_skipNextPixelSection)
						{
							reader.Seek(blockLength, SeekOrigin.Current);
							_skipNextPixelSection = false;
							break;
						}
						int pixelCount = reader.ReadInt32();
						int bytesPerPixel = reader.ReadInt32();
						if (bytesPerPixel > 3)
						{
							bytesPerPixel = 1;  //PixEd sometimes doesnt get this right
						}

                        if (currentPix == null)
                        {
                            int size = (int)Math.Sqrt(pixelCount);
                            currentPix = new PixMap();
                            currentPix.Name = Path.GetFileName(filename);
                            currentPix.Width = size;
                            currentPix.Height = size;
                            _pixMaps.Add(currentPix);
                        }
                        
                        byte[] pixels = reader.ReadBytes(pixelCount * bytesPerPixel);
                        Texture2D texture=null;

                        if (bytesPerPixel == 1)
                        {
                            texture = new Texture2D(Engine.Device, currentPix.Width, currentPix.Height, 1, TextureUsage.None, SurfaceFormat.Color);
                            texture.SetData<byte>(Helpers.GetBytesForImage(pixels, currentPix.Width, currentPix.Height, GameVars.Palette));
                        }
                        else if (bytesPerPixel == 2)
                        {
                            texture = new Texture2D(Engine.Device, currentPix.Width, currentPix.Height, 1, TextureUsage.None, SurfaceFormat.Bgr565);
                            int j = 0;
                            byte[] px = new byte[2];
                            for (int i = 0; i < pixels.Length; i += 2)
                            {
                                byte tmp = pixels[i + 1];
                                pixels[i + 1] = pixels[i];
                                pixels[i] = tmp; 
                            }
                            texture.SetData<byte>(pixels);
                        }
                        else if (bytesPerPixel ==3 )
                        {
                            texture = new Texture2D(Engine.Device, currentPix.Width, currentPix.Height, 1, TextureUsage.None, SurfaceFormat.Color);
                            int j = 0;
                            byte[] px2 = new byte[pixels.Length * 4];
                            for (int i = 0; i < pixels.Length; i += 3)
                            {
                                px2[j++] = pixels[i];
                                px2[j++] = pixels[i+1];
                                px2[j++] = pixels[i+2];
                                px2[j++] = 255;
                            }
                            texture.SetData<byte>(px2);
                        }
                        
                        currentPix.Texture = texture;
						break;

					case PixBlockType.Null:
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
