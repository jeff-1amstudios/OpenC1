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
using OpenC1.Screens;
using OpenC1.Parsers;
using OneAmEngine;
using OneAmEngine.Audio;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Text;

namespace OpenC1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager _graphics;

        public Game1()
        {
            //FixFile();

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.PreferMultiSampling = true;

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            SettingsFile settings = new SettingsFile();  //load openc1settings.txt

            Engine.ScreenSize = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

            _graphics.IsFullScreen = GameVars.FullScreen;
            //_graphics.SynchronizeWithVerticalRetrace = false;

            _graphics.MinimumVertexShaderProfile = ShaderProfile.VS_1_1;
            _graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
        }

        //void FixFile()
        //{
        //    Stream file = File.Open("ped-edit.txt", FileMode.Open);
        //    StreamReader sr = new StreamReader(file);
        //    StringBuilder sb = new StringBuilder();

        //    while (!sr.EndOfStream)
        //    {
        //        sb.AppendLine(sr.ReadLine());
        //        sb.AppendLine(sr.ReadLine());
        //        sb.AppendLine(sr.ReadLine());

        //        while (true)
        //        {
        //            string line = sr.ReadLine();
        //            sb.AppendLine(line);
        //            if (line == "reverse") break;
                    
        //            string v3line = sr.ReadLine();
        //            string[] bits = v3line.Split(new char[] { ',' } ,  StringSplitOptions.RemoveEmptyEntries);
        //            Vector3 v3 = new Vector3(float.Parse(bits[0]), float.Parse(bits[1]), float.Parse(bits[2]));
        //            v3 /= 6;
        //            sb.AppendLine(v3.ToShortString());
        //        }
        //        sb.AppendLine();
        //    }
        //    File.WriteAllText("ped-edit2.txt", sb.ToString());
        //}

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log("CRASH:\r\n" + e.ExceptionObject.ToString());
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

            Engine.DrawDistance = GameVars.DrawDistance;
            Engine.Audio = new MdxSoundEngine();

			string gameDataPath = "GameData";
			if (!Directory.Exists(gameDataPath) || Directory.GetDirectories(gameDataPath).Length == 0)
				Engine.Screen = new ChooseDownloadGameDataScreen(null);
			else
				Engine.Screen = new GameSelectionScreen(null);
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
