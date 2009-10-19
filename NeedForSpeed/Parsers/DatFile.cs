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
        ModelName = 54,
        Materials = 22,
        Vertices = 23,
        TextureCoords = 24,
        FaceMaterials = 26,
        Faces = 53
    }

    class Model
    {
        public string Name { get; set; }
        public List<string> MaterialNames { get; set; }
        public List<Polygon> Polygons { get; set; }
        public int VertexCount { get; set; }
        public int VertexBaseIndex { get; set; }
        public int IndexBufferStart { get; set; }

        public void Render(Texture2D texture)
        {
            GraphicsDevice device = Engine.Instance.Device;
            int baseVert = VertexBaseIndex;
            int indexBufferStart = IndexBufferStart;

            for (int i = 0; i < Polygons.Count; i++)
            {
                Polygon poly = Polygons[i];

                if (GameVariables.CullingDisabled != poly.DoubleSided)
                {
                    device.RenderState.CullMode = (poly.DoubleSided ? CullMode.None : CullMode.CullClockwiseFace);
                    GameVariables.CullingDisabled = poly.DoubleSided;
                }

                if (poly.Texture != null)
                    device.Textures[0] = poly.Texture;
                else
                    device.Textures[0] = texture;

                Engine.Instance.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVert, 0, 3, indexBufferStart + i * 3, 1);
            }
        }
    }

    class DatFile : BaseDataFile
    {
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private VertexDeclaration _vertexDeclaration;
        private BasicEffect _effect;
        
        List<Model> _models = new List<Model>();
        List<Vector3> _vertexPositions = new List<Vector3>();
        List<Vector2> _vertexTextureMap = new List<Vector2>();
        public VertexPositionNormalTexture[] _vertices;
        public List<ushort> _indicies;
        
        public DatFile(string filename)
        {

            Model currentModel = null;

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

                    case (int)BlockType.ModelName:
                        currentModel = new Model();
                        _models.Add(currentModel);
                        reader.Seek(2, SeekOrigin.Current);
                        currentModel.Name = ReadNullTerminatedString(reader);
                        Debug.WriteLine("Model: " + currentModel.Name );
                        break;

                    case (int)BlockType.Vertices:
                        ReadVertexBlock(reader, currentModel);
                        break;

                    case (int)BlockType.Faces:
                        ReadPolygonBlock(reader, currentModel);
                        break;

                    case (int)BlockType.TextureCoords:
                        ReadTextureMapBlock(reader, _vertexTextureMap);
                        break;

                    case (int)BlockType.Materials:
                        ReadMaterialsBlock(reader, currentModel);
                        break;

                    case (int)BlockType.FaceMaterials:
                        ReadFaceMaterialsBlock(reader, currentModel);
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

        private void ReadVertexBlock(EndianBinaryReader reader, Model currentModel)
        {
            currentModel.VertexBaseIndex = _vertexPositions.Count;
            currentModel.VertexCount = reader.ReadInt32();
            
            for (int i = 0; i < currentModel.VertexCount; i++)
            {
                Vector3 vertex = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                _vertexPositions.Add(vertex);
            }
        }

        private void ReadTextureMapBlock(EndianBinaryReader reader, List<Vector2> vertexUVs)
        {
            int texturePointsCount = reader.ReadInt32();
            for (int i = 0; i < texturePointsCount; i++)
            {
                float tU = reader.ReadSingle();
                float tV = reader.ReadSingle();
                vertexUVs.Add(new Vector2(tU, tV));
            }
        }

        private void ReadMaterialsBlock(EndianBinaryReader reader, Model currentModel)
        {
            currentModel.MaterialNames = new List<string>();
            int nbrMaterials = reader.ReadInt32();
            for (int i = 0; i < nbrMaterials; i++)
            {
                string material = ReadNullTerminatedString(reader);
                currentModel.MaterialNames.Add(material);
            }
        }

        private void ReadFaceMaterialsBlock(EndianBinaryReader reader, Model currentModel)
        {
            int nbrFaceMaterials = reader.ReadInt32();
            int bytesPerEntry = reader.ReadInt32();

            for (int i = 0; i < nbrFaceMaterials; i++)
            {
                int matIndex = reader.ReadInt16() - 1;   //-1 because it is 1-based
                if (matIndex > -1)
                    currentModel.Polygons[i].MaterialIndex = matIndex;
            }
        }

        private void ReadPolygonBlock(EndianBinaryReader reader, Model model)
        {
            model.Polygons = new List<Polygon>();
            
            int polygonCount = reader.ReadInt32();

            for (int i = 0; i < polygonCount; i++)
            {
                UInt16 v1 = reader.ReadUInt16();
                UInt16 v2 = reader.ReadUInt16();
                UInt16 v3 = reader.ReadUInt16();

                byte unk1 = reader.ReadByte();
                byte unk2 = reader.ReadByte();
                byte unk3 = reader.ReadByte();

                if (v1 == 99)
                {
                }

                Polygon polygon = new Polygon(v1, v2, v3);
                polygon.CalculateNormal(_vertexPositions);

                model.Polygons.Add(polygon);
            }
        }


        public void Resolve(ResourceCache resources)
        {
            _vertices = new VertexPositionNormalTexture[_vertexPositions.Count];
            int idx=0;
            
            List<UInt16> vertIndexes = new List<UInt16>(_vertexPositions.Count);
            
            foreach (Model model in _models)
            {
                model.IndexBufferStart = vertIndexes.Count;

                foreach (Polygon poly in model.Polygons)
                {
                    vertIndexes.Add(poly.Vertex1);
                    vertIndexes.Add(poly.Vertex2);
                    vertIndexes.Add(poly.Vertex3);

                    if (poly.MaterialIndex >= 0 && model.MaterialNames != null)
                    {
                        Material m = resources.GetMaterial(model.MaterialNames[poly.MaterialIndex]);
                        if (m != null)
                        {
                            poly.DoubleSided = m.DoubleSided;

                            if (m.IsSimpMat)
                            {
                                poly.Texture = m.BaseTexture;
                            }
                            else
                            {
                                PixMap pixmap = resources.GetPixelMap(m.PixName);
                                if (pixmap != null)
                                    poly.Texture = pixmap.Texture;
                            }
                        }
                    }
                }
                for (int i = 0; i < model.VertexCount; i++)
                {
                    Vector3 normal = model.Polygons[i / 3].Normal;
                    _vertices[idx++] = new VertexPositionNormalTexture(_vertexPositions[i + model.VertexBaseIndex], normal, _vertexTextureMap[i + model.VertexBaseIndex]);
                }
            }

            _vertexBuffer = new VertexBuffer(Engine.Instance.Device, VertexPositionNormalTexture.SizeInBytes * _vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData<VertexPositionNormalTexture>(_vertices);

            _indexBuffer = new IndexBuffer(Engine.Instance.Device, typeof(UInt16), vertIndexes.Count, BufferUsage.WriteOnly);
            _indexBuffer.SetData<UInt16>(vertIndexes.ToArray());
            _indicies = vertIndexes;

            _vertexDeclaration = new VertexDeclaration(Engine.Instance.Device, VertexPositionNormalTexture.VertexElements);
            _vertexTextureMap=null; //dont need this data anymore
            _vertexPositions = null;
        }

        private void Rebuffer()
        {

        }

        public BasicEffect SetupRender()
        {
            GraphicsDevice device = Engine.Instance.Device;
            device.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            device.Indices = _indexBuffer;
            device.RenderState.CullMode = CullMode.CullClockwiseFace;
            device.RenderState.FillMode = FillMode.Solid;
            device.VertexDeclaration = _vertexDeclaration;

            if (_effect == null)
            {
                _effect = new BasicEffect(Engine.Instance.Device, null);
                _effect.FogEnabled = true;
                if (GameVariables.DepthCueMode == "dark")
                    _effect.FogColor = new Vector3(0, 0, 0);
                else if (GameVariables.DepthCueMode == "fog" || GameVariables.DepthCueMode == "none")
                    _effect.FogColor = new Vector3(245, 245, 245);
                else
                {
                    Debug.Assert(false);
                }
                _effect.FogStart = Engine.Instance.DrawDistance - 20 * GameVariables.Scale.Z;
                _effect.FogEnd = Engine.Instance.DrawDistance;
                //effect.LightingEnabled = true;
                //effect.EnableDefaultLighting();
                //effect.AmbientLightColor = new Vector3(0.09f, 0.09f, 0.1f);
                //effect.DirectionalLight0.Direction = new Vector3(1.0f, -1.0f, -1.0f); 
                _effect.TextureEnabled = true;
            }

            _effect.View = Engine.Instance.Camera.View;
            _effect.Projection = Engine.Instance.Camera.Projection;

            _effect.Begin(SaveStateMode.None);
            return _effect;
        }


        public void DoneRender()
        {
            _effect.End();
        }

        public Model GetModel(string name)
        {
            return _models.Find(m => m.Name == name); 
        }

        public List<Model> GetModels()
        {
            return _models;
        }

        public void Crush(CrushSection crush)
        {
            foreach (CrushData data in crush.Data)
            {
                Vector3 pos = _vertices[data.RefVertex].Position;
                //Engine.Instance.GraphicsUtils.AddSolidShape(ShapeType.Cube, Matrix.CreateTranslation(pos), Color.White, null);
                Vector3 v = Vector3.Lerp(data.V1, data.V2, (float)new Random().NextDouble());
                _vertices[data.RefVertex].Position = v;// = Vector3.Transform(pos, data.Matrix);
            }

            _vertexBuffer.SetData<VertexPositionNormalTexture>(_vertices);
        }
    }
}
