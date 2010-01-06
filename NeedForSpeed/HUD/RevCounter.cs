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
        int x, y;
        Texture2D _speedoTexture, _speedoLineTexture;
        SpriteFont _font;

        public RevCounter(VehicleChassis vehicle)
        {
            _chassis = vehicle;
            _font = Engine.Instance.ContentManager.Load<SpriteFont>("content/speedo-font");
            
            x = 15;
            y = Engine.Instance.Window.Height - 105;

            _speedoTexture = Engine.Instance.ContentManager.Load<Texture2D>("content/tacho");
            _speedoLineTexture = TextureGenerator.Generate(new Color(255, 0, 0));            
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            Engine.Instance.SpriteBatch.Draw(_speedoTexture, new Rectangle(x, y, 116, 96), Color.White);

            Engine.Instance.SpriteBatch.Draw(_shadow, new Rectangle(x + 44, y + 71, 24, 32), Color.White);
            Engine.Instance.SpriteBatch.DrawString(_font, _chassis.Motor.Gearbox.CurrentGear.ToString(), new Vector2(x + 47, y + 73), Color.Yellow);
            Engine.Instance.SpriteBatch.Draw(_shadow, new Rectangle(x + 74, y + 71, 55, 32), Color.White);
            Engine.Instance.SpriteBatch.DrawString(_font, ((int)_chassis.Speed).ToString("000"), new Vector2(x+77, y+73), Color.GreenYellow);

            float rpmFactor = _chassis.Motor.Rpm / _chassis.Motor.RedlineRpm;
            float rotation = (float)(rpmFactor * 4f) + 0.5f;
            Engine.Instance.SpriteBatch.Draw(_speedoLineTexture, new Vector2(x + 59, y + 58),
                null, Color.White, rotation, new Vector2(0, 0), new Vector2(3, 45), SpriteEffects.None, 0);

        }
    }
}
