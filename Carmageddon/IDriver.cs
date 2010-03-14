using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon
{
    interface IDriver
    {
        Vehicle Vehicle { get; set; }
        void Update();
        void OnRaceStart();
    }
}
