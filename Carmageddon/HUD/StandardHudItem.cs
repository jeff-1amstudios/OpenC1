using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Carmageddon.HUD
{
    class StandardHudItem : BaseHUDItem
    {
        public override void Update()
        {
            
        }

        public override void Render()
        {
            Vector2 pos = ScaleVec2(0.2f, 0.01f);
            DrawShadow(new Rectangle((int)pos.X-5, (int)pos.Y-5, 155, 22));

            Engine.SpriteBatch.DrawString(_whiteFont, "CP", pos, Color.White);
            pos.X += 25f;
            Engine.SpriteBatch.DrawString(_whiteFont, String.Format("{0}/{1}", Race.Current.NextCheckpoint, Race.Current.ConfigFile.Checkpoints.Count), pos, Color.White);
            pos.X += 45f;
            Engine.SpriteBatch.DrawString(_whiteFont, "LAP", pos, Color.White);
            pos.X += 35f;
            Engine.SpriteBatch.DrawString(_whiteFont, String.Format("{0}/{1}", Race.Current.CurrentLap, Race.Current.ConfigFile.LapCount), pos, Color.White);

            pos = ScaleVec2(0.25f, 0.05f);

            DrawShadow(new Rectangle((int)pos.X - 5, (int)pos.Y - 5, 115, 22)); 
            
            Engine.SpriteBatch.DrawString(_whiteFont, "WASTED", pos, Color.White);
            pos.X += 65f;
            Engine.SpriteBatch.DrawString(_whiteFont, Race.Current.NbrDeadOpponents + "/" + Race.Current.Opponents.Count, pos, Color.White);

            pos.Y += 65f;

            Rectangle rect = CenterRectX(0.09f, 0.09f, 0.075f);
            Engine.SpriteBatch.DrawString(_blueFont, "+3:48", new Vector2(rect.X, rect.Y), Color.White);
        }
    }
}
