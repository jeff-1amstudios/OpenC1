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
    abstract class BaseMenuScreen : IGameScreen
    {
        public IGameScreen Parent { get; private set; }
        protected FliPlayer _inAnimation, _outAnimation;
        protected Rectangle _rect;
        protected int _selectedOption;
        protected List<IMenuOption> _options = new List<IMenuOption>();
        protected bool _waitingForOutAnimation;

        public BaseMenuScreen(IGameScreen parent)
        {
            Parent = parent;
            _rect = new Rectangle(0, 0, Engine.Window.Width, Engine.Window.Height);
            Engine.Camera = new SimpleCamera();
        }

        public void ReturnToParent()
        {
            if (Parent is BaseMenuScreen)
            {
                ((BaseMenuScreen)Parent)._inAnimation.Play(false, 0);
            }
            Engine.Screen = Parent;
        }

        #region IGameScreen Members

        public virtual void Update()
        {
            if (_waitingForOutAnimation)
            {
                if (!_outAnimation.IsPlaying)
                {
                    _waitingForOutAnimation = false;
                    OnOutAnimationFinished();
                    return;
                }
            }
            else
            {
                if (Engine.Input.WasPressed(Keys.Escape) && Parent != null)
                {
                    SoundCache.Play(SoundIds.UI_Esc, null, false);
                    ReturnToParent();
                }
                if (Engine.Input.WasPressed(Keys.Down))
                {
                    if (_selectedOption < _options.Count - 1)
                        _selectedOption++;
                    else
                        _selectedOption = 0;
                    SoundCache.Play(SoundIds.UI_UpDown, null, false);
                }
                else if (Engine.Input.WasPressed(Keys.Up))
                {
                    if (_selectedOption > 0)
                        _selectedOption--;
                    else
                        _selectedOption = _options.Count - 1;
                    SoundCache.Play(SoundIds.UI_UpDown, null, false);
                }
                else if (Engine.Input.WasPressed(Keys.Enter))
                {
                    SoundCache.Play(SoundIds.UI_Ok, null, false);
                    PlayOutAnimation();
                }
            }
            _inAnimation.Update();
            _outAnimation.Update();
        }

        private void PlayOutAnimation()
        {
            _outAnimation.Play(false, 0);
            _waitingForOutAnimation = true;
        }

        public virtual void Render()
        {
            Engine.Device.Clear(Color.White);

            Engine.SpriteBatch.Begin();

            if (_waitingForOutAnimation)
                Engine.SpriteBatch.Draw(_outAnimation.GetCurrentFrame(), _rect, Color.White);
            else
                Engine.SpriteBatch.Draw(_inAnimation.GetCurrentFrame(), _rect, Color.White);

            if (ShouldRenderOptions())
            {
                _options[_selectedOption].RenderInSpriteBatch();
                Engine.SpriteBatch.End();
                _options[_selectedOption].RenderOutsideSpriteBatch();
            }
            else
            {
                Engine.SpriteBatch.End();
            }
        }

        public abstract void OnOutAnimationFinished();

        #endregion

        public bool ShouldRenderOptions()
        {
            return !_inAnimation.IsPlaying && !_outAnimation.IsPlaying && !_waitingForOutAnimation;
        }


    }
}
