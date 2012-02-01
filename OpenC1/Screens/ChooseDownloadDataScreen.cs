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
	class ChooseDownloadGameDataScreen : BaseMenuScreen
	{

		int _selectedIndex = 0;

		public ChooseDownloadGameDataScreen(BaseMenuScreen parent)
			: base(parent)
		{
			ScreenEffects.Instance.FadeSpeed = 300;
			ScreenEffects.Instance.UnFadeScreen();
		}

		public override void Render()
		{
			base.Render();

			Engine.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

			RenderDefaultBackground();

			WriteTitleLine("No game data found");

			WriteLine("Download and use Carmageddon demo content", 120);
			WriteLine("(C1 demo, SplatPack demo & SplatPack Xmas demo)?");
			
			//Engine.SpriteBatch.DrawString(_font, "Do you want to download and use", new Vector2(20, 110), Color.White);
			//Engine.SpriteBatch.DrawString(_font, "Carmageddon demo content?", new Vector2(20, 130), Color.White);
			
			string[] options = { "OK", "Cancel" };
			float y = 250;
			for (int i = 0; i < options.Length; i++)
			{
				Color c = Color.White;
				if (_selectedIndex == i)
					Engine.SpriteBatch.DrawString(_font, "< " + options[i] + " >", new Vector2(40, y), Color.YellowGreen);
				else
					Engine.SpriteBatch.DrawString(_font, "  " + options[i], new Vector2(40, y), Color.White);
				y += 35;
			}
			
			WriteLine("Carmageddon demo content is property of Stainless", 470);
			WriteLine("Software Ltd.  See GameData\\readme.txt.");

			Engine.SpriteBatch.End();
		}

		public override void Update()
		{
			base.Update();
			if (Engine.Input.WasPressed(Keys.Up))
			{
				_selectedIndex = 0;
			}
			else if (Engine.Input.WasPressed(Keys.Down))
			{
				_selectedIndex = 1;
			}
		}

		public override void OnOutAnimationFinished()
		{
			if (_selectedIndex == 0)
			{
				Engine.Screen = new DownloadGameDataScreen(null);
			}
			else
			{
				Engine.Game.Exit();
			}
		}
	}
}
