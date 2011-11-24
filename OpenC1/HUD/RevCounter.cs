using System;
using System.Collections.Generic;
using System.Text;
using OneAmEngine;
using OpenC1.Parsers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OpenC1.Physics;

namespace OpenC1.HUD
{
    class RevCounter: BaseHUDItem
    {
        VehicleChassis _chassis;
        float x, y;
        Texture2D _speedoTexture, _speedoLineTexture;

        public RevCounter(VehicleChassis vehicle)
        {
            _chassis = vehicle;

            x = 0.01f;
            y = 0.8f;
            if (GameVars.Emulation == EmulationMode.Demo)
            {
                PixFile pix = new PixFile("tacho.pix");
                _speedoTexture = pix.PixMaps[0].Texture;
            }
            else
            {
                PixFile pix = new PixFile("hirestch.pix");
                _speedoTexture = pix.PixMaps[0].Texture;
            }
            _speedoLineTexture = TextureGenerator.Generate(new Color(255, 0, 0));            
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            Engine.SpriteBatch.Draw(_speedoTexture, ScaleRect(x, y, 0.145f, 0.16f), Color.White);

            DrawShadow(ScaleRect(x + 0.06f, y + 0.112f, 0.03f, 0.057f));
            
            FontRenderer.RenderGear(_chassis.Motor.Gearbox.CurrentGear+1, ScaleVec2(x + 0.065f, y + 0.118f), Color.White, FontScale*2);
            
            DrawShadow(ScaleRect(x + 0.1f, y + 0.112f, 0.068f, 0.057f));
            FontRenderer.Render(Fonts.Speedo, ((int)_chassis.Speed).ToString("000"), ScaleVec2(x + 0.102f, y + 0.118f), Color.White, FontScale);

            float rpmFactor = _chassis.Motor.Rpm / _chassis.Motor.RedlineRpm;
            float rotation = (float)(rpmFactor * 4f) + 0.5f;
            Engine.SpriteBatch.Draw(_speedoLineTexture, ScaleVec2(x + 0.07f, y + 0.09f),
                null, Color.White, rotation, Vector2.Zero, ScaleVec2(0.0037f, 0.075f), SpriteEffects.None, 0);
        }
    }
}
