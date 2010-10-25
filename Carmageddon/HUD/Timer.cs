using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OneAmEngine;

namespace OpenC1.HUD
{
    class Timer : BaseHUDItem
    {
        int x, y;
        public Timer()
        {
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            Rectangle rect = CenterRectX(0, 0.182f, 0.087f);
            DrawShadow(rect);
            
            TimeSpan ts = TimeSpan.FromSeconds(Race.Current.RaceTime.TimeRemaining);
            float nudge = ts.Minutes < 10 ? 13 * FontScale : 0;
            FontRenderer.Render(Fonts.Timer,
                String.Format("{0}:{1}", (int)ts.Minutes, ts.Seconds.ToString("00")), new Vector2(rect.X + 5 + nudge, rect.Y + 7), Color.White, FontScale);
        }
    }
}
