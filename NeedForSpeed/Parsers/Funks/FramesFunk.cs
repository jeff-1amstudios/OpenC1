using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon.Parsers.Funks
{

    class FramesFunk : BaseFunk
    {
        FunkLoopType Loop { get; set; }
        public List<string> FrameNames = new List<string>();

        public FramesFunk()
        {
            
        }

        public override void Update()
        {

        }
    }
}
