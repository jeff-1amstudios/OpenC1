using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NFSEngine;

namespace Carmageddon.EditModes
{
    class OpponentEditMode : IEditMode
    {
        FixedChaseCamera _opponentCamera;
        int _watchingOpponent = 0;

        public OpponentEditMode()
        {
            _opponentCamera = new FixedChaseCamera(6.3f, 3.3f);
            _opponentCamera.FieldOfView = MathHelper.ToRadians(55.55f);
        }

        public void Activate()
        {
            MessageRenderer.Instance.PostHeaderMessage("Edit Mode: Opponents", 3);
        }

        public void Update()
        {
            if (Engine.Input.WasPressed(Keys.D0))
            {
                _watchingOpponent = (_watchingOpponent + 1) % (Race.Current.Opponents.Count + 1);   
            }

            if (_watchingOpponent == 0)
            {
                _opponentCamera.Position = Race.Current.PlayerVehicle.GetBodyBottom();
                _opponentCamera.Orientation = Race.Current.PlayerVehicle.Chassis.Actor.GlobalOrientation.Forward;
                Engine.Camera = _opponentCamera;
            }
            else
            {
                Opponent opponent = Race.Current.Opponents[_watchingOpponent-1];
                _opponentCamera.Position = opponent.Vehicle.GetBodyBottom();
                _opponentCamera.Orientation = opponent.Vehicle.Chassis.Actor.GlobalOrientation.Forward;
                Engine.Camera = _opponentCamera;

                opponent.Vehicle.Chassis.OutputDebugInfo();
            }
        }

        public void Render()
        {
            if (_watchingOpponent == 0)
            {
                Race.Current.PlayerVehicle.Render();
            }

            foreach (OpponentPathNode node in Race.Current.ConfigFile.OpponentPathNodes)
            {
                Engine.DebugRenderer.AddCube(Matrix.CreateTranslation(node.Position), Color.White);
                foreach (OpponentPath path in node.Paths)
                {
                    Color c = Color.Yellow;
                    if (path.Type == PathType.Race) c = Color.Red;
                    if (path.Type == PathType.CheatOnly) c = Color.Blue;
                    Engine.DebugRenderer.AddLine(node.Position, path.End.Position, c);
                }
            }
        }
    }
}
