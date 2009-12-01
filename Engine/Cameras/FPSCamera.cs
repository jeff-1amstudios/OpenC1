//-----------------------------------------------------------------------------
// Copyright (c) 2007 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NFSEngine;

namespace PlatformEngine
{
    /// <summary>
    /// The FirstPersonCamera class implements the logic for a first person
    /// style 3D camera. This class also handles player input that is used
    /// to control the camera. To use this class, create an instance of the
    /// FirstPersonCamera class and then call the Update() method once a
    /// frame from your game's main loop. The FirstPersonCamera's Update()
    /// method will process mouse and keyboard input used to manipulate the
    /// camera. To change the default movement key bindings call the
    /// MapActionToKey() method. Most of the code in this class is used to
    /// simulate camera view bobbing, crouching, and jumping.
    /// </summary>
    public class FPSCamera : ICamera
    {
        
        public const float DEFAULT_FOVX = 60.0f;
        public const float DEFAULT_ROTATION_SPEED = 0.25f;
        
        public const float DEFAULT_ZNEAR = 0.1f;

        private const float GRAVITY = -9.8f;
        private const float DECELERATION = -0.5f;
        private const float STRAFE_SPEED_MULTIPLIER = 15.5f;

        private const float VelocityInversionMultiplier = 20.0f;
        private const float Acceleration = 5.0f;
        private const float Deceleration = -5.0f;
        private const float JumpVelocity = 0.23f;
        private const float MaxSpeed = 1.5f;
        
               
        private float fovx;
        private float znear;

        private float _strafeDelta, _forwardDelta, _velocity;

        private Vector3 _orientation, _position;

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public Vector3 Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        public float DrawDistance { get; set; }
        
        public Matrix View {get; private set; }
        public Matrix Projection {get; private set; }

        public FPSCamera()
        {
            fovx = DEFAULT_FOVX;
            znear = DEFAULT_ZNEAR;

            // Setup initial default view and projection matrices.
            SetPerspective(DEFAULT_FOVX, 4f/3f, DEFAULT_ZNEAR, DrawDistance);
            View = Matrix.Identity;
        }

               
        /// <summary>
        /// Builds a perspective projection matrix based on a horizontal field
        /// of view. The aspect ratio is calculated by dividing the viewport's
        /// width by its height.
        /// </summary>
        /// <param name="fovx">Horizontal field of view in degrees.</param>
        /// <param name="aspect">Aspect ratio.</param>
        /// <param name="znear">Near plane distance.</param>
        /// <param name="zfar">Far plane distance.</param>
        public void SetPerspective(float fovx, float aspect, float znear, float zfar)
        {
            this.fovx = fovx;
            this.znear = znear;

            float e = 1.0f / (float)Math.Tan(MathHelper.ToRadians(fovx) / 2.0f);
            float aspectInv = 1.0f / aspect;
            float fovy = 2.0f * (float)Math.Atan(aspectInv / e);
            float xScale = 1.0f / (float)Math.Tan(0.5f * fovy);
            float yScale = xScale / aspectInv;

            Matrix projection = new Matrix();

            projection.M11 = xScale;
            projection.M22 = yScale;
            projection.M33 = (zfar + znear) / (znear - zfar);
            projection.M34 = -1.0f;
            projection.M43 = (2.0f * zfar * znear) / (znear - zfar);
            
            Projection = projection;
        }

        public void Update(GameTime gameTime)
        {
            InputProvider input = Engine.Instance.Input;
            float elapsedTime = Engine.Instance.ElapsedSeconds;

            _forwardDelta = input.MoveForward * elapsedTime * Acceleration;

            _strafeDelta = input.Strafe * elapsedTime * Acceleration;


            _orientation.X += input.RightThumbDelta * -1;
            _orientation.Y += input.LeftThumbDelta * -1;
            
            UpdateVelocity(gameTime);
            MoveForward();

            _position.X += (float)(Math.Cos(_orientation.X) * input.Strafe);
            _position.Z -= (float)(Math.Sin(_orientation.X) * input.Strafe);
            

            Engine.Instance.Camera.Orientation = Orientation;
            Engine.Instance.Camera.Position = Position;

            Matrix view = Matrix.CreateTranslation(-Position);
            view *= Matrix.CreateRotationY(-_orientation.X);
            view *= Matrix.CreateRotationX(_orientation.Y);
            view *= Matrix.CreateRotationZ(_orientation.Z);

            View = view;
            SetPerspective(DEFAULT_FOVX, Engine.Instance.AspectRatio, DEFAULT_ZNEAR, DrawDistance);
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

        public void MoveForward()
        {
            _position.X -= (float)((Math.Sin(_orientation.X) * Math.Cos(_orientation.Y)) * _velocity);
            _position.Z -= (float)((Math.Cos(_orientation.X) * Math.Cos(_orientation.Y)) * _velocity);
            _position.Y -= _orientation.Y * _velocity;
        }
    }
}