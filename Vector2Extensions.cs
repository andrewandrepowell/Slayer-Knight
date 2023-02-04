using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    public static class Vector2Extensions
    {
        public static Vector2 GetPerpendicular(this Vector2 current) => new Vector2(x: -current.Y, y: current.X);
    }
}
