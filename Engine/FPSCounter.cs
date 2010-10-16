using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace OneAmEngine
{
    public class FrameRateCounter : DrawableGameComponent
    {
        
        public int FrameRate {private set; get; }
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        
        public FrameRateCounter()
            : base(Engine.Game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                FrameRate = frameCounter;
                frameCounter = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            frameCounter++;
        }
    }
}
