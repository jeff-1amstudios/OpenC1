using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using NFSEngine;

namespace Carmageddon.Parsers.Funks
{
    class SlitherFunk : BaseFunk
    {
        public Motion Motion;
        public Vector2 CyclesPerSecond, MoveDistance;
        Vector2 _uvOffset;
        
        float _harmonicMultiplier = 1;
        float _cycleTime, _cyclePosition;
        float _targetTime;

        public void Initialize()
        {
            _targetTime = 1f / Math.Max(CyclesPerSecond.X, CyclesPerSecond.Y);
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
            // this is stupidly complex...
            // _cycleTime is the time it takes to a full slither in/out cycle
            // _cyclePosition is the current position in the cycle. As the cycle gets to 
            // halfway, _cyclePosition starts returning to origin

            _cycleTime += Engine.Instance.ElapsedSeconds * _harmonicMultiplier;

            if (_cycleTime > _targetTime)
            {
                _cycleTime = 0;
            }

            if (_cycleTime > _targetTime / 2)
            {
                _cyclePosition -= Engine.Instance.ElapsedSeconds *_harmonicMultiplier;  //sither back to start
            }
            else
            {
                _cyclePosition = _cycleTime;
            }

            if (Motion == Motion.Harmonic)
            {
                float distance = Math.Abs(_cyclePosition - _targetTime / 4) / (_targetTime / 4);
                if (distance > 0.7f)
                    _harmonicMultiplier = MathHelper.Lerp(2.5f, 0.1f, distance);
                else
                    _harmonicMultiplier = 1;
            }

            _uvOffset = _cyclePosition * CyclesPerSecond * MoveDistance;
        }
    }
}
