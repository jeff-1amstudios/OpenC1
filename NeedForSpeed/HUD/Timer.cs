using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;
using Microsoft.Xna.Framework;

namespace Carmageddon.HUD
{
    class Timer : BaseHUDItem
    {
        SpriteFont _font;
        int x, y;
        public Timer()
        {
            _font = Engine.Instance.ContentManager.Load<SpriteFont>("content/timer-font");

            x = Engine.Instance.Window.Width / 2 - 72;
            y = 5;
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            
            Engine.Instance.SpriteBatch.Draw(_shadow, new Rectangle(x -3, y -5, 144, 45), Color.White);
            
            TimeSpan ts = TimeSpan.FromSeconds(Race.Current.RaceTime.TimeRemaining);
            float nudge = ts.Minutes < 10 ? 13 : 0;
            Engine.Instance.SpriteBatch.DrawString(_font,
                String.Format("{0}:{1}", (int)ts.Minutes, ts.Seconds.ToString("00")), new Vector2(x+nudge, y), Color.White);
        }
    }
}
