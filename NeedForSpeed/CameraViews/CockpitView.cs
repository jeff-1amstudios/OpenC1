using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NFSEngine;
using StillDesign.PhysX;

namespace Carmageddon.CameraViews
{
    class CockpitView : ICameraView
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

            _modelsFile = new DatFile(GameVariables.BasePath + "data\\models\\" + vehicle.CarFile.BonnetModelFile);
            _actorFile = new ActFile(GameVariables.BasePath + "data\\actors\\" + vehicle.CarFile.BonnetActorFile, _modelsFile);
            
            _actorFile.ResolveHierarchy(false, null);
            _actorFile.ResolveMaterials(vehicle.Resources);
            _modelsFile.Resolve(vehicle.Resources);
        }

        #region ICameraView Members

        public bool Selectable
        {
            get { return true; }
        }

        public void Update()
        {
            //_camera.Position = GameVariables.PlayerVehicle.Chassis.Body.GlobalPosition + _vehicle.CarFile.DriverHeadPosition;
            //_camera.Position = GameVariables.PlayerVehicle.Chassis.Body.GlobalPosition +new Vector3(0, 0.5f, 0);
            _camera.Orientation = GameVariables.PlayerVehicle.Chassis.Body.GlobalOrientation.Forward;
            _camera.Up = GameVariables.PlayerVehicle.Chassis.Body.GlobalOrientation.Up;
            //Vector3 vehicleBottom = new Vector3(_vehicle.Chassis.Body.GlobalPosition.X, -53.4348f, _vehicle.Chassis.Body.GlobalPosition.Z);
            _camera.Position = _vehicle.Chassis.Body.GlobalPosition;// vehicleBottom + _vehicle.CarFile.DriverHeadPosition; // +Vector3.Transform(_vehicle.CarFile.DriverHeadPosition, _vehicle.Chassis.Body.GlobalOrientation);
        }

        public void Render()
        {
            //Rectangle rect = new Rectangle(0, 0, 800, 600);
            //Engine.Instance.SpriteBatch.Draw(_cockpitFile.Forward, rect, Color.White);

            GameVariables.PlayerVehicle.Render();
            //float halfHeight = ((BoxShape)GameVariables.PlayerVehicle.Chassis.Body.Shapes[0]).Dimensions.Y / 2;
            //Vector3 pos = _vehicle.Chassis.Body.GlobalPosition - new Vector3(0, halfHeight, 0) +_vehicle.CarFile.DriverHeadPosition;

            
            Vector3 vehicleBottom = new Vector3(_vehicle.Chassis.Body.GlobalPosition.X, -53.4348f, _vehicle.Chassis.Body.GlobalPosition.Z);
            //_camera.Position = vehicleBottom; // +_vehicle.CarFile.DriverHeadPosition; // +Vector3.Transform(_vehicle.CarFile.DriverHeadPosition, _vehicle.Chassis.Body.GlobalOrientation);
            Engine.Instance.DebugRenderer.AddAxis(Matrix.CreateTranslation(vehicleBottom + _vehicle.CarFile.DriverHeadPosition), 4);// GameVariables.PlayerVehicle.Chassis.Body.GlobalPose, 4);


            //_camera.Position = vehicleBottom + _vehicle.CarFile.DriverHeadPosition; // +Vector3.Transform(_vehicle.CarFile.DriverHeadPosition, _vehicle.Chassis.Body.GlobalOrientation);
            
            //_modelsFile.SetupRender();
            //_actorFile.Render(_modelsFile, Matrix.CreateFromQuaternion(_vehicle.Chassis.Body.GlobalOrientationQuat) * Matrix.CreateTranslation(vehicleBottom));
        }

        public void Activate()
        {
            Engine.Instance.Camera = _camera;

            Vector3 vehicleBottom = new Vector3(GameVariables.PlayerVehicle.Chassis.Body.GlobalPosition.X, -53.4348f, GameVariables.PlayerVehicle.Chassis.Body.GlobalPosition.Z);
            //_camera.Orientation = _vehicle.Chassis.Body.GlobalOrientation.Up;
            
            //_camera.Position = vehicleBottom + _vehicle.CarFile.DriverHeadPosition;
            _camera.Position = vehicleBottom + Vector3.Transform(_vehicle.CarFile.DriverHeadPosition, _vehicle.Chassis.Body.GlobalOrientation);
            _camera.Update(); 
        }

        public void Deactivate()
        {
            
        }

        #endregion
    }
}
