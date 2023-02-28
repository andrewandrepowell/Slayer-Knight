using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility;

namespace SlayerKnight
{
    internal struct PhysicsInfo 
    {
        public bool SelfCollided { get; private set; }
        public CollisionInterface Other { get; private set; }
        public Vector2 Point { get; private set; }
        public Vector2 Normal { get; private set; }
        public PhysicsInfo(CollisionInterface other, bool selfCollided, Vector2 point, Vector2 normal)
        {
            Other = other;
            SelfCollided = selfCollided;
            Point = point;
            Normal = normal;
        }
    };
    internal interface PhysicsInterface : CollisionInterface, FeatureInterface<PhysicsManager>
    {
        public bool PhysicsApplied { get; }
        public Vector2 Movement { get; }
        public Vector2 Gravity { get; }
        public float MaxGravspeed { get; }
        public bool Grounded { get; set; } // manager controlled
        public Vector2 Velocity { get; set; } // manager controlled
        public float NormalSpeed { get; set; } // manager controlled
    }
    internal static class PhysicsExtensions
    {
        public static bool GetNext(this PhysicsInterface feature, out PhysicsInfo info) => 
            (feature as FeatureInterface<PhysicsManager>).ManagerObject.GetNext(feature, out info);
    }
    internal class PhysicsManager : UpdateInterface, ManagerInterface<PhysicsInterface>
    {
        private const float updatePeriod = 1 / 30;
        private PhysicsInterface physicsFeature;
        private TimerFeature timerFeature;
        private List<CollisionInfo> synthesisInfos;
        private Channel<PhysicsInfo> infoChannel;
        private Vector2 lstPosition;
        private Vector2 curGravity;
        private Vector2 curMovement;
        private Vector2 memMovement;
        private Vector2 defNormal;
        private Vector2 curGravocity;
        private Vector2 lstNormal;
        private float accGravity;
        private int groundCounter;
        private int wallCounter;
        private int ceilCounter;
        public PhysicsManager(PhysicsInterface physicsFeature)
        {
            this.physicsFeature = physicsFeature;
            (physicsFeature as FeatureInterface<PhysicsManager>).ManagerObject = this;
            timerFeature = new TimerFeature() { Period = updatePeriod };
            synthesisInfos = new List<CollisionInfo>();
            infoChannel = new Channel<PhysicsInfo>(capacity: 10);
            curGravity = Vector2.Zero;
            curMovement = Vector2.Zero;
            memMovement = Vector2.Zero;
            defNormal = Vector2.Zero;
            curGravocity = Vector2.Zero;
            lstPosition = Vector2.Zero;
            lstNormal = Vector2.Zero;
            accGravity = 0;
            groundCounter = 0;
            wallCounter = 0;
            ceilCounter = 0;
        }
        public bool GetNext(PhysicsInterface feature, out PhysicsInfo info)
        {
            if (physicsFeature != feature)
                throw new Exception("The specifid feature isn't associated with this PhysicsManager.");
            info = default;
            if (infoChannel.Count > 0)
            {
                info = infoChannel.Dequeue();
                return true;
            }
            return false;
        }

