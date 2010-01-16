using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework.Input;
using NFSEngine;
using Carmageddon.Gfx;
using System.Diagnostics;
using Carmageddon.CameraViews;
using Carmageddon.Physics;
using System.IO;


namespace Carmageddon
{
    class PlayGameScreen : IGameScreen
    {
        VehicleModel _carModel;
        private Carmageddon.Physics.VehicleChassis _chassis;
        Race _race;
        SkyBox _skybox;
        
        BasicEffect2 _effect;
        ChaseView _chaseView;
        
        FixedChaseCamera _camera;

        public PlayGameScreen()
        {
            if (!SoundCache.IsInitialized)
                SoundCache.Initialize();

            GameVariables.Palette = new PaletteFile(GameVariables.BasePath + "data\\reg\\palettes\\drrender.pal");

            _carModel = new VehicleModel(GameVariables.BasePath + @"data\cars\blkeagle.txt");

            _race = new Race(GameVariables.BasePath + @"data\races\citya1.TXT");

            _skybox = SkyboxGenerator.Generate(_race.HorizonTexture, _race.RaceFile.SkyboxRepetitionsX-3f, _race.RaceFile.DepthCueMode);
            _skybox.HeightOffset = -220 + _race.RaceFile.SkyboxPositionY * 1.5f;

            _camera = new FixedChaseCamera(6.8f, 7);
            _camera.FieldOfView = MathHelper.ToRadians(55.55f);
            Engine.Instance.Camera = _camera;
            //Engine.Instance.Camera = new FPSCamera();

            Engine.Instance.Player = new Driver { VehicleModel = _carModel };

            Engine.Instance.Camera.Position = _race.RaceFile.GridPosition;
            SetupPhysics();

            _race.SetupPhysx(_carModel.Chassis);

            _chaseView = new ChaseView(_carModel);

            //_race.StartCountdown();

        }

        private void SetupPhysics()
        {
            Matrix pose = Matrix.CreateRotationY(MathHelper.ToRadians(_race.RaceFile.GridDirection)) * Matrix.CreateTranslation(_race.RaceFile.GridPosition);
            _chassis = new Carmageddon.Physics.VehicleChassis(Carmageddon.Physics.PhysX.Instance.Scene, pose, 1, _carModel.CarFile);
            _carModel.Chassis = _chassis;
        }


        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            PhysX.Instance.Update();

            _camera.Position = _chassis.Body.GlobalPosition;

            if (!_chassis.InAir)
            {
                _camera.Orientation = _chassis.Body.GlobalOrientation.Forward;
                if (_chassis.Speed > 15)
                {
                    _camera.Rotation = (_chassis.Backwards ? MathHelper.Pi : 0);
                }
                _camera.HeightOverride = 0;
            }
            else
            {
                _camera.HeightOverride = 2;
            }

            _chassis.Accelerate(PlayerVehicleController.Acceleration);
            if (PlayerVehicleController.Brake != 0)
            {
                _chassis.Accelerate(-PlayerVehicleController.Brake);
            }

            _chassis.Steer(PlayerVehicleController.Turn);

            if (PlayerVehicleController.Handbrake)
                _chassis.PullHandbrake();
            else
                _chassis.ReleaseHandbrake();

            //InputProvider input = Engine.Instance.Input;
            //if (input.WasPressed(Keys.C))
            //    _carModel.Crush();
            
            _race.Update();
                        
            _carModel.Update();

            GameVariables.SparksEmitter.ParticleSystem.SetCamera(Engine.Instance.Camera);
            GameVariables.SparksEmitter.ParticleSystem.Update();
            

            Engine.Instance.Camera.Update(gameTime);

            if (_skybox != null) _skybox.Update(gameTime);

            GameConsole.WriteLine("FPS", Engine.Instance.Fps);

            Engine.Instance.Player.Orientation = _chassis.Body.GlobalOrientation;
            Engine.Instance.Player.Velocity = _chassis.Body.LinearVelocity;
            Engine.Instance.Player.Position = _camera.Position;
            Engine.Instance.Player.Update();
        }

        public void Draw()
        {
            GameVariables.NbrDrawCalls = 0;
            if (GameVariables.CullingDisabled)
                Engine.Instance.Device.RenderState.CullMode = CullMode.None;
            else
                Engine.Instance.Device.RenderState.CullMode = CullMode.CullClockwiseFace;

            GameVariables.CurrentEffect = SetupRenderEffect();

            if (_skybox != null) _skybox.Draw();
            GameVariables.NbrSectionsChecked = GameVariables.NbrSectionsRendered = 0;

            Engine.Instance.SpriteBatch.Begin();

            _race.Render();

            GameVariables.TyreSmokeSystem.Render();
            GameVariables.SparksEmitter.ParticleSystem.Render();

            _chaseView.Render();

            GameVariables.CurrentEffect.End();

            GameConsole.WriteLine("Draw Calls", GameVariables.NbrDrawCalls);

            //Carmageddon.Physics.PhysX.Instance.Draw();
        }

        #endregion



        private BasicEffect2 SetupRenderEffect()
        {
            GraphicsDevice device = Engine.Instance.Device;
            
            if (_effect == null)
            {
                _effect = new BasicEffect2();//(Engine.Instance.Device, null);
                _effect.FogEnabled = false;
                if (GameVariables.DepthCueMode == "dark")
                    _effect.FogColor = new Vector3(0, 0, 0);
                else if (GameVariables.DepthCueMode == "fog" || GameVariables.DepthCueMode == "none")
                    _effect.FogColor = new Vector3(245, 245, 245);
                else
                {
                    Debug.Assert(false);
                }
                _effect.FogStart = Engine.Instance.DrawDistance - 45 * GameVariables.Scale.Z;
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
