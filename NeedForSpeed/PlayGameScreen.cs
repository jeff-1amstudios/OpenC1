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
using System.Diagnostics;
using Carmageddon.CameraViews;
using Carmageddon.Physics;


namespace Carmageddon
{
    class PlayGameScreen : IGameScreen
    {
        VehicleModel _carModel;
        private Carmageddon.Physics.VehicleChassis _chassis;
        Race _race;
        SkyBox _skybox;
        List<ICamera> _cameras = new List<ICamera>();
        BasicEffect _effect;
        ChaseView _chaseView;
        
        FixedChaseCamera _camera;

        public PlayGameScreen()
        {

            GameVariables.Palette = new PaletteFile("c:\\games\\carma1\\data\\reg\\palettes\\drrender.pal");

            _carModel = new VehicleModel(@"C:\Games\carma1\data\cars\ivan.txt");

            _race = new Race(@"C:\Games\carma1\data\races\coasta1.TXT");

            _skybox = SkyboxGenerator.Generate(_race.HorizonTexture, _race.RaceFile.SkyboxRepetitionsX - 2, _race.RaceFile.DepthCueMode);
            _skybox.HeightOffset = _race.RaceFile.SkyboxPositionY * 0.1f;

            _camera = new FixedChaseCamera(1.5f * GameVariables.Scale.X, 1.5f * GameVariables.Scale.Y);
            _camera.Position = _race.RaceFile.GridPosition;
            Engine.Instance.Camera = _camera;
            Engine.Instance.Camera = new FPSCamera();

            Engine.Instance.Player = new Driver();
            Engine.Instance.Camera.Position = _race.RaceFile.GridPosition;
            SetupPhysics();

            _race.SetupPhysx(_carModel.Chassis);

            _chaseView = new ChaseView(_carModel);
            
        }

        private void SetupPhysics()
        {
            Matrix pose = Matrix.CreateRotationY(MathHelper.ToRadians(_race.RaceFile.GridDirection)) * Matrix.CreateTranslation(_race.RaceFile.GridPosition);
            _chassis = new Carmageddon.Physics.VehicleChassis(Carmageddon.Physics.PhysX.Instance.Scene, pose, 1, _carModel.Properties);
            _carModel.Chassis = _chassis;
        }


        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            InputProvider input = Engine.Instance.Input;
            if (input.WasPressed(Keys.C))
                _carModel.Crush();
   
            Engine.Instance.Camera.Update(gameTime);
            Engine.Instance.Player.Update(gameTime);

            _race.Update();

            _chassis.Accelerate(PlayerVehicleController.Acceleration);
            if (PlayerVehicleController.Brake != 0)
                _chassis.Accelerate(-PlayerVehicleController.Brake);
            
            _chassis.Steer(PlayerVehicleController.Turn);

            if (PlayerVehicleController.Handbrake)
                _chassis.PullHandbrake();
            else
                _chassis.ReleaseHandbrake();
            
            _carModel.Update(gameTime);
            
            PhysX.Instance.Update(gameTime);

            _camera.Position = _chassis.Body.GlobalPosition;
            
            if (!_carModel.Chassis.InAir)
            {
                _camera.Orientation = _chassis.Body.GlobalOrientation.Forward;
            }

            if (_skybox != null) _skybox.Update(gameTime);

            GameConsole.WriteLine("Speed " + _chassis.Speed);
            GameConsole.WriteLine("FPS: " + Engine.Instance.Fps);
            
        }

        public void Draw()
        {
            GameVariables.NbrDrawCalls = 0;
            if (GameVariables.CullingDisabled)
                Engine.Instance.Device.RenderState.CullMode = CullMode.None;
            else
                Engine.Instance.Device.RenderState.CullMode = CullMode.CullClockwiseFace;


            Engine.Instance.CurrentEffect = SetupRenderEffect();

            if (_skybox != null) _skybox.Draw();

            GameVariables.NbrSectionsChecked = GameVariables.NbrSectionsRendered = 0;
            
            _race.Render();

            _chaseView.Render();

            Engine.Instance.CurrentEffect.End();

            GameConsole.WriteLine("Draw Calls", GameVariables.NbrDrawCalls);

            //Carmageddon.Physics.PhysX.Instance.Draw();
        }

        #endregion



        private BasicEffect SetupRenderEffect()
        {
            GraphicsDevice device = Engine.Instance.Device;
            
            if (_effect == null)
            {
                _effect = new BasicEffect(Engine.Instance.Device, null);
                _effect.FogEnabled = false;
                if (GameVariables.DepthCueMode == "dark")
                    _effect.FogColor = new Vector3(0, 0, 0);
                else if (GameVariables.DepthCueMode == "fog" || GameVariables.DepthCueMode == "none")
                    _effect.FogColor = new Vector3(245, 245, 245);
                else
                {
                    Debug.Assert(false);
                }
                _effect.FogStart = Engine.Instance.DrawDistance - 50 * GameVariables.Scale.Z;
                _effect.FogEnd = Engine.Instance.DrawDistance;
                _effect.FogEnabled = true;
                //_effect.LightingEnabled = true;
                //_effect.EnableDefaultLighting();
                //_effect.AmbientLightColor = new Vector3(0.09f, 0.09f, 0.1f);
                //_effect.DirectionalLight0.Direction = new Vector3(1.0f, -1.0f, -1.0f); 
                _effect.TextureEnabled = true;
            }

            _effect.View = Engine.Instance.Camera.View;
            _effect.Projection = Engine.Instance.Camera.Projection;

            _effect.Begin(SaveStateMode.None);

            return _effect;
        }
    }
}
