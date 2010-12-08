using System;
using System.Collections.Generic;
using System.Text;
using OneAmEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OpenC1.Parsers;

namespace OpenC1.Screens
{
    class DataInfoScreen : BaseMenuScreen
    {
        float _showTime;

        public DataInfoScreen(BaseMenuScreen parent)
            : base(parent)
        {
            //_inAnimation = new AnimationPlayer(LoadAnimation("MAI2AWAY.fli"));
            //_inAnimation.Play(false);
            //_outAnimation = new AnimationPlayer(LoadAnimation("MAI2AWAY.fli"));

            ScreenEffects.Instance.FadeSpeed = 300;
            ScreenEffects.Instance.UnFadeScreen();

            _showTime = Engine.TotalSeconds;
        }

        public override void Render()
        {
            base.Render();

            Engine.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/SG14"), "Load data from: " + GameVars.BasePath, new Vector2(20, 50), Color.White);
            Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/SG14"), "(This can be changed in the OpenC1Settings.txt file)", new Vector2(30, 85), Color.Gray, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);

            string raceText, vehicleText;

            if (Helpers.HasTimePassed(1.5f, _showTime))
            {
                raceText = RacesFile.Instance.Races.Count.ToString();
                vehicleText = OpponentsFile.Instance.Opponents.Count.ToString();

                Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/SG14"), "< continue >", new Vector2(40, 250), Color.White);
            }
            else
            {
                // do some lame spinny animation for a second
                raceText = "" + (char)Engine.Random.Next(33, 122);
                vehicleText = "" + (char)Engine.Random.Next(33, 122);
            }

            Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/SG14"), raceText, new Vector2(40, 160), Color.White);
            Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/SG14"), vehicleText, new Vector2(40, 185), Color.White);
            
            Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/SG14"), "races found in races.txt", new Vector2(85, 160), Color.White);
            Engine.SpriteBatch.DrawString(Engine.ContentManager.Load<SpriteFont>("content/SG14"), "vehicles found in opponents.txt", new Vector2(85, 185), Color.White);
            
            Engine.SpriteBatch.End();

        }

        public override void OnOutAnimationFinished()
        {
            Engine.Screen = new MainMenuScreen(null);
        }
    }
}
