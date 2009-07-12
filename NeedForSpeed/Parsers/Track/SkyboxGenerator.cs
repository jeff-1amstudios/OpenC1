using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace Carmageddon.Parsers.Track
{
    class SkyboxGenerator
    {
        Texture2D _horizon;
        Texture2D _topTexture, _bottomTexture;

        public SkyboxGenerator(Texture2D horizonTexture)
        {
            _horizon = horizonTexture;
            Color[] pixels = new Color[_horizon.Width * _horizon.Height];
            _horizon.GetData<Color>(pixels);

            _topTexture = new Texture2D(Engine.Instance.Device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            _topTexture.SetData<Color>(new Color[] { pixels[0] });  //top left pixel

            _bottomTexture = new Texture2D(Engine.Instance.Device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            _bottomTexture.SetData<Color>(new Color[] { pixels[pixels.Length - 1] }); //bottom right pixel
        }

        public Texture2D Top
        {
            get
            {
                return _topTexture;
            }
        }

        public Texture2D Bottom
        {
            get
            {
                return _bottomTexture;
            }
        }

        public Texture2D Side
        {
            get { return _horizon; }
        }
    }
}
