using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using Carmageddon.HUD;
using PlatformEngine;
using Carmageddon.Physics;

namespace Carmageddon.Screens
{
    class PauseMenuScreen : BaseMenuScreen
    {
        
        public PauseMenuScreen(PlayGameScreen parent)
            : base(parent)
        {
            _inAnimation = new FliPlayer(LoadAnimation("MAINCOME.fli"));
            _inAnimation.Play(false, 0);

            _outAnimation = new FliPlayer(LoadAnimation("MAINAWAY.fli"));

            _options.Add(
               new TextureMenuOption(BaseHUDItem.ScaleRect(0.202f, 0.185f, 0.65f, 0.045f),
                   LoadAnimation("MAINCNGL.fli").Frames[0])
           );
            _options.Add(
                new TextureMenuOption(BaseHUDItem.ScaleRect(0.172f, 0.356f, 0.715f, 0.045f),
                    LoadAnimation("MAINARGL.fli").Frames[0])
            );

            _options.Add(
                new TextureMenuOption(BaseHUDItem.ScaleRect(0.201f, 0.778f, 0.725f, 0.045f),
                    LoadAnimation("MAINQTGL.fli").Frames[0])
            );
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnOutAnimationFinished()
        {
            switch (_selectedOption)
            {
                case 0:
                    ReturnToParent();
                    break;
                case 1:
                    PhysX.Instance.Delete();
                    Engine.Screen = Parent.Parent;
                    break;
                case 2:
                    Engine.Game.Exit();
                    break;
            }
        }
    }
}
