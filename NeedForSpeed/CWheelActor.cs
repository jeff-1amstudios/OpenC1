using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    class CWheelActor
    {
        public CActor Actor;
        public bool Driven;
        public Vector3 Position;
        public bool Steerable;

        public CWheelActor(CActor actor, bool driven, bool steerable)
        {
            Actor = actor;
            Driven = driven;
            Steerable = steerable;
        }

        public bool IsFront { get { return Actor.Name.StartsWith("F"); } }
        public bool IsLeft { get { return Actor.Name[1] == 'L'; } }
    }
}
