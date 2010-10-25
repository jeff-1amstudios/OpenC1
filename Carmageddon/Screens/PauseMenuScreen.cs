using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;
using OpenC1.HUD;
using OpenC1.Physics;
using OneAmEngine;

namespace OpenC1.Screens
{
    class PauseMenuScreen : BaseMenuScreen
    {
        
        public PauseMenuScreen(PlayGameScreen parent)
            : base(parent)
        {
            _inAnimation = new AnimationPlayer(LoadAnimation("MAINCOME.fli"));
            _inAnimation.Play(false);

            _outAnimation = new AnimationPlayer(LoadAnimation("MAINAWAY.fli"));

            _options.Add(
               new TextureMenuOption(BaseHUDItem.ScaleRect(0.202f, 0.185f, 0.65f, 0.045f),
                   LoadAnimation("MAINCNGL.fli")[0])
           );
            _options.Add(
                new TextureMenuOption(BaseHUDItem.ScaleRect(0.172f, 0.356f, 0.715f, 0.045f),
                    LoadAnimation("MAINARGL.fli")[0])
            );

            _options.Add(
                new TextureMenuOption(BaseHUDItem.ScaleRect(0.201f, 0.778f, 0.725f, 0.045f),
                    LoadAnimation("MAINQTGL.fli")[0])
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
                    Engine.Game.Exit();
                    //PhysX.Instance.Delete();
                    //Engine.Screen = Parent.Parent;
                    break;
                case 2:
                    Engine.Game.Exit();
                    break;
            }
        }
    }
}
