using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeedForSpeed.Parsers;
using NeedForSpeed.Parsers.Track;
using Microsoft.Xna.Framework.Input;
using NFSEngine;

namespace NeedForSpeed
{
    class PlayGameMode : IGameMode
    {
        DatFileParser _mesh;
        ActFileParser _actors;
        float _rotation;

        public PlayGameMode()
        {
            //ChaseCamera camera = new ChaseCamera();
            //camera.Reset();
            SimpleCamera camera = new SimpleCamera();
            Engine.Instance.Camera = camera;
            //camera.SetPosition(

            Engine.Instance.Player = new Player();
            Engine.Instance.Player.Position = new Vector3(0, 20, 50);

            PixFileParser pix = new PixFileParser();
            pix.Parse(@"C:\Games\carma1\data\pixelmap\CHUKPINT.PIX");
            _mesh = new DatFileParser();
            _mesh.Parse(@"C:\Games\carma1\data\models\CHUKPINT.DAT");
            _actors = new ActFileParser();
            _actors.Parse(@"C:\Games\carma1\data\actors\CHUKPINT.ACT");
            MatFileParser matFile = new MatFileParser();
            matFile.Parse(@"C:\Games\carma1\data\material\CHUKPINT.MAT");
            _mesh.Resolve(matFile, pix);
            
        }


        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            Engine.Instance.Camera.Update(gameTime);
            Engine.Instance.Player.Update(gameTime);
        }

        public void Draw()
        {
            //Engine.Instance.GraphicsUtils.AddSquareGrid(Matrix.Identity, 20, Color.Green);

            _actors.Render(
                Matrix.CreateRotationY(_rotation) * Matrix.CreateScale(30),
                _mesh);
            
            _rotation += 0.01f;
        }

        #endregion
    }
}
