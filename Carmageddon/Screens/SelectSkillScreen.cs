using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Carmageddon.Parsers;
using Carmageddon.HUD;

namespace Carmageddon.Screens
{
    class SelectSkillScreen : BaseMenuScreen
    {

        public SelectSkillScreen() : base()
        {
            _inAnimation = new FliPlayer(new FliFile(GameVariables.BasePath + "data\\anim\\skilcome.fli"));
            _inAnimation.Play(false, 0);

            _outAnimation = new FliPlayer(new FliFile(GameVariables.BasePath + "data\\anim\\skilaway.fli"));

            _options.Add(new Option
            {
                Texture = new FliFile(GameVariables.BasePath + "data\\anim\\SKILL1GL.fli").Frames[0],
                Rect = BaseHUDItem.ScaleRect(0.181f, 0.318f, 0.68f, 0.065f)
            });
            _options.Add(new Option
            {
                Texture = new FliFile(GameVariables.BasePath + "data\\anim\\SKILL2GL.fli").Frames[0],
                Rect = BaseHUDItem.ScaleRect(0.181f, 0.505f, 0.715f, 0.045f)
            });

            _options.Add(new Option
            {
                Texture = new FliFile(GameVariables.BasePath + "data\\anim\\SKILL3GL.fli").Frames[0],
                Rect = BaseHUDItem.ScaleRect(0.181f, 0.705f, 0.715f, 0.045f)
            });
        }

        public override void Update()
        {
            _options.Clear();

            _options.Add(new Option
            {
                Texture = new FliFile(GameVariables.BasePath + "data\\anim\\SKILL1GL.fli").Frames[0],
                Rect = BaseHUDItem.ScaleRect(0.119f, 0.278f, 0.776f, 0.078f)
            });
            _options.Add(new Option
            {
                Texture = new FliFile(GameVariables.BasePath + "data\\anim\\SKILL2GL.fli").Frames[0],
                Rect = BaseHUDItem.ScaleRect(0.113f, 0.417f, 0.776f, 0.078f)
            });
            _options.Add(new Option
            {
                Texture = new FliFile(GameVariables.BasePath + "data\\anim\\SKILL3GL.fli").Frames[0],
                Rect = BaseHUDItem.ScaleRect(0.119f, 0.557f, 0.776f, 0.078f)
            });
            base.Update();
        }
    }
}
