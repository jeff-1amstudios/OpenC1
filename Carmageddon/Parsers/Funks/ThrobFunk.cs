using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using OneAmEngine;

namespace OpenC1.Parsers.Funks
{
    class ThrobFunk : BaseFunk
    {
        public Motion Motion;
        public Vector2 CyclesPerSecond;
        public Vector2 MoveDistance;
        Vector2 _uvOffset;
        Vector2 _direction;
        float _cycleTime, _cyclePosition, _targetTime, _harmonicMultiplier;

        public override void BeforeRender()
        {
            GameVars.CurrentEffect.TexCoordsOffset = _uvOffset;
            GameVars.CurrentEffect.CommitChanges();
        }

        public override void AfterRender()
        {
            GameVars.CurrentEffect.TexCoordsOffset = Vector2.Zero;
            GameVars.CurrentEffect.CommitChanges();
        }

        public void Initialize()
        {
            _targetTime = 1f / Math.Max(CyclesPerSecond.X, CyclesPerSecond.Y);
            AssignNewTarget();
        }

        public override void Update()
        {
            // this is stupidly complex...
            // _cycleTime is the time it takes to a full slither in/out cycle
            // _cyclePosition is the current position in the cycle. As the cycle gets to 
            // halfway, _cyclePosition starts returning to origin

            _cycleTime += Engine.ElapsedSeconds * _harmonicMultiplier;

            if (_cycleTime > _targetTime)
            {
                _cycleTime = 0;
                AssignNewTarget();
            }

            if (_cycleTime > _targetTime / 2)
            {
                _cyclePosition -= Engine.ElapsedSeconds * _harmonicMultiplier;  //sither back to start
            }
            else
            {
                _cyclePosition = _cycleTime;
            }

            if (Motion == Motion.Harmonic)
            {
                float distance = Math.Abs(_cyclePosition - _targetTime / 4) / (_targetTime / 4);
                if (distance > 0.7f)
                    _harmonicMultiplier = MathHelper.Lerp(5f, 0.2f, distance);
                else
                    _harmonicMultiplier = 1;
            }

            _uvOffset = _cyclePosition * CyclesPerSecond * MoveDistance * _direction;
        }

        private void AssignNewTarget()
        {
            _direction.X = Engine.Random.Next(0.5f, 1f) * (Engine.Random.Next(1, 20) % 2 == 0 ? 1 : -1);
            _direction.Y = Engine.Random.Next(0.5f, 1) * (Engine.Random.Next(1, 20) % 2 == 0 ? 1 : -1);
        }

        
    }
}
