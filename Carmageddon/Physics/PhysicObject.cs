using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using JigLibX.Physics;
using JigLibX.Collision;
using JigLibX.Geometry;
using PlatformEngine;

namespace Carmageddon.Physics
{

    /// <summary>
    /// Helps to combine the physics with the graphics.
    /// </summary>
    public abstract class PhysicObject : DrawableGameComponent
    {
        protected Body body;
        protected CollisionSkin collision;

        protected Model model;
        protected Vector3 color;

        protected Vector3 scale = Vector3.One;

        public Body PhysicsBody { get { return body; } }
        public CollisionSkin PhysicsSkin { get { return collision; } }

        protected static Random random = new Random();

        public PhysicObject(Game game, Model model)
            : base(game)
        {
            this.model = model;
            color = new Vector3(random.Next(255), random.Next(255), random.Next(255));
            color /= 255.0f;
        }

        public PhysicObject(Game game)
            : base(game)
        {
            this.model = null;
            color = new Vector3(random.Next(255), random.Next(255), random.Next(255));
            color /= 255.0f;
        }

        protected Vector3 SetMass(float mass)
        {
            PrimitiveProperties primitiveProperties =
                new PrimitiveProperties(PrimitiveProperties.MassDistributionEnum.Solid, PrimitiveProperties.MassTypeEnum.Density, mass);

            float junk; Vector3 com; Matrix it, itCoM;

            collision.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCoM);
            body.BodyInertia = itCoM;
            body.Mass = junk;

            return com;
        }
        Matrix[] boneTransforms = null;
        int boneCount = 0;

        public abstract void ApplyEffects(BasicEffect effect);
        public override void Draw(GameTime gameTime)
        {

            Matrix world;
            if (body.CollisionSkin != null)
                world = Matrix.CreateScale(scale) * body.CollisionSkin.GetPrimitiveLocal(0).Transform.Orientation * body.Orientation * Matrix.CreateTranslation(body.Position);
            else
                world = Matrix.CreateScale(scale) * body.Orientation * Matrix.CreateTranslation(body.Position);

            Engine.Instance.GraphicsUtils.AddSolidShape(ShapeType.Cube, world, Color.Yellow, null);
            
        }
    }
}
