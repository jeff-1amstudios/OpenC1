using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OneamEngine;
using OneAmEngine.Audio;

namespace OneAmEngine
{
    public static class Engine
    {
        public static Game Game;
        private static ContentManager _contentManager;
        private static ICamera _camera;
        public static IPlayer Player { get; set; }
        private static SpriteBatch _spriteBatch;
        private static FrameRateCounter _fpsCounter;

        public static DebugRenderer DebugRenderer;
        public static IGameScreen Screen { get; set; }
        public static GraphicsDevice Device { get; private set; }
        public static BasicEffect CurrentEffect;
        public static InputProvider Input { get; set; }
        public static float DrawDistance { get; set; }
        public static float ElapsedSeconds { get; private set; }
        public static float TotalSeconds { get; private set; }
        public static RandomGenerator Random { get; private set; }
        public static ISoundEngine Audio { get; set; }
        public static float TimeScale { get; set; }

        private static bool _isFullScreen;
        public static Vector2 ScreenSize;


        public static void Startup(Game game, GraphicsDeviceManager graphics)
        {
            Game = game;
            Device = graphics.GraphicsDevice;
            _isFullScreen = graphics.IsFullScreen;

            DrawDistance = 1000;

            _contentManager = new ContentManager(Game.Services);

            Input = new InputProvider(Game);
            DebugRenderer = new DebugRenderer();
            _spriteBatch = new SpriteBatch(Device);
            _fpsCounter = new FrameRateCounter();
            Random = new RandomGenerator();
            TimeScale = 1;
            Window = Game.Window.ClientBounds;
        }


        public static void Update(GameTime gameTime)
        {
            ElapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds * TimeScale;
            TotalSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            

            GameConsole.Clear();

            _fpsCounter.Update(gameTime);

            Input.Update(gameTime);

            if (Audio != null) Audio.Update();

            Screen.Update();

            ScreenEffects.Instance.Update(gameTime);

            DebugRenderer.Update(gameTime);
        }

        public static void Render(GameTime gameTime)
        {
            Screen.Render();
            DebugRenderer.Draw();
            ScreenEffects.Instance.Draw();
            GameConsole.Render();
            DebugRenderer.DrawText();
            _fpsCounter.Draw(gameTime);
        }

        public static ContentManager ContentManager
        {
            get { return _contentManager; }
        }


        public static ICamera Camera
        {
            get { return _camera; }
            set
            {
                _camera = value;
                _camera.DrawDistance = DrawDistance;
            }
        }

        public static Rectangle Window {get; private set; }
        
        public static float AspectRatio
        {
            get
            {
                if (_isFullScreen)
                    return ScreenSize.X / ScreenSize.Y;
                else
                    return (float)Window.Width / (float)Window.Height;
            }
        }


        public static SpriteBatch SpriteBatch
        {
            get { return _spriteBatch; }
        }

        public static int Fps
        {
            get { return _fpsCounter.FrameRate; }
        }
    }
}
