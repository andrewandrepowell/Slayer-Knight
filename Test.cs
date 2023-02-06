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

namespace SlayerKnight
{
    internal enum TestState { Start, End };
    internal class TestFeature : UpdateInterface, DrawInterface, ControlInterface, CollisionInterface
    {
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private string maskAsset;
        private Texture2D maskTexture;
        private TimerFeature controlTimer;
        private const float controlPeriod = 0.25f;
        public bool Started { get; private set; }
        public Vector2 Position { get; set; }
        public Size Size { get; private set; }
        public ControlFeature ControlFeatureObject { get; private set; }
        public CollisionFeature CollisionFeatureObject { get; private set; }
        public Queue<TestState> StateQueue { get; private set; } // user -> feature
        public TestFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            string maskAsset)
        {
            Started = false;
            ControlFeatureObject = new ControlFeature() { Parent = this };
            controlTimer = new TimerFeature() { Parent = this, Period = controlPeriod, Repeat = true };
            Position = Vector2.Zero;
            StateQueue = new Queue<TestState>();

            maskTexture = contentManager.Load<Texture2D>(maskAsset);
            Color[] maskColor = new Color[maskTexture.Width * maskTexture.Height];
            maskTexture.GetData(maskColor);
            CollisionFeatureObject = new CollisionFeature() { Parent = this, CollisionMask = maskColor, Static = false };
            Size = new Size(width: maskTexture.Width, height: maskTexture.Height);
            contentManager.UnloadAsset(maskAsset);

            this.contentManager = contentManager;
            this.spriteBatch = spriteBatch;
            this.maskAsset = maskAsset;
        }
        private void start()
        {
            if (Started)
                throw new Exception("Should not already be started.");
            maskTexture = contentManager.Load<Texture2D>(maskAsset);
            CollisionFeatureObject.Collidable = true;
            ControlFeatureObject.Activated = true;
            controlTimer.Activated = true;
            Started = true;
        }
        private void end()
        {
            if (!Started)
                throw new Exception("Should be started.");
            contentManager.UnloadAsset(maskAsset);
            CollisionFeatureObject.Collidable = false;
            ControlFeatureObject.Activated = false;
            controlTimer.Activated = false;
            Started = false;
        }
        public void Draw(Matrix? transformMatrix = null)
        {
            if (Started)
            {
                spriteBatch.Begin(transformMatrix: transformMatrix);
                spriteBatch.Draw(texture: maskTexture, position: Position, color: Color.White);
                spriteBatch.End();
            }
        }

        public void Update(float timeElapsed)
        {
            if (StateQueue.Count > 0)
            {
                var state = StateQueue.Dequeue();
                switch (state)
                {
                    case TestState.Start:
                        start();
                        break;
                    case TestState.End:
                        end();
                        break;
                }
            }

            if (Started)
            {
                if (ControlFeatureObject.InfoQueue.Count > 0 && controlTimer.RunQueue.Count > 0)
                {
                    controlTimer.RunQueue.Dequeue();
                    float xMove = 0;
                    float yMove = 0;
                    while (ControlFeatureObject.InfoQueue.Count > 0)
                    {
                        var info = ControlFeatureObject.InfoQueue.Dequeue();
                        switch (info.Action)
                        {
                            case ControlAction.MoveUp:
                                yMove = -1;
                                break;
                            case ControlAction.MoveDown:
                                yMove = +1;
                                break;
                            case ControlAction.MoveLeft:
                                xMove = -1;
                                break;
                            case ControlAction.MoveRight:
                                xMove = +1;
                                break;
                        }
                    }
                    Position += new Vector2(x: xMove, y: yMove);
                }

                if (CollisionFeatureObject.InfoQueue.Count > 0)
                {
                    var info = CollisionFeatureObject.InfoQueue.Dequeue();
                    Position += info.Correction;
                }
            }
        }
    }
}
