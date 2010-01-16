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
    class RevCounter: BaseHUDItem
    {
        VehicleChassis _chassis;
        float x, y;
        Texture2D _speedoTexture, _speedoLineTexture;
        SpriteFont _font;

        public RevCounter(VehicleChassis vehicle)
        {
            _chassis = vehicle;
            _font = Engine.Instance.ContentManager.Load<SpriteFont>("content/speedo-font");

            x = 0.01f;
            y = 0.8f;

            _speedoTexture = Engine.Instance.ContentManager.Load<Texture2D>("content/tacho");
            _speedoLineTexture = TextureGenerator.Generate(new Color(255, 0, 0));            
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            Engine.Instance.SpriteBatch.Draw(_speedoTexture, ScaleRect(x, y, 0.145f, 0.16f), Color.White);

            Engine.Instance.SpriteBatch.Draw(_shadow, ScaleRect(x + 0.06f, y + 0.112f, 0.03f, 0.057f), Color.White);
            if (_chassis.Motor.Gearbox.CurrentGear >= 0)
            {
                Engine.Instance.SpriteBatch.DrawString(_font, _chassis.Motor.Gearbox.CurrentGear.ToString(), ScaleVec2(x + 0.065f, y + 0.118f), Color.Yellow, 0, Vector2.Zero, FontScale, SpriteEffects.None, 0);
            }
            Engine.Instance.SpriteBatch.Draw(_shadow, ScaleRect(x + 0.1f, y + 0.112f, 0.068f, 0.057f), Color.White);
            Engine.Instance.SpriteBatch.DrawString(_font, ((int)_chassis.Speed).ToString("000"), ScaleVec2(x + 0.102f, y + 0.118f), Color.GreenYellow, 0, Vector2.Zero, FontScale, SpriteEffects.None, 0);

            float rpmFactor = _chassis.Motor.Rpm / _chassis.Motor.RedlineRpm;
            float rotation = (float)(rpmFactor * 4f) + 0.5f;
            Engine.Instance.SpriteBatch.Draw(_speedoLineTexture, ScaleVec2(x + 0.07f, y + 0.09f),
                null, Color.White, rotation, Vector2.Zero, ScaleVec2(0.0037f, 0.075f), SpriteEffects.None, 0);

        }
    }
}
