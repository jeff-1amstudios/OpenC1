using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using MiscUtil.IO;
using MiscUtil.Conversion;
using StillDesign.PhysX;
using OpenC1.Physics;

namespace OpenC1.Parsers
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
        CModelGroup _models = new CModelGroup();

        public CModelGroup Models
        {
            get { return _models; }
        }

        public DatFile(string filename)
            : this(filename, false)
        {
        }

        public DatFile(string filename, bool deformMainModel)
        {
			if (filename.EndsWith(".ACT", StringComparison.InvariantCultureIgnoreCase))
				filename = filename.ToUpper().Replace(".ACT", ".DAT"); //fix up some 3rd party vehicle weirdness

            CModel currentModel = null;

            Stream file = OpenDataFile(filename);
            if (!Exists)
                return;

            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, file);

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
                        reader.Seek(2, SeekOrigin.Current);
                        string name = ReadNullTerminatedString(reader);

                        if (deformMainModel && Path.GetFileNameWithoutExtension(name).Equals(Path.GetFileNameWithoutExtension(filename), StringComparison.InvariantCultureIgnoreCase))
                            currentModel = new CDeformableModel();
                        else
                            currentModel = new CModel();

                        currentModel.Name = name;
                        _models.Add(currentModel);

                        break;

                    case (int)BlockType.Vertices:
                        ReadVertexBlock(reader, currentModel);
                        break;

                    case (int)BlockType.Faces:
                        ReadPolygonBlock(reader, currentModel);
                        break;

                    case (int)BlockType.TextureCoords:
                        ReadTextureMapBlock(reader, currentModel);
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
			if (filename == "FAUST.DAT")
			{
			}
            _models.Resolve(true);
        }

        private void ReadVertexBlock(EndianBinaryReader reader, CModel currentModel)
        {
            currentModel.VertexBaseIndex = _models._vertexPositions.Count;
            currentModel.VertexCount = reader.ReadInt32();

            for (int i = 0; i < currentModel.VertexCount; i++)
            {
                Vector3 vertex = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                _models._vertexPositions.Add(vertex);
            }
        }

        private void ReadTextureMapBlock(EndianBinaryReader reader, CModel currentModel)
        {
            currentModel.TextureMapCount = reader.ReadInt32();
            for (int i = 0; i < currentModel.TextureMapCount; i++)
            {
                float tU = reader.ReadSingle();
                float tV = reader.ReadSingle();
                _models._vertexTextureMap.Add(new Vector2(tU, tV));
            }
        }

        private void ReadMaterialsBlock(EndianBinaryReader reader, CModel currentModel)
        {
            currentModel.MaterialNames = new List<string>();
            int nbrMaterials = reader.ReadInt32();
            for (int i = 0; i < nbrMaterials; i++)
            {
                string material = ReadNullTerminatedString(reader);
                currentModel.MaterialNames.Add(material);
            }
        }

        private void ReadFaceMaterialsBlock(EndianBinaryReader reader, CModel currentModel)
        {
            int nbrFaceMaterials = reader.ReadInt32();
            int bytesPerEntry = reader.ReadInt32();

            for (int i = 0; i < nbrFaceMaterials; i++)
            {
                int matIndex = reader.ReadInt16() - 1;   //-1 because it is 1-based
                currentModel.Polygons[i].MaterialIndex = matIndex;
            }
        }

        private void ReadPolygonBlock(EndianBinaryReader reader, CModel model)
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
                polygon.CalculateNormal(_models._vertexPositions, model.VertexBaseIndex);
                model.Polygons.Add(polygon);
            }
        }
    }
}
