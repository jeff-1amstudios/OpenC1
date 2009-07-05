using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace NeedForSpeed.Parsers.Track
{
    enum BlockType
    {
        Name = 54,
        Textures = 22,
        Vertices = 23,
        Faces = 53
    }
    
    public class TrackFile
    {
        VertexBuffer _vertexBuffer;
        VertexBuffer _sceneryVertexBuffer;
        
        Vector3 _scaleFactor = new Vector3(0.000127f, 0.000127f, 0.000127f);

        public TrackFile(string filename)
        {            
            ReadTrackFile(filename);            
        }
        
        private void ReadTrackFile(string filename)
        {
            EndianBinaryReader reader = new EndianBinaryReader(new BigEndianBitConverter(), File.Open(filename, FileMode.Open));

            while (true)
            {
                int type = reader.ReadInt32();
                int size = reader.ReadInt32();

                reader.Seek(size, SeekOrigin.Current);

                if (reader.BaseStream.Position == reader.BaseStream.Length)
                    break;
            }

            reader.Close();
        }
    }
}
