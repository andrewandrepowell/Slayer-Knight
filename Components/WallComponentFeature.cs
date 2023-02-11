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
        public WallComponentFeature(
            Vector2 position,
            Size size,
            Color[] mask,
            List<Vector2> vertices)
        {
            Position = position;
            Size = size;
            CollisionMask = mask;
            CollisionVertices = vertices;
        }
        CollisionManager DirectlyManagedInterface<CollisionManager>.ManagerObject { get; set; }
        public Vector2 Position { get; private set; }
        public Size Size { get; private set; }
        public bool Collidable { get => true; }
        public bool Static { get => true; }
        public Color[] CollisionMask { get; private set; }
        public List<Vector2> CollisionVertices { get; private set; }
        public ChannelInterface<CollisionInfo> CollisionInfoChannel { get => throw new NotImplementedException(); }
        public int DrawLevel { get => 0; }
        public void Update(float timeElapsed)
        {
        }
        public void Draw(Matrix? transformMatrix = null)
        {
        }
    }
}
