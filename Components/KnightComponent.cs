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
    internal class KnightComponent : ComponentInterface, PhysicsInterface, ControlInterface, DestroyInterface
    {
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private LevelInterface LevelFeature;
        private PhysicsManager physicsManager;
        public static Color Identifier { get => new Color(r: 78, g: 111, b: 6, alpha: 255); }
        public int DrawLevel { get; set; } = 0;
        public bool PhysicsApplied { get; set; } = true;
        public Vector2 Movement { get; set; } = Vector2.Zero;
        public Vector2 Gravity { get; private set; } = new Vector2(x: 0, y: .5f);
        public float MaxGravspeed { get; private set; } = 8;
        public bool Grounded { get; set; } = default; // managed by physics manager.
        public Vector2 Position { get; set; } = default;  // managed by physics manager.
        public Size Size { get; private set; } = default; // gets defined by constructor.
        public bool Collidable { get; set; } = true;
        public bool Static { get; set; } = false;
        public Color[] CollisionMask { get; set; } = default; // gets defined by constructor.
        public List<Vector2> CollisionVertices { get; set; } = null; // collision vertices aren't utlized.
        public ControlFeature ControlFeatureObject { get; private set; } = new ControlFeature();
        public bool Destroyed { get; private set; } = false;
        CollisionManager FeatureInterface<CollisionManager>.ManagerObject { get; set; } // managed by associated manager.
        PhysicsManager FeatureInterface<PhysicsManager>.ManagerObject { get; set; } // managed by associated manager.
        public KnightComponent(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            LevelInterface levelFeature)
        {
            this.contentManager = contentManager;
            this.spriteBatch = spriteBatch;
            this.LevelFeature = levelFeature;
            physicsManager = new PhysicsManager(this);
        }
        public void Destroy()
        {
            if (Destroyed)
                throw new Exception("Already destroyed.");
            //
            Destroyed = true;
        }

        public void Draw(Matrix? transformMatrix = null)
        {
            if (Destroyed)
                return;
        }

        public void Update(float timeElapsed)
        {
            if (Destroyed)
                return;

            // Service collisions as reported by the physics manager.
            while ((this as PhysicsInterface).GetNext(out var info))
                ;

            // Service user input.
            while (ControlFeatureObject.GetNext(out var info))
            {
                switch (info.Action)
                {
                    case ControlAction.Jump:
                        break;
                    case ControlAction.MoveLeft:
                        break;
                    case ControlAction.MoveRight:
                        break;
                    default:
                        break;
                }
            }

            // Update the managers. managers.
            physicsManager.Update(timeElapsed);
        }
    }
}
