using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Content;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SlayerKnight.Components
{
    internal class KnightComponent : ComponentInterface, PhysicsInterface, ControlInterface
    {
        const float loopTimerPeriod = 1 / 30;
        readonly private static Vector2 jumpRightOffset = new Vector2(x: 24, y: 16);
        readonly private static Vector2 jumpLeftOffset = new Vector2(x: 8, y: 16);
        readonly private static string maskAsset = "knight/knight_mask_0";
        readonly private static string idleVisualAsset = "knight/knight_idle_visual_0.sf";
        readonly private static string runVisualAsset = "knight/knight_run_visual_0.sf";
        readonly private static string jumpVisualAsset = "knight/knight_jump_visual_0.sf";
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private LevelInterface levelFeature;
        private PhysicsManager physicsManager;
        private AnimatorManager animatorManager;
        private AnimatorFeature idleVisualAnimation;
        private AnimatorFeature runVisualAnimation;
        private AnimatorFeature jumpVisualAnimation;
        private TimerFeature loopTimer = new TimerFeature() { Period = loopTimerPeriod, Activated = true, Repeat = true };
        private Texture2D maskTexture;
        private int jmpCounter = 0;
        private int lftCounter = 0;
        private int rhtCounter = 0;
        private bool facingRight = true;
        public static Color Identifier { get => new Color(r: 78, g: 111, b: 6, alpha: 255); }
        public int DrawLevel { get; set; } = 0;
        public bool PhysicsApplied { get; set; } = true;
        public Vector2 Movement { get; set; } = Vector2.Zero;
        public Vector2 Gravity { get; private set; } = new Vector2(x: 0, y: 1f);
        public float MaxGravspeed { get; private set; } = 8;
        public bool Grounded { get; set; } = default; // managed by physics manager.
        public Vector2 Position { get; set; } = default;  // managed by physics manager.
        public Size Size { get; private set; } = new Size(width: 32, height: 48);
        public bool Collidable { get; set; } = true;
        public bool Static { get; set; } = false;
        public Color[] CollisionMask { get; set; } = default; // gets defined by constructor.
        public List<Vector2> CollisionVertices { get; set; } = null; // collision vertices aren't utlized.
        public ControlFeature ControlFeatureObject { get; private set; } = new ControlFeature() { Activated = true };
        public Vector2 Velocity { get; set; } // managed by associated manager.
        CollisionManager FeatureInterface<CollisionManager>.ManagerObject { get; set; } // managed by associated manager.
        PhysicsManager FeatureInterface<PhysicsManager>.ManagerObject { get; set; } // managed by associated manager.
        public float NormalSpeed { get; set; } // managed by associated manager.

        public KnightComponent(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            LevelInterface levelFeature)
        {
            this.contentManager = contentManager;
            this.spriteBatch = spriteBatch;
            this.levelFeature = levelFeature;
            physicsManager = new PhysicsManager(this);
            animatorManager = new AnimatorManager(contentManager: contentManager, spriteBatch: spriteBatch);
            idleVisualAnimation = new AnimatorFeature(idleVisualAsset) { Offset = new Vector2(x: 16, y: 24) };
            runVisualAnimation = new AnimatorFeature(runVisualAsset) { Offset = new Vector2(x: 16, y: 24) };
            jumpVisualAnimation = new AnimatorFeature(jumpVisualAsset); // offset needs to be set dynamically.
            animatorManager.Features.Add(idleVisualAnimation);
            animatorManager.Features.Add(runVisualAnimation);
            animatorManager.Features.Add(jumpVisualAnimation);
            idleVisualAnimation.Play("idle_0");
            {
                maskTexture = contentManager.Load<Texture2D>(maskAsset);
                if (maskTexture.Width != Size.Width || maskTexture.Height != Size.Height)
                    throw new Exception("The expected dimensions of the knight are incorrected.");
                var totalPixels = Size.Width * Size.Height;
                CollisionMask = new Color[totalPixels];
                maskTexture.GetData(CollisionMask);
            }
        }

        public void Draw(Matrix? transformMatrix = null)
        {
            spriteBatch.Begin(transformMatrix: transformMatrix);
            //spriteBatch.Draw(texture: maskTexture, position: Position, color: Color.White);
            spriteBatch.End();
            animatorManager.Draw(transformMatrix: transformMatrix);
        }

        public void Update(float timeElapsed)
        {
            // Apply the camera.
            {
                int pixelsToWidthEdge = levelFeature.ScreenSize.Width / 2 - 100;
                int pixelsToHeightEdge = levelFeature.ScreenSize.Height / 2 - 100;
                var positionOnScreen = Position - levelFeature.CameraObject.Position;
                var cameraPosition = levelFeature.CameraObject.Position;
                Size lowerThresholds = new Size(width: pixelsToWidthEdge, height: pixelsToHeightEdge);
                Size upperThresholds = new Size(width: levelFeature.ScreenSize.Width - pixelsToWidthEdge, height: levelFeature.ScreenSize.Height - pixelsToHeightEdge);

                // Ensure the camera centers around the knight, but with some wiggle room such that
                //   the camera only moves when the knight gets closer to the edge.
                {
                    if (positionOnScreen.X < lowerThresholds.Width)
                        cameraPosition.X -= lowerThresholds.Width - positionOnScreen.X;

                    if (positionOnScreen.X > upperThresholds.Width)
                        cameraPosition.X += positionOnScreen.X - upperThresholds.Width;

                    if (positionOnScreen.Y < lowerThresholds.Height)
                        cameraPosition.Y -= lowerThresholds.Height - positionOnScreen.Y;

                    if (positionOnScreen.Y > upperThresholds.Height)
                        cameraPosition.Y += positionOnScreen.Y - upperThresholds.Height;
                }

                // Ensure the camera never goes off the edge of the level itself.
                {
                    cameraPosition.X = Math.Clamp(cameraPosition.X, 0, levelFeature.LevelSize.Width - levelFeature.ScreenSize.Width - 1);
                    cameraPosition.Y = Math.Clamp(cameraPosition.Y, 0, levelFeature.LevelSize.Height - levelFeature.ScreenSize.Height - 1);
                }

                // Also make sure the camera's position is always an integer to prevent weird interpolation when 
                //   the screen is drawn.
                {
                    cameraPosition.X = (float)Math.Floor(cameraPosition.X);
                    cameraPosition.Y = (float)Math.Floor(cameraPosition.Y);
                }

                // Set the camera position.
                levelFeature.CameraObject.Position = cameraPosition;
            }

            // Service collisions as reported by the physics manager.
            while ((this as PhysicsInterface).GetNext(out var info))
                ;

            // Service user input.
            Console.WriteLine($"Jump Counter: {jmpCounter}");
            while (ControlFeatureObject.GetNext(out var info))
            {
                switch (info.Action)
                {
                    case ControlAction.Jump:
                        if (info.State == ControlState.Pressed && Grounded)
                            jmpCounter = 15;
                        else if (info.State == ControlState.Released)
                            jmpCounter = Math.Max(jmpCounter - 10, 0);
                        break;
                    case ControlAction.MoveLeft:
                        if (info.State == ControlState.Pressed || info.State == ControlState.Held)
                            lftCounter = 1;
                        break;
                    case ControlAction.MoveRight:
                        if (info.State == ControlState.Pressed || info.State == ControlState.Held)
                            rhtCounter = 1;
                        break;
                    default:
                        break;
                }
            }

            // Service main loop.
            while (loopTimer.GetNext())
            {
                // Apply movement.
                {
                    float jmpAmount = 0;
                    float lftAmount = 0;
                    float rhtAmount = 0;
                    float horAmount = 0;

                    if (jmpCounter > 0)
                    {
                        jmpAmount = 13f;
                        jmpCounter--;
                    }
                    if (lftCounter > 0)
                    {
                        lftAmount = 4;
                        lftCounter--;
                    }
                    if (rhtCounter > 0)
                    {
                        
                        rhtAmount = 4;
                        rhtCounter--;
                    }
                    horAmount = rhtAmount - lftAmount;
                    Movement = new Vector2(x: horAmount, y: -jmpAmount);

                    // Determine facing direction
                    if (horAmount > 0)
                        facingRight = true;
                    else if (horAmount < 0)
                        facingRight = false;
                    

                    // Handle animations.
                    {
                        if (Grounded)
                        {
                            //Console.WriteLine("GROUNDED");
                            if (animatorManager.CurrentFeature != jumpVisualAnimation && 
                                animatorManager.CurrentSpriteSheetAnimation.Name != "start_0" && 
                                jmpAmount > 0)
                            {
                                jumpVisualAnimation.Play(animation: "start_0").Rewind();
                            }
                            
                            if (animatorManager.CurrentFeature == jumpVisualAnimation &&
                                animatorManager.CurrentSpriteSheetAnimation.Name != "end_0" &&
                                animatorManager.CurrentSpriteSheetAnimation.IsComplete &&
                                jmpAmount == 0)
                            {
                                jumpVisualAnimation.Play(animation: "end_0").Rewind();
                            }

                            if ((animatorManager.CurrentFeature != jumpVisualAnimation) || 
                                (animatorManager.CurrentSpriteSheetAnimation.Name == "end_0" && 
                                 animatorManager.CurrentSpriteSheetAnimation.IsComplete))
                            {
                                if (horAmount == 0)
                                    idleVisualAnimation.Play(animation: "idle_0");
                                else
                                    runVisualAnimation.Play(animation: "run_0");
                            }
                        }
                        else
                        {

                            if (animatorManager.CurrentFeature != jumpVisualAnimation || animatorManager.CurrentSpriteSheetAnimation.Name != "start_0" || animatorManager.CurrentSpriteSheetAnimation.IsComplete)
                            {
                                jumpVisualAnimation.Play(animation: "up_0");
                            }
                        }

                        if (facingRight)
                        {
                            animatorManager.CurrentSprite.Effect = SpriteEffects.None;
                            if (animatorManager.CurrentFeature == jumpVisualAnimation)
                                animatorManager.CurrentOffset = jumpRightOffset;
                        }
                        else
                        {
                            animatorManager.CurrentSprite.Effect = SpriteEffects.FlipHorizontally;
                            if (animatorManager.CurrentFeature == jumpVisualAnimation)
                                animatorManager.CurrentOffset = jumpLeftOffset;
                        }
                    }  
                }
            }

            // Update the managers and features.
            physicsManager.Update(timeElapsed);
            loopTimer.Update(timeElapsed);
            animatorManager.Update(timeElapsed);
            animatorManager.Position = Position;
        }
    }
}
