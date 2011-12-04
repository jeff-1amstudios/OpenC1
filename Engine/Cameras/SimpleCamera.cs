using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace OneAmEngine
{

	public class SimpleCamera : ICamera
	{

		public SimpleCamera()
		{
            Up = Vector3.Up;
			AspectRatio = Engine.AspectRatio;
			NearPlaneDistance = 0.01f;
			DrawDistance = 1000;
			FieldOfView = MathHelper.ToRadians(45.0f);
			Update();
		}
		
		public Vector3 Orientation {get; set; }
		
		public Vector3 Position {get; set; }

        public Vector3 Up { get; set; }
		
		public float AspectRatio {get; set; }
		
		public float FieldOfView {get; set; }

		public float NearPlaneDistance {get; set; }

		public float DrawDistance {get; set; }

		public Matrix View {get; private set; }

		public Matrix Projection {get; private set; }
		
		public void Update()
		{
            View = Matrix.CreateLookAt(Position, Position + (Orientation * 2), Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlaneDistance, DrawDistance);
		} 
	}
}
