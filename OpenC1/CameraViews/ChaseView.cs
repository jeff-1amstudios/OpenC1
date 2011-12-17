using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.HUD;
using Microsoft.Xna.Framework.Graphics;

using Microsoft.Xna.Framework;
using OpenC1.Physics;
using OneAmEngine;
using Microsoft.Xna.Framework.Input;

namespace OpenC1.CameraViews
{
    class ChaseView : ICameraView
    {
        List<BaseHUDItem> _hudItems = new List<BaseHUDItem>();
        Vehicle _vehicle;
        FixedChaseCamera _camera;
		const float DefaultChaseDistance = 6.3f;
		const float DefaultChaseHeight = 2.3f;
		float _chaseCameraPositionMultiplier = 1;

        public ChaseView(Vehicle vehicle)
        {
			_camera = new FixedChaseCamera(DefaultChaseDistance, DefaultChaseHeight);
            _camera.FieldOfView = MathHelper.ToRadians(55.55f);

            _vehicle = vehicle;

            _hudItems.Add(new StandardHudItem());
            _hudItems.Add(new RevCounter(_vehicle.Chassis));
            _hudItems.Add(new Timer());   
        }

        #region ICameraView Members


        public bool Selectable
        {
            get { return true; }
        }

        public void Update()
        {
			
			if (Engine.Input.IsKeyDown(Keys.PageDown))
			{
				_chaseCameraPositionMultiplier += Engine.ElapsedSeconds;
			}
			else if (Engine.Input.IsKeyDown(Keys.PageUp))
			{
				_chaseCameraPositionMultiplier -= Engine.ElapsedSeconds;
				if (_chaseCameraPositionMultiplier < 1)
					_chaseCameraPositionMultiplier = 1;
			}

            VehicleChassis chassis = _vehicle.Chassis;
            _camera.Position = _vehicle.GetBodyBottom();
            
            if (!chassis.InAir)
            {
                _camera.Orientation = chassis.Actor.GlobalOrientation.Forward;
                if (chassis.Speed > 15)
                {
                    _camera.RotateTo(chassis.Backwards ? MathHelper.Pi : 0);
                }
                if (Race.Current.RaceTime.IsStarted) _camera.MinHeight = 0;
				_camera.SetChaseDistance(DefaultChaseDistance * _chaseCameraPositionMultiplier, DefaultChaseHeight * _chaseCameraPositionMultiplier);
            }
            else
            {
                if (Race.Current.RaceTime.IsStarted) _camera.MinHeight = 2;
				_camera.SetChaseDistance(7, DefaultChaseHeight);
            }

            foreach (BaseHUDItem item in _hudItems)
                item.Update();

            Engine.Camera = _camera;
        }

        public void Render()
        {
            _vehicle.Render();

            foreach (BaseHUDItem item in _hudItems)
                item.Render();            
        }

        public void Activate()
        {
            Engine.Camera = _camera;
        }

        public void Deactivate()
        {
            
        }

        #endregion
    }
}
