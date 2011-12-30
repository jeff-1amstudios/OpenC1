using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using OpenC1.HUD;
using OpenC1.Parsers;
using OneAmEngine;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace OpenC1.Screens
{
    abstract class BaseMenuScreen : IGameScreen
    {
        public IGameScreen Parent { get; private set; }
        protected AnimationPlayer _inAnimation, _outAnimation;
        protected Rectangle _rect;
        protected int _selectedOption;
        protected List<IMenuOption> _options = new List<IMenuOption>();
        protected bool _waitingForOutAnimation;
		public SpriteFont _font;
		private int _currentLine;
		private Viewport v;

        public BaseMenuScreen(IGameScreen parent)
        {
            Parent = parent;
			Viewport viewport = Engine.Device.Viewport;
			_rect = new Rectangle(0, 0, Engine.Device.Viewport.Width, Engine.Device.Viewport.Height);
            Engine.Camera = new SimpleCamera();
			_font = Engine.ContentManager.Load<SpriteFont>("content/M42");
        }

        public void ReturnToParent()
        {
			if (Parent is BaseMenuScreen)
			{
				if (((BaseMenuScreen)Parent)._inAnimation != null)
					((BaseMenuScreen)Parent)._inAnimation.Play(false);
			}
            Engine.Screen = Parent;
        }

		public void RenderDefaultBackground()
		{
			Engine.SpriteBatch.Draw(Engine.ContentManager.Load<Texture2D>("content/menu-background"), _rect, Color.White);
		}

		public void WriteTitleLine(string text)
		{
			Engine.SpriteBatch.DrawString(_font, text, new Vector2(20, 50), Color.Red, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
			_currentLine = 80;
		}

		public void WriteLine(string text)
		{
			Engine.SpriteBatch.DrawString(_font, text, new Vector2(30, _currentLine), Color.LightGray);
			_currentLine += 20;
		}

		public void WriteLine(string text, int line)
		{
			_currentLine = line;
			WriteLine(text);
		}

        #region IGameScreen Members

		public virtual void Update()
		{
			if (_waitingForOutAnimation)
			{
				if (_outAnimation == null || !_outAnimation.IsPlaying)
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
					if (SoundCache.IsInitialized) SoundCache.Play(SoundIds.UI_Esc, null, false);
					ReturnToParent();
				}
				if (Engine.Input.WasPressed(Keys.Down))
				{
					if (_selectedOption < _options.Count - 1)
						_selectedOption++;
					else
						_selectedOption = 0;
					if (SoundCache.IsInitialized) SoundCache.Play(SoundIds.UI_UpDown, null, false);
				}
				else if (Engine.Input.WasPressed(Keys.Up))
				{
					if (_selectedOption > 0)
						_selectedOption--;
					else
						_selectedOption = _options.Count - 1;
					if (SoundCache.IsInitialized) SoundCache.Play(SoundIds.UI_UpDown, null, false);
				}
				else if (Engine.Input.WasPressed(Keys.Enter))
				{
					if (_options.Count == 0 || _options[_selectedOption].CanBeSelected)
					{
						if (SoundCache.IsInitialized) SoundCache.Play(SoundIds.UI_Ok, null, false);
						PlayOutAnimation();
					}
				}
			}
			if (_inAnimation != null) _inAnimation.Update();
			if (_outAnimation != null) _outAnimation.Update();
		}

        private void PlayOutAnimation()
        {
            if (_outAnimation != null) _outAnimation.Play(false);
            _waitingForOutAnimation = true;
        }

        public virtual void Render()
        {
            Engine.Device.Clear(Color.Black);

            Engine.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            if (_outAnimation != null && _waitingForOutAnimation)
                Engine.SpriteBatch.Draw(_outAnimation.GetCurrentFrame(), _rect, Color.White);
            else if (_inAnimation != null)
                Engine.SpriteBatch.Draw(_inAnimation.GetCurrentFrame(), _rect, Color.White);

            Vector2 pos = BaseHUDItem.ScaleVec2(0.01f, 0.96f);
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Engine.SpriteBatch.DrawString(_font, "Open C1 v" + v.Major + "." + v.Minor + "." + v.Build + " - " + GameVars.BasePath , pos, Color.Red, 0, Vector2.Zero, 1.1f, SpriteEffects.None, 0);

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
            return _options.Count > 0 && !_inAnimation.IsPlaying && !_outAnimation.IsPlaying && !_waitingForOutAnimation;
        }

        public static List<Texture2D> LoadAnimation(string filename)
        {
            FliFile fli = new FliFile(filename);
            if (fli.Exists)
                return fli.Frames;
            filename = filename.Substring(0, filename.Length - 3) + "png";
            if (File.Exists(GameVars.BasePath + "anim\\" + filename))
            {
                return new List<Texture2D> { (Texture2D)Texture.FromFile(Engine.Device, GameVars.BasePath + "anim\\" + filename) };
            }

            filename = filename.Substring(0, filename.Length - 3) + "pix";
            PixFile pix = new PixFile(filename);
            if (pix.Exists)
                return new List<Texture2D> { pix.PixMaps[0].Texture };

            return null;
        }
    }
}
