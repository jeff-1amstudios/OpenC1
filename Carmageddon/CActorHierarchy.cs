using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Carmageddon.Parsers.Grooves;
using NFSEngine;

namespace Carmageddon
{
    class CActorHierarchy
    {
        List<CActor> _actors = new List<CActor>();
        public DatFile ModelsFile { get; set; }

        
        public CActor Root
        {
            get { return _actors[0]; }
        }
                

        public void Add(CActor actor)
        {
            _actors.Add(actor);
        }

        public List<CActor> All()
        {
            List<CActor> actors = new List<CActor>();
            Action<CActor> resolver = null;
            resolver = (actor) =>
            {
                actors.Add(actor);
                foreach (CActor child in actor.Children)
                    resolver(child);
            };
            resolver(Root);
            return actors;
        }

        public CActor GetByName(string name)
        {
            string nameWithExt = name + ".ACT";
            List<CActor> all = All();
            return all.Find(a => a.Name == nameWithExt || a.Name == name);
        }

        /// <summary>
        /// Pre-calculate recursive transformations and apply scaling, ignoring groove animations
        /// </summary>
        public void ResolveTransforms(bool removeRootTransform, List<BaseGroove> grooves)
        {
            if (removeRootTransform)
            {
                Root.Matrix.Translation = Vector3.Zero;
            }

            ResolveTransformations(Matrix.Identity, Root, grooves);
            ScaleTransformations(GameVariables.Scale, Root);
        }

        private void ResolveTransformations(Matrix world, CActor actor, List<BaseGroove> grooves)
        {
            if (grooves != null && grooves.Exists(g => g.ActorName == actor.Name))
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

        internal void ResolveMaterials()
        {
            Action<CActor> resolver = null;
            resolver = (actor) =>
            {
                if (actor.MaterialName != null)
                {
                    actor.Material = ResourceCache.GetMaterial(actor.MaterialName);
                }
                foreach (CActor child in actor.Children)
                    resolver(child);
            };
            resolver(Root);
        }


        public BoundingFrustum Render(Matrix world, BoundingFrustum frustum)
        {
            ModelsFile.SetupRender();

            GameVariables.NbrSectionsRendered = GameVariables.NbrSectionsChecked = 0;
                        
            bool overrideActor = world != Matrix.Identity;

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < _actors.Count; i++)
            {
                RenderChildren(frustum, _actors[i], world, false);
            }

            GameVariables.CurrentEffect.CurrentTechnique.Passes[0].End();

            GameConsole.WriteLine("Checked: " + GameVariables.NbrSectionsChecked + ", Rendered: " + GameVariables.NbrSectionsRendered);

            return frustum;
        }

        private void RenderChildren(BoundingFrustum frustum, CActor actor, Matrix world, bool parentAnimated)
        {
            if (actor.IsWheel) return;

            bool intersects;

            if (frustum == null)
            {
                intersects = true;
            }
            else
            {
                intersects = actor.BoundingBox.Max.X == 0;
                if (!intersects)
                {
                    frustum.Intersects(ref actor.BoundingBox, out intersects);
                    GameVariables.NbrSectionsChecked++;
                }
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

        //public Matrix CalculateDynamicActorMatrix(CActor actorToFind)
        //{
        //    bool done = false;
        //    Matrix m = CalculateDynamicActorMatrixInternal(_actors[0], Matrix.Identity, actorToFind, ref done);
        //    return m * GameVariables.ScaleMatrix;
        //}

        //private Matrix CalculateDynamicActorMatrixInternal(CActor actor, Matrix matrix, CActor actorToFind, ref bool done)
        //{
        //    if (done) return matrix;

        //    matrix = matrix * actor.Matrix;

        //    if (actorToFind == actor)
        //    {
        //        done = true;
        //        return matrix;
        //    }
        //    foreach (CActor child in actor.Children)
        //    {
        //        Matrix m = CalculateDynamicActorMatrixInternal(child, matrix, actorToFind, ref done);
        //        if (done) return m;
        //    }
        //    return matrix;
        //}

        public void RecalculateActorParent(CActor actor)
        {
            actor.Parent.Children.Remove(actor); //remove it from current parent

            bool found = false;
            for (int i = 0; i < _actors.Count; i++)
            {
                MoveChildren(actor, _actors[i], null, ref found);
            }
        }

        private void MoveChildren(CActor actorToMove, CActor parent, CActor parentParent, ref bool found)
        {
            if (found || parent.BoundingBox.Max.X == 0)
            {
                return;
            }

            if (parent.BoundingBox.Contains(actorToMove.PhysXActor.GlobalPosition) == ContainmentType.Contains)
            {
                foreach (CActor child in parent.Children)
                    MoveChildren(actorToMove, child, parent, ref found);
                if (!found)
                {
                    if (!parent.Children.Contains(actorToMove))
                    {
                        parent.Children.Add(actorToMove);
                    }
                    found = true;
                }
            }
        }
    }
}
