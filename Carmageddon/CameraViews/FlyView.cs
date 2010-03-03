using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;

namespace Carmageddon.CameraViews
{
    class FlyView : ICameraView
    {
        FPSCamera _camera;

        public FlyView()
        {
            _camera = new FPSCamera();
            _camera.SetPerspective(55.55f, Engine.Instance.AspectRatio, 1, 500);
        }

        #region ICameraView Members

        public bool Selectable
        {
            get { return true; }
        }

        public void Update()
        {
            
        }

        public void Render()
        {
            GameVariables.PlayerVehicle.Render();
        }

        public void Activate()
        {
            Engine.Instance.Camera = _camera;
            _camera.Position = GameVariables.PlayerVehicle.Chassis.Body.GlobalPosition;
        }

        public void Deactivate()
        {
            
        }

        #endregion
    }
}
