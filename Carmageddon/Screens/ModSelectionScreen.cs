using System;
using System.Collections.Generic;
using System.Text;
using OneAmEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OpenC1.Parsers;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace OpenC1.Screens
{
    class ModSelectionScreen : BaseMenuScreen
    {
        float _showTime;
		int _selectedIndex = 0;
		List<string> _mods = new List<string>();


        public ModSelectionScreen(BaseMenuScreen parent)
            : base(parent)
        {
            //_inAnimation = new AnimationPlayer(LoadAnimation("MAI2AWAY.fli"));
            //_inAnimation.Play(false);
            //_outAnimation = new AnimationPlayer(LoadAnimation("MAI2AWAY.fli"));

            ScreenEffects.Instance.FadeSpeed = 300;
            ScreenEffects.Instance.UnFadeScreen();

            _showTime = Engine.TotalSeconds;

			string[] mods = Directory.GetDirectories(GameVars.BasePath);
			_mods.AddRange(mods);
        }

        public override void Render()
        {
            base.Render();

            Engine.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

			Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/M42"), "OpenC1 - Available games:", new Vector2(20, 50), Color.White);

			float y = 90;
			for (int i = 0; i < _mods.Count; i++)
			{
				string name = new DirectoryInfo(_mods[i]).Name;
				Color c = Color.White;
				if (i == _selectedIndex)
					Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/M42"), "< " + name + " >", new Vector2(40, y), Color.YellowGreen);
				else
					Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/M42"), "  " + name, new Vector2(40, y), Color.White);
				y += 35;
			}
            
            
            string raceText, vehicleText;

            if (Helpers.HasTimePassed(1.5f, _showTime))
            {
                //raceText = RacesFile.Instance.Races.Count.ToString();
                //vehicleText = OpponentsFile.Instance.Opponents.Count.ToString();

                //Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/M42"), "< continue >", new Vector2(40, 250), Color.White);
            }
            else
            {
                // do some lame spinny animation for a second
                //raceText = "" + (char)Engine.Random.Next(33, 122);
                //vehicleText = "" + (char)Engine.Random.Next(33, 122);
            }

            //Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/M42"), raceText, new Vector2(40, 160), Color.White);
            //Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/M42"), vehicleText, new Vector2(40, 185), Color.White);

            //Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/M42"), "races found in races.txt", new Vector2(85, 160), Color.White);
            //Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/M42"), "vehicles found in opponents.txt", new Vector2(85, 185), Color.White);
            
            Engine.SpriteBatch.End();

        }

		public override void Update()
		{
			base.Update();
			if (Engine.Input.WasPressed(Keys.Up))
				_selectedIndex = Math.Max(0, _selectedIndex-1);
			else if (Engine.Input.WasPressed(Keys.Down))
				_selectedIndex = Math.Min(_mods.Count-1, _selectedIndex+1);
		}

		public override void OnOutAnimationFinished()
		{
			GameVars.BasePath = _mods[_selectedIndex] + "\\";
			GameVars.DetectEmulationMode();
			Engine.Screen = new MainMenuScreen(this);
		}
	}
}
