using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.CameraViews;

namespace Carmageddon.GameModes
{
    class FlyMode : GameMode
    {
        FlyView _fpsView;

        public FlyMode()
        {
            _fpsView = new FlyView(Race.Current.PlayerVehicle);   
        }

        public override void Activate()
        {
            _fpsView.Activate();
            MessageRenderer.Instance.PostHeaderMessage("Edit Mode: Fly", 3);
        }

        public override void Update()
        {
            _fpsView.Update();
            
        }

        public override void Render()
        {
            _fpsView.Render();
        }
    }
}
