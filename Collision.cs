using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using MonoGame.Extended;
using System.Collections;

namespace Utility
{
    public struct CollisionInfo
    {
        public CollisionInterface Other { get; private set; }
        public Vector2 Point { get; private set; }
        public Vector2 Correction { get; private set; }
        public Vector2 Normal { get; private set; }
        public CollisionInfo(CollisionInterface other, Vector2 point, Vector2 correction, Vector2 normal)
        {
            Other = other;
            Point = point;
            Correction = correction;
            Normal = normal;
        }
    }
    public interface CollisionInterface : DirectlyManagedInterface<CollisionManager>
    {
        public Vector2 Position { get; }
        public Size Size { get; }
        public bool Collidable { get; } 
        public bool Static { get; }
        public Color[] CollisionMask { get; }
        public List<Vector2> CollisionVertices { get; }
        public ChannelInterface<CollisionInfo> CollisionInfoChannel { get; }
    }
    public static class CollisionManagerExtensions
    {
        public static bool CheckForCollision(this CollisionInterface collidable0) => CollisionManager.CheckForCollision(collidable0);
    }
    public class CollisionManager
    {
        public DirectlyManagedList<CollisionInterface, CollisionManager> Features { get; private set; }
        public CollisionManager() => Features = new DirectlyManagedList<CollisionInterface, CollisionManager>(manager: this);
        public static bool CheckForCollision(CollisionInterface collidable0)
        {
            bool collisionOccured = false;

            // For each collidable in the manager, check for collisions with other collidables in the manager.
            foreach (var collidable1 in collidable0.ManagerObject.Features)
            {
                if (collidable0 != collidable1 && CheckForCollision(
                    collidable0: collidable0, collidable1: collidable1,
                    correction0: out Vector2 correction0, correction1: out Vector2 correction1,
                    point0: out Vector2 point0, point1: out Vector2 point1,
                    normal0: out Vector2 normal0, normal1: out Vector2 normal1))
                {
                    if (!collidable0.Static)
                        collidable0.CollisionInfoChannel.Enqueue(new CollisionInfo(other: collidable1, point: point0, correction: correction0, normal: normal0));
                    if (!collidable1.Static)
                        collidable1.CollisionInfoChannel.Enqueue(new CollisionInfo(other: collidable0, point: point1, correction: correction1, normal: normal1));
                    collisionOccured = true;
                }
            }

            return collisionOccured;
        }

