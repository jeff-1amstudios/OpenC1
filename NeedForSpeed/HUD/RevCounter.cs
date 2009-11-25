using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using NFSEngine;

namespace Carmageddon.HUD
{
    class RevCounter
    {
        int x, y;
        Texture2D _speedoTexture, _speedoLineTexture;

        public RevCounter()
        {
            
            x = 20;
            y = Engine.Instance.Window.Height - 150;

            _speedoTexture = Engine.Instance.ContentManager.Load<Texture2D>("Content\\tacho");
            _speedoLineTexture = TextureGenerator.Generate(new Color(255, 0, 0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpmFactor">0..1 percentage of rpm compared to redline</param>
        public void Render(float rpmFactor)
        {
            Engine.Instance.SpriteBatch.Begin();
            
            Color color = new Color(255,255,255,255);
            Engine.Instance.SpriteBatch.Draw(_speedoTexture, new Rectangle(x, y, 112, 88), color);

            float rotation = (float)(rpmFactor * 4f) + 0.5f;
            Engine.Instance.SpriteBatch.Draw(_speedoLineTexture, new Vector2(x+55, y+50), 
                null, color, rotation, new Vector2(0,0), new Vector2(3, 45), SpriteEffects.None, 0);


            Engine.Instance.SpriteBatch.End();

            Engine.Instance.Device.RenderState.DepthBufferEnable = true;
            Engine.Instance.Device.RenderState.AlphaBlendEnable = false;
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

        }
    }
}
