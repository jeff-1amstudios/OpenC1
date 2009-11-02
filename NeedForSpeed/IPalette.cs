using System;
using System.Collections.Generic;

using System.Text;

namespace Carmageddon
{
    interface IPalette
    {
        byte[] GetRGBBytesForPixel(int pixel);
    }
}
