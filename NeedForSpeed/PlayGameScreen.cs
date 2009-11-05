using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework.Input;
using NFSEngine;
using Carmageddon.Track;


namespace Carmageddon
{
    class PlayGameScreen : IGameScreen
    {
        VehicleModel _carModel;
        Race _race;
        SkyBox _skybox;
        List<ICamera> _cameras = new List<ICamera>();

        private Carmageddon.Physics.VehicleChassis _physxVehicle;

        FixedChaseCamera _camera;

        public PlayGameScreen()
        {

            Engine.Instance.Device.SamplerStates[0].MinFilter = TextureFilter.None;
            Engine.Instance.Device.SamplerStates[0].MagFilter = TextureFilter.None;
            Engine.Instance.Device.SamplerStates[0].MipFilter = TextureFilter.None;

            GameVariables.Palette = new PaletteFile("c:\\games\\carma1\\data\\reg\\palettes\\drrender.pal");

            _carModel = new VehicleModel(@"C:\Games\carma1\data\cars\blkeagle.txt");

            _race = new Race(@"C:\Games\carma1\data\races\cityb1.TXT");
            
            //if (_race.HorizonTexture != null)
            //{
            //    _skybox = SkyboxGenerator.Generate(_race.HorizonTexture, _race.RaceFile.SkyboxRepetitionsX - 1);
            //    _skybox.HeightOffset = _race.RaceFile.SkyboxPositionY * 0.012f;
            //}

            _camera = new FixedChaseCamera(12.0f,3.0f);
            _camera.Position = _race.RaceFile.GridPosition;
            Engine.Instance.Camera = _camera;
            Engine.Instance.Camera = new FPSCamera();

            Engine.Instance.Player = new Driver();
            Engine.Instance.Camera.Position = _race.RaceFile.GridPosition;
            SetupPhysics();
        }

        private void SetupPhysics()
        {
            Matrix pose = Matrix.CreateRotationY(MathHelper.ToRadians(_race.RaceFile.GridDirection)) * Matrix.CreateTranslation(_race.RaceFile.GridPosition);
            _physxVehicle = new Carmageddon.Physics.VehicleChassis(Carmageddon.Physics.PhysX.Instance.Scene, pose, 1, _carModel.Properties);
            _carModel.Chassis = _physxVehicle;
        }


        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            
            if (_skybox != null) _skybox.Update(gameTime);

            InputProvider input = Engine.Instance.Input;
            if (input.WasPressed(Keys.C))
                _carModel.Crush();

            
            Engine.Instance.Camera.Update(gameTime);
            Engine.Instance.Player.Update(gameTime);

            if (input.IsKeyDown(Keys.Up) || input.IsKeyDown(Keys.Down))
            {
                if (input.IsKeyDown(Keys.Up))
                    _physxVehicle.Accelerate(1.0f);
                else
                    _physxVehicle.Accelerate(-1.0f);
            }
            else
                _physxVehicle.Accelerate(0.0f);

            if (input.IsKeyDown(Keys.Left) || input.IsKeyDown(Keys.Right))
            {
                if (input.IsKeyDown(Keys.Left))
                    _physxVehicle.SteerLeft();
                else
                    _physxVehicle.SteerRight();
            }
            else
                _physxVehicle.StopSteering();

            if (input.IsKeyDown(Keys.Space))
            {
                _physxVehicle.Handbrake = true;
            }
            else
            {
                _physxVehicle.Handbrake = false;
            }


            _physxVehicle.Update(gameTime);
            Carmageddon.Physics.PhysX.Instance.Update(gameTime);

            _camera.Position = _physxVehicle.Body.GlobalPosition;
            _camera.Orientation = _physxVehicle.Body.GlobalOrientation.Forward;

            //GameConsole.WriteLine(Engine.Instance.Camera.Position);
            GameConsole.WriteLine("Speed " + _physxVehicle.Speed);
            GameConsole.WriteLine("FPS: " + Engine.Instance.Fps);
            
        }

        public void Draw()
        {
            GameVariables.NbrDrawCalls = 0;
            if (_skybox != null) _skybox.Draw();

            //Engine.Instance.GraphicsUtils.AddSquareGrid(Matrix.CreateTranslation(new Vector3(_race.RaceFile.GridPosition.X, 5, _race.RaceFile.GridPosition.Z)), 20, Color.Yellow);
            
            //_carModel.RenderBody(Matrix.CreateTranslation(new Vector3(_race.RaceFile.GridPosition.X, 5, _race.RaceFile.GridPosition.Z)));

            GameVariables.NbrSectionsChecked = GameVariables.NbrSectionsRendered = 0;
            _race.Render();


            _carModel.Render();

            GameConsole.WriteLine("Draw Calls", GameVariables.NbrDrawCalls);

            //Carmageddon.Physics.PhysX.Instance.Draw();
            
        }

        #endregion
    }
}
