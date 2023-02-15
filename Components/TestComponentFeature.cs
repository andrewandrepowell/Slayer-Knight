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
    internal class TestComponentFeature : ComponentInterface, PhysicsInterface, ControlInterface
    {
        const string testComponentMaskAsset = "test/test_component_mask_asset_0";
        const float loopTimerPeriod = 1 / 30;
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private Texture2D testComponentMaskTexture;
        private TimerFeature loopTimerFeature;
        private List<Vector2> correctionVectors;
        private OutputInterface<string> goToOutput;
        private string roomIdentifier;
        private PhysicsInfo? prevPhysicsInfo;
        private PhysicsManager physicsManager;
        public static Color Identifier { get => new Color(r: 112, g: 146, b: 190, alpha: 255); }
        CollisionManager DirectlyManagedInterface<CollisionManager>.ManagerObject { get; set; }
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
        public bool PhysicsApplied { get; set; }
        public Vector2 Movement { get; set; }
        public Vector2 Gravity { get; set; }
        public ChannelInterface<PhysicsInfo> PhysicsInfoChannel { get; private set; }
        public float MaxGravspeed { get; set; }
        public bool Grounded { get; set; }

        public TestComponentFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            string roomIdentifier,
            OutputInterface<string> goToOutput)
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
            this.goToOutput = goToOutput;
            this.roomIdentifier = roomIdentifier;
            prevPhysicsInfo = null;
            PhysicsApplied = true;
            Movement = Vector2.Zero;
            Gravity = new Vector2(x: 0, y: .5f);
            PhysicsInfoChannel = new Channel<PhysicsInfo>(capacity: 10);
            MaxGravspeed = 8;
            physicsManager = new PhysicsManager(this);
        }
        public void Draw(Matrix? transformMatrix = null)
        {
            if (Destroyed)
                return;

            spriteBatch.Begin(transformMatrix: transformMatrix);
            spriteBatch.Draw(texture: testComponentMaskTexture, position: Position, color: Color.White);
            if (prevPhysicsInfo != null)
            {
                spriteBatch.DrawPoint(position: prevPhysicsInfo.Value.Point, color: Color.Red, size: 6);
                spriteBatch.DrawLine(
                    point1: prevPhysicsInfo.Value.Point,
                    point2: prevPhysicsInfo.Value.Point + 64 * prevPhysicsInfo.Value.Normal,
                    color: Color.Blue, thickness: 4);
            }
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
                float xMove = 0, yMove = 0; bool changeRooms = false;
                while (ControlFeatureObject.InfoChannel.Count > 0)
                {
                    var info = ControlFeatureObject.InfoChannel.Dequeue();
                    switch (info.Action)
                    {
                        case ControlAction.MoveUp:
                            yMove -= 16;
                            break;
                        case ControlAction.MoveDown:
                            yMove += 4;
                            break;
                        case ControlAction.MoveLeft:
                            xMove -= 6;
                            break;
                        case ControlAction.MoveRight:
                            xMove += 6;
                            break;
                        case ControlAction.Jump:
                            if (info.State == ControlState.Released)
                                changeRooms = true;
                            break;
                    }
                }
                xMove = Math.Clamp(xMove, -6, 6);
                yMove = Math.Clamp(yMove, -16, 4);
                Movement = new Vector2(x: xMove, y: yMove);

                if (changeRooms)
                {
                    if (roomIdentifier == "first_level")
                    {
                        goToOutput.Enqueue("second_level");
                    }
                    else if (roomIdentifier == "second_level")
                    {
                        goToOutput.Enqueue("first_level");
                    }
                }
            }

            while (PhysicsInfoChannel.Count > 0)
            {
                PhysicsInfoChannel.Dequeue();
            }

            if (DestroyChannel.Count > 0)
            {
                DestroyChannel.Dequeue();
                contentManager.UnloadAsset(testComponentMaskAsset);
                Destroyed = true;
            }

            loopTimerFeature.Update(timeElapsed);
            physicsManager.Update(timeElapsed);
        }
    }
}