        private static bool CheckForCollision(
            CollisionInterface collidable0, CollisionInterface collidable1, 
            out Vector2 correction0, out Vector2 correction1, 
            out Vector2 point0, out Vector2 point1,
            out Vector2 normal0, out Vector2 normal1)
        {
            correction0 = Vector2.Zero;
            correction1 = Vector2.Zero;
            point0 = Vector2.Zero;
            point1 = Vector2.Zero;
            normal0 = Vector2.Zero;
            normal1 = Vector2.Zero;

            // If both of the collidables are not collidable, then there is no collision.
            if (!collidable0.Collidable && !collidable1.Collidable)
                return false;

            // Determine the bounding rectangles for this physics and the other physics.
            Rectangle bounds0 = new Rectangle(location: collidable0.Position.ToPoint(), size: collidable0.Size);
            Rectangle bounds1 = new Rectangle(location: collidable1.Position.ToPoint(), size: collidable1.Size);

            // Determine whether the bounding rectangles intersect.
            // If they don't, don't bother going further.
            if (!bounds0.Intersects(bounds1))  
                return false;

            // Find the bounding rectangle that represents the overlap between 
            // the two bounding rectangles. 
            Rectangle intersection = Rectangle.Intersect(bounds0, bounds1);

            // Find the equivalent rectangles that represent the intersection relative
            // to the two mask bounding rectangles.
            // These are needed to extract the corresponding pixel data from the masks.
            Rectangle intersection0 = new Rectangle(
                x: intersection.X - bounds0.X,
                y: intersection.Y - bounds0.Y,
                width: intersection.Width,
                height: intersection.Height);
            Rectangle intersection1 = new Rectangle(
                x: intersection.X - bounds1.X,
                y: intersection.Y - bounds1.Y,
                width: intersection.Width,
                height: intersection.Height);

            // Get the pixel data for the masks.
            Color[] colorData0 = collidable0.CollisionMask.Extract(size: collidable0.Size, region: intersection0);
            Color[] colorData1 = collidable1.CollisionMask.Extract(size: collidable1.Size, region: intersection1);

            // Get the mask that represents all points of intersection.
            bool[] collisionMask = colorData0.Zip(colorData1, (c0, c1) => c0.A != 0 && c1.A != 0).ToArray();
            if (!collisionMask.Contains(true))
                return false;

            // Declare variables needed to determine the amount of overlap in the top, bottom, left, and right regions of
            // each collider's intersection.
            int midHeight0 = bounds0.Height / 2;
            int midWidth0 = bounds0.Width / 2;
            int midHeight1 = bounds1.Height / 2;
            int midWidth1 = bounds1.Width / 2;
            int topSum0 = 0, topSum1 = 0;
            int bottomSum0 = 0, bottomSum1 = 0;
            int leftSum0 = 0, leftSum1 = 0;
            int rightSum0 = 0, rightSum1 = 0;
            
            // Declare the variables needed for computing the collision points.
            int rowMin = intersection.Height - 1;
            int rowMax = 0;
            int colMin = intersection.Width - 1;
            int colMax = 0;

            // In order to determine the overlap correction distances, arrays for the column and row counts are declared.
            int[] colCounts = new int[intersection.Width];
            int[] rowCounts = new int[intersection.Height];

            // The following nested for-loop determines the following for each collider.
            //  - The amount of overlap in the top, bottom, left, and right regions of each intersection.
            //  - The minimum and maximum row and column indices of the intersection.
            //  - Column and row counts, used for determing correction distances.
            for (int row = 0; row < intersection.Height; row++)
                for (int col = 0; col < intersection.Width; col++)
                    if (collisionMask[col + row * intersection.Width])
                    {
                        if (row + intersection0.Y < midHeight0)
                            topSum0++;
                        else
                            bottomSum0++;
                        if (col + intersection0.X < midWidth0)
                            leftSum0++;
                        else
                            rightSum0++;

                        if (row + intersection1.Y < midHeight1)
                            topSum1++;
                        else
                            bottomSum1++;
                        if (col + intersection1.X < midWidth1)
                            leftSum1++;
                        else
                            rightSum1++;

                        if (row < rowMin)
                            rowMin = row;
                        if (row > rowMax)
                            rowMax = row;
                        if (col < colMin)
                            colMin = col;
                        if (col > colMax)
                            colMax = col;

                        colCounts[col]++;
                        rowCounts[row]++;
                    }

            // Compute overlap correction distances aand mid points. 
            int overlapWidth = rowCounts.Max();
            int overlapHeight = colCounts.Max();

            // Index of row and column maxes are used to determine collision points.
            int rowOfMax = Array.IndexOf(rowCounts, overlapWidth);
            int colOfMax = Array.IndexOf(colCounts, overlapHeight);

            // Mid points are used to determine collision points.
            int pointMidX = (colMax + colMin) / 2;
            int pointMidY = (rowMax + rowMin) / 2;

            // Adjustments are used to more accurately determine correction vectors.
            float adjustX0 = (collidable0.Position.X - bounds0.X);
            float adjustY0 = (collidable0.Position.Y - bounds0.Y);
            float adjustX1 = (collidable1.Position.X - bounds1.X);
            float adjustY1 = (collidable1.Position.Y - bounds1.Y);

            // Compute the correction vectors and collision points.
            float correctionOffsetY0, correctionOffsetY1;
            float correctionOffsetX0, correctionOffsetX1;
            float pointY0, pointY1;
            float pointX0, pointX1;
            if (overlapHeight < overlapWidth)
            {
                correctionOffsetX0 = 0;
                correctionOffsetX1 = 0;
                pointX0 = colOfMax + intersection1.X + collidable1.Position.X;
                pointX1 = colOfMax + intersection0.X + collidable0.Position.X;
                
                if (bottomSum0 > topSum0)
                {
                    correctionOffsetY0 = -overlapHeight - adjustY0;
                    pointY0 = rowMin + intersection1.Y + collidable1.Position.Y;
                }
                else
                {
                    correctionOffsetY0 = overlapHeight - adjustY0;
                    pointY0 = rowMax + intersection1.Y + collidable1.Position.Y;
                }
                if (bottomSum1  > topSum1)
                {
                    correctionOffsetY1 = -overlapHeight - adjustY1;
                    pointY1 = rowMin + intersection0.Y + collidable0.Position.Y;
                }
                else
                {
                    correctionOffsetY1 = overlapHeight - adjustY1;
                    pointY1 = rowMax + intersection0.Y + collidable0.Position.Y;
                }
            }
            else
            {
                correctionOffsetY0 = 0;
                correctionOffsetY1 = 0;
                pointY0 = rowOfMax + intersection1.Y + collidable1.Position.Y;
                pointY1 = rowOfMax + intersection0.Y + collidable0.Position.Y;

                if (rightSum0  > leftSum0)
                {
                    correctionOffsetX0 = -overlapWidth - adjustX0;
                    pointX0 = colMin + intersection1.X + collidable1.Position.X;
                }
                else
                {
                    correctionOffsetX0 = overlapWidth - adjustX0;
                    pointX0 = colMax + intersection1.X + collidable1.Position.X;
                }
                if (rightSum1  > leftSum1)
                {
                    correctionOffsetX1 = -overlapWidth - adjustX1;
                    pointX1 = colMin + intersection0.X + collidable0.Position.X;
                }
                else
                {
                    correctionOffsetX1 = overlapWidth - adjustX1;
                    pointX1 = colMax + intersection0.X + collidable0.Position.X;
                }
            }

            correction0 = new Vector2(
                x: correctionOffsetX0,
                y: correctionOffsetY0);
            point0 = new Vector2(
                x: pointX0,
                y: pointY0);
            correction1 = new Vector2(
                x: correctionOffsetX1,
                y: correctionOffsetY1);
            point1 = new Vector2(
                x: pointX1,
                y: pointY1);
            normal0 = GetNormal(otherVertices: collidable1.CollisionVertices, otherPosition: collidable1.Position, currentPoint: point0);
            normal1 = GetNormal(otherVertices: collidable0.CollisionVertices, otherPosition: collidable0.Position, currentPoint: point1);

            return true;
        }

