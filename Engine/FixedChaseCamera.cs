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
        public FixedChaseCamera()
		{
		}

        public Vector3 RightVec = Vector3.Right;
        public Vector3 UpVector = Vector3.Up;
        AverageValueVector3 _lookAt = new AverageValueVector3(60);

		
		/// <summary>
		/// Position of camera in world space.
		/// </summary>
		public Vector3 Position
		{
			get { return _position; }
			set { _position = value; }
		}
		private Vector3 _position;

        public Vector3 ChaseDirection
        {
            set {
                
                //_lookAt.AddValue(value);
                _chaseDirection = value;
            }
        }
        private Vector3 _chaseDirection;


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
		private float nearPlaneDistance = 1.0f;

		/// <summary>
		/// Distance to the far clipping plane.
		/// </summary>
		public float FarPlaneDistance
		{
			get { return farPlaneDistance; }
			set { farPlaneDistance = value; }
		}
		private float farPlaneDistance = 3000;


		/// <summary>
		/// View transform matrix.
		/// </summary>
		public Matrix View
		{
			get { return _view; }
		}
		private Matrix _view;

		/// <summary>
		/// Projecton transform matrix.
		/// </summary>
		public Matrix Projection
		{
			get { return _projection; }
		}
		private Matrix _projection;


		public void Update(GameTime gameTime)
		{
            float distance = 70;
            _lookAt.AddValue(new Vector3(0, 25, 0) + (-_chaseDirection * new Vector3(distance, distance, distance)));
            Vector3 avgLookAt = _lookAt.GetAveragedValue();
            Vector3 cameraPosition = _position + avgLookAt;
            _view = Matrix.CreateLookAt(cameraPosition, cameraPosition - avgLookAt + new Vector3(0,15,0), UpVector);
            _projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlaneDistance, FarPlaneDistance);

            //Engine.Instance.GraphicsUtils.AddSolidShape(ShapeType.Cube, Matrix.CreateScale(3) * Matrix.CreateTranslation(_position + avgLookAt), Microsoft.Xna.Framework.Graphics.Color.Yellow, null);
		}

		public void SetPosition(Vector3 position)
		{
			_position = position;
		}

		public void FollowObject(GameObject obj)
		{
		}

        
    }
}
