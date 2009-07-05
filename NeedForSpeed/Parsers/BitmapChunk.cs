using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NeedForSpeed.Parsers
{
    enum BitmapEntryType
    {
        Unknown,
        Texture,
        Palette
    }

    class BitmapEntry
    {
        public string Id;
        public int Offset;
        public Texture2D Texture;
        public BitmapEntryType Type;
    }

    class BitmapChunk : BaseChunk
    {
        byte[] _palette;

        List<BitmapEntry> _bitmaps = new List<BitmapEntry>();

        internal List<BitmapEntry> Bitmaps
        {
            get { return _bitmaps; }
        }
        
        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Debug.WriteLine(">> Loading bitmap chunk");

            int itemCount = reader.ReadInt32();
            string directoryName = new string(reader.ReadChars(4));

            
            for (int i = 0; i < itemCount; i++)
            {
                BitmapEntry entry = new BitmapEntry();
                entry.Id = new string(reader.ReadChars(4));
                entry.Offset = reader.ReadInt32();
                _bitmaps.Add(entry);
                Debug.WriteLine("Bitmap " + entry.Id);
            }

            //Load palette first
            foreach (BitmapEntry entry in _bitmaps)
            {
                if (entry.Id.ToUpper() == "!PAL")
                {
                    reader.BaseStream.Position = _offset + entry.Offset;
                    ReadBitmapData(reader, entry);
                    break;
                }
            }
            
            foreach (BitmapEntry entry in _bitmaps)
            {
                if (entry.Type != BitmapEntryType.Palette)
                {
                    reader.BaseStream.Position = _offset + entry.Offset;
                    ReadBitmapData(reader, entry);
                }
            }
        }

        private void ReadBitmapData(BinaryReader reader, BitmapEntry entry)
        {
            reader.ReadByte();
            reader.ReadBytes(3);
            int width = reader.ReadInt16();
            int height = reader.ReadInt16();
            reader.ReadInt32();
            reader.ReadInt32();

            if (entry.Id.ToUpper() == "!PAL")
                ReadPalette(reader, entry);
            else
                ReadTexture(reader, entry, width, height);
        }

        private void ReadTexture(BinaryReader reader, BitmapEntry entry, int width, int height)
        {
            byte[] pixelData = reader.ReadBytes(width * height);

            int overhang = 0; // (4 - ((width * 4) % 4));
            int stride = (width * 4) + overhang;

            byte[] imgData = new byte[stride * height];
            int curPosition = 0;
            for (int i = 0; i < height; i++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixel = pixelData[width * i + x];
                    
                    if (pixel == 0xFF)
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

            entry.Texture = new Texture2D(Engine.Instance.Device, width, height, 1, TextureUsage.None, SurfaceFormat.Color);
            entry.Texture.SetData<byte>(imgData);
            entry.Texture.Save("c:\\temp\\" + entry.Id + ".png", ImageFileFormat.Png);
            entry.Texture.Dispose();
            entry.Texture = Texture2D.FromFile(Engine.Instance.Device, "c:\\temp\\" + entry.Id + ".png");
            entry.Type = BitmapEntryType.Texture;
        }
        
        private void ReadPalette(BinaryReader reader, BitmapEntry entry)
        {
            if (_palette == null /*entry.Id == "!PAL"*/)
            {
                _palette = reader.ReadBytes(3 * 256);
            }
            entry.Type = BitmapEntryType.Palette;
        }

        private byte[] GetRGBForPixel(int pixel)
        {
            byte[] rgb = new byte[3];
            rgb[0] = _palette[pixel * 3];
            rgb[1] = _palette[pixel * 3 + 1];
            rgb[2] = _palette[pixel * 3 + 2];            
            return rgb;
        }

        public BitmapEntry FindByName(string name)
        {
            return _bitmaps.Find(delegate(BitmapEntry entry)
            {
                return entry.Id == name;
            });
        }

        //private void EnsureDefaultPaletteLoaded()
        //{
        //    if (_defaultPalette == null)
        //    {
        //        _defaultPalette = new byte[0];
        //        BinaryReader paletteReader = new BinaryReader(new FileStream(@"C:\Games\NFSSE\SIMDATA\CARFAMS\CARPAL.FSH", FileMode.Open));
        //        BitmapChunk paletteChunk = new BitmapChunk();
        //        paletteChunk.SkipHeader(paletteReader);
        //        paletteChunk.Read(paletteReader);
        //        _defaultPalette = paletteChunk._palette;
        //        paletteReader.Close();
        //    }            
        //}
    }
}
