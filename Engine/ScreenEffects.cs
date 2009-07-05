using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace PlatformEngine
{
    public class ScreenEffects : IDrawableObject
    {
        public event EventHandler FadeCompleted;

        private enum FadeDirection
        {
            None,
            FadeIn,
            FadeOut
        }

        private static ScreenEffects _instance;
        public static ScreenEffects Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ScreenEffects();
                return _instance;
            }
        }

        private Color _fadeColor;
        private float _alpha;
        private FadeDirection _fadeDirection;

        private ScreenEffects()
        {
            _fadeDirection = FadeDirection.None;
        }

        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeScreen()
        {
            _alpha = 0;
            _fadeDirection = FadeDirection.FadeOut;
        }

        public void UnFadeScreen()
        {
            _alpha = 255;
            _fadeDirection = FadeDirection.FadeIn;
        }

        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            if (_fadeDirection == FadeDirection.FadeOut)
            {
                _alpha += 350.0f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_alpha >= 255)
                    CompleteFade();
            }
            else if (_fadeDirection == FadeDirection.FadeIn)
            {
                _alpha -= 250.0f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_alpha <= 0)
                    CompleteFade();
            }
            
        }

        public void Draw()
        {
            if (_fadeDirection != FadeDirection.None)
            {
                Viewport viewport = Engine.Instance.Device.Viewport;

                SpriteBatch spriteBatch = new SpriteBatch(Engine.Instance.Device);
                spriteBatch.Begin();

                
                spriteBatch.Draw(Engine.Instance.ContentManager.Load<Texture2D>("Content\\Textures\\blank"),
                                 new Rectangle(0, 0, viewport.Width, viewport.Height),
                                 new Color(0, 0, 0, (byte)_alpha));

                spriteBatch.End();
            }
        }

        private void CompleteFade()
        {
            _fadeDirection = FadeDirection.None;
            if (FadeCompleted != null)
            {
                FadeCompleted(this, null);
            }
        }

        #endregion
    }
}
