using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.HUD;

namespace Carmageddon.CameraViews
{
    class ChaseView : ICameraView
    {

        VehicleModel _vehicle;

        public ChaseView(VehicleModel vehicle)
        {
            _vehicle = vehicle;
        }

        #region ICameraView Members

        RevCounter _revCounter = new RevCounter();

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

            _revCounter.Render(_vehicle.Chassis.Motor.Rpm / _vehicle.Chassis.Motor.RedlineRpm);
        }

        public void Activate()
        {
            
        }

        public void Deactivate()
        {
            
        }

        #endregion
    }
}
