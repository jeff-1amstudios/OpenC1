using System;
using System.Collections.Generic;

using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    class WheelActor
    {
        public Actor Actor {get; private set;}
        public Matrix RotationMatrix = Matrix.Identity;
 
        public WheelActor(Actor actor)
        {
            Actor = actor;
        }

        public void UpdateAxleSpeed(float speed)
        {
            RotationMatrix *= Matrix.CreateRotationX(MathHelper.ToRadians(speed));
        }
}
}
