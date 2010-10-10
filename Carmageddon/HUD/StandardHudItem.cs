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
            Vector2 pos = ScaleVec2(0.22f, 0.01f);
            DrawShadow(new Rectangle((int)pos.X-5, (int)pos.Y-5, 155, 24));

            Engine.SpriteBatch.DrawString(_whiteFont, "CP", pos, Color.White);
            pos.X += 25f;
            Engine.SpriteBatch.DrawString(_whiteFont, String.Format("{0}/{1}", Race.Current.NextCheckpoint, Race.Current.ConfigFile.Checkpoints.Count), pos, Color.White);
            pos.X += 45f;
            Engine.SpriteBatch.DrawString(_whiteFont, "LAP", pos, Color.White);
            pos.X += 35f;
            Engine.SpriteBatch.DrawString(_whiteFont, String.Format("{0}/{1}", Race.Current.CurrentLap, Race.Current.ConfigFile.LapCount), pos, Color.White);

            pos = ScaleVec2(0.22f, 0.054f);

            DrawShadow(new Rectangle((int)pos.X - 5, (int)pos.Y - 5, 155, 24));             
            Engine.SpriteBatch.DrawString(_whiteFont, "WASTED", pos, Color.White);
            
            pos.X += 65f;
            Engine.SpriteBatch.DrawString(_whiteFont, Race.Current.NbrDeadOpponents + "/" + Race.Current.Opponents.Count, pos, Color.White);

            pos.X += 240;
            DrawShadow(new Rectangle((int)pos.X - 5, (int)pos.Y - 5, 140, 24)); 
            Engine.SpriteBatch.DrawString(_whiteFont, Race.Current.NbrDeadPeds + "/" + Race.Current.ConfigFile.Peds.Count, pos, Color.White);
            
            
            pos.X += 80;
            Engine.SpriteBatch.DrawString(_whiteFont, "KILLS", pos, Color.White);
        }
    }
}
