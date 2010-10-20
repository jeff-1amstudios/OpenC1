using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using OneAmEngine;

namespace OpenC1.Parsers.Grooves
{
    class SpinGroove : BaseGroove
    {
        public Axis Axis;
        public PathGroove Path;

        Matrix _currentRotation = Matrix.Identity;
        
        public override void Update()
        {
            if (_actor == null)
                return; //BUSTER.TXT

            Matrix rot;
            switch (Axis)
            {
                case Axis.X:
                    rot = Matrix.CreateRotationX(Speed * 6.28f /*=rads*/ * Engine.ElapsedSeconds);
                    break;
                case Axis.Y:
                    rot = Matrix.CreateRotationY(Speed * 6.28f /*=rads*/ * Engine.ElapsedSeconds);
                    break;
                case Axis.Z:
                    rot = Matrix.CreateRotationZ(Speed * 6.28f /*=rads*/ * Engine.ElapsedSeconds);
                    break;
                default:
                    throw new NotImplementedException();
            }

            _currentRotation *= rot;

            _actor.Matrix =
                _scale * _actorRotation
                * Matrix.CreateTranslation(-CenterOfMovement) * _currentRotation * Matrix.CreateTranslation(CenterOfMovement)
                * _translation;

            if (Path != null)
            {
                _actor.Matrix *= Matrix.CreateTranslation(Path.UpdateMovement());
            }
        }
    }
}
