using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NFSEngine;
using Carmageddon.CameraViews;

namespace Carmageddon.EditModes
{
    class OpponentEditMode : IEditMode
    {
        FixedChaseCamera _opponentCamera;
        FlyView _fpsView;
        int _watchingOpponent = 0;

        public OpponentEditMode()
        {
            //_opponentCamera = new FixedChaseCamera(6.3f, 3.3f);
            //_opponentCamera.FieldOfView = MathHelper.ToRadians(55.55f);
            _fpsView = new FlyView(Race.Current.PlayerVehicle);
        }

        public void Activate()
        {
            _fpsView.Activate();
            MessageRenderer.Instance.PostHeaderMessage("Edit Mode: Opponents", 3);
        }

        public void Update()
        {
            //if (Engine.Input.WasPressed(Keys.D0))
            //{
            //    _watchingOpponent = (_watchingOpponent + 1) % (Race.Current.Opponents.Count + 1);   
            //}

            //if (_watchingOpponent == 0)
            //{
            //    _opponentCamera.Position = Race.Current.PlayerVehicle.GetBodyBottom();
            //    _opponentCamera.Orientation = Race.Current.PlayerVehicle.Chassis.Actor.GlobalOrientation.Forward;
            //    Engine.Camera = _opponentCamera;
            //}
            //else
            //{
            //    Opponent opponent = Race.Current.Opponents[_watchingOpponent-1];
            //    _opponentCamera.Position = opponent.Vehicle.GetBodyBottom();
            //    _opponentCamera.Orientation = opponent.Vehicle.Chassis.Actor.GlobalOrientation.Forward;
            //    Engine.Camera = _opponentCamera;

            //    opponent.Vehicle.Chassis.OutputDebugInfo();
            //}
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
                        if (path.Type == PathType.Cheat) c = Color.Blue;
                        //Engine.DebugRenderer.AddLine(node.Position, path.End.Position, c);

                        Vector3 offs = new Vector3(0, 0, path.Width);

                        Vector3 s3 = path.End.Position - node.Position;
                        s3.Normalize();
                        s3 *= path.Width;
                        Vector3 s1 = Vector3.Transform(s3, Matrix.CreateRotationY(90) * Matrix.CreateTranslation(node.Position));
                        s1.Y = node.Position.Y;
                        Vector3 s2 = Vector3.Transform(s3, Matrix.CreateRotationY(90) * Matrix.CreateTranslation(path.End.Position));
                        s2.Y = path.End.Position.Y;
                        
                        Engine.DebugRenderer.AddLine(s1, s2, c);
                        s1 = Vector3.Transform(s3, Matrix.CreateRotationY(-90) * Matrix.CreateTranslation(node.Position));
                        s1.Y = node.Position.Y;
                        s2 = Vector3.Transform(s3, Matrix.CreateRotationY(-90) * Matrix.CreateTranslation(path.End.Position));
                        s2.Y = path.End.Position.Y;
                        Engine.DebugRenderer.AddLine(s1, s2, c);
                    
                }
            }
        }
    }
}
