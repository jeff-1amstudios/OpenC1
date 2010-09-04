using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using Microsoft.Xna.Framework.Graphics;
using PlatformEngine;

namespace Carmageddon
{
    class FliPlayer
    {
        bool _loop;
        bool _playing;
        float _currentFrameTime;
        int _currentFrame;
        FliFile _fli;

        public FliPlayer(FliFile fli)
        {
            _fli = fli;
        }

        public bool IsPlaying { get { return _playing; } }

        public void Play(bool loop, float delay)
        {
            _currentFrame = 0;
            _currentFrameTime = -delay;
            _playing = true;
            _loop = loop;
        }

        public void Update()
        {
            if (!_playing) return;

            _currentFrameTime += Engine.ElapsedSeconds;

            if (_currentFrameTime > ((float)_fli.FrameRate / 1000))
            {
                _currentFrame++;
                if (_currentFrame == _fli.Frames.Count)
                {
                    if (_loop)
                        _currentFrame = 0;
                    else
                    {
                        _currentFrame--;
                        _playing = false;
                    }
                }
                _currentFrameTime = 0;
            }
        }

        public Texture2D GetCurrentFrame()
        {
            return _fli.Frames[_currentFrame];
        }
    }
}
