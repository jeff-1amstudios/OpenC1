using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;

namespace Carmageddon.Parsers.Funks
{
    class RollFunk : BaseFunk
    {
        public Vector2 Speed;
        Vector2 _uvOffset;

        public RollFunk()
        {
            
        }

        public override void BeforeRender()
        {
            GameVariables.CurrentEffect.TexCoordsOffset = _uvOffset;
            GameVariables.CurrentEffect.CommitChanges();
        }

        public override void AfterRender()
        {
            GameVariables.CurrentEffect.TexCoordsOffset = Vector2.Zero;
            GameVariables.CurrentEffect.CommitChanges();
        }

        public override void Update()
        {
            _uvOffset += Speed * 0.8f * Engine.Instance.ElapsedSeconds;
            if (_uvOffset.X > 1) _uvOffset.X = 1 - _uvOffset.X;
            if (_uvOffset.Y > 1) _uvOffset.Y = 1 - _uvOffset.Y;
        }
    }
}
