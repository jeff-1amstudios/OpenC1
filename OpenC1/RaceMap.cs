using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using OpenC1.Parsers;
using Microsoft.Xna.Framework;
using OneAmEngine;

namespace OpenC1
{
    class RaceMap
    {
        Texture2D _mapTexture, _player, _opponent, _deadOpponent;
        Race _race;
        public bool Show;
        private Rectangle _mapRect;

        public RaceMap(Race race)
        {
            _race = race;
            PixFile pix = new PixFile(race.ConfigFile.MapTexture);
            if (pix.Exists)
                _mapTexture = pix.PixMaps[0].Texture;
            _player = Engine.ContentManager.Load<Texture2D>("content/map-icon-player");
            _opponent = Engine.ContentManager.Load<Texture2D>("content/map-icon-opponent");
            _deadOpponent = Engine.ContentManager.Load<Texture2D>("content/map-icon-opponent-dead");

            _mapRect = new Rectangle(0, 60, Engine.Window.Width, Engine.Window.Height - 90);
        }

        public void Render()
        {
            if (_mapTexture == null) return;

            Engine.SpriteBatch.Draw(_mapTexture, _mapRect, new Color(255,255,255, 200));
            Vector3 pos = _race.PlayerVehicle.Position;
            pos /= GameVars.Scale;
            Vector3 translated = Vector3.Transform(pos, _race.ConfigFile.MapTranslation);
            translated /= new Vector3(320, 200, 1);
            translated *= new Vector3(_mapRect.Width, _mapRect.Height, 1);
            translated += new Vector3(_mapRect.Left, _mapRect.Top, 0);

            Vector3 direction = _race.PlayerVehicle.Chassis.Actor.GlobalOrientation.Forward;
            float rotation = (float)Math.Atan2(direction.Z, direction.X) + MathHelper.Pi;            
            Engine.SpriteBatch.Draw(_player, new Vector2(translated.X, translated.Y), null, Color.White, rotation, new Vector2(8,8), 1f, SpriteEffects.None, 0);

            foreach (Opponent o in _race.Opponents)
            {
                if (o.Driver is CopDriver)
                    continue;

                pos = o.Vehicle.Position;
                pos /= GameVars.Scale;
                translated = Vector3.Transform(pos, _race.ConfigFile.MapTranslation);
                translated /= new Vector3(320, 200, 1);
                translated *= new Vector3(_mapRect.Width, _mapRect.Height, 1);
                translated += new Vector3(_mapRect.Left, _mapRect.Top, 0);
                direction = o.Vehicle.Chassis.Actor.GlobalOrientation.Forward;
                rotation = (float)Math.Atan2(direction.Z, direction.X) + MathHelper.Pi;
                Engine.SpriteBatch.Draw(o.IsDead ? _deadOpponent : _opponent,
                    new Vector2(translated.X, translated.Y), null, Color.White, rotation, new Vector2(8, 8), 1f, SpriteEffects.None, 0);
            }
        }
    }
}
