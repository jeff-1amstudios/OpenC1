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

            Engine.Instance.SpriteBatch.DrawString(_whiteFont, "CP", pos, Color.White);
            pos.X += 25f;
            Engine.Instance.SpriteBatch.DrawString(_whiteFont, String.Format("{0}/{1}", Race.Current.NextCheckpoint, Race.Current.Config.Checkpoints.Count), pos, Color.White);
            pos.X += 45f;
            Engine.Instance.SpriteBatch.DrawString(_whiteFont, "LAP", pos, Color.White);
            pos.X += 35f;
            Engine.Instance.SpriteBatch.DrawString(_whiteFont, String.Format("{0}/{1}", Race.Current.CurrentLap, Race.Current.Config.LapCount), pos, Color.White);

            pos = ScaleVec2(0.25f, 0.05f);

            DrawShadow(new Rectangle((int)pos.X - 5, (int)pos.Y - 5, 115, 22)); 
            
            Engine.Instance.SpriteBatch.DrawString(_whiteFont, "WASTED", pos, Color.White);
            pos.X += 65f;
            Engine.Instance.SpriteBatch.DrawString(_whiteFont, "0/0", pos, Color.White);
           
        }
    }
}
