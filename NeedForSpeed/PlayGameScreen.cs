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
using Particle3DSample;


namespace Carmageddon
{
    class PlayGameScreen : IGameScreen
    {
        VehicleModel _playerVehicle;
        Race _race;
        
        BasicEffect2 _effect;
        List<ICameraView> _views = new List<ICameraView>();
        int _currentView = 0;
        
        

        public PlayGameScreen()
        {
            if (!SoundCache.IsInitialized)
                SoundCache.Initialize();

            GameVariables.Palette = new PaletteFile(GameVariables.BasePath + "data\\reg\\palettes\\drrender.pal");

            _race = new Race(GameVariables.BasePath + @"data\races\citya1.TXT");

            string car = "blkeagle.txt";
            _playerVehicle = new VehicleModel(GameVariables.BasePath + @"data\cars\" + car);
            _playerVehicle.SetupChassis(_race.RaceFile.GridDirection, _race.RaceFile.GridPosition);
            GameVariables.PlayerVehicle = _playerVehicle;

            Engine.Instance.Player = new Driver { VehicleModel = _playerVehicle };

            SetupPhysics();

            _race.SetupPhysx(_playerVehicle.Chassis);

            _views.Add(new ChaseView(_playerVehicle));
            _views.Add(new CockpitView(_playerVehicle, GameVariables.BasePath + @"data\32x20x8\cars\" + car));
            _views.Add(new FlyView());
            _views[_currentView].Activate();
        }

        private void SetupPhysics()
        {
            
        }


        #region IDrawableObject Members

        public void Update()
        {
            VehicleChassis playerCar = _playerVehicle.Chassis;
            playerCar.Accelerate(PlayerVehicleController.Acceleration);
            if (PlayerVehicleController.Brake != 0)
            {
                playerCar.Accelerate(-PlayerVehicleController.Brake);
            }
            playerCar.Steer(PlayerVehicleController.Turn);

            if (PlayerVehicleController.Handbrake)
                playerCar.PullHandbrake();
            else
                playerCar.ReleaseHandbrake();

            _race.Update();                        
            _playerVehicle.Update();

            PhysX.Instance.Update();

            foreach (ParticleSystem system in ParticleSystem.AllParticleSystems)
                system.Update();

            if (Engine.Instance.Input.WasPressed(Keys.C))
            {
                _views[_currentView].Deactivate();
                _currentView = (_currentView + 1) % _views.Count;
                _views[_currentView].Activate();
            }

            _views[_currentView].Update();
            Engine.Instance.Camera.Update();


            GameConsole.WriteLine("FPS", Engine.Instance.Fps);
        }

        public void Render()
        {
            GameVariables.NbrDrawCalls = 0;
            if (GameVariables.CullingDisabled)
                Engine.Instance.Device.RenderState.CullMode = CullMode.None;
            else
                Engine.Instance.Device.RenderState.CullMode = CullMode.CullClockwiseFace;

            GameVariables.CurrentEffect = SetupRenderEffect();

            GameVariables.NbrSectionsChecked = GameVariables.NbrSectionsRendered = 0;

            Engine.Instance.SpriteBatch.Begin();

            _race.Render();

            _views[_currentView].Render();

            foreach (ParticleSystem system in ParticleSystem.AllParticleSystems)
            {
                system.Render();
            }

            Engine.Instance.SpriteBatch.End();
            Engine.Instance.Device.RenderState.DepthBufferEnable = true;
            Engine.Instance.Device.RenderState.AlphaBlendEnable = false;
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            GameVariables.CurrentEffect.End();

            GameConsole.WriteLine("Draw Calls", GameVariables.NbrDrawCalls);

            Carmageddon.Physics.PhysX.Instance.Draw();
        }

        #endregion



        private BasicEffect2 SetupRenderEffect()
        {
            GraphicsDevice device = Engine.Instance.Device;
            
            if (_effect == null)
            {
                _effect = new BasicEffect2();
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
