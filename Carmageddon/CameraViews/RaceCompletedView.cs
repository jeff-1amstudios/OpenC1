using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Carmageddon.Physics;
using OneAmEngine;

namespace Carmageddon.CameraViews
{
    class RaceCompletedView : ICameraView
    {
        FixedChaseCamera _camera;

        public RaceCompletedView(Vehicle vehicle)
        {
            _camera = new FixedChaseCamera(6.3f, 2.3f);
            _camera.FieldOfView = MathHelper.ToRadians(55.55f);
            _camera.RotationSpeed = 0.8f;
            _camera.HeightOverride = 10;
        }

        #region ICameraView Members


        public bool Selectable
        {
            get { return true; }
        }

        public void Update()
        {
            VehicleChassis chassis = Race.Current.PlayerVehicle.Chassis;
            _camera.Position = Race.Current.PlayerVehicle.GetBodyBottom();

            _camera.Orientation = chassis.Actor.GlobalOrientation.Forward;

            if (_camera.Rotation == MathHelper.Pi * 2)
            {
                _camera.ResetRotation();
                _camera.RotateTo(MathHelper.Pi * 2);
            }

            //Engine.Camera = _camera;
        }

        public void Render()
        {
            Race.Current.PlayerVehicle.Render();
        }

        public void Activate()
        {
            Engine.Camera = _camera;
            _camera.ResetRotation();
            _camera.RotateTo(MathHelper.Pi * 2);
        }

        public void Deactivate()
        {

        }

        #endregion
    }
}
