using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenC1.Parsers;
using Microsoft.Xna.Framework.Input;
using OpenC1.Gfx;
using System.Diagnostics;
using OpenC1.CameraViews;
using OpenC1.Physics;
using System.IO;
using OneAmEngine;
using OpenC1.Screens;
using OpenC1.GameModes;
using Microsoft.Xna.Framework.Storage;


namespace OpenC1
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

            _race = new Race(GameVars.BasePath + "races\\" + GameVars.SelectedRaceInfo.RaceFilename, GameVars.SelectedCarFileName);
            
            _modes.Add(new StandardGameMode());
            _modes.Add(new FlyMode());
            _modes.Add(new OpponentEditMode());
            _modes.Add(new PedEditMode());
            GameMode.Current = _modes[_currentEditMode];
			
        }


        public void Update()
        {
            PhysX.Instance.Simulate();
            PhysX.Instance.Fetch();

            _race.Update();

            foreach (ParticleSystem system in ParticleSystem.AllParticleSystems)
                system.Update();

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
                        
            GameMode.Current.Update();
            _race.PlayerVehicle.Chassis.OutputDebugInfo();

            Engine.Camera.Update();
            
            GameConsole.WriteLine("FPS", Engine.Fps);   
        }

        public void Render()
        {
            Engine.Device.Clear(GameVars.FogColor);
			
            GameVars.NbrDrawCalls = 0;
            
            GameVars.CurrentEffect = SetupRenderEffect();
			
            GameVars.NbrSectionsChecked = GameVars.NbrSectionsRendered = 0;

            Engine.SpriteBatch.Begin();

            _race.Render();
            _modes[_currentEditMode].Render();

			Engine.Device.RenderState.CullMode = CullMode.None;

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

            GameConsole.WriteLine("Position", Race.Current.PlayerVehicle.GetBodyBottom() / 6);

            GameConsole.WriteLine("Draw Calls", GameVars.NbrDrawCalls);

            OpenC1.Physics.PhysX.Instance.Draw();
        }
		

        private BasicEffect2 SetupRenderEffect()
        {
            GraphicsDevice device = Engine.Device;

            if (_effect == null)
            {
                _effect = new BasicEffect2();

                if (Race.Current.ConfigFile.DepthCueMode == DepthCueMode.Dark)
                {
                    GameVars.FogColor = new Color(0, 0, 0);
                }
                else if (Race.Current.ConfigFile.DepthCueMode == DepthCueMode.Fog)
                {
                    GameVars.FogColor = new Color(245, 245, 245);
                }
                else
                {
                    Trace.Assert(false);
                }
				_effect.FogEnabled = true;
                _effect.FogColor = GameVars.FogColor.ToVector3();
                _effect.FogEnd = Engine.DrawDistance * 6 * (1 / Race.Current.ConfigFile.FogAmount);
				_effect.FogStart = _effect.FogEnd - 200;
                _effect.TextureEnabled = true;
                _effect.TexCoordsMultiplier = 1;
                _effect.PreferPerPixelLighting = true;
				_effect.LightingEnabled = false;
            }

			Engine.Device.RenderState.AlphaTestEnable = true;
			Engine.Device.RenderState.ReferenceAlpha = 200;
			Engine.Device.RenderState.AlphaFunction = CompareFunction.Greater;

			if (GameVars.CullingOff)
				Engine.Device.RenderState.CullMode = CullMode.None;
			else
				Engine.Device.RenderState.CullMode = CullMode.CullClockwiseFace;
			
            _effect.View = Engine.Camera.View;
            _effect.Projection = Engine.Camera.Projection;

            _effect.Begin(SaveStateMode.None);

            return _effect;
        }

        private void TakeScreenshot()
        {
            int count = Directory.GetFiles(StorageContainer.TitleLocation+"\\", "ndump*.bmp").Length + 1;
            string name = "\\ndump" + count.ToString("000") + ".bmp";

            GraphicsDevice device = Engine.Device;
            using (ResolveTexture2D screenshot = new ResolveTexture2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight, 1, SurfaceFormat.Color))
            {
                device.ResolveBackBuffer(screenshot);
                screenshot.Save(StorageContainer.TitleLocation + name, ImageFileFormat.Bmp);
            }

            //MessageRenderer.Instance.PostHeaderMessage("Screenshot dumped to " + name, 3);
        }
    }
}
