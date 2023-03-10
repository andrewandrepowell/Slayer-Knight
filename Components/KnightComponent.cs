using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
    internal class KnightComponent : ComponentInterface, PhysicsInterface, HasControlInterface, HasSoundInterface
    {
        const float loopTimerPeriod = 1 / 30;
        readonly private static Vector2 jumpRightOffset = new Vector2(x: 24, y: 16);
        readonly private static Vector2 jumpLeftOffset = new Vector2(x: 8, y: 16);
        readonly private static string maskAsset = "knight/knight_mask_0";
        readonly private static string idleVisualAsset = "knight/knight_idle_visual_0.sf";
        readonly private static string runVisualAsset = "knight/knight_run_visual_0.sf";
        readonly private static string jumpVisualAsset = "knight/knight_jump_visual_0.sf";
        readonly private static string attackVisualAsset = "knight/knight_attack_visual_0.sf";
        readonly private static string runSoundAsset = "knight/knight_run_sound_0";
        readonly private static string jumpSoundAsset = "knight/knight_jump_sound_0";
        readonly private static string landSoundAsset = "knight/knight_jump_sound_1";
        readonly private static string attackSoundAsset = "knight/knight_attack_sound_0";
        readonly private static string dashSoundAsset = "knight/knight_dash_sound_0";
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private LevelInterface levelFeature;
        private PhysicsManager physicsManager;
        private AnimatorManager animatorManager;
        private AnimatorFeature idleVisualAnimation;
        private AnimatorFeature runVisualAnimation;
        private AnimatorFeature jumpVisualAnimation;
        private AnimatorFeature attackVisualAnimation;
        private SoundFeature runSoundSound;
        private SoundFeature jumpSoundSound;
        private SoundFeature landSoundSound;
        private SoundFeature attackSoundSound;
        private SoundFeature dashSoundSound;
        private TimerFeature loopTimer = new TimerFeature() { Period = loopTimerPeriod, Activated = true, Repeat = true };
        private Texture2D maskTexture;
        private int jumpCounter = 0;
        private int leftCounter = 0;
        private int rightCounter = 0;
        private int dashActiveCounter = 0;
        private int dashCooldownCounter = 0;
        private int invulnerableCounter = 0;
        private bool attackActive = false;
        private bool attackNextCombo = false;
        private bool facingRight = true;
        private bool dashTouchedGround = true;
        public static Color Identifier { get => new Color(r: 78, g: 111, b: 6, alpha: 255); }
        public int DrawLevel { get; set; } = 0;
        public bool PhysicsApplied { get; set; } = true;
        public bool PhysicsStatic { get; private set; } = false;
        public bool IsMob => false;
        public Vector2 Movement { get; set; } = Vector2.Zero;
        public Vector2 Gravity { get; set; } = new Vector2(x: 0, y: 1f);
        public float MaxGravspeed { get; set; } = 8;
        public bool Grounded { get; set; } = default; // managed by physics manager.
        public bool Walled { get; set; } = default; // managed by physics manager.
        public Vector2 Position { get; set; } = default;  // managed by physics manager.
        public Vector2 Center => Position + new Vector2(x: Size.Width / 2, y: Size.Height / 2);
        public Size Size { get; private set; } = new Size(width: 32, height: 48);
        public bool Collidable { get; set; } = true;
        public bool Static { get; set; } = false;
        public Color[] CollisionMask { get; set; } = default; // gets defined by constructor.
        public List<Vector2> CollisionVertices { get; set; } = null; // collision vertices aren't utlized.
        public ControlFeature ControlFeatureObject { get; private set; } = new ControlFeature() { Activated = true };
        public SoundManager SoundManagerObject { get; private set; }
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
            {
                animatorManager = new AnimatorManager(contentManager: contentManager, spriteBatch: spriteBatch);
                idleVisualAnimation = new AnimatorFeature(idleVisualAsset) { Offset = new Vector2(x: 16, y: 24) };
                runVisualAnimation = new AnimatorFeature(runVisualAsset) { Offset = new Vector2(x: 16, y: 24) };
                jumpVisualAnimation = new AnimatorFeature(jumpVisualAsset); // offset needs to be set dynamically.
                attackVisualAnimation = new AnimatorFeature(attackVisualAsset) { Offset = new Vector2(x: 16, y: 24) };
                animatorManager.Features.Add(idleVisualAnimation);
                animatorManager.Features.Add(runVisualAnimation);
                animatorManager.Features.Add(jumpVisualAnimation);
                animatorManager.Features.Add(attackVisualAnimation);
                idleVisualAnimation.Play("idle_0");
            }
            {
                maskTexture = contentManager.Load<Texture2D>(maskAsset);
                if (maskTexture.Width != Size.Width || maskTexture.Height != Size.Height)
                    throw new Exception("The expected dimensions of the knight are incorrected.");
                var totalPixels = Size.Width * Size.Height;
                CollisionMask = new Color[totalPixels];
                maskTexture.GetData(CollisionMask);
            }
            {
                SoundManagerObject = new SoundManager(contentManager);
                runSoundSound = new SoundFeature(runSoundAsset) { Volume = 0.0075f * 20, IsLooped = true };
                jumpSoundSound = new SoundFeature(jumpSoundAsset) { Volume = 0.01f * 20 };
                landSoundSound = new SoundFeature(landSoundAsset) { Volume = 0.0060f * 20 };
                attackSoundSound = new SoundFeature(attackSoundAsset) { Volume = 0.01f * 20 };
                dashSoundSound = new SoundFeature(dashSoundAsset) { Volume = 0.01f * 20, IsLooped = true };
                SoundManagerObject.Features.Add(runSoundSound);
                SoundManagerObject.Features.Add(jumpSoundSound);
                SoundManagerObject.Features.Add(landSoundSound);
                SoundManagerObject.Features.Add(attackSoundSound);
                SoundManagerObject.Features.Add(dashSoundSound);
            }
        }

        private void serviceCamera()
        {
            // Apply the camera.
            // Might be a good idea to move the cemera stuff into the timer loop, since it's
            //   not really necessary to update the camera as soon as possible.
            {
                int pixelsToWidthEdge = levelFeature.ScreenSize.Width / 2 - 100;
                int pixelsToHeightEdge = levelFeature.ScreenSize.Height / 2 - 25;
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
        }

        private void serviceControls()
        {
            // Service user input.
            while (ControlFeatureObject.GetNext(out var info))
            {
                switch (info.Action)
                {
                    case ControlAction.Jump:
                        if (info.State == ControlState.Pressed && Grounded)
                            jumpCounter = 15;
                        else if (info.State == ControlState.Released)
                            jumpCounter = Math.Max(jumpCounter - 10, 0);
                        break;
                    case ControlAction.MoveLeft:
                        if (info.State == ControlState.Pressed || info.State == ControlState.Held)
                            leftCounter = 1;
                        break;
                    case ControlAction.MoveRight:
                        if (info.State == ControlState.Pressed || info.State == ControlState.Held)
                            rightCounter = 1;
                        break;
                    case ControlAction.Dash:
                        if (info.State == ControlState.Pressed && dashCooldownCounter == 0 && dashTouchedGround)
                        {
                            dashActiveCounter = 6;
                            dashCooldownCounter = 30;
                            dashTouchedGround = false;
                        }
                        break;
                    case ControlAction.Attack:
                        if (info.State == ControlState.Pressed)
                        {
                            if (attackActive && (animatorManager.CurrentFeature != attackVisualAnimation || animatorManager.CurrentSpriteSheetAnimation.Name != "attack_2"))
                                attackNextCombo = true;
                            attackActive = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void serviceAttack()
        {
            if (animatorManager.CurrentFeature == attackVisualAnimation && animatorManager.CurrentSpriteSheetAnimation.IsComplete && !attackNextCombo)
                attackActive = false;
        }

        private void serviceMovement()
        {
            float jumpAmount = 0;
            float leftAmount = 0;
            float rightAmount = 0;
            float horAmount = 0;

            // Order of if-else-statements matter
            // since they indicate priority of mechanics.

            // Implement attack movement.
            if (attackActive)
            {
                //jumpCounter = 0;
                //leftCounter = 0;
                //rightCounter = 0;

                float amount;
                if (animatorManager.CurrentFeature == attackVisualAnimation && 
                    animatorManager.CurrentSpriteSheetAnimation.CurrentFrameIndex <= 1)
                    amount = 4;
                else
                    amount = 0;

                if (facingRight && rightCounter > 0)
                    rightAmount = amount;
                else if (!facingRight && leftCounter > 0)
                    leftAmount = amount;
            }

            // Implement dash mechanic.
            else if (dashActiveCounter > 0)
            {
                jumpCounter = 0;
                leftCounter = 0;
                rightCounter = 0;

                if (facingRight)
                    rightAmount = 12;
                else
                    leftAmount = 12;
            }

            // Implement jump and moving left/right mechanics.
            else
            {
                if (jumpCounter > 0)
                    jumpAmount = 13f;
                if (leftCounter > 0)
                    leftAmount = 4;
                if (rightCounter > 0)
                    rightAmount = 4;
            }
            horAmount = rightAmount - leftAmount;

            // Dashes get reset when knight is on ground.
            if (Grounded)
            {
                dashTouchedGround = true;
            }

            // Determine facing direction
            if (horAmount > 0)
                facingRight = true;
            else if (horAmount < 0)
                facingRight = false;

            // Set the movement. 
            Movement = new Vector2(x: horAmount, y: -jumpAmount);
        }

        private void serviceMedia()
        {
            if (attackActive)
            {
                if (animatorManager.CurrentFeature != attackVisualAnimation)
                {
                    attackVisualAnimation.Play(animation: "attack_0").Rewind();
                    attackSoundSound.Play();
                }
                else if (animatorManager.CurrentSpriteSheetAnimation.IsComplete &&
                         attackNextCombo)
                {
                    attackNextCombo = false; // unfortunately, the attack next combo state has to be reset with the end of the animation.
                    if (animatorManager.CurrentSpriteSheetAnimation.Name == "attack_0")
                    {
                        attackVisualAnimation.Play(animation: "attack_1").Rewind();
                        attackSoundSound.Play();
                    }
                    else if (animatorManager.CurrentSpriteSheetAnimation.Name == "attack_1")
                    {
                        attackVisualAnimation.Play(animation: "attack_2").Rewind();
                        attackSoundSound.Play();
                    }
                }
                
            }
            else if (dashActiveCounter > 0)
            {
                runVisualAnimation.Play(animation: "dash_0");
                if (SoundManagerObject.CurrentFeature != dashSoundSound || SoundManagerObject.CurrentSoundEffectInstance.State != SoundState.Playing)
                    dashSoundSound.Play();

            }
            else if (Grounded)
            {
                if ((animatorManager.CurrentFeature != jumpVisualAnimation ||
                    animatorManager.CurrentSpriteSheetAnimation.Name != "start_0") &&
                    jumpCounter > 0)
                {
                    jumpVisualAnimation.Play(animation: "start_0").Rewind();
                    jumpSoundSound.Play();
                }

                if (animatorManager.CurrentFeature == jumpVisualAnimation &&
                    animatorManager.CurrentSpriteSheetAnimation.Name != "end_0" &&
                    animatorManager.CurrentSpriteSheetAnimation.IsComplete &&
                    jumpCounter == 0)
                {
                    jumpVisualAnimation.Play(animation: "end_0").Rewind();
                    landSoundSound.Play();
                }

                if ((animatorManager.CurrentFeature != jumpVisualAnimation) ||
                    (animatorManager.CurrentSpriteSheetAnimation.Name == "end_0" &&
                        animatorManager.CurrentSpriteSheetAnimation.IsComplete))
                {
                    if ((leftCounter == 0 && rightCounter == 0) || (leftCounter > 0 && rightCounter > 0))
                    {
                        idleVisualAnimation.Play(animation: "idle_0");
                        SoundManagerObject.CurrentSoundEffectInstance.Stop();
                    }
                    else
                    {
                        runVisualAnimation.Play(animation: "run_0");
                        if (SoundManagerObject.CurrentFeature != runSoundSound || SoundManagerObject.CurrentSoundEffectInstance.State != SoundState.Playing)
                            runSoundSound.Play();
                    }
                }
            }
            else
            {

                if (animatorManager.CurrentFeature != jumpVisualAnimation || animatorManager.CurrentSpriteSheetAnimation.Name != "start_0" || animatorManager.CurrentSpriteSheetAnimation.IsComplete)
                {
                    jumpVisualAnimation.Play(animation: "up_0");
                    SoundManagerObject.CurrentSoundEffectInstance.Stop();
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

            // Flash the knight when invulnerable.
            if (invulnerableCounter > 0)
            {
                var v0 = invulnerableCounter % 30;
                if (v0 >= 15)
                {
                    var v1 = v0 - 15;
                    animatorManager.CurrentSprite.Alpha = (float)v1 / 15;
                }
                else
                {
                    var v1 = v0;
                    animatorManager.CurrentSprite.Alpha = (float)(15 - v1) / 15;
                }
            }
            else
            {
                animatorManager.CurrentSprite.Alpha = 1;
            }
        }

        private void serviceCollisions()
        {
            // Service collisions as reported by the physics manager.
            while ((this as PhysicsInterface).GetNext(out var info))
            {
                // Check to see if the knight should take damage from potential damage inducing object.
                if (info.Other is DamagingInterface damager && damager.Active && invulnerableCounter == 0)
                {
                    // Knight should become invulnerable for a bit to prevent further damage.
                    invulnerableCounter = 2 * 30;

                    Console.WriteLine($"Took damage!");
                }
            }
        }

        private void decrementCounters()
        {
            // This method is dedicated to decrementing all counters at the same time.
            // The counter should be decremented at the end of the update loop.
            if (jumpCounter > 0)
                jumpCounter--;
            if (leftCounter > 0)
                leftCounter--;
            if (rightCounter > 0)
                rightCounter--;
            if (dashActiveCounter > 0)
                dashActiveCounter--;
            if (dashCooldownCounter > 0)
                dashCooldownCounter--;
            if (invulnerableCounter > 0)
                invulnerableCounter--;
        }

        public void Draw(Matrix? transformMatrix = null)
        {
            //spriteBatch.Begin(transformMatrix: transformMatrix);
            //spriteBatch.Draw(texture: maskTexture, position: Position, color: Color.White);
            //spriteBatch.End();
            animatorManager.Draw(transformMatrix: transformMatrix);
        }

        public void Update(float timeElapsed)
        {
            serviceCollisions();
            serviceControls();

            // Service main loop.
            while (loopTimer.GetNext())
            {
                serviceCamera();
                serviceMovement();
                serviceAttack();
                serviceMedia();
                decrementCounters();
            }

            // Update the managers and features.
            physicsManager.Update(timeElapsed);
            loopTimer.Update(timeElapsed);
            animatorManager.Update(timeElapsed);
            animatorManager.Position = Position;
        }
    }
}
