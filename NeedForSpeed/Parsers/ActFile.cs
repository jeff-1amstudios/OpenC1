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

    class CActor
    {
        public string Name { get; private set; }
        public string ModelName { get; set; }
        public Model Model { get; set; }
        public string MaterialName { get; set; }
        public Texture2D Texture { get; set; }
        public Matrix Matrix;
        internal List<CActor> Children { get; set; }
        public int Level { get; set; }
        public BoundingBox BoundingBox;
        public byte[] Flags;
        public bool IsWheel;
        private StillDesign.PhysX.Actor _physXActor;
        public CActor()
        {
            Children = new List<CActor>();
        }

        public void SetName(string name)
        {
            Name = name;
            IsWheel = (name.StartsWith("FLPIVOT") || name.StartsWith("FRPIVOT") || name.StartsWith("RLWHEEL") || name.StartsWith("RRWHEEL"));
        }

        internal void AttachPhysxActor(StillDesign.PhysX.Actor instance)
        {
            // if this CActor is attached to a PhysX object, reduce the Matrix to a scale, 
            // as the position/orienation will come from PhysX
            _physXActor = instance;
            Vector3 scaleout, transout;
            Quaternion b;
            Matrix.Decompose(out scaleout, out b, out transout);
            Matrix = Matrix.CreateScale(scaleout);
        }

        public Matrix GetDynamicMatrix()
        {
            if (_physXActor == null) return Matrix;
            return Matrix * _physXActor.GlobalPose;
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


        List<CActor> _actors = new List<CActor>();
        
        public ActFile(string filename, DatFile modelFile, bool resolveTransforms)
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



            if (resolveTransforms)
            {
                ResolveTransformations(Matrix.Identity, _actors[0]);
            }


            ScaleTransformations(GameVariables.Scale, _actors[0]);  
        }

        /// <summary>
        /// Pre-calculate recursive transformations and apply scaling
        /// </summary>
        private void ResolveTransformations(Matrix world, CActor actor)
        {                
            Debug.WriteLine(actor.Name + ", " + actor.ModelName + ", " + actor.Flags[0] + ":" + actor.Flags[1]);
            if (actor.Matrix.Translation != Vector3.Zero)
            {
            }
            actor.Matrix = world * actor.Matrix;

            //Vector3 scale2, trans;
            //Quaternion rot;
            //actor.Matrix.Decompose(out scale2, out rot, out trans);

            //scale2 *= scale;
            //trans *= scale;
            //actor.Matrix = world * Matrix.CreateScale(scale2) *
            //    Matrix.CreateFromQuaternion(rot) *
            //    Matrix.CreateTranslation(trans);


            foreach (CActor child in actor.Children)
                ResolveTransformations(actor.Matrix, child);
        }

        private void ScaleTransformations(Vector3 scale, CActor actor)
        {
            Vector3 scale2, trans;
            Quaternion rot;
            actor.Matrix.Decompose(out scale2, out rot, out trans);
            actor.Matrix = actor.Matrix * Matrix.CreateScale(scale);
            //scale2 *= scale;
            //trans *= scale;
            //actor.Matrix = Matrix.CreateScale(scale2) *
            //    Matrix.CreateFromQuaternion(rot) *
            //    Matrix.CreateTranslation(trans);

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
                    Material material = resources.GetMaterial(actor.MaterialName);
                    if (material != null)
                    {
                        PixMap pixMap = resources.GetPixelMap(material.PixName);
                        if (pixMap != null)
                            actor.Texture = pixMap.Texture;
                    }
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

            Engine.Instance.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < _actors.Count; i++)
            {
                RenderChildren(frustum, _actors[i], ref world, overrideActor);
            }

            Engine.Instance.CurrentEffect.CurrentTechnique.Passes[0].End();

            GameConsole.WriteLine("Checked: " + GameVariables.NbrSectionsChecked + ", Rendered: " + GameVariables.NbrSectionsRendered);
        }

        private void RenderChildren(BoundingFrustum frustum, CActor actor, ref Matrix world, bool overrideMatrix)
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
                    Matrix m = actor.GetDynamicMatrix();
                    if (actor.Level == 0 && overrideMatrix)
                        m.Translation = Vector3.Zero;

                    Engine.Instance.CurrentEffect.World = m * world;
                    Engine.Instance.CurrentEffect.CommitChanges();
                    
                    actor.Model.Render(actor.Texture);

                    //    Vector3 a, c;
                    //    Quaternion b;
                    //    actor.Matrix.Decompose(out a, out b, out c);

                    //    //Engine.Instance.DebugRenderer.AddAxis(actor.Matrix, 4);
                    //    //if (b.Y == 0)
                    //    //{
                    //    //float w = actor.bb.Max.X - actor.bb.Min.X;
                    //    //float h = actor.bb.Max.Y - actor.bb.Min.Y;
                    //    //float l = actor.bb.Max.Z - actor.bb.Min.Z;
                    //    //Engine.Instance.DebugRenderer.AddWireframeCube(
                    //    //    Matrix.CreateScale(w, h, l) *
                    //    //    Matrix.CreateTranslation(actor.bb.GetCenter()) *
                            
                    //    //    Matrix.CreateFromQuaternion(b) *
                    //    //    Matrix.CreateTranslation(c)
                    //    //    , Color.Yellow);
                    //    //}
                        
                    //}

                    GameVariables.NbrSectionsRendered++;
                }
                foreach (CActor child in actor.Children)
                    RenderChildren(frustum, child, ref world, overrideMatrix);
            }            
        }

        public void RenderSingle(CActor actor)
        {
            Matrix m = actor.Matrix;
            m.Translation = Vector3.Zero;
            Engine.Instance.CurrentEffect.World = m * Engine.Instance.CurrentEffect.World;
            Engine.Instance.CurrentEffect.CommitChanges();
            
            actor.Model.Render(actor.Texture);            
        }

        public void RenderSingleModel(CActor actor, Matrix m)
        {
            Engine.Instance.CurrentEffect.World = m;
            Engine.Instance.CurrentEffect.CommitChanges();
            actor.Model.Render(actor.Texture);
        }
    }
}
