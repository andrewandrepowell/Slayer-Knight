using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    public static class Vector2Extensions
    {
        public static Vector2 GetPerpendicular(this Vector2 current) => new Vector2(x: -current.Y, y: current.X);
        public static float Cross(this Vector2 v0, Vector2 v1) => v0.Y * v1.X - v0.X * v1.Y;
        
        public static bool IsBetweenTwoVectors(this Vector2 current, Vector2 v0, Vector2 v1)
        {
            // https://stackoverflow.com/questions/13640931/how-to-determine-if-a-vector-is-between-two-other-vectors
            var v0xc = v0.Cross(current);
            var v0xv1 = v0.Cross(v1);
            var v1xc = v1.Cross(current);
            var v1xv0 = v1.Cross(v0);
            var v0dv1 = v0.Dot(v1);
            var v0dc = v0.Dot(current);

            // This block is to handle a special case
            // where the original formulation fails!
            if (v0xv1 == 0 && v0dv1 > 0)
            {
                if (v0xc == 0 && v0dc > 0)
                    return true;
                else
                    return false;
            }

            var c0 = v0xc * v0xv1;
            var c1 = v1xc * v1xv0;
            if (c0 >= 0 && c1 >= 0)
            {
                return true;
            }
            return false;
        }
        
    }
}
