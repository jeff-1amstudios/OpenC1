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

namespace Carmageddon.Parsers
{
    enum BlockType
    {
        Null = 0,
        PartName = 54,
        Materials = 22,
        Vertices = 23,
        TextureCoords = 24,
        FaceMaterials = 26,
        Faces = 53
    }

    class DatFile : BaseDataFile
    {
        List<Vector3> _vertices = new List<Vector3>();
        List<Vector2> _vertexTextureMap = new List<Vector2>();
        
        List<Polygon> _polygons = new List<Polygon>();
        private VertexBuffer _vertexBuffer;
        private List<string> _materialNames = new List<string>();
        string _currentPartName;
        int _currentPolygonIndex;

        public DatFile(string filename)
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
                        _currentPartName = ReadNullTerminatedString(reader);
                        Debug.WriteLine("PartName: " + _currentPartName);                        
                        break;

                    case (int)BlockType.Vertices:
                        ReadVertexBlock(reader);
                        break;

                    case (int)BlockType.Faces:
                        ReadPolygonBlock(reader, _currentPartName);
                        break;

                    case (int)BlockType.TextureCoords:
                        ReadTextureMapBlock(reader);
                        break;

                    case (int)BlockType.Materials:
                        ReadMaterialsBlock(reader);
                        break;

                    case (int)BlockType.FaceMaterials:
                        ReadFaceMaterialsBlock(reader);
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
            _vertexTextureMap.Clear();
            _materialNames.Clear();
            _currentPolygonIndex = _polygons.Count;

            int vertexCount = reader.ReadInt32();
            
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 vertex = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                _vertices.Add(vertex);
                Debug.WriteLine(vertex);
            }
        }

        private void ReadTextureMapBlock(EndianBinaryReader reader)
        {
            int texturePointsCount = reader.ReadInt32();
            for (int i = 0; i < texturePointsCount; i++)
            {
                float tU = reader.ReadSingle();
                float tV = reader.ReadSingle();
                _vertexTextureMap.Add(new Vector2(tU, tV));
            }
        }

        private void ReadMaterialsBlock(EndianBinaryReader reader)
        {
            int nbrMaterials = reader.ReadInt32();
            for (int i = 0; i < nbrMaterials; i++)
            {
                string material = ReadNullTerminatedString(reader);
                _materialNames.Add(material);
            }
        }

        private void ReadFaceMaterialsBlock(EndianBinaryReader reader)
        {
            int nbrFaceMaterials = reader.ReadInt32();
            int bytesPerEntry = reader.ReadInt32();

            for (int i = 0; i < nbrFaceMaterials; i++)
            {
                int matIndex = reader.ReadInt16() - 1;
                if (matIndex > -1)
                    _polygons[_currentPolygonIndex + i].MaterialName = _materialNames[matIndex]; //-1 because it is 1-based
            }
        }

        private void ReadPolygonBlock(EndianBinaryReader reader, string modelName)
        {
            int polygonCount = reader.ReadInt32();

            for (int i = 0; i < polygonCount; i++)
            {
                Polygon polygon = new Polygon(modelName);

                int v1 = reader.ReadInt16();
                int v2 = reader.ReadInt16();
                int v3 = reader.ReadInt16();
                byte unk1 = reader.ReadByte();
                byte unk2 = reader.ReadByte();
                byte unk3 = reader.ReadByte();

                polygon.Vertices.Add(_vertices[v1]);
                polygon.Vertices.Add(_vertices[v2]);
                polygon.Vertices.Add(_vertices[v3]);
                polygon.TextureCoords.Add(_vertexTextureMap[v1]);
                polygon.TextureCoords.Add(_vertexTextureMap[v2]);
                polygon.TextureCoords.Add(_vertexTextureMap[v3]);

                _polygons.Add(polygon);
            }
        }


        public void Resolve(MatFile matFile, PixFile pix)
        {
            int vertCount = 0;
            
            List<VertexPositionNormalTexture> allVerts = new List<VertexPositionNormalTexture>();
            foreach (Polygon poly in _polygons)
            {
                if (poly.MaterialName != null)
                {
                    Material m = matFile.GetMaterial(poly.MaterialName);
                    if (m != null)
                    {
                        poly.DoubleSided = m.DoubleSided;

                        PixMap pixmap = pix.GetPixelMap(m.PixName);
                        if (pixmap != null)
                            poly.Texture = pixmap.Texture;
                    }
                }
                if (poly.Texture == null)
                {
                }

                poly.VertexBufferIndex = vertCount;
                vertCount += poly.VertexCount;
                allVerts.AddRange(poly.GetVertices());
            }

            _vertexBuffer = new VertexBuffer(Engine.Instance.Device, VertexPositionNormalTexture.SizeInBytes * vertCount, BufferUsage.WriteOnly);
            _vertexBuffer.SetData<VertexPositionNormalTexture>(allVerts.ToArray());
        }

        public BasicEffect SetupRender()
        {
            GraphicsDevice device = Engine.Instance.Device;
            device.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            device.RenderState.CullMode = CullMode.CullClockwiseFace;
            device.RenderState.FillMode = FillMode.Solid;
            device.VertexDeclaration = new VertexDeclaration(Engine.Instance.Device, VertexPositionNormalTexture.VertexElements);

            BasicEffect effect = new BasicEffect(Engine.Instance.Device, null);
            effect.LightingEnabled = true;
            effect.EnableDefaultLighting();
            

            effect.FogEnabled = true;
            effect.FogColor = new Vector3(245, 245, 245);
            effect.FogStart = 2500;
            effect.FogEnd = 4000;
            //effect.AmbientLightColor = new Vector3(0.09f, 0.09f, 0.1f);
            //effect.DirectionalLight0.Direction = new Vector3(1.0f, -1.0f, -1.0f);
            effect.View = Engine.Instance.Camera.View;
            effect.Projection = Engine.Instance.Camera.Projection;
            
            effect.TextureEnabled = true;
            effect.Begin(SaveStateMode.SaveState);
            return effect;
        }

        public void DoneRender(BasicEffect effect)
        {
            effect.End();
        }

        public void Render(Matrix world, BasicEffect effect, Actor actor)
        {
            GraphicsDevice device = Engine.Instance.Device;
            effect.World = world;
            effect.CurrentTechnique.Passes[0].Begin();

            foreach (Polygon poly in _polygons)
            {
                if (poly.PartName != actor.ModelName) continue;

                device.RenderState.CullMode = (poly.DoubleSided ? CullMode.None : CullMode.CullClockwiseFace);
                if (poly.Texture != null)
                    device.Textures[0] = poly.Texture;
                else
                    device.Textures[0] = actor.Texture;

                Engine.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, poly.VertexBufferIndex, poly.VertexCount / 3);
                
            }
            effect.CurrentTechnique.Passes[0].End();
        }
    }
}
