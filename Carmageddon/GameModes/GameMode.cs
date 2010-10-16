using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon.GameModes
{
    abstract class GameMode
    {
        static GameMode _mode;

        public static GameMode Current
        {
            get
            {
                return _mode;
            }
            set
            {
                value.Activate();
                _mode = value;
            }
        }

        public abstract void Activate();
        public abstract void Update();
        public abstract void Render();
    }
}
