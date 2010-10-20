using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OpenC1.HUD;
using Microsoft.Xna.Framework.Input;
using OneAmEngine;

namespace OpenC1.Screens
{
    
    class MainMenuScreen : BaseMenuScreen
    {

        public MainMenuScreen(BaseMenuScreen parent)
            : base(parent)
        {

            _inAnimation = new FliPlayer(LoadAnimation("MAI2COME.fli"), 2);
            _inAnimation.Play(false);
            ScreenEffects.Instance.FadeSpeed = 300;
            ScreenEffects.Instance.UnFadeScreen();

            _outAnimation = new FliPlayer(LoadAnimation("MAI2AWAY.fli"));

            _options.Add(
                new TextureMenuOption(BaseHUDItem.ScaleRect(0.181f, 0.256f, 0.68f, 0.045f),
                    LoadAnimation("MAI2N1GL.fli").Frames[0])
            );

            _options.Add(
                new TextureMenuOption(BaseHUDItem.ScaleRect(0.180f, 0.711f, 0.715f, 0.045f),
                    LoadAnimation("MAI2QTGL.fli").Frames[0])
            );
        }

        public override void OnOutAnimationFinished()
        {
            if (_selectedOption == 0)
                Engine.Screen = new SelectSkillScreen(this);
            else if (_selectedOption == 1)
                Engine.Game.Exit();
        }
    }
}
