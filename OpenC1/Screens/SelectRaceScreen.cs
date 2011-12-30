using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OpenC1.HUD;
using OneAmEngine;

namespace OpenC1.Screens
{
    class SelectRaceScreen : BaseMenuScreen
    {

		public SelectRaceScreen(BaseMenuScreen parent)
			: base(parent)
		{
			_inAnimation = new AnimationPlayer(LoadAnimation("chrccome.fli"));
			_inAnimation.Play(false);

			_outAnimation = new AnimationPlayer(LoadAnimation("chrcaway.fli"));

			if (GameVars.Emulation == EmulationMode.Demo || GameVars.Emulation == EmulationMode.SplatPackDemo)  //only 1 track in demo mode
				_options.Add(new RaceOption(RacesFile.Instance.Races[0]));
			else
				foreach (var race in RacesFile.Instance.Races)
					_options.Add(new RaceOption(race));
		}

        public override void Update()
        {
            Engine.Camera.Position = new Vector3(-1.5f, 3.5f, 10);
            Engine.Camera.Orientation = new Vector3(0, -0.28f, -1);
            base.Update();
        }

        public override void OnOutAnimationFinished()
        {
            GameVars.SelectedRaceInfo = RacesFile.Instance.Races[_selectedOption];
            GameVars.SelectedRaceScene = ((RaceOption)_options[_selectedOption])._scene;
            ReturnToParent();
        }
    }

    class RaceOption : IMenuOption
    {
        RaceInfo _info;
        public Texture2D _scene;

        public RaceOption(RaceInfo info)
        {
            _info = info;
        }

        #region IMenuOption Members

		public bool CanBeSelected
		{
			get { return true; }
		}

        public void RenderInSpriteBatch()
        {
			if (_scene == null)
			{
				_scene = BaseMenuScreen.LoadAnimation(_info.FliFileName)[0];
			}
            Engine.SpriteBatch.Draw(_scene, BaseHUDItem.ScaleVec2(0.23f, 0.19f), null, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 1);
            Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/LucidaConsole"), _info.Description, BaseHUDItem.ScaleVec2(0.54f, 0.19f), new Color(0, 220, 0), 0, Vector2.Zero, 1.2f, SpriteEffects.None, 1);
        }

        public void RenderOutsideSpriteBatch()
        {            
        }

        #endregion
    }

}
