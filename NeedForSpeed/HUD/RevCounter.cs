using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using NFSEngine;
using Carmageddon.Physics;

namespace Carmageddon.HUD
{
    class RevCounter
    {
        VehicleChassis _chassis;
        int x, y;
        Texture2D _speedoTexture, _speedoLineTexture, _shadow;
        SpriteFont _font;

        public RevCounter(VehicleChassis vehicle)
        {
            _chassis = vehicle;
            _font = Engine.Instance.ContentManager.Load<SpriteFont>("content/speedo-font");
            
            x = 15;
            y = Engine.Instance.Window.Height - 105;

            _speedoTexture = Engine.Instance.ContentManager.Load<Texture2D>("Content\\tacho");
            _speedoLineTexture = TextureGenerator.Generate(new Color(255, 0, 0));
            _shadow = TextureGenerator.Generate(new Color(0f, 0f, 0f, 0.4f));
        }

        public void Render()
        {
            Engine.Instance.SpriteBatch.Begin();
            
            Color color = new Color(255,255,255,255);
            Engine.Instance.SpriteBatch.Draw(_speedoTexture, new Rectangle(x, y, 112, 88), color);

            Engine.Instance.SpriteBatch.Draw(_shadow, new Rectangle(x + 40, y + 63, 24, 32), Color.White);
            Engine.Instance.SpriteBatch.DrawString(_font, _chassis.Motor.Gearbox.CurrentGear.ToString(), new Vector2(x + 43, y + 65), Color.Yellow);
            Engine.Instance.SpriteBatch.Draw(_shadow, new Rectangle(x + 70, y + 63, 55, 32), Color.White);
            Engine.Instance.SpriteBatch.DrawString(_font, ((int)_chassis.Speed).ToString("000"), new Vector2(x+73, y+65), Color.GreenYellow);

            float rpmFactor = _chassis.Motor.Rpm / _chassis.Motor.RedlineRpm;
            float rotation = (float)(rpmFactor * 4f) + 0.5f;
            Engine.Instance.SpriteBatch.Draw(_speedoLineTexture, new Vector2(x + 55, y + 50),
                null, color, rotation, new Vector2(0, 0), new Vector2(3, 45), SpriteEffects.None, 0);

            Engine.Instance.SpriteBatch.End();

            Engine.Instance.Device.RenderState.DepthBufferEnable = true;
            Engine.Instance.Device.RenderState.AlphaBlendEnable = false;
            Engine.Instance.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Engine.Instance.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

        }
    }
}
