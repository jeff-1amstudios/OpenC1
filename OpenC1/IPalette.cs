using System;
using System.Collections.Generic;

using System.Text;

namespace OpenC1
{
    interface IPalette
    {
        byte[] GetRGBBytesForPixel(int pixel);
    }
}
