using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Carmageddon.HUD;
using OneAmEngine;

namespace Carmageddon.Screens
{
    class SelectRaceScreen : BaseMenuScreen
    {

        public SelectRaceScreen(BaseMenuScreen parent)
            : base(parent)
        {
            _inAnimation = new FliPlayer(LoadAnimation("chrccome.fli"));
            _inAnimation.Play(false);

            _outAnimation = new FliPlayer(LoadAnimation("chrcaway.fli"));

            _options.Add(new RaceOption(RacesFile.Instance.Races[0]));
        }

        public override void Update()
        {
            Engine.Camera.Position = new Vector3(-1.5f, 3.5f, 10);
            Engine.Camera.Orientation = new Vector3(0, -0.28f, -1);
            base.Update();
            
            if (GameVars.Emulation == EmulationMode.Demo)  //only 1 track in demo mode
                return;
            
            if (_selectedOption == _options.Count - 1 && RacesFile.Instance.Races.Count > _selectedOption + 1)
            {
                _options.Add(new RaceOption(RacesFile.Instance.Races[_selectedOption + 1]));
            }
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
            _scene = BaseMenuScreen.LoadAnimation(_info.FliFileName).Frames[0];
        }

        #region IMenuOption Members

        public void RenderInSpriteBatch()
        {
            Engine.SpriteBatch.Draw(_scene, BaseHUDItem.ScaleVec2(0.23f, 0.19f), null, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 1);
            Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/LucidaConsole"), _info.Description, BaseHUDItem.ScaleVec2(0.54f, 0.19f), new Color(0, 220, 0), 0, Vector2.Zero, 1.2f, SpriteEffects.None, 1);
        }

        public void RenderOutsideSpriteBatch()
        {            
        }

        #endregion
    }

}
