using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SlayerKnight.Components
{
    internal class InteractiveComponentFeature : ComponentInterface, CollisionInterface
    {
        const string maskAsset = "general/interactive_mask_asset_0";
        public static Color Identifier { get => new Color(r: 70, g: 150, b: 50, alpha: 255); }
        public int DrawLevel { get => 0; }
        public Vector2 Position { get; private set; }
        public Size Size { get; private set; }
        public bool Collidable { get => true; }
        public bool Static { get => true; }
        public Color[] CollisionMask { get; private set; }
        public List<Vector2> CollisionVertices { get => null; }
        public ChannelInterface<CollisionInfo> CollisionInfoChannel { get; private set; }
        public InteractiveComponentFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch)
        {
        }
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
