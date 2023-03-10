using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SlayerKnight.Components
{
    internal interface WallInterface : CollisionInterface
    {
    }
    internal static class WallExtensioins
    {
        public static bool IsBetween(this WallInterface wall, Vector2 p0, Vector2 p1)
        {
            var v0 = p1 - p0;
            var wp = wall.Position - p0;
            var cp = wp + new Vector2(x: wall.Size.Width / 2, y: wall.Size.Height / 2);
            if (cp.LengthSquared() > v0.LengthSquared())
                return false;
            var wtl = wp;
            var wtr = wp + wall.Size.Width * Vector2.UnitX;
            var wbl = wp + wall.Size.Height * Vector2.UnitY;
            var wbr = new Vector2(x: wp.X + wall.Size.Width, y: wp.Y + wall.Size.Height);
            var wvps = new (Vector2, Vector2)[] 
            {
                (wtl, wtr),
                (wtr, wbr),
                (wbr, wbl),
                (wbl, wtl)
            };
            foreach ((var wvp0, var wvp1) in wvps)
                if (v0.IsBetweenTwoVectors(wvp0, wvp1))
                    return true;
            return false;
        }
    }
    internal class WallComponent : ComponentInterface, WallInterface
    {
        private Vector2 position;
        public WallComponent(
            Vector2 position,
            Size size,
            Color[] mask,
            List<Vector2> vertices)
        {
            this.position = position;
            Size = size;
            CollisionMask = mask;
            CollisionVertices = vertices;
        }
        CollisionManager FeatureInterface<CollisionManager>.ManagerObject { get; set; }
        public Vector2 Position { get => position; set => throw new NotImplementedException(); }
        public Size Size { get; private set; }
        public bool Collidable { get => true; set => throw new NotImplementedException(); }
        public bool Static { get => true; set => throw new NotImplementedException(); }
        public Color[] CollisionMask { get; private set; }
        public List<Vector2> CollisionVertices { get; private set; }
        public int DrawLevel { get => 0; }
        public void Update(float timeElapsed)
        {
        }
        public void Draw(Matrix? transformMatrix = null)
        {
        }
    }
}
