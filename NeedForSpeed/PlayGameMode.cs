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
using JigLibX.Physics;
using JigLibX.Collision;
using Carmageddon.Physics;
using JigLibX.Geometry;

namespace Carmageddon
{
    class PlayGameMode : IGameScreen
    {
        Vehicle _car;
        Race _race;
        SkyBox _skybox;

        PhysicsSystem _physicSystem;
        CarObject _carObject;
        ConstraintWorldPoint _objectController = new ConstraintWorldPoint();
        ConstraintVelocity _damperController = new ConstraintVelocity();
        FixedChaseCamera _camera;

        public PlayGameMode()
        {
            //Engine.Instance.Device.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
            //Engine.Instance.Device.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;

            GameVariables.Palette = new PaletteFile("c:\\games\\carma1\\data\\reg\\palettes\\drrender.pal");

            _car = new Vehicle(@"C:\Games\carma1\data\cars\blkeagle.txt");

            _race = new Race(@"C:\Games\carma1\data\races\COLICIUM.TXT");

            if (_race.HorizonTexture != null)
            {
                _skybox = SkyboxGenerator.Generate(_race.HorizonTexture, _race.RaceFile.SkyboxRepetitionsX - 1);
                _skybox.HeightOffset = _race.RaceFile.SkyboxPositionY * 0.012f;
            }
            _camera = new FixedChaseCamera();
            Engine.Instance.Camera = _camera;// new FPSCamera(Engine.Instance.Game);// camera;

            Engine.Instance.Player = new Driver();
            Engine.Instance.Player.Position = _race.RaceFile.GridPosition;
            Engine.Instance.Player.SetRotation(MathHelper.ToRadians(_race.RaceFile.GridDirection));
            
            SetupPhysics();
        }

        private void SetupPhysics()
        {
            _physicSystem = new PhysicsSystem();
            _physicSystem.CollisionSystem = new CollisionSystemBrute();
            _physicSystem.EnableFreezing = true;
            _physicSystem.SolverType = PhysicsSystem.Solver.Normal;
            _physicSystem.CollisionSystem.UseSweepTests = true;
            _physicSystem.NumCollisionIterations = 8;
            _physicSystem.NumContactIterations = 8;
            _physicSystem.NumPenetrationRelaxtionTimesteps = 15;

            //PlaneObject planeObj = new PlaneObject(Engine.Instance.Game, null, 220.0f);
            TriangleMeshObject tri = new TriangleMeshObject(Engine.Instance.Game, _race.GetTrackActors());           

            _carObject = new CarObject(Engine.Instance.Game, null, null,
                true,   // front wheel drive 
                true,   // rear wheel drive 
                30.0f,  // max steering angle 
                4f,     // steering rate 
                3.6f,   // front lateral traction 
                3.1f,   // rear lateral traction 
                1f,     // front longintudinal traction 
                2f,     // rear longitudinal traction 
                2.5f,   // handbrake rear lateral traction  
                .75f,   // handbrake rear longitudinal traction 
                6f,     // starting slide factor 
                15f,    // threshold 1 slide factor 
                30f,    // threshold 2 slide factor 
                .7f,    // slip threshold 1 
                10.0f,  // slip threshold 2 
                2.0f,   // slide speed factor 
                0.7f,   // traction loss factor on slip 
                0.3f,   // suspension travel 
                0.90f,  // wheel radius 
                -0.10f, // wheel mounting point  
                0.6f,   // spring rate 
                0.6f,   // shock dampening 
                4,      // wheel rays 
                2.0f,   // roll resistance 
                5600.0f, // top speed 
                2000.0f, // torque 
                _physicSystem.Gravity.Length() // gravity  
                );

            _carObject.Car.Chassis.Body.MoveTo(Engine.Instance.Player.Position + new Vector3(0, 20,0), Matrix.Identity);
            _carObject.Car.EnableCar();
            _carObject.Car.Chassis.Body.AllowFreezing = false;
        }


        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            Engine.Instance.Camera.Update(gameTime);
            Engine.Instance.Player.Update(gameTime);
            if (_skybox != null) _skybox.Update(gameTime);

            InputProvider input = Engine.Instance.Input;
            if (input.IsKeyDown(Keys.Up) || input.IsKeyDown(Keys.Down))
            {
                if (input.IsKeyDown(Keys.Up))
                    _carObject.Car.Accelerate = 1.0f;
                else
                    _carObject.Car.Accelerate = -1.0f;
            }
            else
                _carObject.Car.Accelerate = 0.0f;

            if (input.IsKeyDown(Keys.Left) || input.IsKeyDown(Keys.Right))
            {
                if (input.IsKeyDown(Keys.Left))
                    _carObject.Car.Steer = 1.0f;
                else
                    _carObject.Car.Steer = -1.0f;
            }
            else
                _carObject.Car.Steer = 0.0f;

            if (input.IsKeyDown(Keys.B))
                _carObject.Car.HBrake = 1.0f;
            else
                _carObject.Car.HBrake = 0.0f;

            _camera.Position = _carObject.Car.Chassis.Body.Position;
            _camera.ChaseDirection = _carObject.Car.Chassis.Body.Orientation.Right;



            float timeStep = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
            if (timeStep < 1.0f / 60.0f) _physicSystem.Integrate(timeStep);
            else _physicSystem.Integrate(1.0f / 60.0f);

            GameConsole.WriteLine(Engine.Instance.Camera.Position, 0);
        }

        public void Draw()
        {
            if (_skybox != null) _skybox.Draw();
                        
            _race.Render();
            //_car.Render();

            _carObject.Draw(null);
        }

        #endregion
    }
}
