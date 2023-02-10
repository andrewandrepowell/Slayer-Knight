using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using SlayerKnight;
using Utility;

namespace SlayerKnight.Components
{
    internal class TestComponentFeature : ComponentInterface, CollisionInterface, ControlInterface
    {
        const string testComponentMaskAsset = "test/test_component_mask_asset";
        const float loopTimerPeriod = 1 / 30;
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private Texture2D testComponentMaskTexture;
        private TimerFeature loopTimerFeature;
        private List<Vector2> correctionVectors;
        public static Color Identifier { get => new Color(r: 112, g: 146, b: 190, alpha: 255); }
        public Vector2 Position { get; set; }
        public Size Size { get; private set; }
        public bool Collidable { get; set; }
        public bool Static { get; set; }
        public Color[] CollisionMask { get; private set; }
        public List<Vector2> CollisionVertices => null;
        public ChannelInterface<CollisionInfo> CollisionInfoChannel { get; private set; }
        public bool Destroyed { get; private set; }
        public ChannelInterface<object> DestroyChannel { get; private set; }
        public ControlFeature ControlFeatureObject { get; private set; }
        public int DrawLevel { get => 0; }
        public TestComponentFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch)
        {
            this.contentManager = contentManager;
            this.spriteBatch = spriteBatch;
            testComponentMaskTexture = contentManager.Load<Texture2D>(testComponentMaskAsset);
            Position = Vector2.Zero;
            Size = new Size(width: testComponentMaskTexture.Width, height: testComponentMaskTexture.Height);
            Collidable = true;
            Static = false;
            CollisionMask = new Color[testComponentMaskTexture.Width * testComponentMaskTexture.Height];
            testComponentMaskTexture.GetData(CollisionMask);
            CollisionInfoChannel = new Channel<CollisionInfo>(capacity: 10);
            Destroyed = false;
            DestroyChannel = new Channel<object>();
            ControlFeatureObject = new ControlFeature() { Activated = true };
            loopTimerFeature = new TimerFeature() { Activated = true, Repeat = true, Period = loopTimerPeriod };
            correctionVectors = new List<Vector2>();
        }
        public void Draw(Matrix? transformMatrix = null)
        {
            if (Destroyed)
                return;

            spriteBatch.Begin(transformMatrix: transformMatrix);
            spriteBatch.Draw(texture: testComponentMaskTexture, position: Position, color: Color.White);
            spriteBatch.End();
        }
        public void Update(float timeElapsed)
        {
            if (Destroyed)
                return;

            while (ControlFeatureObject.InfoChannel.Count > 4)
                ControlFeatureObject.InfoChannel.Dequeue();

            if (loopTimerFeature.RunChannel.Count > 0)
            {
                loopTimerFeature.RunChannel.Dequeue();
                float xMove = 0, yMove = 0;
                while (ControlFeatureObject.InfoChannel.Count > 0)
                {
                    var info = ControlFeatureObject.InfoChannel.Dequeue();
                    switch (info.Action)
                    {
                        case ControlAction.MoveUp:
                            yMove -= 4;
                            break;
                        case ControlAction.MoveDown:
                            yMove += 4;
                            break;
                        case ControlAction.MoveLeft:
                            xMove -= 4;
                            break;
                        case ControlAction.MoveRight:
                            xMove += 4;
                            break;
                    }
                }
                xMove = Math.Clamp(xMove, -4, 4);
                yMove = Math.Clamp(yMove, -4, 4);
                Position += new Vector2(x: xMove, y: yMove);
            }

            {
                correctionVectors.Clear();
                while (CollisionInfoChannel.Count > 0)
                {
                    var info = CollisionInfoChannel.Dequeue();
                    correctionVectors.Add(info.Correction);
                }
                if (correctionVectors.Count > 0)
                {
                    Position += new Vector2(
                        x: correctionVectors.Select(v => v.X).Average(),
                        y: correctionVectors.Select(v => v.Y).Average()); ;
                }
            }

            if (DestroyChannel.Count > 0)
            {
                DestroyChannel.Dequeue();
                contentManager.UnloadAsset(testComponentMaskAsset);
                Destroyed = true;
            }

            loopTimerFeature.Update(timeElapsed);
        }
    }
}
