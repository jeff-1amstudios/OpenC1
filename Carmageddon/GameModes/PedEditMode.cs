using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.CameraViews;
using OneAmEngine;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.IO;

namespace OpenC1.GameModes
{
    class PedEditMode : GameMode
    {
        ChaseView _view;

        List<Vector3> _currentPath;

        public PedEditMode()
        {
            _view = new ChaseView(Race.Current.PlayerVehicle);
        }

        public override void Activate()
        {
            _view.Activate();
            MessageRenderer.Instance.PostHeaderMessage("Edit Mode: Pedestrians", 3);
        }

        public override void Update()
        {
            if (Engine.Input.WasPressed(Keys.D1))
            {
                MessageRenderer.Instance.PostHeaderMessage("Adding new pedestrian", 3);
                _currentPath = new List<Vector3>();
                _currentPath.Add(Race.Current.PlayerVehicle.GetBodyBottom());
            }
            else if (Engine.Input.WasPressed(Keys.D2))
            {
                _currentPath.Add(Race.Current.PlayerVehicle.GetBodyBottom());
            }
            else if (Engine.Input.WasPressed(Keys.D3))
            {
                _currentPath.Add(Race.Current.PlayerVehicle.GetBodyBottom());
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("1				// Ref num");
                sb.AppendLine(_currentPath.Count + 1 + "	// instructions");
                sb.AppendLine("1    // initial instruction");
                foreach (Vector3 pos in _currentPath)
                {
                    sb.AppendLine("point");
                    sb.AppendLine((pos/6).ToShortString());
                }
                sb.AppendLine("reverse");
                File.AppendAllText("ped-edit.txt", sb.ToString());
            }
            _view.Update();
        }

        public override void Render()
        {
            _view.Render();
        }
    }
}
