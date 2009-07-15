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
using PlatformEngine;
using NFSEngine;

namespace Carmageddon.Parsers
{

    class Actor
    {
        public string Name { get; set; }
        public string ModelName { get; set; }
        public Model Model { get; set; }
        public string MaterialName { get; set; }
        public Texture2D Texture { get; set; }
        public Matrix Matrix { get; set; }
        internal List<Actor> Children { get; set; }
        public int Level { get; set; }
        public BoundingBox BoundingBox;

        public Actor()
        {
            Children = new List<Actor>();
        }

        public bool IsTopLevel
        {
            get { return Name.Contains(" ") && Level > 0; }
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
        
        public ActFile(string filename, DatFile modelFile)
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

                        reader.Seek(2, SeekOrigin.Current);
                        currentActor.Name = ReadNullTerminatedString(reader);
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

            // Pre-calculate recursive transformations
            foreach (Actor actor in _actors)
                ResolveTransformations(actor, Matrix.Identity, 0);

            Matrix scale = Matrix.CreateScale(GameVariables.Scale);
            foreach (Actor a in flatActorList)
                a.Matrix = a.Matrix * scale;
        }

        private void ResolveTransformations(Actor actor, Matrix world, int level)
        {
            actor.Level = level;
            //Debug.WriteLine(level + " - " + actor.Name);
            actor.Matrix = world * actor.Matrix;
            foreach (Actor child in actor.Children)
                ResolveTransformations(child, actor.Matrix, level + 1);
        }


        public void ResolveMaterials(ResourceCache resources)
        {
            foreach (Actor a in _actors)
            {
                if (a.MaterialName != null)
                {
                    Material material = resources.GetMaterial(a.MaterialName);
                    if (material != null)
                    {
                        PixMap pixMap = resources.GetPixelMap(material.PixName);
                        if (pixMap != null)
                            a.Texture = pixMap.Texture;
                    }
                }
            }
        }


        public void Render(DatFile models)
        {
            GameVariables.NbrSectionsRendered = GameVariables.NbrSectionsChecked = 0;

            BasicEffect effect = models.SetupRender();
            BoundingFrustum frustum = new BoundingFrustum(Engine.Instance.Camera.View * Engine.Instance.Camera.Projection);
            
            for (int i = 0; i < _actors.Count; i++)
            {
                RenderChildren(frustum, _actors[i], effect);
            }

            models.DoneRender();

            GameConsole.WriteLine("Checked: " + GameVariables.NbrSectionsChecked + ", Rendered: " + GameVariables.NbrSectionsRendered, 0);
        }

        private void RenderChildren(BoundingFrustum frustum, Actor actor, BasicEffect effect)
        {
            bool intersects;
            
            intersects = actor.BoundingBox.Max.X == 0;
            if (!intersects)
            {
                frustum.Intersects(ref actor.BoundingBox, out intersects);
                GameVariables.NbrSectionsChecked++;
            }
            
            if (intersects)
            {
                //Vector3 center = (_topLevelActors[i].BoundingBox.Max + _topLevelActors[i].BoundingBox.Min) / 2;
                //Vector3 size = _topLevelActors[i].BoundingBox.Max - _topLevelActors[i].BoundingBox.Min;
                //Engine.Instance.GraphicsUtils.AddWireframeCube(Matrix.CreateScale(size) * Matrix.CreateTranslation(center), Color.Yellow);
                
                if (actor.Model != null)
                {
                    Render(effect, actor);
                    GameVariables.NbrSectionsRendered++;
                }
                foreach (Actor child in actor.Children)
                    RenderChildren(frustum, child, effect);
            }            
        }
                

        public void Render(BasicEffect effect, Actor actor)
        {
            GraphicsDevice device = Engine.Instance.Device;
            effect.World = actor.Matrix;
            effect.CurrentTechnique.Passes[0].Begin();

            foreach (Polygon poly in actor.Model.Polygons)
            {
                if (_cullingDisabled != poly.DoubleSided)
                {
                    device.RenderState.CullMode = (poly.DoubleSided ? CullMode.None : CullMode.CullClockwiseFace);
                    _cullingDisabled = poly.DoubleSided;
                }
                
                if (poly.Texture != null)
                    device.Textures[0] = poly.Texture;
                else
                    device.Textures[0] = actor.Texture;

                Engine.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, poly.VertexBufferIndex, poly.VertexCount / 3);
            }
            effect.CurrentTechnique.Passes[0].End();
        }
    }
}
