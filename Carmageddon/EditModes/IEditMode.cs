using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon.EditModes
{
    interface IEditMode
    {
        void Activate();
        void Update();
        void Render();
    }
}
