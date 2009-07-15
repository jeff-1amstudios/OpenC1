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
    class PlayGameMode : IGameScreen
    {
        Car _car;
        Race _race;

        public PlayGameMode()
        {
            //Engine.Instance.Device.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
            //Engine.Instance.Device.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;

            GameVariables.Palette = new PaletteFile("c:\\games\\carma1\\data\\reg\\palettes\\drrender.pal");

            //_car = new Car(@"C:\Games\carma1\data\cars\blkeagle.txt");

            _race = new Race(@"C:\Games\carma1\data\races\cityb2.TXT");

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
        }

        public void Draw()
        {
            //_car.Render();
            _race.Render();
        }

        #endregion
    }
}
