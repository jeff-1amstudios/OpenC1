using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NFSEngine;
using Carmageddon.HUD;
using Microsoft.Xna.Framework.Input;

namespace Carmageddon.Screens
{
    class Option
    {
        public Rectangle Rect;
        public Texture2D Texture;
    }

    class MainMenuScreen : BaseMenuScreen
    {
        
        public MainMenuScreen() : base()
        {
            _inAnimation = new FliPlayer(new FliFile(GameVariables.BasePath + "data\\anim\\MAI2COME.fli"));
            _inAnimation.Play(false, 1);

            _outAnimation = new FliPlayer(new FliFile(GameVariables.BasePath + "data\\anim\\MAI2AWAY.fli"));

            _options.Add(new Option
            {
                Texture = new FliFile(GameVariables.BasePath + "data\\anim\\MAI2N1GL.fli").Frames[0],
                Rect = BaseHUDItem.ScaleRect(0.181f, 0.256f, 0.68f, 0.045f)
            });
            _options.Add(new Option
            {
                Texture = new FliFile(GameVariables.BasePath + "data\\anim\\MAI2QTGL.fli").Frames[0],
                Rect = BaseHUDItem.ScaleRect(0.180f, 0.711f, 0.715f, 0.045f)
            });
        }

        public override void OnOutAnimationFinished()
        {
            if (_currentOption == 0)
                Engine.Screen = new SelectSkillScreen();
            else if (_currentOption == 1)
                Engine.Game.Exit();
        }
    }
}
