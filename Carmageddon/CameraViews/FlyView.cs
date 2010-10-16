using System;
using System.Collections.Generic;
using System.Text;
using OneAmEngine;

namespace Carmageddon.CameraViews
{
    class FlyView : ICameraView
    {
        FPSCamera _camera;
        Vehicle _vehicle;

        public FlyView(Vehicle vehicle)
        {
            _vehicle = vehicle;
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
            _vehicle.Render();
            Engine.Camera = _camera;
        }

        public void Activate()
        {
            Engine.Camera = _camera;
            _camera.Position = _vehicle.Position;
        }

        public void Deactivate()
        {
            
        }

        #endregion
    }
}
