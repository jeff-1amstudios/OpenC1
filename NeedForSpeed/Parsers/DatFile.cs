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
    }

    class DatFile : BaseDataFile
    {
        private VertexBuffer _vertexBuffer;
        private BasicEffect _effect;
        
        List<Model> _models = new List<Model>();

        public DatFile(string filename)
        {
            //List<Polygon> _polygons = new List<Polygon>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> vertexTextureMap = new List<Vector2>();
            
            Model currentModel=null;

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
                        ReadVertexBlock(reader, vertices);
                        break;

                    case (int)BlockType.Faces:
                        ReadPolygonBlock(reader, currentModel, vertices, vertexTextureMap);
                        break;

                    case (int)BlockType.TextureCoords:
                        ReadTextureMapBlock(reader, vertexTextureMap);
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

        private void ReadVertexBlock(EndianBinaryReader reader, List<Vector3> vertices)
        {
            vertices.Clear();
            int vertexCount = reader.ReadInt32();
            
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 vertex = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                //vertex *= GameConfig.Scale;
                vertices.Add(vertex);
            }
        }

        private void ReadTextureMapBlock(EndianBinaryReader reader, List<Vector2> vertexUVs)
        {
            vertexUVs.Clear();
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

        private void ReadPolygonBlock(EndianBinaryReader reader, Model model, List<Vector3> vertices, List<Vector2> textureUVs)
        {
            model.Polygons = new List<Polygon>();

            int polygonCount = reader.ReadInt32();

            for (int i = 0; i < polygonCount; i++)
            {
                Polygon polygon = new Polygon();

                int v1 = reader.ReadInt16();
                int v2 = reader.ReadInt16();
                int v3 = reader.ReadInt16();
                byte unk1 = reader.ReadByte();
                byte unk2 = reader.ReadByte();
                byte unk3 = reader.ReadByte();

                polygon.Vertices.Add(vertices[v1]);
                polygon.Vertices.Add(vertices[v2]);
                polygon.Vertices.Add(vertices[v3]);
                polygon.TextureCoords.Add(textureUVs[v1]);
                polygon.TextureCoords.Add(textureUVs[v2]);
                polygon.TextureCoords.Add(textureUVs[v3]);

                model.Polygons.Add(polygon);
            }
        }


        public void Resolve(ResourceCache resources)
        {
            int vertCount = 0;
            
            List<VertexPositionNormalTexture> allVerts = new List<VertexPositionNormalTexture>();
            foreach (Model model in _models)
            {
                foreach (Polygon poly in model.Polygons)
                {
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
                    poly.VertexBufferIndex = vertCount;
                    vertCount += poly.VertexCount;
                    allVerts.AddRange(poly.GetVertices());
                }
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
                _effect.FogStart = GameVariables.DrawDistance - 1000;
                _effect.FogEnd = GameVariables.DrawDistance;
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
    }
}
