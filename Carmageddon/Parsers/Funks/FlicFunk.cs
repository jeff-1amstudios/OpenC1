using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace Carmageddon.Parsers.Funks
{
    class FlicFunk : BaseFunk
    {
        public FunkLoopType Loop { get; private set; }
        float _speed;
        List<Texture2D> _frames;
        float _currentFrameTime;
        int _currentFrame;

        public FlicFunk(string fliname)
        {
            FliFile fli = ResourceCache.GetFliFile(GameVariables.BasePath + "data\\anim\\" + fliname);
            _frames = fli.Frames;
            _speed = (float)fli.FrameRate / 1000; // to seconds
        }


        public override void Update()
        {
            _currentFrameTime += Engine.ElapsedSeconds;
            if (_currentFrameTime > _speed)
            {
                _currentFrame++;
                if (_currentFrame == _frames.Count) _currentFrame = 0;
                _currentFrameTime = 0;

                Material.Texture = _frames[_currentFrame];
            }
        }
    }
}
