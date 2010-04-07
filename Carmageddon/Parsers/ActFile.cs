using System;
using System.Collections.Generic;

using System.Text;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using NFSEngine;
using Carmageddon.Parsers.Grooves;

namespace Carmageddon.Parsers
{

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

        CActorHierarchy _actors = new CActorHierarchy();

        public CActorHierarchy Hierarchy
        {
            get { return _actors; }
        }

        public ActFile(string filename, CModelGroup models)
        {
            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, File.Open(filename, FileMode.Open));

            CActor currentActor = null;
            Stack<CActor> actorStack = new Stack<CActor>();
            List<CActor> flatActorList = new List<CActor>();

            while (true)
            {
                int blockLength = 0;
                ActorBlockType blockType = (ActorBlockType)reader.ReadInt32();
                blockLength = reader.ReadInt32();

                switch (blockType)
                {
                    case ActorBlockType.Name:

                        currentActor = new CActor();

                        if (actorStack.Count == 0)
                        {
                            _actors.Add(currentActor);
                        }
                        else
                        {
                            CActor parent = actorStack.Peek();
                            currentActor.Parent = parent;
                            parent.Children.Add(currentActor);
                        }

                        flatActorList.Add(currentActor);
                        actorStack.Push(currentActor);

                        currentActor.Flags = reader.ReadBytes(2);
                        currentActor.SetName(ReadNullTerminatedString(reader));
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

                        break;

                    case ActorBlockType.HierarchyStart:
                        //Debug.WriteLine("Hierarchy start");
                        break;

                    case ActorBlockType.ActorEnd:
                        actorStack.Pop();
                        //Debug.WriteLine("Hierarchy end");
                        break;

                    case ActorBlockType.MaterialNames:
                        currentActor.MaterialName = ReadNullTerminatedString(reader);
                        break;

                    case ActorBlockType.ModelName:
                        string modelName = ReadNullTerminatedString(reader);
                        currentActor.ModelName = modelName;
                        currentActor.Model = models.GetModel(modelName);
                        break;

                    case ActorBlockType.BoundingBox:
                        currentActor.BoundingBox = new BoundingBox(
                            new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) * GameVariables.Scale,
                            new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) * GameVariables.Scale
                            );
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

            _actors.Models = models;
            _actors.ResolveMaterials();
        }        
    }
}
