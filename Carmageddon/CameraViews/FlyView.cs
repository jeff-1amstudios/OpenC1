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
            _camera.SetPerspective(55.55f, Engine.AspectRatio, 1, 500);
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
            Race.Current.PlayerVehicle.Render();
            Engine.Camera = _camera;
        }

        public void Activate()
        {
            Engine.Camera = _camera;
            _camera.Position = Race.Current.PlayerVehicle.Position;
        }

        public void Deactivate()
        {
            
        }

        #endregion
    }
}
