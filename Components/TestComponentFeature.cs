using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using SlayerKnight;
using Utility;

namespace SlayerKnight.Components
{
    internal class TestComponentFeature : UpdateInterface, DrawInterface, CollisionInterface, StartInterface
    {
        public Vector2 Position => throw new NotImplementedException();

        public Size Size => throw new NotImplementedException();

        public bool Collidable => throw new NotImplementedException();

        public bool Static => throw new NotImplementedException();

        public Color[] CollisionMask => throw new NotImplementedException();

        public List<Vector2> CollisionVertices => throw new NotImplementedException();

        public Channel<CollisionInfo> CollisionInfoChannel => throw new NotImplementedException();

        public bool Destroyed => throw new NotImplementedException();

        public Channel<object> DestroyChannel => throw new NotImplementedException();

        public bool Started => throw new NotImplementedException();

        public Channel<StartAction> StartChannel => throw new NotImplementedException();

        public void Draw(Matrix? transformMatrix = null)
        {
            throw new NotImplementedException();
        }
        public void Update(float timeElapsed)
        {
            throw new NotImplementedException();
        }
    }
}
