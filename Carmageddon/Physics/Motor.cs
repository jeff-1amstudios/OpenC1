using System;
using System.Collections.Generic;
using System.Text;
using PlatformEngine;
using Microsoft.Xna.Framework;

namespace Carmageddon.Physics
{
    class Motor
    {
        private float DriveTrainMultiplier = 31;

        private List<float> _powerCurve;
        private float _maxPower;
        private float _redlineRpm;
        private BaseGearbox _gearbox;
        private float _rpm, _prevRpm, _prevEngagedRpm;
        private float _throttle;
        private float _currentPowerOutput;
        private float _rpmLimiter;
        private float _lastCarSpeed;

        public float CurrentPowerOutput
        {
            get
            {
                if (_gearbox.GearEngaged)
                {
                    float power = _currentPowerOutput * _throttle; //* _gearbox.CurrentRatio;
                    if (AtRedline && !WheelsSpinning) power *= 0.2f;
                    return power;
                }
                else
                    return 0;
            }
        }

        public float CurrentFriction
        {
            get
            {
                if (_gearbox.GearEngaged)
                    return 200;
                else
                    return 0;
            }
        }

        public float MaxPower
        {
            get { return _maxPower; }
            set { _maxPower = value * 132.5f; }  //magic number...
        }

        public bool AtRedline
        {
            get { return _rpm >= _redlineRpm; }
        }

        public float Throttle
        {
            get { return _throttle; }
            set { _throttle = value; }
        }

        public float Rpm
        {
            get { return _rpm; }
        }

        public float RedlineRpm
        {
            get { return _redlineRpm; }
        }

        public bool IsAccelerating
        {
            get { return _rpm > _prevRpm; }
        }

        internal BaseGearbox Gearbox
        {
            get { return _gearbox; }
        }

        public Motor(List<float> powerCurve, float maxPower, float redline, float driveTrainMultipler, BaseGearbox gearbox)
        {
            _powerCurve = powerCurve;
            MaxPower = maxPower;
            _redlineRpm = redline;
            _gearbox = gearbox;
            _gearbox.CurrentGear = 1;
            _gearbox.Motor = this;
            DriveTrainMultiplier = 0.155f * driveTrainMultipler;
        }


        public void Update(float carSpeed)
        {
            _prevRpm = _rpm;
            _lastCarSpeed = carSpeed;

            if (_rpm >= _redlineRpm)
            {
                _rpm = _redlineRpm;
            }

            if (_rpmLimiter > 0)
            {
                if (!WheelsSpinning) _currentPowerOutput = 0;
                _rpmLimiter -= Engine.ElapsedSeconds;
            }
            else
                _currentPowerOutput = _maxPower * MathHelper.Lerp(_powerCurve[(int)_rpm], _powerCurve[(int)_rpm + 1], _rpm - (int)_rpm);

            if (_gearbox.GearEngaged)
            {
                if (_gearbox.CurrentGear == 0)
                {
                    _currentPowerOutput = 0;
                }
                //if (_gearbox.CurrentGear == -1)
                  //  _currentPowerOutput *= -1;

                if (_gearbox.CurrentGear == 0 || WheelsSpinning)
                {
                    HandleRpmNoLoad();
                }
                else
                {
                    _rpm = Math.Abs(carSpeed * _gearbox.CurrentRatio / DriveTrainMultiplier);
                    if (_rpm < 0.8f)
                        _rpm = 0.8f;  //idle speed
                }
                _prevEngagedRpm = _rpm;
            }
            else
            {
                _rpm = MathHelper.Lerp(_prevEngagedRpm,
                    carSpeed * _gearbox.NextRatio / DriveTrainMultiplier, _gearbox.Clutch);
            }

            if (_rpm < 0.8f)
                _rpm = 0.8f;

            if (_rpm >= _redlineRpm)
            {
                _rpmLimiter = 0.2f;
            }

            _gearbox.Update(_rpm / _redlineRpm);
        }

        public float GetPowerAtRpmForGear(float rpm, int gear)
        {
            float power = _maxPower * MathHelper.Lerp(_powerCurve[(int)_redlineRpm], _powerCurve[(int)_redlineRpm + 1], _redlineRpm - (int)_redlineRpm);
            power *= _gearbox.Ratios[gear];
            return power;
        }

        public void Idle()
        {
            _rpm = 0.8f;
            _gearbox.CurrentGear = 1;
        }

        public float GetRpmForGear(int gear)
        {
            return _lastCarSpeed * _gearbox.Ratios[gear] / DriveTrainMultiplier;
        }

        private void HandleRpmNoLoad()
        {
            if (_throttle == 0.0f || _rpmLimiter > 0)
            {
                _rpm -= Engine.ElapsedSeconds * 4.4f;

                if (_rpm < 0.8f)
                    _rpm = 0.8f;
            }
            else
            {
                _rpm += Engine.ElapsedSeconds * _throttle * 8f;
            }
        }

        public bool WheelsSpinning { get; set; }

        public bool CanChangeDown
        {
            get
            {
                return GetRpmForGear(_gearbox.CurrentGear) / RedlineRpm < 0.9f;
            }
        }
    }
}
