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

namespace NeedForSpeed.Parsers
{

    class Actor
    {
        public string Name { get; set; }
        public string ModelName { get; set; }
        public Matrix Matrix { get; set; }
        public List<Actor> Children { get; set; }

        public Actor()
        {
            Children = new List<Actor>();
        }
    }

    class ActFileParser : BaseParser
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

        public void Parse(string filename)
        {
            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, File.Open(filename, FileMode.Open));

            Actor currentActor = null;
            Stack<Actor> _actorStack = new Stack<Actor>();

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
                            _actors.Add(currentActor);
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
                        string material = ReadNullTerminatedString(reader);
                        break;

                    case ActorBlockType.ModelName:
                        currentActor.ModelName = ReadNullTerminatedString(reader);
                        Debug.WriteLine("ModelName: " + currentActor.ModelName);

                        for (int i = _actors.Count - 1; i >= 0; i--)
                        {
                            if (_actors[i].ModelName != null)
                                break;
                            _actors[i].ModelName = currentActor.ModelName;
                        }
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

            foreach (Actor a in _actors)
                Resolve(a, Matrix.Identity);
        }

        private void Resolve(Actor a, Matrix world)
        {
            a.Matrix = world * a.Matrix;   
            foreach (Actor child in a.Children)
                Resolve(child, a.Matrix);
        }


        public void Render(Matrix world, DatFileParser models)
        {
            BasicEffect effect = models.SetupRender();
            foreach (Actor a in _actors)
            {
                RenderInternal(a, world, effect, models);
            }
            models.DoneRender(effect);
        }

        private void RenderInternal(Actor a, Matrix world, BasicEffect effect, DatFileParser models)
        {
            if (a.ModelName != null) models.Render(a.Matrix * world, effect, a.ModelName);
            foreach (Actor child in a.Children)
                RenderInternal(child, world, effect, models);
        }
    }
}
