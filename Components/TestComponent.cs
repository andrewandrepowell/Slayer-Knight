using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    internal class TestComponent : ComponentInterface, PhysicsInterface, ControlInterface
    {
        const string testComponentMaskAsset = "test/test_component_mask_asset_0";
        const float loopTimerPeriod = 1 / 30;
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private Texture2D testComponentMaskTexture;
        private TimerFeature loopTimerFeature;
        private LevelInterface levelFeature;
        private PhysicsInfo? prevPhysicsInfo;
        private PhysicsManager physicsManager;
        public static Color Identifier { get => new Color(r: 112, g: 146, b: 190, alpha: 255); }
        CollisionManager FeatureInterface<CollisionManager>.ManagerObject { get; set; }
        public Vector2 Position { get; set; }
        public Size Size { get; private set; }
        public bool Collidable { get; set; }
        public bool Static { get; set; }
        public Color[] CollisionMask { get; private set; }
        public List<Vector2> CollisionVertices => null;
        public ControlFeature ControlFeatureObject { get; private set; }
        public int DrawLevel { get => 0; }
        public bool PhysicsApplied { get; set; }
        public Vector2 Movement { get; set; }
        public Vector2 Gravity { get; set; }
        public float MaxGravspeed { get; set; }
        public bool Grounded { get; set; }
        public Vector2 Velocity { get; set; }
        public float NormalSpeed { get; set; }
        PhysicsManager FeatureInterface<PhysicsManager>.ManagerObject { get; set; }
        public TestComponent(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            LevelInterface levelFeature)
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
            ControlFeatureObject = new ControlFeature() { Activated = true };
            loopTimerFeature = new TimerFeature() { Activated = true, Repeat = true, Period = loopTimerPeriod };
            this.levelFeature = levelFeature;
            prevPhysicsInfo = null;
            PhysicsApplied = true;
            Movement = Vector2.Zero;
            Gravity = new Vector2(x: 0, y: 1);
            MaxGravspeed = 8;
            physicsManager = new PhysicsManager(this);
        }
        public void Draw(Matrix? transformMatrix = null)
        {
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

            {
                var screenBounds = spriteBatch.GraphicsDevice.Viewport.Bounds;
                levelFeature.CameraObject.Position = Position - screenBounds.Center.ToVector2();
            }

            {
                float xMove = 0, yMove = 0; bool changeRooms = false;
                while (ControlFeatureObject.GetNext(out var info))
                {
                    switch (info.Action)
                    {
                        case ControlAction.MoveUp:
                            yMove -= 20;
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
                yMove = Math.Clamp(yMove, -20, 4);

                if (loopTimerFeature.GetNext())
                {
                    Movement = new Vector2(x: xMove, y: yMove);

                    if (changeRooms)
                    {
                        if (levelFeature.Identifier == "first_level")
                        {
                            levelFeature.GoTo("second_level");
                        }
                        else if (levelFeature.Identifier == "second_level")
                        {
                            levelFeature.GoTo("first_level");
                        }
                    }
                }
            }

            while (this.GetNext(out var info))
                continue;

            loopTimerFeature.Update(timeElapsed);
            physicsManager.Update(timeElapsed);
        }
    }
}
