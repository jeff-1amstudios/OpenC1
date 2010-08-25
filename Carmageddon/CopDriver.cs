using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon
{
    class CopDriver : CpuDriver
    {
        const float WAKEUP_DISTANCE = 50f;

        public CopDriver()
            : base()
        {
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
                }
            }
            else if (_state != CpuDriverState.Attacking)
            {
                SetState(CpuDriverState.Sleeping);
            }
            base.Update();
        }
    }
}
