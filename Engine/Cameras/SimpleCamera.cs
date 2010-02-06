using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;

namespace NFSEngine
{

	public class SimpleCamera : ICamera
	{

		public SimpleCamera()
		{
            Up = Vector3.Up;
		}

		/// <summary>
		/// Look at point in world space.
		/// </summary>
		public Vector3 Orientation {get; set; }
		
		/// <summary>
		/// Position of camera in world space.
		/// </summary>
		public Vector3 Position {get; set; }

        public Vector3 Up { get; set; }

		/// <summary>
		/// Perspective aspect ratio. Default value should be overriden by application.
		/// </summary>
		public float AspectRatio
		{
			get { return aspectRatio; }
			set { aspectRatio = value; }
		}
		private float aspectRatio = 4.0f / 3.0f;

		/// <summary>
		/// Perspective field of view.
		/// </summary>
		public float FieldOfView
		{
			get { return fieldOfView; }
			set { fieldOfView = value; }
		}
		private float fieldOfView = MathHelper.ToRadians(45.0f);

		/// <summary>
		/// Distance to the near clipping plane.
		/// </summary>
		public float NearPlaneDistance
		{
			get { return nearPlaneDistance; }
			set { nearPlaneDistance = value; }
		}
		private float nearPlaneDistance = 0.01f;

		/// <summary>
		/// Distance to the far clipping plane.
		/// </summary>
		public float DrawDistance {get; set; }

		/// <summary>
		/// View transform matrix.
		/// </summary>
		public Matrix View {get; private set; }

		/// <summary>
		/// Projecton transform matrix.
		/// </summary>
		public Matrix Projection {get; private set; }
		

		/// <summary>
		/// Animates the camera from its current position towards the desired offset
		/// behind the chased object. The camera's animation is controlled by a simple
		/// physical spring attached to the camera and anchored to the desired position.
		/// </summary>
		public void Update()
		{
            Vector3.Normalize(Orientation);
            View = Matrix.CreateLookAt(Position, Position + (Orientation * 2), Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlaneDistance, DrawDistance);
		} 
	}
}
