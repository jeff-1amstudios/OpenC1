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

            _race = new Race(GameVariables.BasePath + @"data\races\industa1.TXT");

            string car = "blkeagle.txt";
            _playerVehicle = new VehicleModel(GameVariables.BasePath + @"data\cars\" + car, true);
            _playerVehicle.SetupChassis(_race.ConfigFile.GridPosition, _race.ConfigFile.GridDirection);

            GameVariables.PlayerVehicle = _playerVehicle;

            Engine.Player = new Driver { VehicleModel = _playerVehicle };


            _views.Add(new ChaseView(_playerVehicle));
            _views.Add(new CockpitView(_playerVehicle, GameVariables.BasePath + @"data\64x48x8\cars\" + car));
            _views.Add(new FlyView());
            _views[_currentView].Activate();
        }


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

            if (Engine.Input.WasPressed(Keys.C))
            {
                _views[_currentView].Deactivate();
                _currentView = (_currentView + 1) % _views.Count;
                _views[_currentView].Activate();
            }
            if (Engine.Input.WasPressed(Keys.P))
            {
                TakeScreenshot();
            }
            if (Engine.Input.WasPressed(Keys.L))
            {
                GameVariables.LightingEnabled = !GameVariables.LightingEnabled;
                MessageRenderer.Instance.PostMessage("Lighting: " + (GameVariables.LightingEnabled ? "Enabled" : "Disabled"), 2);
                _effect = null;
            }

            _views[_currentView].Update();
            Engine.Camera.Update();


            GameConsole.WriteLine("FPS", Engine.Fps);
        }

        public void Render()
        {
            Engine.Device.Clear(GameVariables.FogColor);
            GameVariables.NbrDrawCalls = 0;
            if (GameVariables.CullingDisabled)
                Engine.Device.RenderState.CullMode = CullMode.None;
            else
                Engine.Device.RenderState.CullMode = CullMode.CullClockwiseFace;

            GameVariables.CurrentEffect = SetupRenderEffect();

            GameVariables.NbrSectionsChecked = GameVariables.NbrSectionsRendered = 0;

            Engine.SpriteBatch.Begin();

            _race.Render();
            _views[_currentView].Render();

            foreach (ParticleSystem system in ParticleSystem.AllParticleSystems)
            {
                system.Render();
            }

            Engine.SpriteBatch.End();
            Engine.Device.RenderState.DepthBufferEnable = true;
            Engine.Device.RenderState.AlphaBlendEnable = false;
            Engine.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            GameVariables.CurrentEffect.End();

            GameConsole.WriteLine("Draw Calls", GameVariables.NbrDrawCalls);

            Carmageddon.Physics.PhysX.Instance.Draw();
        }



        private BasicEffect2 SetupRenderEffect()
        {
            GraphicsDevice device = Engine.Device;

            if (_effect == null)
            {
                _effect = new BasicEffect2();
                if (Race.Current.ConfigFile.DepthCueMode == DepthCueMode.Dark)
                {
                    _effect.FogColor = new Vector3(0, 0, 0);
                    GameVariables.FogColor = new Color(0, 0, 0);
                }
                else if (Race.Current.ConfigFile.DepthCueMode == DepthCueMode.Fog)
                {
                    _effect.FogColor = new Vector3(245, 245, 245);
                    GameVariables.FogColor = new Color(245, 245, 245);
                }
                else
                {
                    Debug.Assert(false);
                }

                _effect.FogStart = Engine.DrawDistance - 45 * GameVariables.Scale.Z;
                _effect.FogEnd = Engine.DrawDistance;
                _effect.FogEnabled = true;
                _effect.TextureEnabled = true;

                if (GameVariables.LightingEnabled)
                {
                    //_effect.EnableDefaultLighting();
                    _effect.LightingEnabled = true;
                    _effect.DiffuseColor = new Vector3(1);
                    _effect.DirectionalLight0.DiffuseColor = new Vector3(1);
                    Vector3 dir = new Vector3(-1f, 1, -1f);
                    dir.Normalize();
                    _effect.DirectionalLight0.Direction = dir;
                    _effect.DirectionalLight0.Enabled = true;
                    _effect.SpecularColor = new Vector3(0.3f);
                    _effect.SpecularPower = 16;
                    _effect.AmbientLightColor = new Vector3(0.65f);
                    //_effect.PreferPerPixelLighting = true;
                }
            }

            _effect.View = Engine.Camera.View;
            _effect.Projection = Engine.Camera.Projection;

            _effect.Begin(SaveStateMode.None);

            return _effect;
        }

        private void TakeScreenshot()
        {
            int count = Directory.GetFiles(GameVariables.BasePath + "data", "ndump*.jpg").Length;
            string name = "ndump" + count.ToString("000") + ".jpg";

            GraphicsDevice device = Engine.Device;
            new ResolveTexture2D(device, 10, 10, 1, SurfaceFormat.Color);
            using (ResolveTexture2D screenshot = new ResolveTexture2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight, 1, SurfaceFormat.Color))
            {
                device.ResolveBackBuffer(screenshot);
                screenshot.Save(GameVariables.BasePath + "data\\" + name, ImageFileFormat.Jpg);
            }

            MessageRenderer.Instance.PostMessage("Screenshot dumped to " + name, 3);
        }
    }
}
