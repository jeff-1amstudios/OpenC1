using System;
using System.Collections.Generic;

using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;

namespace Carmageddon.Physics
{
    class WheelUserData
    {
        private const float MaxAirTime = 2;
        private float _airTime, _lastAirTime;

        public void UpdateContactForce(float force)
        {
            if (force == 0)
            {
                if (_airTime < MaxAirTime)
                {
                    _airTime += Engine.Instance.ElapsedSeconds;
                    _lastAirTime = _airTime;
                }
                else
                {
                }
            }
            else
            {
                if (_airTime > 0)
                    _airTime -= Engine.Instance.ElapsedSeconds * 0.5f;
            }
        }

        public float GetStiffness(float stiffness)
        {
            if (_airTime > 0f)
            {
                //_stiffness += Engine.Instance.ElapsedSeconds;
                //return _stiffness;
                return MathHelper.Lerp(0.7f, 0.01f, _airTime / MaxAirTime);
            }
            
            _airTime = 0;
            return stiffness;
        }
    }
}