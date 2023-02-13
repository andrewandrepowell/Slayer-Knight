using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
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
    internal interface PhysicsInterface : CollisionInterface
    {
        public bool PhysicsApplied { get; set; }
        public Vector2 Movement { get; set; }
        public Vector2 Gravity { get; set; }
        public float MaxGravspeed { get; set; }
        
        public ChannelInterface<PhysicsInfo> PhysicsInfoChannel { get; }
    }
    internal class PhysicsManager : UpdateInterface
    {
        private const float updatePeriod = 1 / 30;
        private PhysicsInterface physicsFeature;
        private TimerFeature timerFeature;
        private List<Vector2> correctionVectors;
        private Vector2 curGravity;
        private Vector2 curMovement;
        private Vector2 defNormal;
        private Vector2 curGravocity;
        public PhysicsManager(PhysicsInterface physicsFeature)
        {
            this.physicsFeature = physicsFeature;
            timerFeature = new TimerFeature() { Period = updatePeriod };
            correctionVectors = new List<Vector2>();
            curGravity = Vector2.Zero;
            curMovement = Vector2.Zero;
            defNormal = Vector2.Zero;
            curGravocity = Vector2.Zero;
        }

        public void Update(float timeElapsed)
        {
            // The physics applied flag simply activates the timer.  
            timerFeature.Activated = physicsFeature.PhysicsApplied;

            // Update the normal direction from ground if user decides to change gravity.
            if (curGravity != physicsFeature.Gravity)
            {
                defNormal = -Vector2.Normalize(physicsFeature.Gravity);
                curGravity = physicsFeature.Gravity;
            }

            // Service collisions based on other collisions features colliding into the physics feature.
            while (physicsFeature.CollisionInfoChannel.Count > 0)
            {
                // Acknowledge the collision and store info.
                var info = physicsFeature.CollisionInfoChannel.Dequeue();

                // Create physics info so that the user can react to the collision.
                physicsFeature.PhysicsInfoChannel.Enqueue(new PhysicsInfo(
                    other: info.Other, 
                    selfCollided: false, 
                    normal: info.Normal,
                    point: info.Point));
            }

            // Apply physics for each
            while (timerFeature.RunChannel.Count > 0)
            {
                // Acknowledge timer.
                timerFeature.RunChannel.Dequeue();

                // Update the position based on current velocity based on gravity and movement.
                physicsFeature.Position += (curGravocity + curMovement);

                // Update current velocity based on gravity.
                curGravocity += physicsFeature.Gravity;

                // Limit the speed based on the gravity to the maximum speed based on gravity.
                var curGravspeed = curGravocity.Length();
                if (curGravspeed > physicsFeature.MaxGravspeed)
                    curGravocity = physicsFeature.MaxGravspeed * -defNormal;

                // Update current velocity based on movement.
                curMovement = physicsFeature.Movement;

                // Check for collisions.
                physicsFeature.CheckForCollision();

                // In case there are any collisions, clear the list of correction vectors.
                correctionVectors.Clear();

                // Go through each collision as a result of the change of position.
                while (physicsFeature.CollisionInfoChannel.Count > 0)
                {
                    // Acknowledge the collision and store info.
                    var info = physicsFeature.CollisionInfoChannel.Dequeue();

                    // Collect the correction vectors.
                    correctionVectors.Add(info.Correction);

                    // Correct current movement.
                    {
                        // Correct current horizontal movement.
                        // The idea is, so long as the incline isn't too steep, the horizontal movement will rotate with the incline. 
                        Vector2 horMovement;
                        if (Vector2.Dot(defNormal, info.Normal) > 0.25f)
                        {
                            horMovement = physicsFeature.Movement.X * info.Normal.GetPerpendicular(); // horizontal movement rotates with ground.   
                        }
                        else
                        {
                            horMovement = Vector2.Zero; // squash horizontal movement if colliding with wall.
                        }

                        // Correct current vertical movement.
                        // The idea is, so long as there isn't something in the way, vertical movement is free to occur.
                        Vector2 verMovement;
                        if (Vector2.Dot(physicsFeature.Movement.Y * defNormal, info.Normal) >= 0)
                        {
                            verMovement = physicsFeature.Movement.Y * defNormal;
                        }
                        else
                        {
                            verMovement = Vector2.Zero;
                        }

                        // Combination of horizontal and vertical movement defines the current movement.
                        curMovement = horMovement + verMovement;
                    }

                    // Velocity based on gravity is reset once the ground is touched.
                    if (Vector2.Dot(curGravocity, info.Normal) < 0)
                    {
                        curGravocity = Vector2.Zero;
                    }

                    // Create physics info so that the user can react to the collision.
                    physicsFeature.PhysicsInfoChannel.Enqueue(new PhysicsInfo(
                        other: info.Other, 
                        selfCollided: true, 
                        point: info.Point,
                        normal: info.Normal));
                }

                // Apply correction to position if there are only collisions based on physics.
                if (correctionVectors.Count > 0)
                    physicsFeature.Position += CollisionManager.SynthesizeCorrections(correctionVectors);
            }

            timerFeature.Update(timeElapsed);
        }
    }
}
