using System;
using System.Collections.Generic;
using System.Text;
using NFSEngine.Audio;

namespace Carmageddon
{
    class CopDriver : CpuDriver
    {
        const float WAKEUP_DISTANCE = 50f;

        ISound _siren;

        public CopDriver()
            : base()

        {
            _siren = SoundCache.CreateInstance(SoundIds.CopSiren, true);
        }

        public override void OnRaceStart()
        {
            base.OnRaceStart();
            _catchupDistance = float.MaxValue;  //dont ever want to re-position cops
            _state = CpuDriverState.Sleeping;
        }

        public override void Update()
        {
            if (_state == CpuDriverState.Sleeping)
            {
                if (DistanceFromPlayer < WAKEUP_DISTANCE)
                {
                    SetState(CpuDriverState.Attacking);
                    _siren.Play(true);
                }
            }
            else if (_state != CpuDriverState.Attacking)
            {
                SetState(CpuDriverState.Sleeping);
                _siren.Stop();
            }
            else if (_state == CpuDriverState.Attacking)
            {
                _siren.Position = Vehicle.Position;
                _siren.Velocity = Vehicle.Chassis.Actor.LinearVelocity;
            }
            base.Update();
        }
    }
}
