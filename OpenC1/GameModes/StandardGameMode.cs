using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.CameraViews;
using Microsoft.Xna.Framework.Input;
using OneAmEngine;
using OpenC1.Screens;

namespace OpenC1.GameModes
{
    class StandardGameMode : GameMode
    {
        int _currentView = 0;
        List<ICameraView> _views = new List<ICameraView>();

        public StandardGameMode()
        {
            _views.Add(new ChaseView(Race.Current.PlayerVehicle));
            if (GameVars.Emulation == EmulationMode.Demo)
                _views.Add(new CockpitView(Race.Current.PlayerVehicle, GameVars.BasePath + @"32x20x8\cars\" + Race.Current.PlayerVehicle.Config.FileName));
            else
                _views.Add(new CockpitView(Race.Current.PlayerVehicle, GameVars.BasePath + @"64x48x8\cars\" + Race.Current.PlayerVehicle.Config.FileName));
            _views[_currentView].Activate();
        }

        public override void Activate()
        {
            _views[_currentView].Activate();
            MessageRenderer.Instance.PostHeaderMessage("Race mode", 3);
        }

        public override void Update()
        {
			if (Engine.Input.WasPressed(Keys.Escape))
			{
				Engine.Screen = new PauseMenuScreen((PlayGameScreen)Engine.Screen);
				return;
			}

            if (Engine.Input.WasPressed(Keys.C))  //cockpit / external view
            {
                _views[_currentView].Deactivate();
                _currentView = (_currentView + 1) % _views.Count;
                _views[_currentView].Activate();
            }

            _views[_currentView].Update();
        }

        public override void Render()
        {
            _views[_currentView].Render();
        }
    }
}
