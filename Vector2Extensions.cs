using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    public static class Vector2Extensions
    {
        public static Vector2 GetPerpendicular(this Vector2 current) => new Vector2(x: -current.Y, y: current.X);
        public static float Cross(this Vector2 v0, Vector2 v1) => v0.Y * v1.X - v0.X * v1.Y;
        // https://stackoverflow.com/questions/13640931/how-to-determine-if-a-vector-is-between-two-other-vectors
        public static bool IsBetweenTwoVectors(this Vector2 current, Vector2 v0, Vector2 v1) => v0.Cross(current) * v0.Cross(v1) >= 0 && v1.Cross(current) * v1.Cross(v0) >= 0;
        
    }
}
