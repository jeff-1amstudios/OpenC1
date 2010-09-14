    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.GamerServices;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Net;
    using Microsoft.Xna.Framework.Storage;
    using PlatformEngine;
    using NFSEngine;
using NFSEngine.Audio;
using Carmageddon.Screens;
using Carmageddon.Parsers;

    namespace Carmageddon
    {
        /// <summary>
        /// This is the main type for your game
        /// </summary>
        public class Game1 : Microsoft.Xna.Framework.Game
        {
            GraphicsDeviceManager _graphics;

            public Game1()
            {
                _graphics = new GraphicsDeviceManager(this);
                Content.RootDirectory = "Content";
                
                _graphics.PreferredBackBufferWidth = 800;
                _graphics.PreferredBackBufferHeight = 600;
                _graphics.PreferMultiSampling = true;

                Engine.ScreenSize = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

                _graphics.IsFullScreen = false;
                //_graphics.SynchronizeWithVerticalRetrace = false;

                _graphics.MinimumVertexShaderProfile = ShaderProfile.VS_2_0;
                _graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;

            }

            /// <summary>
            /// Allows the game to perform any initialization it needs to before starting to run.
            /// This is where it can query for any required services and load any non-graphic
            /// related content.  Calling base.Initialize will enumerate through any components
            /// and initialize them as well.
            /// </summary>
            protected override void Initialize()
            {
                base.Initialize();

                Engine.Startup(this, _graphics);

                SettingsFile settings = new SettingsFile();
                GameVars.DrawDistance = settings.DrawDistance * 10;

                Engine.DrawDistance = GameVars.DrawDistance;
                Engine.Audio = new NFSEngine.Audio.MdxSoundEngine();


                //enable per-pixel transparency
                Engine.Device.RenderState.AlphaTestEnable = true;
                Engine.Device.RenderState.ReferenceAlpha = 200;
                Engine.Device.RenderState.AlphaFunction = CompareFunction.Greater;

                if (!SoundCache.IsInitialized)
                {
                    Engine.Audio.SetDefaultVolume(-500);
                    SoundCache.Initialize();
                }

                GameVars.Palette = new PaletteFile(GameVars.BasePath + "data\\reg\\palettes\\drrender.pal");

                Engine.Screen = new MainMenuScreen(null); // new PlayGameScreen();
            }

            /// <summary>
            /// LoadContent will be called once per game and is the place to load
            /// all of your content.
            /// </summary>
            protected override void LoadContent()
            {
            }

            /// <summary>
            /// UnloadContent will be called once per game and is the place to unload
            /// all content.
            /// </summary>
            protected override void UnloadContent()
            {
                Engine.ContentManager.Unload();
            }

            /// <summary>
            /// Allows the game to run logic such as updating the world,
            /// checking for collisions, gathering input, and playing audio.
            /// </summary>
            /// <param name="gameTime">Provides a snapshot of timing values.</param>
            protected override void Update(GameTime gameTime)
            {
                //check for exit
                //if (Engine.Input.WasPressed(Keys.Escape))
                //{
                //    Exit();
                //}

                Engine.Update(gameTime);

                base.Update(gameTime);
            }

            /// <summary>
            /// This is called when the game should draw itself.
            /// </summary>
            /// <param name="gameTime">Provides a snapshot of timing values.</param>
            protected override void Draw(GameTime gameTime)
            {
                Engine.Render(gameTime);
                base.Draw(gameTime);
            }
        }
    }
