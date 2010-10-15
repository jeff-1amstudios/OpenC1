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

        public SelectSkillScreen(BaseMenuScreen parent)
            : base(parent)
        {
            _selectedOption = 1;

            _inAnimation = new FliPlayer(LoadAnimation("skilcome.fli"));
            _inAnimation.Play(false, 0);

            _outAnimation = new FliPlayer(LoadAnimation("skilaway.fli"));

            _options.Add(new TextureMenuOption(
                BaseHUDItem.ScaleRect(0.119f, 0.278f, 0.776f, 0.078f),
                LoadAnimation("SKILL1GL.fli").Frames[0])
                );

            _options.Add(new TextureMenuOption(
                BaseHUDItem.ScaleRect(0.113f, 0.417f, 0.776f, 0.078f),
                LoadAnimation("SKILL2GL.fli").Frames[0])
                );

            _options.Add(new TextureMenuOption(
                 BaseHUDItem.ScaleRect(0.119f, 0.557f, 0.776f, 0.078f),
                 LoadAnimation("SKILL3GL.fli").Frames[0])
                 );
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnOutAnimationFinished()
        {
            Engine.Screen = new StartRaceScreen(this);
        }
    }
}
