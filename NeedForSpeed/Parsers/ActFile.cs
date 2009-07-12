using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon.Parsers
{

    class Actor
    {
        public string Name { get; set; }
        public string ModelName { get; set; }
        public string MaterialName { get; set; }
        public Texture2D Texture { get; set; }
        public Matrix Matrix { get; set; }
        internal List<Actor> Children { get; set; }

        public Actor()
        {
            Children = new List<Actor>();
        }
    }

    class ActFile : BaseDataFile
    {
        enum ActorBlockType
        {
            Null = 0,
            Name = 35,
            ModelName = 36,
            Unknown = 37,
            MaterialNames = 38,
            TransformMatrix = 43,
            HierarchyStart = 41,
            ActorEnd = 42,
            BoundingBox = 50
        }


        List<Actor> _actors = new List<Actor>();

        public ActFile(string filename)
        {
            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, File.Open(filename, FileMode.Open));

            Actor currentActor = null;
            Stack<Actor> _actorStack = new Stack<Actor>();
            List<Actor> actorsTemp = new List<Actor>();

            while (true)
            {
                int blockLength = 0;
                ActorBlockType blockType = (ActorBlockType)reader.ReadInt32();
                blockLength = reader.ReadInt32();

                switch (blockType)
                {
                    case ActorBlockType.Name:

                        currentActor = new Actor();

                        if (_actorStack.Count > 0)
                            _actorStack.Peek().Children.Add(currentActor);
                        else
                        {
                            actorsTemp.Add(currentActor);
                        }

                        _actorStack.Push(currentActor);

                        reader.Seek(2, SeekOrigin.Current);
                        currentActor.Name = ReadNullTerminatedString(reader);
                        Debug.WriteLine("Name: " + currentActor.Name);
                        break;

                    case ActorBlockType.TransformMatrix:

                        Matrix matrix = new Matrix();
                        matrix.M11 = reader.ReadSingle();
                        matrix.M12 = reader.ReadSingle();
                        matrix.M13 = reader.ReadSingle();

                        matrix.M21 = reader.ReadSingle();
                        matrix.M22 = reader.ReadSingle();
                        matrix.M23 = reader.ReadSingle();

                        matrix.M31 = reader.ReadSingle();
                        matrix.M32 = reader.ReadSingle();
                        matrix.M33 = reader.ReadSingle();

                        matrix.M41 = reader.ReadSingle();
                        matrix.M42 = reader.ReadSingle();
                        matrix.M43 = reader.ReadSingle();
                        matrix.M44 = 1;
                        
                        currentActor.Matrix = matrix;
                        Debug.WriteLine(matrix);

                        break;

                    case ActorBlockType.HierarchyStart:
                        Debug.WriteLine("Hierarchy start");
                        break;

                    case ActorBlockType.ActorEnd:
                        Debug.WriteLine("Actor end");
                        _actorStack.Pop();
                        break;

                    case ActorBlockType.MaterialNames:
                        currentActor.MaterialName = ReadNullTerminatedString(reader);
                        break;

                    case ActorBlockType.ModelName:
                        currentActor.ModelName = ReadNullTerminatedString(reader);
                        Debug.WriteLine("ModelName: " + currentActor.ModelName);
                        break;

                    case ActorBlockType.Null:
                        break;

                    default:
                        reader.Seek(blockLength, SeekOrigin.Current);
                        break;
                }
                if (reader.BaseStream.Position == reader.BaseStream.Length)
                    break;
            }
            reader.Close();

            // Flatten out the tree structure for performance
            foreach (Actor a in actorsTemp)
                ResolveHierarchy(a, Matrix.Identity);

            foreach (Actor a in _actors)
                a.Children = null;
        }

        private void ResolveHierarchy(Actor a, Matrix world)
        {
            a.Matrix = world * a.Matrix;
            _actors.Add(a);
            foreach (Actor child in a.Children)
                ResolveHierarchy(child, a.Matrix);
        }


        public void ResolveMaterials(MatFile materials, PixFile pix)
        {
            foreach (Actor a in _actors)
            {
                if (a.MaterialName != null)
                {
                    Material material = materials.GetMaterial(a.MaterialName);
                    if (material != null)
                    {
                        PixMap pixMap = pix.GetPixelMap(material.PixName);
                        if (pixMap != null)
                            a.Texture = pixMap.Texture;
                    }
                }
            }
        }


        public void Render(Matrix world, DatFile models)
        {
            BasicEffect effect = models.SetupRender();
            foreach (Actor a in _actors)
            {
                if (a.ModelName != null) models.Render(a.Matrix * world, effect, a);
            }
            models.DoneRender(effect);
        }
    }
}
