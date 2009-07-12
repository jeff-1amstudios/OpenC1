using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Carmageddon.Parsers.Track;
using Microsoft.Xna.Framework.Input;
using NFSEngine;

namespace Carmageddon
{
    class PlayGameMode : IGameMode
    {
        Car _car;

        public PlayGameMode()
        {
            Engine.Instance.Device.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
            Engine.Instance.Device.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;

            _car = new Car(@"C:\Games\carma1\data\cars\blkeagle.txt");

            SimpleCamera camera = new SimpleCamera();
            Engine.Instance.Camera = new FPSCamera(Engine.Instance.Game);// camera;

            Engine.Instance.Player = new Player();
            Engine.Instance.Player.Position = new Vector3(0, 20, 50);
        }


        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            Engine.Instance.Camera.Update(gameTime);
            Engine.Instance.Player.Update(gameTime);

            _car.Update(gameTime);
        }

        public void Draw()
        {
            _car.Render();
        }

        #endregion
    }
}