        public void Update(float timeElapsed)
        {
            // The physics applied flag simply activates the timer.  
            timerFeature.Activated = physicsFeature.PhysicsApplied;

            // Grounded is simply when the ground counter is not zero.
            physicsFeature.Grounded = groundCounter > 0;

            // Update the normal direction from ground if user decides to change gravity.
            if (curGravity != physicsFeature.Gravity)
            {
                if (physicsFeature.Gravity == Vector2.Zero)
                {
                    defNormal = Vector2.Zero;
                    curGravocity = Vector2.Zero;
                    accGravity = 0;
                }
                else
                {
                    defNormal = -Vector2.Normalize(physicsFeature.Gravity);
                    curGravocity = -defNormal;
                    accGravity = -Vector2.Dot(defNormal, physicsFeature.Gravity);
                }
                curGravity = physicsFeature.Gravity;
                groundCounter = 0;
                wallCounter = 0;
                ceilCounter = 0;
            }

            // Service collisions based on other collisions features colliding into the physics feature.
            while ((physicsFeature as CollisionInterface).GetNext(out var info))
            {
                // Create physics info so that the user can react to the collision.
                infoChannel.Enqueue(new PhysicsInfo(
                    other: info.Other, 
                    selfCollided: false, 
                    normal: info.Normal,
                    point: info.Point));
            }

            // Apply physics for each
            while (timerFeature.GetNext())
            {
                // Update the velocity with the velocity based on gravity and movement.
                physicsFeature.Velocity = curGravocity + curMovement;

                // Update the normal speed. 
                // This was added so that the user has a way to determine if the physics feature is
                // moving upward or moving downward. 
                physicsFeature.NormalSpeed = Vector2.Dot(physicsFeature.Velocity, defNormal);

                // Update the position based on current velocity.
                // This will get corrected if any glitches and/or collisions occur.
                physicsFeature.Position += physicsFeature.Velocity;

                // Update current velocity based on gravity.
                curGravocity += physicsFeature.Gravity;

                // Limit the speed based on the gravity to the maximum speed based on gravity.
                var curGravspeed = curGravocity.Length();
                if (curGravspeed > physicsFeature.MaxGravspeed)
                    curGravocity = physicsFeature.MaxGravspeed * -defNormal;

                // Update memorized movement. 
                // Memorized movement is a mechanic where, under specific circumstances,
                //   movement isn't allowed to instantly change to a specified movement, i.e. movement specified
                //   by the physics feature.
                // Movement in this context refers to velocity in response to user-demanded movement, as opposed
                //   to gravity or some other factor.
                {
                    // Service the case where the specified movement is a jump, i.e. negative Y movemement.
                    // If a jump's speed is lowered, the speed should gradually diminish in accordance to gravity.
                    // If the jump is raised, the speed will instantly change to that speed.
                    if (physicsFeature.Movement.Y <= 0)
                    {
                        // If specified jump speed is greater than or equal to memorized jump speed, just
                        // set the memorized jump speed to the specified amount.
                        if (physicsFeature.Movement.Y <= memMovement.Y)
                            memMovement.Y = physicsFeature.Movement.Y;

                        // If specified jump speed is less than the memorized jump speed,
                        // gradually reduce the speed with gravity until the specified amount is reached.
                        else
                        {
                            // Slowly diminish memorized movement based on gravity.
                            memMovement.Y += accGravity;

                            // Make sure the memorized movement doesn't flip to the opposite direction.
                            if (memMovement.Y > physicsFeature.Movement.Y)
                                memMovement.Y = physicsFeature.Movement.Y;
                        }
                    }

                    // For now, if the specified vertical movement is in  downwards direction, just set the memorized
                    // speed in the vertical direction to the specified movement.
                    else
                        memMovement.Y = physicsFeature.Movement.Y;

                    // For now, the memorized horizontal movement, i.e. running left and right,
                    // is set to the specified horizontal movement. May implement sliding/friction later;
                    // The code will change here once that happens.
                    memMovement.X = physicsFeature.Movement.X;
                }

                // Decrement the ground counter. 
                // Value greater than zero indicates the physics feature is considered
                //   in its ground state.
                if (groundCounter != 0)
                    groundCounter--;

                // Decrement the wall counter.
                if (wallCounter != 0)
                    wallCounter--;

                if (ceilCounter != 0)
                    ceilCounter--;

                // Check for collisions.
                physicsFeature.CheckForCollision();

                // In case there are any collisions, clear the list of infos intended for synthesis.
                synthesisInfos.Clear();

                // Collect all the collisions infos.
                while ((physicsFeature as CollisionInterface).GetNext(out var info))
                    synthesisInfos.Add(info);

                // Synthesize infos and send the physic infos to physics feature if any collisions occurred.
                if (synthesisInfos.Count > 0)
                {
                    // Synthesize all the collisions infos.
                    var info = CollisionManager.SynthesizeInfos(synthesisInfos);
                    Console.WriteLine($"info={info.Correction} {info.Normal} {synthesisInfos.Count} {curMovement} {curGravocity}");

                    // Correct current movement.
                    {
                        // Correct current horizontal movement.
                        // The idea is, so long as the incline isn't too steep, the horizontal movement will rotate with the incline.
                        // This state also determines whether the physics feature is considered grounded or not.
                        Vector2 horMovement;
                        if (Vector2.Dot(defNormal, info.Normal) > 0.4f)
                        {
                            //Console.WriteLine("GROUNDED");
                            horMovement = memMovement.X * info.Normal.GetPerpendicular(); // horizontal movement rotates with ground.
                            groundCounter = 2; // increasing ground counter implies the physics feature is grounded.
                            curGravocity = -defNormal; // velocity based on gravity is reset back to negative default normal.
                        }
                        else
                        {
                            horMovement = Vector2.Zero; // squash horizontal movement if colliding with wall.
                            wallCounter = 3;
                        }                        

                        // Correct current vertical movement.
                        // The idea is, so long as there isn't something in the way, vertical movement is free to occur.
                        Vector2 verMovement;
                        if (Vector2.Dot(-memMovement.Y * defNormal, info.Normal) >= 0)
                        {
                            //Console.WriteLine("NOT HITTING CEILING");
                            verMovement = -memMovement.Y * defNormal;
                        }
                        else
                        {
                            verMovement = Vector2.Zero;
                            ceilCounter = 6;
                        }

                        // Combination of horizontal and vertical movement defines the current movement.
                        //Console.WriteLine($"Hor: {horMovement}, Ver: {verMovement}");
                        curMovement = horMovement + verMovement;
                    }

                    // Send physic infos to physics feature.
                    foreach (var other in info.Others)
                        infoChannel.Enqueue(new PhysicsInfo(
                            other: other, 
                            selfCollided: true, 
                            point: info.Point,
                            normal: info.Normal));

                    // Correction position so that physics feature doesn't overlap with other collisions features.
                    physicsFeature.Position += info.Correction;

                    // This piece solely of disgusting code is tp resolve another glitch:
                    // If the phsics feature is stuck, simply move it in the direction normal
                    // to the collision feature it ran into.
                    if (groundCounter == 0 && physicsFeature.Position == lstPosition)
                        physicsFeature.Position += info.Normal;

                    // Remember the previous normal from collision surface.
                    lstNormal = info.Normal;
                }
                else if (groundCounter > 0)
                {
                    Vector2 verMovement, horMovement;

                    horMovement = memMovement.X * lstNormal.GetPerpendicular();
                    verMovement = -memMovement.Y * defNormal;

                    curMovement = horMovement + verMovement;
                }
                else if (groundCounter == 0)
                {
                    curMovement = memMovement;
                }

                // This piece solely of disgusting code is tp resolve another glitch:
                // The physics feature will start bouncing around when grounded and
                // touching a wall. To prevent this from happening, always reset back
                // to last position.
                if (groundCounter > 0 && wallCounter > 0)
                    physicsFeature.Position = lstPosition;
                else if (ceilCounter > 0)
                    physicsFeature.Position = new Vector2(x: physicsFeature.Position.X, y: lstPosition.Y);
                else
                    lstPosition = physicsFeature.Position;

                Console.WriteLine($"Current Movement: {curMovement} {groundCounter} {memMovement}");
            }
            

            timerFeature.Update(timeElapsed);
        }
        IList<PhysicsInterface> ManagerInterface<PhysicsInterface>.Features { get => throw new NotImplementedException("For the PhysicsManager, pass feature into constructor."); }
        void ManagerInterface<PhysicsInterface>.DestroyFeature(PhysicsInterface feature) => throw new NotImplementedException("For the PhysicsManager, pass feature into constructor.");
        void ManagerInterface<PhysicsInterface>.SetupFeature(PhysicsInterface feature) => throw new NotImplementedException("For the PhysicsManager, pass feature into constructor.");
    }
}
