using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;
using Microsoft.Xna.Framework;
using OpenC1.Parsers.Grooves;
using OneAmEngine;


namespace OpenC1
{
    class CActorHierarchy
    {
        List<CActor> _actors = new List<CActor>();
        public CModelGroup Models { get; set; }
        public bool RenderWheelsSeparately { get; set; }

        public CActorHierarchy()
        {
            RenderWheelsSeparately = true;
        }

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
        public void ResolveTransforms(bool removeRootTranslation, List<BaseGroove> grooves)
        {
            if (removeRootTranslation)
            {
                Root.Matrix.Translation = Vector3.Zero;
            }

            ResolveTransformations(Matrix.Identity, Root, grooves);
            ScaleTransformations(GameVars.Scale, Root);


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


        public void Render(Matrix world, BoundingFrustum frustum)
        {
            Models.SetupRender();

            GameVars.NbrSectionsRendered = GameVars.NbrSectionsChecked = 0;

            bool overrideActor = world != Matrix.Identity;

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].Begin();

            for (int i = 0; i < _actors.Count; i++)
            {
                RenderChildren(frustum, _actors[i], world, false);
            }

            GameVars.CurrentEffect.CurrentTechnique.Passes[0].End();

            GameConsole.WriteLine("Checked: " + GameVars.NbrSectionsChecked + ", Rendered: " + GameVars.NbrSectionsRendered);
        }

        private void RenderChildren(BoundingFrustum frustum, CActor actor, Matrix world, bool parentAnimated)
        {
            if (RenderWheelsSeparately && actor.IsWheel) return;

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
                    GameVars.NbrSectionsChecked++;
                }
            }

            if (intersects)
            {
                Matrix m = actor.GetDynamicMatrix();

                if (actor.IsAnimated || parentAnimated)
                {
                    if (actor.IsAnimated && !parentAnimated)
                    {
                        world = m * actor.ParentMatrix * GameVars.ScaleMatrix * world;
                    }
                    else
                    {
                        world = m * world;
                    }

                    GameVars.CurrentEffect.World = world;
                    parentAnimated = true;
                }
                else
                {
                    GameVars.CurrentEffect.World = m * world;
                }

                GameVars.CurrentEffect.CommitChanges();

                if (actor.Model != null)
                {
                    actor.Model.Render(actor.Material);
                }

                GameVars.NbrSectionsRendered++;

                foreach (CActor child in actor.Children)
                    RenderChildren(frustum, child, world, parentAnimated);
            }
        }

        public void RenderSingle(CActor actor)
        {
            Matrix m = actor.Matrix;
            m.Translation = Vector3.Zero;
            GameVars.CurrentEffect.World = m * GameVars.CurrentEffect.World;
            GameVars.CurrentEffect.CommitChanges();

            actor.Model.Render(actor.Material);
        }

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
