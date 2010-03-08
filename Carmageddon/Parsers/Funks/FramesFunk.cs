using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon.Parsers.Funks
{

    class FramesFunk : BaseFunk
    {
        FunkLoopType Loop { get; set; }
        public List<string> FrameNames = new List<string>();
        List<Texture2D> _frames = new List<Texture2D>();
        public float Speed;

        float _currentFrameTime;
        int _currentFrame;

        public FramesFunk()
        {

        }

        public override void Resolve()
        {
            base.Resolve();

            foreach (string frameName in FrameNames)
            {
                _frames.Add(ResourceCache.GetPixelMap(frameName).Texture);
            }
        }

        public override void Update()
        {
            _currentFrameTime += Engine.ElapsedSeconds;
            if (_currentFrameTime > Speed)
            {
                _currentFrame++;
                if (_currentFrame == _frames.Count) _currentFrame = 0;
                _currentFrameTime = 0;

                Material.Texture = _frames[_currentFrame];
            }
        }
    }
}
