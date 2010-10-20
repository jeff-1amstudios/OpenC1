using System;
using System.Collections.Generic;
using System.Text;

namespace OpenC1
{
    interface IDriver
    {
        bool ModerateSteeringAtSpeed { get; }
        Vehicle Vehicle { get; set; }
        void Update();
        void OnRaceStart();
    }
}
