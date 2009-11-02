using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace Carmageddon
{
    public class Player : GameObject
    {
        private enum Posture
        {
            Standing,
            Jumping,
            Falling
        };

        public event EventHandler LifeLost;

        private Posture _posture;
        private float _strafeDelta, _forwardDelta;

        private float _jumpingVelocity;

        private const float VelocityInversionMultiplier = 20.0f;
        private const float Acceleration = 5.0f;
        private const float Deceleration = -5.0f;
        private const float JumpVelocity = 0.23f;
        private const float MaxSpeed = 0.5f;

        private bool _shouldJump;
        private float _worldHeight;
        private float _health;

        public Player()
        {
            _posture = Posture.Standing;
            _health = 1.0f;
        }

        public bool IsInAir
        {
            get
            {
                return _worldHeight < _position.Y;
            }
        }

        public float Health
        {
            get { return _health; }
            set
            {
                if (_health == value)
                    return;
                _health = value;
                if (_health == 0.0f)
                {
                    if (LifeLost != null)
                        LifeLost(this, null);
                }
            }
        }

        public bool IsDead
        {
            get { return _health == 0.0f; }
        }


        public override void Update(GameTime gameTime)
        {
            InputProvider input = Engine.Instance.Input;
            float elapsedTime = Engine.Instance.ElapsedSeconds;

            _forwardDelta = input.MoveForward * elapsedTime * Acceleration;

            _strafeDelta = input.Strafe * elapsedTime * Acceleration;

            Rotate(input.RightThumbDelta * -1);
            Pitch(input.LeftThumbDelta * -1);
            UpdatePosition(gameTime);
            Strafe(input.Strafe * 1.0f);

            Engine.Instance.Camera.Orientation = _orientation;
            Engine.Instance.Camera.Position = _position;
            
        }

        public override void Render()
        {

        }

        private void UpdatePosition(GameTime gameTime)
        {

            // Update the vertical position to account for the
            // player jumping. Jump() transitions the posture from standing
            // to jumping to falling and back to standing.
            Jump(gameTime);

            // Update velocity based on player's input.
            UpdateVelocity(gameTime);

            // Update position along the XZ plane.
            MoveForward();
            //Move(_velocity.X, 0.0f, _velocity.Z)

        }


        private void UpdateVelocity(GameTime gameTime)
        {
            float elapsedTimeSec = Engine.Instance.ElapsedSeconds;

            // Accelerate or decelerate as camera is moved forward or backward.
            float acceleration = Acceleration;

            if (_forwardDelta != 0.0f)
            {
                // Speed up the transition from moving backwards to moving
                // forwards and vice versa. Otherwise there will be too much
                // of a delay as the camera slows down and then accelerates.
                if ((_forwardDelta > 0.0f && _velocity < 0.0f) ||
                    (_forwardDelta < 0.0f && _velocity > 0.0f))
                {
                    acceleration *= VelocityInversionMultiplier;
                }

                _velocity += _forwardDelta * acceleration;
            }
            else
            {

                if (_velocity > 0.0f)
                {
                    _velocity += Deceleration * elapsedTimeSec;

                    if (_velocity < 0.0f)
                        _velocity = 0.0f;
                }
                else if (_velocity < 0.0f)
                {
                    _velocity -= Deceleration * elapsedTimeSec;

                    if (_velocity > 0.0f)
                        _velocity = 0.0f;
                }

            }

            if (_velocity > MaxSpeed)
            {
                _velocity = MaxSpeed;
                acceleration = 0;
            }

            if (_velocity < -MaxSpeed)
            {
                _velocity = -MaxSpeed;
                acceleration = 0;
            }
        }


        private void Jump(GameTime gameTime)
        {
            float elapsedTimeSec = Engine.Instance.ElapsedSeconds;


            // Transition between jumping states.
            switch (_posture)
            {
                case Posture.Standing:
                    if (_shouldJump)
                    {
                        _posture = Posture.Jumping;
                        _jumpingVelocity = 42.0f * elapsedTimeSec;
                    }

                    break;

                case Posture.Jumping:
                    _shouldJump = true;
                    _jumpingVelocity -= 0.2f * elapsedTimeSec;
                    _position += Vector3.Up * _jumpingVelocity;

                    if (_jumpingVelocity <= 0)
                    {
                        _posture = Posture.Falling;
                        _jumpingVelocity = 0.0f;
                    }

                    break;

                case Posture.Falling:
                    _shouldJump = true;
                    _jumpingVelocity -= 0.6f * elapsedTimeSec;
                    _position += Vector3.Up * _jumpingVelocity;

                    if (_position.Y < _worldHeight)
                    {
                        _position.Y = _worldHeight;
                        _posture = Posture.Standing;
                        _jumpingVelocity = 0.0f;
                        _shouldJump = false;
                    }

                    break;

                default:
                    _shouldJump = false;
                    break;
            }
        }

        public override Vector3 GetCameraPosition()
        {
            return new Vector3(_position.X, _position.Y + 2.0f, _position.Z);
        }

        public void StandAt(Vector3 playerPos)
        {
            _posture = Posture.Standing;
            _position = playerPos;
            _velocity = 0.0f;
        }

        public bool Jump(float velocity)
        {
            if (_posture == Posture.Standing)
            {
                _posture = Posture.Jumping;
                _jumpingVelocity = velocity;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
