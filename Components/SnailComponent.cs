using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SlayerKnight.Components
{
    internal class SnailComponent : ComponentInterface, PhysicsInterface, HasSoundInterface
    {
        private enum ComponentState { Inactive, Roam, Agress }
        private enum AttackState { Prepare, Fire, Recover };
        readonly private static string maskAsset = "snail/snail_mask_0";
        readonly private static string walkVisualAsset = "snail/snail_walk_visual_0.sf";
        readonly private static string deadVisualAsset = "snail/snail_dead_visual_0.sf";
        readonly private static string hideVisualAsset = "snail/snail_hide_visual_0.sf";
        readonly private static Random random = new Random();
        readonly private static Vector2 detectionCone = new Vector2(x: 8, y: 4);
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private LevelInterface levelFeature;
        private PhysicsManager physicsManager;
        private TimerFeature loopTimer = new TimerFeature() { Period = 1 / 30, Activated = true, Repeat = true };
        private Texture2D maskTexture;
        private AnimatorManager animatorManager;
        private AnimatorFeature walkVisualAnimation;
        private AnimatorFeature deadVisualAnimation;
        private AnimatorFeature hideVisualAnimation;
        private ComponentState componentState = ComponentState.Inactive;
        private AttackState attackState = AttackState.Prepare;
        private int activateCounter = 0;
        private int agressCounter = 0;
        private int jumpCounter = 0;
        private int leftCounter = 0;
        private int rightCounter = 0;
        private int recoverCounter = 0;
        private float activateDistance;
        private float agressDistance;
        private KnightComponent knightComponent;
        private List<WallInterface> wallComponents;
        private bool facingRight;
        private bool touchedWall = false;
        private bool touchedGround = false;
        public static Color Identifier { get => new Color(r: 45, g: 67, b: 226, alpha: 255); }
        public int DrawLevel { get; set; } = 0;
        public bool PhysicsApplied { get; private set; } = true;
        public bool PhysicsStatic { get; private set; } = false;
        public bool IsMob => true;
        public Vector2 Movement { get; set; } = Vector2.Zero;
        public Vector2 Gravity { get; set; } = new Vector2(x: 0, y: 1f);
        public float MaxGravspeed { get; set; } = 8;
        public bool Grounded { get; set; } = default; // managed by physics manager.
        public bool Walled { get; set; } = default; // managed by physics manager.
        public Vector2 Velocity { get; set; } = default; // managed by associated manager.
        public float NormalSpeed { get; set; } = default; // managed by associated manager.
        public Vector2 Position { get; set; } = default;  // managed by physics manager.
        public Vector2 Center => Position + new Vector2(x: Size.Width / 2, y: Size.Height / 2);
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
            activateDistance = Math.Max(levelFeature.ScreenSize.Width, levelFeature.ScreenSize.Height) * 1.25f;
            agressDistance = levelFeature.ScreenSize.Width * 0.5f;
            facingRight = random.Next(1) == 0;
            SoundManagerObject = new SoundManager(contentManager);
            physicsManager = new PhysicsManager(this);
            {
                animatorManager = new AnimatorManager(contentManager: contentManager, spriteBatch: spriteBatch);
                walkVisualAnimation = new AnimatorFeature(walkVisualAsset) { Offset = new Vector2(x: 24, y: 16) };
                deadVisualAnimation = new AnimatorFeature(deadVisualAsset) { Offset = new Vector2(x: 24, y: 16) };
                hideVisualAnimation = new AnimatorFeature(hideVisualAsset) { Offset = new Vector2(x: 24, y: 16) };
                animatorManager.Features.Add(walkVisualAnimation);
                animatorManager.Features.Add(deadVisualAnimation);
                animatorManager.Features.Add(hideVisualAnimation);
                walkVisualAnimation.Play("walk_0");
            }
            {
                maskTexture = contentManager.Load<Texture2D>(maskAsset);
                if (maskTexture.Width != Size.Width || maskTexture.Height != Size.Height)
                    throw new Exception("The expected dimensions of the snail are incorrected.");
                var totalPixels = Size.Width * Size.Height;
                CollisionMask = new Color[totalPixels];
                maskTexture.GetData(CollisionMask);
            }
        }

        public void Draw(Matrix? transformMatrix)
        {
            //spriteBatch.Begin(transformMatrix: transformMatrix);
            //spriteBatch.Draw(texture: maskTexture, position: Position, color: Color.White);
            //spriteBatch.End();
            animatorManager.Draw(transformMatrix: transformMatrix);
        }

        public void Update(float timeElapsed)
        {
            determineComponents();
            serviceCollisions();
            while (loopTimer.GetNext())
            {
                serviceState();
                serviceMedia();
                serviceCounters();
            }

            // Update the managers and features.
            physicsManager.Update(timeElapsed);
            loopTimer.Update(timeElapsed);
            animatorManager.Update(timeElapsed);
            animatorManager.Position = Position;
        }
        private bool IsKnightClose(float distance) => (knightComponent.Center - Center).LengthSquared() < (distance * distance);
        private bool IsKnightVisible()
        {
            foreach (var wall in wallComponents)
                if (wall.IsBetween(Center, knightComponent.Center))
                    return false;
            return true;
        }
        private bool IsFacingKnight()
        {
            var knightVector = knightComponent.Center - Center;
            if (facingRight)
            {
                var topOfCone = new Vector2(x: detectionCone.X, y: -detectionCone.Y);
                var bottomOfCone = new Vector2(x: detectionCone.X, y: detectionCone.Y);
                return knightVector.IsBetweenTwoVectors(topOfCone, bottomOfCone);
            }
            else
            {
                var topOfCone = new Vector2(x: -detectionCone.X, y: -detectionCone.Y);
                var bottomOfCone = new Vector2(x: -detectionCone.X, y: detectionCone.Y);
                return knightVector.IsBetweenTwoVectors(bottomOfCone, topOfCone);
            }
        }
        private void determineComponents()
        {
            if (knightComponent == null)
            {
                var knightList = levelFeature.Features.OfType<KnightComponent>().ToList();
                if (knightList.Count != 1)
                    throw new Exception("There should be 1 knight in the level.");
                knightComponent = knightList.First();
            }
            if (wallComponents == null)
            {
                wallComponents = levelFeature.Features.OfType<WallInterface>().Where(w => w is not MobWallComponent).ToList();
            }
        }
        private void serviceCollisions()
        {
            // Service collisions as reported by the physics manager.
            while ((this as PhysicsInterface).GetNext(out var info))
                ;
        }
        private void serviceState()
        {
            // Once the activation occurs, search for potential target, then decide to 
            // activate or deactivate based on the distance from that component.
            // If too far, deactivate snail, if close and inactive cause the snail to roam.
            if (activateCounter == 0)
            {
                if (!IsKnightClose(activateDistance))
                    componentState = ComponentState.Inactive;
                else if (componentState == ComponentState.Inactive)
                    componentState = ComponentState.Roam;
            }

            // Only begin the agression once the knight is close enough to the snail
            // and if the snail has visibility of the knight.
            Console.WriteLine($"{IsKnightClose(agressDistance)}, {IsKnightVisible()}, {IsFacingKnight()}");
            if (agressCounter == 0 && componentState != ComponentState.Inactive)
            {
                if (IsKnightClose(agressDistance))
                {
                    if (IsKnightVisible())
                    {
                        if (componentState != ComponentState.Agress  && IsFacingKnight())
                        {
                            componentState = ComponentState.Agress;
                            attackState = AttackState.Prepare;
                        }
                    }
                    else
                    {
                        componentState = ComponentState.Roam;
                    }
                }
                else
                {
                    componentState = ComponentState.Roam;
                }
            }

            switch (componentState)
            {
                case ComponentState.Inactive:
                    Movement = Vector2.Zero;
                    break;
                case ComponentState.Agress:
                    switch (attackState)
                    {
                        case AttackState.Prepare:
                            {
                                if (Grounded)
                                {
                                    if (!touchedGround)
                                        attackState = AttackState.Fire;
                                    else if (jumpCounter == 0 && 
                                        animatorManager.CurrentFeature == hideVisualAnimation &&
                                        animatorManager.CurrentSpriteSheetAnimation.Name == "hide_0" &&
                                        animatorManager.CurrentSpriteSheetAnimation.IsComplete)
                                        jumpCounter = 4;
                                }
                            }
                            break;
                        case AttackState.Fire:
                            {
                                var rock = new RockComponent(
                                    contentManager: contentManager,
                                    spriteBatch: spriteBatch);
                                rock.Movement = 4 * Vector2.Normalize(knightComponent.Center - Center);
                                rock.Position = Center - new Vector2(x: rock.Size.Width / 2, y: rock.Size.Height / 2);
                                levelFeature.NewFeatures.Add(rock);
                                recoverCounter = 4 * 30;
                                attackState = AttackState.Recover;
                            }
                            break;
                        case AttackState.Recover:
                            {
                                if (recoverCounter == 0)
                                    attackState = AttackState.Prepare;
                            }
                            break;
                    }
                    Movement = (jumpCounter > 0 ? -7 : 0) * Vector2.UnitY;
                    break;
                case ComponentState.Roam:
                    {
                        float leftAmount = 0;
                        float rightAmount = 0;

                        if (Walled && !touchedWall)
                            facingRight = !facingRight;

                        if (animatorManager.CurrentFeature == walkVisualAnimation)
                        {
                            if (facingRight && rightCounter == 0)
                                rightCounter = 3;
                            else if (!facingRight && leftCounter == 0)
                                leftCounter = 3;
                        }

                        if (leftCounter > 1)
                            leftAmount = 2.5f;
                        if (rightCounter > 1)
                            rightAmount = 2.5f;

                        Movement = new Vector2(x: rightAmount-leftAmount, y: 0);
                    }
                    break;
            }

            // Determine if wall was touched.
            if (Walled)
                touchedWall = true;
            else
                touchedWall = false;

            // Determine if ground was touched.
            if (Grounded)
                touchedGround = true;
            else
                touchedGround = false;
        }
        private void serviceMedia()
        {
            if (componentState == ComponentState.Roam)
            {
                if (animatorManager.CurrentFeature == hideVisualAnimation)
                {
                    var animation = animatorManager.CurrentSpriteSheetAnimation;
                    if (animation.IsComplete)
                    {
                        if (animation.Name == "hide_0")
                        {
                            hideVisualAnimation.Play("reveal_0");
                        }
                        if (animation.Name == "reveal_0")
                        {
                            walkVisualAnimation.Play("walk_0");
                        }
                    }
                }
                else
                {
                    walkVisualAnimation.Play("walk_0");
                }
            }

            if (componentState == ComponentState.Agress)
            {
                if (animatorManager.CurrentFeature != hideVisualAnimation || 
                    animatorManager.CurrentSpriteSheetAnimation.Name != "hide_0" || 
                    !animatorManager.CurrentSpriteSheetAnimation.IsComplete)
                {
                    hideVisualAnimation.Play("hide_0");
                }   
            }

            if (facingRight)
            {
                animatorManager.CurrentSprite.Effect = SpriteEffects.FlipHorizontally;
            }
            else
            {
                animatorManager.CurrentSprite.Effect = SpriteEffects.None;
            }
        }
        private void serviceCounters()
        {
            if (activateCounter > 0)
                activateCounter--;
            else
                activateCounter = 30 * 4;

            if (agressCounter > 0)
                agressCounter--;
            else
                agressCounter = 30 * 1;

            if (jumpCounter > 0)
                jumpCounter--;

            if (leftCounter > 0)
                leftCounter--;

            if (rightCounter > 0)
                rightCounter--;

            if (recoverCounter > 0)
                recoverCounter--;
        }
        CollisionManager FeatureInterface<CollisionManager>.ManagerObject { get; set; } // managed by associated manager.
        PhysicsManager FeatureInterface<PhysicsManager>.ManagerObject { get; set; } // managed by associated manager.
    }
}
