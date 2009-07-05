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
        MeshChunk _mesh;
        float _rotation;

        public PlayGameMode()
        {
            //ChaseCamera camera = new ChaseCamera();
            //camera.Reset();
            SimpleCamera camera = new SimpleCamera();
            Engine.Instance.Camera = camera;
            camera.SetPosition(new Vector3(0, 0, 50));

            Engine.Instance.Player = new Player();

            _mesh = new MeshChunk();
            _mesh.Parse(@"C:\Games\carma1\\data\\models\\otis.DAT");
            _mesh.Resolve(null, "*");
        }


        #region IDrawableObject Members

        public void Update(GameTime gameTime)
        {
            Engine.Instance.Camera.Update(gameTime);
            Engine.Instance.Player.Update(gameTime);
        }

        public void Draw()
        {
            //Engine.Instance.Player.Draw();
            _mesh.Render(Matrix.CreateRotationY(_rotation) * Matrix.CreateScale(200f) * Matrix.CreateTranslation(new Vector3(30, -30, -280)));
            _rotation += 0.01f;
        }

        #endregion
    }
}
