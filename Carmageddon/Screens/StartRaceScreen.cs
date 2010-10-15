using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.HUD;
using PlatformEngine;

namespace Carmageddon.Screens
{
    class StartRaceScreen : BaseMenuScreen
    {
        SelectRaceScreen _selectRaceScreen;
        SelectCarScreen _selectCarScreen;

        public StartRaceScreen(BaseMenuScreen parent)
            : base(parent)
        {
            _inAnimation = new FliPlayer(LoadAnimation("strtcome.fli"));
            _inAnimation.Play(false, 0);

            _outAnimation = new FliPlayer(LoadAnimation("strtaway.fli"));

            Texture2D buttonSelectionRect = LoadAnimation("SMLBUTGL.fli").Frames[0];

            _options.Add(new TextureMenuOption(
               BaseHUDItem.ScaleRect(0.7f, 0.139f, 0.21f, 0.11f),
               buttonSelectionRect)
               );

            //_options.Add(new TextureMenuOption(
            //    BaseHUDItem.ScaleRect(0.7f, 0.28f, 0.21f, 0.11f),
            //    buttonSelectionRect)
            //    );

            _options.Add(new TextureMenuOption(
                BaseHUDItem.ScaleRect(0.7f, 0.565f, 0.21f, 0.11f),
                buttonSelectionRect)
                );

            _options.Add(new TextureMenuOption(
                BaseHUDItem.ScaleRect(0.7f, 0.735f, 0.21f, 0.11f),
                buttonSelectionRect)
                );

            _selectedOption = 2;

            if (GameVars.SelectedRaceInfo == null)
            {
                GameVars.SelectedRaceInfo = RacesFile.Instance.Races[0];
                GameVars.SelectedRaceScene = LoadAnimation(GameVars.SelectedRaceInfo.FliFileName).Frames[0];
            }
            if (GameVars.SelectedCarFileName == null)
                GameVars.SelectedCarFileName = "blkeagle.txt";
        }

        public override void Render()
        {
            base.Render();

            if (base.ShouldRenderOptions())
            {
                Engine.SpriteBatch.Begin();
                Engine.SpriteBatch.Draw(GameVars.SelectedRaceScene, BaseHUDItem.ScaleRect(0.205f, 0.16f, 0.333f, 0.7f), Color.White);
                Engine.SpriteBatch.End();
            }
        }


        public override void OnOutAnimationFinished()
        {
            switch (_selectedOption)
            {
                case 0:
                    if (_selectRaceScreen == null) _selectRaceScreen = new SelectRaceScreen(this);
                    Engine.Screen = _selectRaceScreen;
                    break;
                case 1:
                    if (_selectCarScreen == null) _selectCarScreen = new SelectCarScreen(this);
                    Engine.Screen = _selectCarScreen;
                    break;
                case 2:
                    Engine.Screen = new LoadRaceScreen(this);
                    break;
            }
        }
    }
}
