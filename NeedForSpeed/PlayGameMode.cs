using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework.Input;
using NFSEngine;
using Carmageddon.Track;

namespace Carmageddon
{
    class PlayGameMode : IGameScreen
    {
        Car _car;
        Race _race;
        SkyBox _skybox;

        public PlayGameMode()
        {
            //Engine.Instance.Device.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
            //Engine.Instance.Device.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;

            GameVariables.Palette = new PaletteFile("c:\\games\\carma1\\data\\reg\\palettes\\drrender.pal");

            _car = new Car(@"C:\Games\carma1\data\cars\blkeagle.txt");

            _race = new Race(@"C:\Games\carma1\data\races\citya1.TXT");

            SkyboxGenerator gen = new SkyboxGenerator(_race.HorizonTexture);
            _skybox = gen.Generate();
            
            Engine.Instance.Camera = new FPSCamera(Engine.Instance.Game);// camera;

            Engine.Instance.Player = new Player();
            Engine.Instance.Player.Position = _race.RaceFile.GridPosition;
            Engine.Instance.Player.SetRotation(MathHelper.ToRadians(_race.RaceFile.GridDirection));
        }


        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            _skybox.Update(gameTime);
            Engine.Instance.Camera.Update(gameTime);
            Engine.Instance.Player.Update(gameTime);
        }

        public void Draw()
        {
            GameConsole.WriteLine(Engine.Instance.Camera.Position, 0);
            _skybox.Draw();
            
            _race.Render();
            _car.Render();
        }

        #endregion
    }
}
