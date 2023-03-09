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
    internal class SnailComponent : ComponentInterface, PhysicsInterface, HasSoundInterface
    {
        private enum ComponentState { Inactive, Walk }
        const float loopTimerPeriod = 1 / 30;
        readonly private static string maskAsset = "snail/snail_mask_0";
        readonly private static string walkVisualAsset = "snail/snail_walk_visual_0.sf";
        readonly private static string deadVisualAsset = "snail/snail_dead_visual_0.sf";
        readonly private static string hideVisualAsset = "snail/snail_hide_visual_0.sf";
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private LevelInterface levelFeature;
        private PhysicsManager physicsManager;
        private TimerFeature loopTimer = new TimerFeature() { Period = loopTimerPeriod, Activated = true, Repeat = true };
        private Texture2D maskTexture;
        private AnimatorManager animatorManager;
        private AnimatorFeature walkVisualAnimation;
        private AnimatorFeature deadVisualAnimation;
        private AnimatorFeature hideVisualAnimation;
        private ComponentState componentState = ComponentState.Walk;
        public static Color Identifier { get => new Color(r: 45, g: 67, b: 226, alpha: 255); }
        public int DrawLevel { get; set; } = 0;
        public bool PhysicsApplied { get; set; } = true;
        public Vector2 Movement { get; set; } = Vector2.Zero;
        public Vector2 Gravity { get; set; } = new Vector2(x: 0, y: 1f);
        public float MaxGravspeed { get; set; } = 8;
        public bool Grounded { get; set; } = default; // managed by physics manager.
        public Vector2 Velocity { get; set; } = default; // managed by associated manager.
        public float NormalSpeed { get; set; } = default; // managed by associated manager.
        public Vector2 Position { get; set; } = default;  // managed by physics manager.
        public Size Size { get; private set; } = new Size(width: 48, height: 32);
        public bool Collidable { get; set; } = true;
        public bool Static { get; set; } = false;
        public Color[] CollisionMask { get; set; } = default; // gets defined by constructor.
        public List<Vector2> CollisionVertices { get; set; } = null; // collision vertices aren't utlized.
        public SoundManager SoundManagerObject { get; private set; }
        public SnailComponent(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            LevelInterface levelFeature)
        {
            this.contentManager = contentManager;
            this.spriteBatch = spriteBatch;
            this.levelFeature = levelFeature;
            SoundManagerObject = new SoundManager(contentManager);
            physicsManager = new PhysicsManager(this);
            {
                animatorManager = new AnimatorManager(contentManager: contentManager, spriteBatch: spriteBatch);
                walkVisualAnimation = new AnimatorFeature(walkVisualAsset) { Offset = new Vector2(x: 24, y: 16) };
                deadVisualAnimation = new AnimatorFeature(deadVisualAsset) { Offset = Vector2.Zero };
                hideVisualAnimation = new AnimatorFeature(hideVisualAsset) { Offset = Vector2.Zero };
                animatorManager.Features.Add(walkVisualAnimation);
                animatorManager.Features.Add(deadVisualAnimation);
                animatorManager.Features.Add(hideVisualAnimation);
                walkVisualAnimation.Play("walk_0");
            }
            {
                maskTexture = contentManager.Load<Texture2D>(maskAsset);
                if (maskTexture.Width != Size.Width || maskTexture.Height != Size.Height)
                    throw new Exception("The expected dimensions of the knight are incorrected.");
                var totalPixels = Size.Width * Size.Height;
                CollisionMask = new Color[totalPixels];
                maskTexture.GetData(CollisionMask);
            }
        }

        public void Draw(Matrix? transformMatrix)
        {
            spriteBatch.Begin(transformMatrix: transformMatrix);
            spriteBatch.Draw(texture: maskTexture, position: Position, color: Color.White);
            spriteBatch.End();
            animatorManager.Draw(transformMatrix: transformMatrix);
        }

        public void Update(float timeElapsed)
        {
            serviceCollisions();
            while (loopTimer.GetNext())
            {
            }

            // Update the managers and features.
            physicsManager.Update(timeElapsed);
            loopTimer.Update(timeElapsed);
            animatorManager.Update(timeElapsed);
            animatorManager.Position = Position;
        }
        private void serviceCollisions()
        {
            // Service collisions as reported by the physics manager.
            while ((this as PhysicsInterface).GetNext(out var info))
                ;
        }
        CollisionManager FeatureInterface<CollisionManager>.ManagerObject { get; set; } // managed by associated manager.
        PhysicsManager FeatureInterface<PhysicsManager>.ManagerObject { get; set; } // managed by associated manager.
    }
}
