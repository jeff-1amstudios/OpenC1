using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using System.Diagnostics;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace NeedForSpeed.Parsers
{
    enum BlockType
    {
        Null = 0,
        PartName = 54,
        Textures = 22,
        Vertices = 23,
        Faces = 53
    }

    class MeshChunk
    {
        List<Vector3> _vertices = new List<Vector3>();
        List<Vector2> _vertexTextureMap = new List<Vector2>();
        
        List<Polygon> _polygons = new List<Polygon>();
        private VertexBuffer _vertexBuffer;
        private List<string> _textureNames = new List<string>();
        string _currentPartName;
                

        public void Parse(string filename)
        {

            EndianBinaryReader reader = new EndianBinaryReader(new BigEndianBitConverter(), File.Open(filename, FileMode.Open));

            while (true)
            {
                int type = reader.ReadInt32();
                int size = 0;
                if (type != (int)BlockType.Null)
                    size = reader.ReadInt32();

                switch (type)
                {
                    case (int)BlockType.Null:
                        break;

                    case (int)BlockType.PartName:
                        reader.Seek(2, SeekOrigin.Current);
                        _currentPartName = Encoding.ASCII.GetString(reader.ReadBytes(size - 3));
                        Debug.WriteLine("PartName: " + _currentPartName);
                        reader.Seek(1, SeekOrigin.Current);
                        break;

                    case (int)BlockType.Vertices:
                        ReadVertexBlock(reader);
                        break;

                    case (int)BlockType.Faces:
                        ReadPolygonBlock(reader);
                        break;

                    case (int)BlockType.Textures:
                        ReadTextureNameBlock(reader, size);
                        break;

                    default:
                        Debug.WriteLine("Unknown section: " + type);
                        reader.Seek(size, SeekOrigin.Current);
                        break;
                }

                if (reader.BaseStream.Position == reader.BaseStream.Length)
                    break;
            }

            reader.Close();
            
        }

        private void ReadVertexBlock(EndianBinaryReader reader)
        {
            _vertices.Clear();
            int vertexCount = reader.ReadInt32();
            float scale = 100000;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 vertex = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                //Debug.WriteLine(vertex);
                _vertices.Add(vertex);
            }
        }

        private void ReadTextureMapBlock(BinaryReader reader, int texturePointsCount)
        {
            for (int i = 0; i < texturePointsCount; i++)
            {
                int tU = reader.ReadInt32();
                int tV = reader.ReadInt32();
                _vertexTextureMap.Add(new Vector2(tU, tV));
            }
        }

        private void ReadTextureNameBlock(EndianBinaryReader reader, int size)
        {
            int nbrTextures = reader.ReadInt32();
            string currentName = "";
            for (int i = 0; i < nbrTextures; i++)
            {
                byte ch = reader.ReadByte();
                while (ch != 0)
                {
                    currentName += (char)ch;
                    ch = reader.ReadByte();
                }
                //Debug.WriteLine(currentName);
                currentName = "";
            }            
        }
        
        private void ReadPolygonBlock(EndianBinaryReader reader)
        {
            int polygonCount = reader.ReadInt32();

            for (int i = 0; i < polygonCount; i++)
            {                
                Polygon polygon = new Polygon(PolygonShape.Triangle, _currentPartName);
                
                int v1 = reader.ReadInt16();
                int v2 = reader.ReadInt16();
                int v3 = reader.ReadInt16();
                byte unk1 = reader.ReadByte();
                byte unk2 = reader.ReadByte();
                byte unk3 = reader.ReadByte();

                //Debug.WriteLine("Face: " + v1 + " " + v2 + " " + v3 + " {" + unk1 + " " + unk2 + " " + unk3 + "}");

                //Vertices for polygon
                //polygonVertexMap.BaseStream.Position = poly1Index * sizeof(int);
                //int v1 = polygonVertexMap.ReadInt32();
                //int v2 = polygonVertexMap.ReadInt32();
                //int v3 = polygonVertexMap.ReadInt32();
                //int v4 = polygonVertexMap.ReadInt32();


                if (polygon.Shape == PolygonShape.Triangle || polygon.Shape == PolygonShape.Quad)
                {
                    polygon.Vertices.Add(_vertices[v1]);
                    polygon.Vertices.Add(_vertices[v2]);
                    polygon.Vertices.Add(_vertices[v3]);
                    polygon.TextureCoords.Add(Vector2.Zero);
                    polygon.TextureCoords.Add(Vector2.Zero);
                    polygon.TextureCoords.Add(Vector2.Zero);

                }

                _polygons.Add(polygon);
            }
        }


        public void Resolve(BitmapChunk bitmapChunk, string part)
        {
            int vertCount = 0;

            List<VertexPositionNormalTexture> allVerts = new List<VertexPositionNormalTexture>();
            foreach (Polygon poly in _polygons)
            {
                if (poly.PartName != part && part != "*")
                    continue;

                if (poly.TextureName != null)
                {
                    poly.ResolveTexture(bitmapChunk.FindByName(poly.TextureName));
                }

                poly.VertexBufferIndex = vertCount;
                vertCount += poly.VertexCount;
                allVerts.AddRange(poly.GetVertices());
            }

            _vertexBuffer = new VertexBuffer(Engine.Instance.Device, VertexPositionNormalTexture.SizeInBytes * vertCount, BufferUsage.WriteOnly);
            _vertexBuffer.SetData<VertexPositionNormalTexture>(allVerts.ToArray());
        }

        public void Render(Matrix world)
        {
            Engine.Instance.Device.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            Engine.Instance.Device.RenderState.CullMode = CullMode.CullClockwiseFace;
            Engine.Instance.Device.RenderState.FillMode = FillMode.Solid;
            Engine.Instance.Device.VertexDeclaration = new VertexDeclaration(Engine.Instance.Device, VertexPositionNormalTexture.VertexElements);
            
            BasicEffect effect = new BasicEffect(Engine.Instance.Device, null);
            effect.LightingEnabled = true;
            effect.EnableDefaultLighting();
            //effect.AmbientLightColor = new Vector3(0.09f, 0.09f, 0.1f);
            //effect.DirectionalLight0.Direction = new Vector3(1.0f, -1.0f, -1.0f);
            effect.View = Engine.Instance.Camera.View;
            effect.Projection = Engine.Instance.Camera.Projection;
            effect.World = world;
            
            foreach (Polygon poly in _polygons)
            {

                effect.Texture = poly.Texture;
                effect.TextureEnabled = true;

                effect.Begin(SaveStateMode.SaveState);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    Engine.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, poly.VertexBufferIndex, poly.VertexCount/3);
                    pass.End();
                }
                effect.End();
            }
        }
    }
}
