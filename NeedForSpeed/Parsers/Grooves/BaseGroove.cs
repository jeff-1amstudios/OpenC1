using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

enum Axis
{
    X,
    Y,
    Z
}

public enum Motion
{
    Linear,
    Harmonic,
    Absolute
}

namespace Carmageddon.Parsers.Grooves
{
    
    abstract class BaseGroove
    {
        protected CActor _actor;
        public string ActorName;
        public float Speed;
        public Vector3 CenterOfMovement;
        public int Id { get { return (int)Speed; } }
        
        public Matrix _scale, _translation;
        public Matrix _actorRotation;

        public virtual void SetActor(CActor actor)
        {
            _actor = actor;

            Vector3 s, t;
            Quaternion r;
            actor.Matrix.Decompose(out s, out r, out t);
            _scale = Matrix.CreateScale(s);
            _translation = Matrix.CreateTranslation(t);
            _actorRotation = Matrix.CreateFromQuaternion(r);
        }

        public abstract void Update();

        public bool IsWheelActor
        {
            get
            {
                return (ActorName.StartsWith("FLWHEEL") || ActorName.StartsWith("FRWHEEL") ||
                    ActorName.StartsWith("RLWHEEL") || ActorName.StartsWith("RRWHEEL") ||
                    ActorName.StartsWith("FRPIVOT") || ActorName.StartsWith("FLPIVOT") ||
                    ActorName.StartsWith("MRPIVOT") || ActorName.StartsWith("MLPIVOT") ||
                    ActorName.StartsWith("MLWHEEL") || ActorName.StartsWith("MRWHEEL"));
            }
        }
    }
}
