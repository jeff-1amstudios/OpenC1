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
using Carmageddon.EditModes;
using Carmageddon.Screens;


namespace Carmageddon
{
    class PlayGameScreen : IGameScreen
    {
        public IGameScreen Parent { get; private set; }

        Race _race;
        BasicEffect2 _effect;   
        List<GameMode> _modes = new List<GameMode>();
       
        int _currentEditMode = 0;

        public PlayGameScreen(IGameScreen parent)
        {
            Parent = parent;
            GC.Collect();

            //GameVars.SelectedCarFileName = "blkeagle.txt";
            _race = new Race(GameVars.BasePath + "data\\races\\" + GameVars.SelectedRaceInfo.RaceFilename, GameVars.SelectedCarFileName);
            
            _modes.Add(new NormalMode());
            _modes.Add(new OpponentEditMode());
            GameMode.Current = _modes[_currentEditMode];
        }


        public void Update()
        {
            PhysX.Instance.Simulate();
            PhysX.Instance.Fetch();

            _race.Update();

            foreach (ParticleSystem system in ParticleSystem.AllParticleSystems)
                system.Update();

            if (Engine.Input.WasPressed(Keys.Escape))
            {
                Engine.Screen = new PauseMenuScreen(this);
                return;
            }

            if (Engine.Input.WasPressed(Keys.F4))
            {
                _currentEditMode = (_currentEditMode + 1) % _modes.Count;
                GameMode.Current = _modes[_currentEditMode];
            }
            if (Engine.Input.WasPressed(Keys.P))
            {
                TakeScreenshot();
                //MessageRenderer.Instance.PostMainMessage("destroy.pix", 50, 0.7f, 0.003f, 1.4f);
            }
            if (Engine.Input.WasPressed(Keys.L))
            {
                GameVars.LightingEnabled = !GameVars.LightingEnabled;
                MessageRenderer.Instance.PostHeaderMessage("Lighting: " + (GameVars.LightingEnabled ? "Enabled" : "Disabled"), 2);
                _effect = null;
            }
                        
            GameMode.Current.Update();
            _race.PlayerVehicle.Chassis.OutputDebugInfo();

            Engine.Camera.Update();
            
            GameConsole.WriteLine("FPS", Engine.Fps);

            
        }

        public void Render()
        {
            Engine.Device.Clear(GameVars.FogColor);
            GameVars.NbrDrawCalls = 0;
            if (GameVars.CullingDisabled)
                Engine.Device.RenderState.CullMode = CullMode.None;
            else
                Engine.Device.RenderState.CullMode = CullMode.CullClockwiseFace;

            GameVars.CurrentEffect = SetupRenderEffect();

            GameVars.NbrSectionsChecked = GameVars.NbrSectionsRendered = 0;

            Engine.SpriteBatch.Begin();

            _race.Render();
            _modes[_currentEditMode].Render();

            foreach (ParticleSystem system in ParticleSystem.AllParticleSystems)
            {
                system.Render();
            }

            Engine.SpriteBatch.End();
            Engine.Device.RenderState.DepthBufferEnable = true;
            Engine.Device.RenderState.AlphaBlendEnable = false;
            Engine.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            GameVars.CurrentEffect.End();
            
            

            GameConsole.WriteLine("Draw Calls", GameVars.NbrDrawCalls);

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
                    GameVars.FogColor = new Color(0, 0, 0);
                }
                else if (Race.Current.ConfigFile.DepthCueMode == DepthCueMode.Fog)
                {
                    _effect.FogColor = new Vector3(245, 245, 245);
                    GameVars.FogColor = new Color(245, 245, 245);
                }
                else
                {
                    Debug.Assert(false);
                }

                _effect.FogStart = Engine.DrawDistance - 45 * GameVars.Scale.Z;
                _effect.FogEnd = Engine.DrawDistance;
                _effect.FogEnabled = true;
                _effect.TextureEnabled = true;
                _effect.TexCoordsMultiplier = 1;

                if (GameVars.LightingEnabled)
                {
                    //_effect.EnableDefaultLighting();
                    _effect.LightingEnabled = true;
                    //_effect.DiffuseColor = new Vector3(1);
                    _effect.AmbientLightColor = new Vector3(0.8f);
                    _effect.DirectionalLight0.DiffuseColor = new Vector3(1);
                    
                    Vector3 dir = new Vector3(-1f, 0.9f, -1f);
                    dir.Normalize();
                    _effect.DirectionalLight0.Direction = dir;
                    _effect.DirectionalLight0.Enabled = true;

                    //_effect.DirectionalLight1.DiffuseColor = new Vector3(0.8f);
                    //dir = new Vector3(1f, 1, 1f);
                    //dir.Normalize();
                    //_effect.DirectionalLight1.Direction = dir;
                    //_effect.DirectionalLight1.Enabled = true;


                    //_effect.SpecularColor = new Vector3(0.3f);
                    //_effect.SpecularPower = 100;
                    //_effect.AmbientLightColor = new Vector3(0.65f);
                    //_effect.DirectionalLight1.Enabled = false;
                    //_effect.DirectionalLight2.Enabled = false;
                    //_effect.TextureEnabled = false;
                    //_effect.DirectionalLight0.Enabled = true;

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
            int count = Directory.GetFiles(GameVars.BasePath + "data", "ndump*.jpg").Length;
            string name = "ndump" + count.ToString("000") + ".jpg";

            GraphicsDevice device = Engine.Device;
            new ResolveTexture2D(device, 10, 10, 1, SurfaceFormat.Color);
            using (ResolveTexture2D screenshot = new ResolveTexture2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight, 1, SurfaceFormat.Color))
            {
                device.ResolveBackBuffer(screenshot);
                screenshot.Save(GameVars.BasePath + "data\\" + name, ImageFileFormat.Jpg);
            }

            MessageRenderer.Instance.PostHeaderMessage("Screenshot dumped to " + name, 3);
        }
    }
}
