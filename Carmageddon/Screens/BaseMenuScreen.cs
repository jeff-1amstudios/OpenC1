using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using NFSEngine;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.HUD;

namespace Carmageddon.Screens
{
    class BaseMenuScreen : IGameScreen
    {
        protected FliPlayer _inAnimation, _outAnimation;
        protected Rectangle _rect;
        protected int _currentOption;
        protected List<Option> _options = new List<Option>();
        protected bool _waitingForOutAnimation;

        public BaseMenuScreen()
        {
            _rect = new Rectangle(0, 0, Engine.Window.Width, Engine.Window.Height);
            Engine.Camera = new SimpleCamera();
        }

        #region IGameScreen Members

        public virtual void Update()
        {
            if (_waitingForOutAnimation)
            {
                if (!_outAnimation.IsPlaying)
                {
                    OnOutAnimationFinished();
                    return;
                }
            }

            if (Engine.Input.WasPressed(Keys.Down))
            {
                if (_currentOption < _options.Count - 1) _currentOption++;
                SoundCache.Play(SoundIds.UI_UpDown, null, false);
            }
            else if (Engine.Input.WasPressed(Keys.Up))
            {
                if (_currentOption > 0) _currentOption--;
                SoundCache.Play(SoundIds.UI_UpDown, null, false);
            }
            else if (Engine.Input.WasPressed(Keys.Enter))
            {
                SoundCache.Play(SoundIds.UI_Ok, null, false);
                _outAnimation.Play(false, 0);
                _waitingForOutAnimation = true;
            }
            _inAnimation.Update();
            _outAnimation.Update();
        }

        public void Render()
        {
            Engine.Device.Clear(Color.Black);

            Engine.SpriteBatch.Begin();

            if (_waitingForOutAnimation)
                Engine.SpriteBatch.Draw(_outAnimation.GetCurrentFrame(), _rect, Color.White);
            else
                Engine.SpriteBatch.Draw(_inAnimation.GetCurrentFrame(), _rect, Color.White);

            if (!_inAnimation.IsPlaying && !_outAnimation.IsPlaying && !_waitingForOutAnimation)
            {
                Engine.SpriteBatch.Draw(_options[_currentOption].Texture, _options[_currentOption].Rect, Color.White);
            }

            Engine.SpriteBatch.End();
        }

        #endregion

        public virtual void OnOutAnimationFinished()
        {
        }
    }
}
