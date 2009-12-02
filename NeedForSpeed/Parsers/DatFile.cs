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

    
    class DatFile : BaseDataFile
    {
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private VertexDeclaration _vertexDeclaration;
        
        List<Model> _models = new List<Model>();
        List<Vector3> _vertexPositions = new List<Vector3>();
        List<Vector2> _vertexTextureMap = new List<Vector2>();
        public VertexPositionNormalTexture[] _vertices;
        public List<ushort> _indices;
        
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
                model.Polygons.Sort(delegate(Polygon p1, Polygon p2) { return p1.MaterialIndex.CompareTo(p2.MaterialIndex); });

                Polygon currentPoly = null;

                foreach (Polygon poly in model.Polygons)
                {
                    poly.NbrPrims = 1;
                    vertIndexes.Add(poly.Vertex1);
                    vertIndexes.Add(poly.Vertex2);
                    vertIndexes.Add(poly.Vertex3);

                    if (poly.MaterialIndex >= 0 && model.MaterialNames != null)
                    {
                        CMaterial material = resources.GetMaterial(model.MaterialNames[poly.MaterialIndex]);
                        if (material != null)
                        {
                            poly.DoubleSided = material.DoubleSided;
                            poly.Material = material;
                        }

                        if (currentPoly != null && poly.MaterialIndex == currentPoly.MaterialIndex)
                        {
                            poly.Skip = true;
                            currentPoly.NbrPrims = currentPoly.NbrPrims + 1;
                        }
                        else
                        {
                            currentPoly = poly;
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
            _indices = vertIndexes;

            _vertexDeclaration = new VertexDeclaration(Engine.Instance.Device, VertexPositionNormalTexture.VertexElements);
            _vertexTextureMap=null; //dont need this data anymore
            _vertexPositions = null;
        }


        public void SetupRender()
        {
            GraphicsDevice device = Engine.Instance.Device;
            device.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            device.Indices = _indexBuffer;
            device.VertexDeclaration = _vertexDeclaration;
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
