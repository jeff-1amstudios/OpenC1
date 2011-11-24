using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using OneAmEngine;

namespace OpenC1.Parsers.Grooves
{

    class RockGroove : BaseGroove
    {
        public Motion Motion;
        public float MaxAngle;
        public Axis Axis;
        public PathGroove Path;

        float _currentRock;
        float _direction = 1;
        float _speed2 = 2;
        float _flashRock;

        public override void Update()
        {
            if (_actor == null)
                return;

            switch (Motion)
            {
                case Motion.Harmonic:
                    _currentRock += _direction * Engine.ElapsedSeconds * Speed * 6.28f * _speed2;

                    float distance = (MaxAngle - Math.Abs(_currentRock)) / MaxAngle;
                    //if (distance < 0.15f)
                    _speed2 = MathHelper.Lerp(0.05f, 1f, distance);
                    //else
                    //    _speed2 = 1;
                    break;
                case Motion.Linear:
                case Motion.Absolute:
                    //case Motion.Flash:
                    _currentRock += _direction * Engine.ElapsedSeconds * Speed * 6.28f;
                    break;
                case Motion.Flash:
                    _flashRock += _direction * Engine.ElapsedSeconds * Speed * 6.28f;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (Motion == Motion.Flash)
            {
                if (Math.Abs(_flashRock) > MaxAngle)
                    _currentRock = _flashRock;
                _direction *= -1;
                if (_flashRock < 0)
                    _currentRock = _flashRock = -MaxAngle;
                else
                    _currentRock = _flashRock = MaxAngle;
            }
            else
            {
            if (Math.Abs(_currentRock) > MaxAngle)
            {
                _direction *= -1;
                if (_currentRock < 0)
                    _currentRock = -MaxAngle;
                else
                    _currentRock = MaxAngle;
            }
            }

            Matrix rot;
            switch (Axis)
            {
                case Axis.X:
                    rot = Matrix.CreateRotationX(_currentRock);
                    break;
                case Axis.Y:
                    rot = Matrix.CreateRotationY(_currentRock);
                    break;
                case Axis.Z:
                    rot = Matrix.CreateRotationZ(_currentRock);
                    break;
                default:
                    throw new NotImplementedException();
            }

            _actor.Matrix =
                _scale * _actorRotation
                * Matrix.CreateTranslation(-CenterOfMovement) * rot * Matrix.CreateTranslation(CenterOfMovement)
                * _translation;

            if (Path != null)
            {
                _actor.Matrix *= Matrix.CreateTranslation(Path.UpdateMovement());
            }
        }
    }
}
