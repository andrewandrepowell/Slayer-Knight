using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace SlayerKnight.Components
{
    internal class RockComponent : ComponentInterface, PhysicsInterface, DestroyInterface
    {
        readonly private static string maskAsset = "rock/rock_mask_0";
        readonly private static string rockVisualAsset = "kingdom/kingdom_tileset_visual_asset_2.sf";
        readonly private static Random random = new Random();
        private PhysicsManager physicsManager;
        private Texture2D maskTexture;
        private AnimatorManager animatorManager;
        private AnimatorFeature rockVisualAnimation;
        private TimerFeature loopTimer = new TimerFeature() { Period = 1 / 30, Activated = true, Repeat = true };
        private int destroyCounter = 30 * 5;
        public int DrawLevel => 1;
        public Vector2 Position { get; set; }
        public Vector2 Center => Position + new Vector2(x: Size.Width / 2, y: Size.Height / 2);
        public Size Size => new Size(width: 16, height: 16);
        public bool Collidable { get; set; } = true;
        public bool Static => false;
        public bool PhysicsStatic => true;
        public Color[] CollisionMask { get; private set; }
        public List<Vector2> CollisionVertices => null;
        public bool Destroyed { get; private set; } = false;
        public bool PhysicsApplied { get; private set; } = true;
        public bool IsMob => false;
        public Vector2 Movement { get; set; } = default;
        public Vector2 Gravity { get; set; } = default;
        public float MaxGravspeed { get; set; } = 8;
        public bool Grounded { get; set; }
        public bool Walled { get; set; }
        public Vector2 Velocity { get; set; }
        public float NormalSpeed { get; set; }
        public void Draw(Matrix? transformMatrix = null) => animatorManager.Draw(transformMatrix: transformMatrix);
        public void Update(float timeElapsed)
        {
            serviceDestroy();
            while (loopTimer.GetNext())
                serviceCounters();
            loopTimer.Update(timeElapsed);
            physicsManager.Update(timeElapsed);
            animatorManager.Update(timeElapsed);
            animatorManager.Position = Position;
        }
        public RockComponent(
            ContentManager contentManager,
            SpriteBatch spriteBatch)
        {
            physicsManager = new PhysicsManager(this);
            {
                maskTexture = contentManager.Load<Texture2D>(maskAsset);
                if (maskTexture.Width != Size.Width || maskTexture.Height != Size.Height)
                    throw new Exception("The expected dimensions of the snail are incorrected.");
                var totalPixels = Size.Width * Size.Height;
                CollisionMask = new Color[totalPixels];
                maskTexture.GetData(CollisionMask);
            }
            {
                animatorManager = new AnimatorManager(contentManager: contentManager, spriteBatch: spriteBatch);
                rockVisualAnimation = new AnimatorFeature(rockVisualAsset) { Offset = new Vector2(x: 8, y: 8) };
                animatorManager.Features.Add(rockVisualAnimation);
                rockVisualAnimation.Play($"rock_{random.Next(5)}");
            }
        }
        public void Destroy()
        {
            loopTimer.Activated = false;
            PhysicsApplied = false;
            Destroyed = true;
        }
        private void serviceDestroy()
        {
            if (Destroyed)
                return;

            if (Grounded || Walled || destroyCounter == 0)
                Destroy();
        }
        private void serviceCounters()
        {
            if (Destroyed)
                return;

            if (destroyCounter > 0)
                destroyCounter--;
        }
        CollisionManager FeatureInterface<CollisionManager>.ManagerObject { get; set; }
        PhysicsManager FeatureInterface<PhysicsManager>.ManagerObject { get; set; }
    }
}
