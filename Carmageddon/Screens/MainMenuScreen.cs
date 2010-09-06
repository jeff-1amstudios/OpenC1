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
    
    class MainMenuScreen : BaseMenuScreen
    {

        public MainMenuScreen(BaseMenuScreen parent)
            : base(parent)
        {
            _inAnimation = new FliPlayer(new FliFile(GameVars.BasePath + "data\\anim\\MAI2COME.fli"));
            _inAnimation.Play(false, 1);

            _outAnimation = new FliPlayer(new FliFile(GameVars.BasePath + "data\\anim\\MAI2AWAY.fli"));

            _options.Add(
                new TextureMenuOption(BaseHUDItem.ScaleRect(0.181f, 0.256f, 0.68f, 0.045f),
                    new FliFile(GameVars.BasePath + "data\\anim\\MAI2N1GL.fli").Frames[0])
            );

            _options.Add(
                new TextureMenuOption(BaseHUDItem.ScaleRect(0.180f, 0.711f, 0.715f, 0.045f),
                    new FliFile(GameVars.BasePath + "data\\anim\\MAI2QTGL.fli").Frames[0])
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
