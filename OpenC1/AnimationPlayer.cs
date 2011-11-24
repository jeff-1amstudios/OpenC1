using System;
using System.Collections.Generic;
using System.Text;
using OpenC1.Parsers;
using Microsoft.Xna.Framework.Graphics;
using OneAmEngine;

namespace OpenC1
{
    class AnimationPlayer
    {
        bool _loop;
        bool _playing;
        float _currentFrameTime;
        int _currentFrame;
        List<Texture2D> _frames;

        public AnimationPlayer(List<Texture2D> frames)
            : this(frames, 0)
        {
        }

        public AnimationPlayer(List<Texture2D> frames, int startFrame)
        {
            _frames = frames;
            _currentFrame = Math.Min(_frames.Count - 1, startFrame);
        }

        public bool IsPlaying { get { return _playing; } }

        public void Play(bool loop)
        {
            _playing = true;
            _loop = loop;
        }

        public void Update()
        {
            if (!_playing) return;

            _currentFrameTime += Engine.ElapsedSeconds;

            if (_currentFrameTime > ((float)100 / 1000))
            {
                _currentFrame++;
                if (_currentFrame == _frames.Count)
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
            return _frames[_currentFrame];
        }
    }
}
