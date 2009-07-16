using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Collision;
using JigLibX.Physics;

namespace Carmageddon.Physics
{
    class PlaneObject : PhysicObject
    {

        public PlaneObject(Game game, Model model, float d)
            : base(game, model)
        {
            body = new Body();
            collision = new CollisionSkin(null);
            collision.AddPrimitive(new JigLibX.Geometry.Plane(Vector3.Up, d), new MaterialProperties(0.2f, 0.7f, 0.6f));
            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.AddCollisionSkin(collision);
        }

        public override void ApplyEffects(BasicEffect effect)
        {
            //
        }
    }
}