        public static List<Vector2> GetVertices(Color[] maskData, Size size, Color startColor, Color includeColor, Color excludeColor)
        {
            List<Vector2> includeVertices = new List<Vector2>();
            List<Vector2> excludeVertices = new List<Vector2>();
            Vector2 startVertex = Vector2.Zero;
            bool startFound = false;
            for (int row = 0; row < size.Height; row++)
                for (int col = 0; col < size.Width; col++)
                {
                    Color pixelColor = maskData[col + row * size.Width];
                    if (pixelColor == startColor)
                    {
                        if (startFound)
                            throw new Exception("Found more than one starting pixel");
                        startFound = true;
                        Vector2 vertix = new Vector2(col, row);
                        startVertex = vertix;
                        includeVertices.Add(vertix);
                    }
                    else if (pixelColor == includeColor)
                    {
                        includeVertices.Add(new Vector2(col, row));
                    }
                    else if (pixelColor == excludeColor)
                    {
                        excludeVertices.Add(new Vector2(col, row));
                    }
                }
            if (!startFound)
                throw new Exception("Could not find starting pixel");
            List<Vector2> centerVertices = new List<Vector2>(includeVertices.Count + excludeVertices.Count);
            centerVertices.AddRange(includeVertices);
            centerVertices.AddRange(excludeVertices);
            Vector2 centerPoint = new Vector2(
                x: centerVertices.Select((x) => x.X).Average(), 
                y: centerVertices.Select((x) => x.Y).Average());
            double startAngle = Math.Atan2(
                y: startVertex.Y - centerPoint.Y,
                x: startVertex.X - centerPoint.X);
            var angles = includeVertices
                .Select((x) => Math.Atan2(
                    y: x.Y - centerPoint.Y,
                    x: x.X - centerPoint.X)).ToList()
                .Select((x) => x - startAngle)
                .Select((x) => WrapMinMax(x, 0, 2 * Math.PI));
            List<Vector2> verticesSorted = includeVertices.Zip(angles, (x, y) => (x, y)).OrderBy((x) => x.y).Select((x) => x.x).ToList();
            return verticesSorted;
        }

        // https://stackoverflow.com/questions/4633177/c-how-to-wrap-a-float-to-the-interval-pi-pi
        // See Tim Cas' answer.
        private static double WrapMax(double x, double max) => (max + x % max) % max;
        private static int WrapMax(int x, int max) => (max + x % max) % max;
        private static double WrapMinMax(double x, double min, double max) => min + WrapMax(x - min, max - min);
        private static Vector2 GetNormal(IList<Vector2> otherVertices, Vector2 otherPosition, Vector2 currentPoint)
        {
            if (otherVertices == null || otherVertices.Count < 3)
                return Vector2.Zero;

            Vector2 localPoint = currentPoint - otherPosition;
            List<float> distances = otherVertices.Select(v => Vector2.DistanceSquared(v, localPoint)).ToList();
            
            int minIndex = distances.IndexOf(distances.Min());
            int indexLower = WrapMax(minIndex - 1, otherVertices.Count);
            int indexHigher = WrapMax(minIndex + 1, otherVertices.Count);
            
            (Vector2 vertix, float distance, int index)[] tuples = new (Vector2 vertix, float distance, int index)[]
            {
                (otherVertices[minIndex], distances[minIndex], minIndex),
                (otherVertices[indexLower], distances[indexLower], indexLower),
                (otherVertices[indexHigher], distances[indexHigher], indexHigher)
            };

            (Vector2 vertix, int index)[] pairs = tuples
                .OrderBy(tuple => tuple.distance)
                .Take(2)
                .Select(tuple => (tuple.vertix, tuple.index))
                .OrderBy(pair => pair.index)
                .ToArray();

            Vector2 direction;
            if ((pairs[1].index - pairs[0].index) > 1)
                direction = pairs[0].vertix - pairs[1].vertix;
            else
                direction = pairs[1].vertix - pairs[0].vertix;

            Vector2 collisionNormal = Vector2.Normalize(-direction.GetPerpendicular());
            return collisionNormal;
        }
    }
}
