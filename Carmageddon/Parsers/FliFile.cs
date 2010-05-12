using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace Carmageddon.Parsers
{

    class FliPalette : IPalette
    {
        byte[] _paletteData;

        public FliPalette(byte[] data)
        {
            _paletteData = data;
        }

        #region IPalette Members

        public byte[] GetRGBBytesForPixel(int pixel)
        {
            byte[] rgb = new byte[3];
            rgb[0] = _paletteData[pixel * 3];
            rgb[1] = _paletteData[pixel * 3 + 1];
            rgb[2] = _paletteData[pixel * 3 + 2];
            return rgb;
        }

        #endregion
    }


    class FliFile
    {
        public string Filename;
        FliPalette _palette;
        ushort _height, _width;
        List<Texture2D> _frames = new List<Texture2D>();
        byte[] _lastFramePixels;
        public uint FrameRate { get; private set; }

        public List<Texture2D> Frames
        {
            get { return _frames; }
        }

        public FliFile(string filename)
        {
            Filename = filename;
            BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open));
            int filesize = reader.ReadInt32();
            ushort type = reader.ReadUInt16();
            //Debug.Assert(type == 0xAF12);
            ushort frames = reader.ReadUInt16();
            _width = reader.ReadUInt16();
            _height = reader.ReadUInt16();
            ushort colorDepth = reader.ReadUInt16();
            ushort flags = reader.ReadUInt16();
            FrameRate = reader.ReadUInt32();
            reader.ReadUInt16(); //reserved
            reader.ReadUInt32(); //date
            uint creator = reader.ReadUInt32();
            reader.ReadUInt32(); //date
            reader.ReadUInt32();
            ushort aspectX = reader.ReadUInt16();
            ushort aspectY = reader.ReadUInt16();
            reader.BaseStream.Seek(38, SeekOrigin.Current);
            uint frame1 = reader.ReadUInt32();
            uint frame2 = reader.ReadUInt32();
            reader.BaseStream.Seek(40, SeekOrigin.Current);

            for (int i = 0; i < frames; i++)
            {
                ReadChunk(reader);
            }

            reader.Close();
        }

        private void ReadChunk(BinaryReader reader)
        {
            uint chunkLength = reader.ReadUInt32();
            ushort type = reader.ReadUInt16();

            switch (type)
            {
                case 0xF1FA:
                    ReadFrameChunk(reader);
                    break;
                case 4:
                    ReadPalette256Chunk(reader);
                    break;
                case 7:
                    ReadFLCDeltaChunk(reader);
                    break;
                case 11:
                    ReadPalette64Chunk(reader);
                    break;
                case 12:
                    ReadFLIDeltaChunk(reader);
                    break;
                case 15:
                    ReadRLEPixelChunk(reader);
                    break;
                default:
                    break;
            }
        }


        private void ReadFrameChunk(BinaryReader reader)
        {
            ushort subChunks = reader.ReadUInt16();
            reader.BaseStream.Seek(8, SeekOrigin.Current);
            
            for (int i = 0; i < subChunks; i++)
            {
                ReadChunk(reader);
            }
        }


        private void ReadPalette256Chunk(BinaryReader reader)
        {
            short nbrPackets = reader.ReadInt16();
            byte skipCount = reader.ReadByte();
            byte copyCount = reader.ReadByte();
            Debug.Assert(copyCount == 0);
            _palette = new FliPalette(reader.ReadBytes(768));
        }

        private void ReadPalette64Chunk(BinaryReader reader)
        {
            short nbrPackets = reader.ReadInt16();
            byte skipCount = reader.ReadByte();
            byte copyCount = reader.ReadByte();
            Debug.Assert(copyCount == 0);
            byte[] palette = reader.ReadBytes(768);
            for (int i = 0; i < 256; i++)
                palette[i] *= 4;
            _palette = new FliPalette(palette);
        }

        private void ReadRLEPixelChunk(BinaryReader reader)
        {
            List<byte> uncompressed = new List<byte>(_height * _width);
            long startOffset = reader.BaseStream.Position;

            for (int i = 0; i < _height; i++)
            {
                byte packetcount = reader.ReadByte();
                int bytesUncompressed = 0;

                for (int p = 0; p < packetcount; p++)
                {
                    int count = (int)reader.ReadSByte();
                    if (count < 0)
                        uncompressed.AddRange(reader.ReadBytes(-count));
                    else if (count > 0)
                    {
                        byte data = reader.ReadByte();
                        for (int d = 0; d < count; d++)
                            uncompressed.Add(data);
                    }
                    bytesUncompressed += Math.Abs(count);
                }
            }
            
            // Compressed data must be zero-padded to end on a 16-bit boundary
            if (((reader.BaseStream.Position - startOffset) % 2) == 1)
            {
                reader.BaseStream.Seek(1, SeekOrigin.Current);
            }

            AddFrame(uncompressed.ToArray());
        }

        private void ReadFLIDeltaChunk(BinaryReader reader)
        {
            long startOffset = reader.BaseStream.Position;

            byte[] buffer = new byte[_lastFramePixels.Length];
            Array.Copy(_lastFramePixels, buffer, _lastFramePixels.Length);

            short startLine = reader.ReadInt16();
            long position = startLine * _width;
            int lines = reader.ReadInt16();

            for (int i = startLine; i < lines + startLine; i++)
            {
                position = i * _width;
                byte packetcount = reader.ReadByte();

                for (int p = 0; p < packetcount; p++)
                {
                    byte skip = reader.ReadByte();
                    position += skip;
                    int count = (int)reader.ReadSByte();
                    Debug.Assert(count != 0);
                    if (count > 0)
                    {
                        Array.Copy(reader.ReadBytes(count), 0, buffer, position, count);
                        position += count;
                    }
                    else
                    {
                        byte data = reader.ReadByte();
                        for (int d = 0; d < -count; d++)
                            buffer[position++] = data;
                    }
                }
            }

            // Compressed data must be zero-padded to end on a 16-bit boundary
            if (((reader.BaseStream.Position - startOffset) % 2) == 1)
            {
                reader.BaseStream.Seek(1, SeekOrigin.Current);
            }

            AddFrame(buffer);
        }

        private void ReadFLCDeltaChunk(BinaryReader reader)
        {
            long startOffset = reader.BaseStream.Position;

            byte[] buffer = new byte[_lastFramePixels.Length];
            Array.Copy(_lastFramePixels, buffer, _lastFramePixels.Length);
            
            short lines = reader.ReadInt16();
            Int32 pos = 0;
            short l = 0;
            byte skip;
            int change;
            short color;

            while (lines > 0)
            {
                pos = l * _width;

                short b = reader.ReadInt16();
                
                // Number of packets following
                if ((b & 0xC000) == 0x0000)
                {
                    b &= 0x3FFF;   // Number of packets in low 14 bits

                    for (int j = 0; j < b; j++)
                    {
                        // Skip unchanged pixels
                        skip = reader.ReadByte();
                        pos += skip;

                        // Pixels to change
                        change = reader.ReadSByte();
                        
                        if (change > 0)
                        {
                            for (int i = 0; i < change; i++)
                            {
                                color = reader.ReadInt16();
                                buffer[pos++] = (byte)(color & 0x00FF);
                                buffer[pos++] = (byte)((color >> 8) & 0x00FF);
                            }
                        }
                        else if (change < 0)
                        {
                            change = -change;
                            color = reader.ReadInt16();
                            for (int i = 0; i < change; i++)
                            {
                                buffer[pos++] = (byte)(color & 0x00FF);
                                buffer[pos++] = (byte)((color >> 8) & 0x00FF);
                            }
                        }
                    }
                    lines--;
                    l++;
                }
                else
                    // Number of lines that we should skip
                    if ((b & 0xC000) == 0xC000)
                        l -= b;
                    else
                        // Color of last pixel in row
                        buffer[pos++] = (byte)(b & 0x00FF);
            }

            // Compressed data must be zero-padded to end on a 32-bit boundary
            if (((reader.BaseStream.Position - startOffset) % 4) == 1)
            {
                reader.BaseStream.Seek(2, SeekOrigin.Current);
            }

            AddFrame(buffer);
        }

        private void AddFrame(byte[] pixels)
        {
            Texture2D texture = new Texture2D(Engine.Device, _width, _height, 1, TextureUsage.None, SurfaceFormat.Color);
            texture.SetData<byte>(Helpers.GetBytesForImage(pixels, _width, _height, _palette));
            //texture.Save("c:\\temp\\fli" + _frames.Count + ".png", ImageFileFormat.Png);
            _frames.Add(texture);
            _lastFramePixels = pixels;
        }
    }
}
