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

        List<CActor> _actors = new List<CActor>();

        public ActFile(string filename, DatFile modelFile)
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
        }

        /// <summary>
        /// Pre-calculate recursive transformations and apply scaling, ignoring groove animations
        /// </summary>
        public void ResolveHierarchy(bool removeRootTransform, List<BaseGroove> grooves)
        {
            if (removeRootTransform)
            {
                _actors[0].Matrix.Translation = Vector3.Zero;
            }

            ResolveTransformations(Matrix.Identity, _actors[0], grooves);
            ScaleTransformations(GameVariables.Scale, _actors[0]);
        }

        private void ResolveTransformations(Matrix world, CActor actor, List<BaseGroove> grooves)
        {
            if (grooves.Exists(g => g.ActorName == actor.Name))
            {
                actor.ParentMatrix = world;
                actor.IsAnimated = true;
                return;
            }
            //Debug.WriteLine(actor.Name + ", " + actor.ModelName + ", " + actor.Flags[0] + ":" + actor.Flags[1] + "Animated: " + actor.IsAnimated);
            actor.Matrix = world * actor.Matrix;

            foreach (CActor child in actor.Children)
                ResolveTransformations(actor.Matrix, child, grooves);
        }

        private void ScaleTransformations(Vector3 scale, CActor actor)
        {
            if (actor.IsAnimated) return;
            actor.Matrix = actor.Matrix * Matrix.CreateScale(scale);

            foreach (CActor child in actor.Children)
                ScaleTransformations(scale, child);
        }

        public void ResolveMaterials(ResourceCache resources)
        {
            Action<CActor> resolver = null;
            resolver = (actor) =>
            {
                if (actor.MaterialName != null)
                {
                    actor.Material = resources.GetMaterial(actor.MaterialName);
                }
                foreach (CActor child in actor.Children)
                    resolver(child);
            };
            resolver(_actors[0]);
        }

        public List<CActor> GetAllActors()
        {
            List<CActor> actors = new List<CActor>();
            Action<CActor> resolver = null;
            resolver = (actor) =>
            {
                actors.Add(actor);
                foreach (CActor child in actor.Children)
                    resolver(child);
            };
            resolver(_actors[0]);
            return actors;
        }

        public CActor First
        {
            get { return _actors[0]; }
        }  

        public CActor GetByName(string name)
        {
            string nameWithExt = name + ".ACT";
            List<CActor> all = GetAllActors();
            return all.Find(a => a.Name == nameWithExt || a.Name == name);
        }


        public void Render(DatFile models, Matrix world)
        {
            GameVariables.NbrSectionsRendered = GameVariables.NbrSectionsChecked = 0;

            BoundingFrustum frustum = new BoundingFrustum(Engine.Instance.Camera.View * Engine.Instance.Camera.Projection);
            

            bool overrideActor = world != Matrix.Identity;

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < _actors.Count; i++)
            {
                RenderChildren(frustum, _actors[i], world, false);
            }

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();

            GameConsole.WriteLine("Checked: " + GameVariables.NbrSectionsChecked + ", Rendered: " + GameVariables.NbrSectionsRendered);
        }

        private void RenderChildren(BoundingFrustum frustum, CActor actor, Matrix world, bool parentAnimated)
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
                    Matrix m = actor.GetDynamicMatrix();

                    if (actor.IsAnimated || parentAnimated)
                    {
                        
                        if (actor.IsAnimated && !parentAnimated)
                        {
                            world = m * actor.ParentMatrix * GameVariables.ScaleMatrix * world;
                        }
                        else
                        {
                            world = m * world;
                        }

                        GameVariables.CurrentEffect.World = world;
                        parentAnimated = true;
                    }
                    else
                    {
                        GameVariables.CurrentEffect.World = m * world;
                    }

                    GameVariables.CurrentEffect.CommitChanges();

                    actor.Model.Render(actor.Material);

                    GameVariables.NbrSectionsRendered++;
                }
                foreach (CActor child in actor.Children)
                    RenderChildren(frustum, child, world, parentAnimated);
            }            
        }

        public void RenderSingle(CActor actor)
        {
            Matrix m = actor.Matrix;
            m.Translation = Vector3.Zero;
            GameVariables.CurrentEffect.World = m * GameVariables.CurrentEffect.World;
            GameVariables.CurrentEffect.CommitChanges();
            
            actor.Model.Render(actor.Material);            
        }

        public Matrix CalculateDynamicActorMatrix(CActor actorToFind)
        {
            bool done = false;
            Matrix m = CalculateDynamicActorMatrixInternal(_actors[0], Matrix.Identity, actorToFind, ref done);
            return m * GameVariables.ScaleMatrix;
        }

        private Matrix CalculateDynamicActorMatrixInternal(CActor actor, Matrix matrix, CActor actorToFind, ref bool done)
        {
            if (done) return matrix;

            matrix = matrix * actor.Matrix;

            if (actorToFind == actor)
            {
                done = true;
                return matrix;
            }
            foreach (CActor child in actor.Children)
            {
                Matrix m = CalculateDynamicActorMatrixInternal(child, matrix, actorToFind, ref done);
                if (done) return m;
            }
            return matrix;
        }
    }
}
