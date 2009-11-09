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

namespace Carmageddon.Parsers
{

    class Actor
    {
        public string Name { get; private set; }
        public string ModelName { get; set; }
        public Model Model { get; set; }
        public string MaterialName { get; set; }
        public Texture2D Texture { get; set; }
        public Matrix Matrix { get; set; }
        internal List<Actor> Children { get; set; }
        public int Level { get; set; }
        public BoundingBox BoundingBox;
        public byte[] Flags;
        public bool IsWheel;
        public Actor()
        {
            Children = new List<Actor>();
        }

        public void SetName(string name)
        {
            Name = name;
            IsWheel = (name.StartsWith("FLPIVOT") || name.StartsWith("FRPIVOT") || name.StartsWith("RLWHEEL") || name.StartsWith("RRWHEEL"));
        }
    }

    class Hierarchy
    {
        internal List<Actor> Children { get; set; }

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
        bool _cullingDisabled = false;
        
        public ActFile(string filename, DatFile modelFile, bool resolveTransforms)
        {
            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, File.Open(filename, FileMode.Open));

            Actor currentActor = null;
            Stack<Actor> actorStack = new Stack<Actor>();
            List<Actor> flatActorList = new List<Actor>();

            while (true)
            {
                int blockLength = 0;
                ActorBlockType blockType = (ActorBlockType)reader.ReadInt32();
                blockLength = reader.ReadInt32();

                switch (blockType)
                {
                    case ActorBlockType.Name:

                        currentActor = new Actor();

                        if (actorStack.Count == 0)
                        {
                            _actors.Add(currentActor);
                        }
                        else
                        {
                            actorStack.Peek().Children.Add(currentActor);
                            currentActor.Level = actorStack.Peek().Level + 1;
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
                        currentActor.Model = modelFile.GetModel(modelName);
                        //Debug.WriteLine("ModelName: " + modelName);
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
            
            if (resolveTransforms)
                ResolveTransformations(Matrix.Identity, _actors[0]);

            ScaleTransformations(Matrix.CreateScale(GameVariables.Scale), _actors[0]);
        }

        /// <summary>
        /// Pre-calculate recursive transformations and apply scaling
        /// </summary>
        private void ResolveTransformations(Matrix world, Actor actor)
        {                
            Debug.WriteLine(actor.Name + ", " + actor.ModelName + ", " + actor.Flags[0] + ":" + actor.Flags[1]);
            actor.Matrix = world * actor.Matrix;
            foreach (Actor child in actor.Children)
                ResolveTransformations(actor.Matrix, child);
        }

        private void ScaleTransformations(Matrix scale, Actor actor)
        {
            actor.Matrix *= scale;
            foreach (Actor child in actor.Children)
                ScaleTransformations(scale, child);
        }


        public void ResolveMaterials(ResourceCache resources)
        {
            Action<Actor> resolver = null;
            resolver = (actor) =>
            {
                if (actor.MaterialName != null)
                {
                    Material material = resources.GetMaterial(actor.MaterialName);
                    if (material != null)
                    {
                        PixMap pixMap = resources.GetPixelMap(material.PixName);
                        if (pixMap != null)
                            actor.Texture = pixMap.Texture;
                    }
                }
                foreach (Actor child in actor.Children)
                    resolver(child);
            };
            resolver(_actors[0]);
        }

        public List<Actor> GetAllActors()
        {
            List<Actor> actors = new List<Actor>();
            Action<Actor> resolver = null;
            resolver = (actor) =>
            {
                actors.Add(actor);
                foreach (Actor child in actor.Children)
                    resolver(child);
            };
            resolver(_actors[0]);
            return actors;
        }

        public Actor First
        {
            get { return _actors[0]; }
        }  

        public Actor GetByName(string name)
        {
            string nameWithExt = name + ".ACT";
            List<Actor> all = GetAllActors();
            return all.Find(a => a.Name == nameWithExt || a.Name == name);
        }


        public void Render(DatFile models, Matrix world)
        {
            GameVariables.NbrSectionsRendered = GameVariables.NbrSectionsChecked = 0;

            BoundingFrustum frustum = new BoundingFrustum(Engine.Instance.Camera.View * Engine.Instance.Camera.Projection);

            Engine.Instance.CurrentEffect = models.SetupRender();

            bool overrideActor = world != Matrix.Identity;

            Engine.Instance.CurrentEffect.CurrentTechnique.Passes[0].Begin();



            //Engine.Instance.Device.SamplerStates[0].MipMapLevelOfDetailBias = -1.9f;

            for (int i = 0; i < _actors.Count; i++)
            {
                RenderChildren(frustum, _actors[i], ref world, overrideActor);
            }

            Engine.Instance.CurrentEffect.CurrentTechnique.Passes[0].End();

            models.DoneRender();

            GameConsole.WriteLine("Checked: " + GameVariables.NbrSectionsChecked + ", Rendered: " + GameVariables.NbrSectionsRendered);
        }

        private void RenderChildren(BoundingFrustum frustum, Actor actor, ref Matrix world, bool overrideMatrix)
        {
            if (actor.IsWheel) return;

            bool intersects;
            
            intersects = actor.BoundingBox.Max.X == 0;
            if (!intersects)
            {
                frustum.Intersects(ref actor.BoundingBox, out intersects);
                GameVariables.NbrSectionsChecked++;
            }
            
            if (intersects)
            {
                if (actor.Model != null)
                {
                    //Render
                    Matrix m = actor.Matrix;
                    if (actor.Level == 0 && overrideMatrix)
                        m.Translation = Vector3.Zero;

                    Engine.Instance.CurrentEffect.World = m * world;
                    Engine.Instance.CurrentEffect.CommitChanges();
                    
                    actor.Model.Render(actor.Texture);

                    GameVariables.NbrSectionsRendered++;
                }
                foreach (Actor child in actor.Children)
                    RenderChildren(frustum, child, ref world, overrideMatrix);
            }            
        }

        public void RenderSingle(Actor actor)
        {
            Matrix m = actor.Matrix;
            m.Translation = Vector3.Zero;
            Engine.Instance.CurrentEffect.World = m * Engine.Instance.CurrentEffect.World;
            Engine.Instance.CurrentEffect.CommitChanges();
            
            actor.Model.Render(actor.Texture);            
        }
    }
}
