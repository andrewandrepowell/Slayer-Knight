using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Utility
{
    public static class ColorExtensions
    {
        public static Color Add(this Color color, Color color2)
        {
            return new Color(
                r: Math.Min(color.R + color2.R, 255),
                g: Math.Min(color.G + color2.G, 255),
                b: Math.Min(color.B + color2.B, 255),
                alpha: Math.Min(color.A + color2.A, 255));
        }
    }
}
