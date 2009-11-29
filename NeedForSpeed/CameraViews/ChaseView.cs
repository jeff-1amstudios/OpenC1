using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.HUD;

namespace Carmageddon.CameraViews
{
    class ChaseView : ICameraView
    {

        RevCounter _revCounter;
        VehicleModel _vehicle;

        public ChaseView(VehicleModel vehicle)
        {
            _vehicle = vehicle;
            _revCounter = new RevCounter(_vehicle.Chassis);
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
            _revCounter.Render();
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
