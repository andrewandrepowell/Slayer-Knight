using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    public static class Vector2Extensions
    {
        public static Vector2 GetPerpendicular(this Vector2 current) => new Vector2(x: -current.Y, y: current.X);
        public static bool IsBetweenTwoVectors(this Vector2 current, Vector2 v0, Vector2 v1)
        {
            var d0 = Vector2.Dot(current, v0);
            var d1 = Vector2.Dot(current, v1);
            if (d0 == 0 || d1 == 0)
                return false;
            return Math.Sign(d0) == Math.Sign(d1);
        }
    }
}
