using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;

namespace NFSEngine
{
    /// <summary>
    /// Camera that stays a fixed distance behind an object but swings freely
    /// </summary>
    public class FixedChaseCamera : ICamera
    {
        private Vector3 _chaseDistance;
        float _currentRotation;
        public float HeightOverride;
        float _height;

        public FixedChaseCamera(float chaseDistance, float height)
		{
            _chaseDistance = new Vector3(chaseDistance, 1, chaseDistance);
            _height = height;
            AspectRatio = Engine.AspectRatio;
            FieldOfView = MathHelper.ToRadians(45f);
            NearPlaneDistance = 1.0f;
            View = Matrix.CreateLookAt(Vector3.One, Vector3.UnitZ, Vector3.Up);
		}

        AverageValueVector3 _lookAt = new AverageValueVector3(45);
		
		/// <summary>
		/// Position of camera in world space.
		/// </summary>
		public Vector3 Position {get; set; }

        public Vector3 Orientation {get; set; }

		/// <summary>
		/// Perspective aspect ratio. Default value should be overriden by application.
		/// </summary>
		public float AspectRatio {get; set; }
		
		/// <summary>
		/// Perspective field of view.
		/// </summary>
        public float FieldOfView { get; set; }

		/// <summary>
		/// Distance to the near clipping plane.
		/// </summary>
		public float NearPlaneDistance {get; set; }

		/// <summary>
		/// Distance to the far clipping plane.
		/// </summary>
		public float DrawDistance {get; set; }		

		/// <summary>
		/// View transform matrix.
		/// </summary>
        public Matrix View { get; private set; }

		/// <summary>
		/// Projecton transform matrix.
		/// </summary>
		public Matrix Projection {get; private set; }

        /// <summary>
        /// Rotation around the target
        /// </summary>
        public float Rotation { get; set; }
		
        public void Update()
		{
            if (_currentRotation != Rotation)
            {
                if (_currentRotation < Rotation)
                    _currentRotation += Engine.ElapsedSeconds*3;
                else
                    _currentRotation -= Engine.ElapsedSeconds*3;
                if (Math.Abs(_currentRotation - Rotation) < 0.05f)
                    _currentRotation = Rotation;
            }

            Vector3 pos = (-Vector3.Normalize(Orientation) * _chaseDistance);
            if (HeightOverride != 0)
                pos.Y = HeightOverride;
            else
                pos.Y += _height;
            _lookAt.AddValue(pos);
            Vector3 avgLookAt = _lookAt.GetAveragedValue();
            Vector3 cameraPosition = Position + Vector3.Transform(avgLookAt, Matrix.CreateRotationY(_currentRotation));
            
            View = Matrix.CreateLookAt(cameraPosition, Position + new Vector3(0, 1.3f, 0), Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlaneDistance, DrawDistance);
		}
    }
}
