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
    internal class WallComponentFeature : ComponentInterface, CollisionInterface
    {
        private Vector2 position;
        public WallComponentFeature(
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
