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
        public FixedChaseCamera(float chaseDistance, float height)
		{
            _chaseDistance = new Vector3(chaseDistance, height, chaseDistance);
            AspectRatio = Engine.Instance.AspectRatio;
            FieldOfView = MathHelper.ToRadians(45f);
            NearPlaneDistance = 1.0f;
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
		
        public void Update(GameTime gameTime)
		{
            _lookAt.AddValue(new Vector3(0, 2f, 0) + (-Orientation * _chaseDistance));
            Vector3 avgLookAt = _lookAt.GetAveragedValue();
            Vector3 cameraPosition = Position + avgLookAt;
            View = Matrix.CreateLookAt(Position + avgLookAt, Position + new Vector3(0,1.0f,0), Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlaneDistance, DrawDistance);
		}        
    }
}
