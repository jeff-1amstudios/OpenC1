using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NFSEngine;
using StillDesign.PhysX;
using Carmageddon.HUD;
using System.Diagnostics;

namespace Carmageddon.CameraViews
{
    class CockpitView : BaseHUDItem, ICameraView
    {
        CockpitFile _cockpitFile;
        //FPSCamera _camera;
        SimpleCamera _camera;
        ActFile _actorFile;
        DatFile _modelsFile;
        VehicleModel _vehicle;

        public CockpitView(VehicleModel vehicle, string cockpitFile)
        {
            _vehicle = vehicle;
            _cockpitFile = new CockpitFile(cockpitFile);
            //_camera = new FPSCamera();
            //_camera.SetPerspective(55.55f, Engine.Instance.AspectRatio, 0.1f, 500);
            _camera = new SimpleCamera();
            _camera.FieldOfView = MathHelper.ToRadians(55.55f);
            _camera.AspectRatio = Engine.Instance.AspectRatio;

            _modelsFile = new DatFile(GameVariables.BasePath + "data\\models\\" + vehicle.Config.BonnetModelFile);
            _actorFile = new ActFile(GameVariables.BasePath + "data\\actors\\" + vehicle.Config.BonnetActorFile, _modelsFile);

            _actorFile.ResolveHierarchy(false, null);
            _actorFile.ResolveMaterials(vehicle.Resources);
            _modelsFile.Resolve(vehicle.Resources);

            //move head back
            _vehicle.Config.DriverHeadPosition.Z += 0.11f;

            foreach (var x in _cockpitFile.LeftHands)
            {
                x.Position1 += new Vector2(-20, 0);
                x.Position2 += new Vector2(-20, 0);
                x.Position1 /= new Vector2(640, 480);
                x.Position2 /= new Vector2(640, 480);
            }
            foreach (var x in _cockpitFile.RightHands)
            {
                x.Position1 += new Vector2(-20, 0);
                x.Position2 += new Vector2(-20, 0);
                x.Position1 /= new Vector2(640, 480);
                x.Position2 /= new Vector2(640, 480);
            }
            _cockpitFile.CenterHands.Position1 += new Vector2(-20, 0);
            _cockpitFile.CenterHands.Position2 += new Vector2(-20, 0);
            _cockpitFile.CenterHands.Position1 /= new Vector2(640, 480);
            _cockpitFile.CenterHands.Position2 /= new Vector2(640, 480);

        }

        #region ICameraView Members

        public bool Selectable
        {
            get { return true; }
        }

        public override void Update()
        {
            Vector3 forward = _vehicle.Chassis.Body.GlobalOrientation.Forward;
            forward.Y -= 0.1198517f;
            _camera.Orientation = forward; // Vector3.Transform(forward, Matrix.CreateRotationX(0.12f));

            _camera.Up = _vehicle.Chassis.Body.GlobalOrientation.Up;
            Vector3 vehicleBottom = new Vector3(_vehicle.Chassis.Body.GlobalPosition.X, -53.4348f, _vehicle.Chassis.Body.GlobalPosition.Z);
            vehicleBottom = GetBodyBottom();
            
            _camera.Position = vehicleBottom + Vector3.Transform(_vehicle.Config.DriverHeadPosition, _vehicle.Chassis.Body.GlobalOrientation) + new Vector3(0, 0.018f, 0);
            GameConsole.WriteLine("pos", _camera.Orientation.Y);
            
        }

        public override void Render()
        {
            Rectangle src = new Rectangle(32, 20, 640, 480);
            Rectangle rect = new Rectangle(0, 0, 800, 600);
            Engine.Instance.SpriteBatch.Draw(_cockpitFile.Forward, rect, src, Color.White);

            
            float steerRatio = _vehicle.Chassis.SteerRatio;
                        
            
            CockpitHandFrame frame = null;
            if (steerRatio < -0.2)
            {
                if (steerRatio < -0.8f)
                {
                    int hands = Math.Min(2, _cockpitFile.RightHands.Count - 1);
                    frame = _cockpitFile.RightHands[hands];
                }
                else if (steerRatio < -0.5f)
                    frame = _cockpitFile.RightHands[1];
                else if (steerRatio < -0.2f)
                    frame = _cockpitFile.RightHands[0];
                
            }
            else if (steerRatio > 0.2f)
            {
                if (steerRatio > 0.8f)
                    frame = _cockpitFile.LeftHands[0];
                else if (steerRatio > 0.5f)
                    frame = _cockpitFile.LeftHands[1];
                else if (steerRatio > 0.2)
                {
                    int hands = Math.Min(2, _cockpitFile.LeftHands.Count - 1);
                    frame = _cockpitFile.LeftHands[1];
                }
            }
            else
            {
                frame = _cockpitFile.CenterHands;
            }

            if (frame.Texture1 != null)
                Engine.Instance.SpriteBatch.Draw(frame.Texture1, ScaleVec2(frame.Position1), Color.White);
            if (frame.Texture2 != null)
                Engine.Instance.SpriteBatch.Draw(frame.Texture2, ScaleVec2(frame.Position2), Color.White);
            
            Vector3 vehicleBottom = new Vector3(_vehicle.Chassis.Body.GlobalPosition.X, -53.4348f, _vehicle.Chassis.Body.GlobalPosition.Z);
            vehicleBottom = GetBodyBottom();
            
            _modelsFile.SetupRender();
            _actorFile.Render(_modelsFile, Matrix.CreateFromQuaternion(_vehicle.Chassis.Body.GlobalOrientationQuat) * Matrix.CreateTranslation(vehicleBottom));
        }

        public void Activate()
        {
            Engine.Instance.Camera = _camera;

            //Vector3 vehicleBottom = new Vector3(GameVariables.PlayerVehicle.Chassis.Body.GlobalPosition.X, -53.4348f, GameVariables.PlayerVehicle.Chassis.Body.GlobalPosition.Z);
            Vector3 vehicleBottom = GetBodyBottom();
            
            _camera.Position = vehicleBottom + Vector3.Transform(_vehicle.Config.DriverHeadPosition, _vehicle.Chassis.Body.GlobalOrientation);
            _camera.Update(); 
        }

        public void Deactivate()
        {
            
        }

        #endregion

        private Vector3 GetBodyBottom()
        {
            Vector3 pos = _vehicle.Chassis.Body.GlobalPosition;
            pos.Y = pos.Y - _vehicle.Config.WheelActors[0].Position.Y - _vehicle.Config.DrivenWheelRadius;
            return pos;
            Vector3 vehicleBottom = new Vector3(GameVariables.PlayerVehicle.Chassis.Body.GlobalPosition.X, -53.4348f, GameVariables.PlayerVehicle.Chassis.Body.GlobalPosition.Z);
        }
    }
}
