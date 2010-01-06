using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NFSEngine;
using NFSEngine.Audio;

namespace PlatformEngine
{
    public class Engine : DrawableGameComponent
    {
        private ContentManager _contentManager;
        private ICamera _camera;
        public IPlayer Player { get; set; }
        private SpriteBatch _spriteBatch;
        private FrameRateCounter _fpsCounter;

        public DebugRenderer DebugRenderer;
        public IGameScreen Screen { get; set; }
        public GraphicsDevice Device { get; private set; }
        public BasicEffect CurrentEffect;
        public IWorld World { get; set; }
        public InputProvider Input { get; set; }
        public float DrawDistance { get; set; }
        public float ElapsedSeconds { get; private set; }
        public Random RandomNumber { get; private set; }
        public ISoundEngine Audio { get; set; }
                
        public static Engine Instance;

        public static void Initialize(Game game, GraphicsDeviceManager graphics)
        {
            Debug.Assert(Instance == null);
            Instance = new Engine(game);
            
            Instance.EngineStartup(graphics);
        }

        
        private Engine(Game game)
            : base(game)
        {
             
        }

        private void EngineStartup(GraphicsDeviceManager graphics)
        {
            Device = graphics.GraphicsDevice;

            DrawDistance = 1000;

            _contentManager = new ContentManager(base.Game.Services);

            //Game bits
            Input = new InputProvider(base.Game);
            DebugRenderer = new DebugRenderer();
            _spriteBatch = new SpriteBatch(Device);
            _fpsCounter = new FrameRateCounter();
            RandomNumber = new Random();

            base.Game.Components.Add(this);
        }
        

        public override void Update(GameTime gameTime)
        {
            GameConsole.Clear();

            ElapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _fpsCounter.Update(gameTime);

            base.Update(gameTime);
            
            Input.Update(gameTime);

            if (Audio != null) Audio.Update();
            
            Screen.Update(gameTime);

            ScreenEffects.Instance.Update(gameTime);

            DebugRenderer.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {            
            Screen.Draw();
            DebugRenderer.Draw();
            ScreenEffects.Instance.Draw();
            GameConsole.Render();
            DebugRenderer.DrawText();
            _fpsCounter.Draw(gameTime);
        }

        public ContentManager ContentManager
        {
            get { return _contentManager; }
        }

        
        public ICamera Camera
        {
            get { return _camera; }
            set
            {
                _camera = value;
                _camera.DrawDistance = DrawDistance;
            }
        }

        public Rectangle Window
        {
            get { return Game.Window.ClientBounds; }
        }

        public float AspectRatio
        {
            get
            {
                Rectangle r = Window;
                return (float)r.Width / (float)r.Height;
            }
        }
        

        public SpriteBatch SpriteBatch
        {
            get { return _spriteBatch; }
        }

        public int Fps
        {
            get { return _fpsCounter.FrameRate; }
        }

        //public bool EnableBloom
        //{
        //    set
        //    {
        //        if (value)
        //        {
        //            _game.Components.Add(new BloomComponent(_game));
        //        }
        //        else
        //        {
        //            foreach (IGameComponent component in _game.Components)
        //            {
        //                if (component is BloomComponent)
        //                {
        //                    _game.Components.Remove(component);
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
