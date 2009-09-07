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
    class PlayGameMode : IGameScreen
    {
        Vehicle _car;
        Race _race;
        SkyBox _skybox;

        private Carmageddon.Physics.PhysicsVehicle _basicVehicle;

        FixedChaseCamera _camera;

        public PlayGameMode()
        {
            //Engine.Instance.Device.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
            //Engine.Instance.Device.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;

            GameVariables.Palette = new PaletteFile("c:\\games\\carma1\\data\\reg\\palettes\\drrender.pal");

            //_car = new Vehicle(@"C:\Games\carma1\data\cars\blkeagle.txt");

            _race = new Race(@"C:\Games\carma1\data\races\citya1.TXT");
            _basicVehicle = new Carmageddon.Physics.PhysicsVehicle(Carmageddon.Physics.PhysX.Instance.Scene, _race.RaceFile.GridPosition, 1);

            //if (_race.HorizonTexture != null)
            //{
            //    _skybox = SkyboxGenerator.Generate(_race.HorizonTexture, _race.RaceFile.SkyboxRepetitionsX - 1);
            //    _skybox.HeightOffset = _race.RaceFile.SkyboxPositionY * 0.012f;
            //}

            _camera = new FixedChaseCamera(12);
            _camera.Position = _race.RaceFile.GridPosition;
            Engine.Instance.Camera = _camera;// new FPSCamera(Engine.Instance.Game);// camera;
            

            Engine.Instance.Player = new Player();// new Driver();
            Engine.Instance.Player.Position = _race.RaceFile.GridPosition;
            
            Engine.Instance.Player.SetRotation(MathHelper.ToRadians(_race.RaceFile.GridDirection));
            
            SetupPhysics();
        }

        private void SetupPhysics()
        {
        }


        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            
            if (_skybox != null) _skybox.Update(gameTime);

            InputProvider input = Engine.Instance.Input;
            if (input.WasPressed(Keys.C))
                _car.Crush();

            _basicVehicle.Update(gameTime);
            
            Engine.Instance.Camera.Update(gameTime);
            Engine.Instance.Player.Update(gameTime);

            Carmageddon.Physics.PhysX.Instance.Update(gameTime);

            if (input.IsKeyDown(Keys.Up) || input.IsKeyDown(Keys.Down))
            {
                if (input.IsKeyDown(Keys.Up))
                    _basicVehicle.Accelerate(1.0f);
                else
                    _basicVehicle.Accelerate(-1.0f);
            }
            else
                _basicVehicle.Accelerate(0.0f);

            if (input.IsKeyDown(Keys.Left) || input.IsKeyDown(Keys.Right))
            {
                if (input.IsKeyDown(Keys.Left))
                    _basicVehicle.Steer(-0.78f);
                else
                    _basicVehicle.Steer(0.78f);
            }
            else
                _basicVehicle.Steer(0.0f);

            //if (input.IsKeyDown(Keys.B))
            //    _carObject.Car.HBrake = 1.0f;
            //else
            //    _carObject.Car.HBrake = 0.0f;

            _camera.Position = _basicVehicle.VehicleGlobalPosition;
            _camera.Orientation = _basicVehicle.VehicleGlobalOrientation.Forward;


            //float timeStep = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
            //if (timeStep < 1.0f / 60.0f) _physicSystem.Integrate(timeStep);
            //else _physicSystem.Integrate(1.0f / 60.0f);

            GameConsole.WriteLine(Engine.Instance.Camera.Position, 0);
        }

        public void Draw()
        {
            if (_skybox != null) _skybox.Draw();

            //_car.Render();
            _race.Render();

            Carmageddon.Physics.PhysX.Instance.Draw();
            
        }

        #endregion
    }
}
