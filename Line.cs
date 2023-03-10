using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    /// <summary>
    /// Represents a line using the standard form Ax + By = C.
    /// The main intent is to determine the intersection between lines.
    /// </summary>
    public struct Line
    {
        const float determinantThreshold = 0.001f;

        // https://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines
        public float A { get; private set; }
        public float B { get; private set; } 
        public float C { get; private set; }
        public float X0 { get; private set; } 
        public float Y0 { get; private set; } 
        public float X1 { get; private set; } 
        public float Y1 { get; private set; }
        public Vector2 P0 => new Vector2(x: X0, y: Y0);
        public Vector2 P1 => new Vector2(x: X1, y: Y1);


        /// <summary>
        /// Creates a line from two points. 
        /// The order of the two points don't matter since a line doesn't have direction.
        /// </summary>
        /// <param name="p0">One of the two points.</param>
        /// <param name="p1">One of the two points.</param>
        public Line(Vector2 p0, Vector2 p1) : this(p0.X, p0.Y, p1.X, p1.Y)
        {
        }

        /// <summary>
        /// Creates a line two points.
        /// The order of the two points don't matter since a line doesn't have direction.
        /// </summary>
        /// <param name="x0">X component of point 0.</param>
        /// <param name="y0">Y component of point 0.</param>
        /// <param name="x1">X component of point 1.</param>
        /// <param name="y1">Y component of point 1.</param>
        public Line(float x0, float y0, float x1, float y1)
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
            A = y1 - y0;
            B = x0 - x1;
            C = A * x0 + B * y0;
        }

        /// <summary>
        /// Determines the determinant between two lines.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static float Determinant(Line current, Line other)
        {
            return current.A * other.B - other.A * current.B;
        }

        /// <summary>
        /// Determines whether the two lines actually intersect.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns>True if intersection does occur, false is parallel lines.</returns>
        public static bool Intersects(Line current, Line other)
        {
            return Math.Abs(Determinant(current, other)) > determinantThreshold;
        }

        public static bool Intersect(Line current, Line other, out Vector2 intersection)
        {
            intersection = Vector2.Zero;
            float determinant = Determinant(current, other);
            if (Math.Abs(determinant) <= determinantThreshold)
                return false;
            intersection = new Vector2(
                x: (other.B * current.C - current.B * other.C) / determinant,
                y: (current.A * other.C - other.A * current.C) / determinant);
            return true;
        }

        public static bool Intersects(Line current, Rectangle other, Vector2 otherOffset)
        {
            float left = other.Left + otherOffset.X;
            float right = other.Right + otherOffset.X;
            float top = other.Top + otherOffset.Y;
            float bottom = other.Bottom + otherOffset.Y;
            Line[] lines = new Line[]
            {
                new Line(x0: left, y0: top, x1: right, y1: top),
                new Line(x0: right, y0: top, x1: right, y1: bottom),
                new Line(x0: right, y0: bottom, x1: left, y1: bottom),
                new Line(x0: left, y0: bottom, x1: left, y1: top)
            };
            foreach (var line in lines)
            {
                if (Intersects(current: current, other: line))
                {
                    // https://math.stackexchange.com/questions/1698835/find-if-a-vector-is-between-2-vectors
                }
                
            }
            return false;
        }

        /// <summary>
        /// Creates 4 lines of a rectangle.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns>An array of length 4. Index 0 is the top line, 1 is the right line, 2 is the bottom ine, and 3 is the left line.</returns>
        public static Line[] Lines(Rectangle rectangle)
        { // 0 - top, 1 - right, 2 - bottom, 3 - left
            Line[] lines = new Line[]
            {
                new Line(
                    x0: rectangle.Left,
                    y0: rectangle.Top,
                    x1: rectangle.Right,
                    y1: rectangle.Top),
                new Line(
                    x0: rectangle.Right,
                    y0: rectangle.Top,
                    x1: rectangle.Right,
                    y1: rectangle.Bottom),
                new Line(
                    x0: rectangle.Right,
                    y0: rectangle.Bottom,
                    x1: rectangle.Left,
                    y1: rectangle.Bottom),
                new Line(
                    x0: rectangle.Left,
                    y0: rectangle.Bottom,
                    x1: rectangle.Left,
                    y1: rectangle.Top)
            };
            return lines;
        }
    }
}