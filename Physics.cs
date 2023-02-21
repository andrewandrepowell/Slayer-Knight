﻿using Microsoft.Xna.Framework;
using MonoGame.Extended;
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
        public bool Grounded { get; set; }
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
        private Vector2 curGravity;
        private Vector2 curMovement;
        private Vector2 defNormal;
        private Vector2 curGravocity;
        private int grdCounter;
        public PhysicsManager(PhysicsInterface physicsFeature)
        {
            this.physicsFeature = physicsFeature;
            (physicsFeature as FeatureInterface<PhysicsManager>).ManagerObject = this;
            timerFeature = new TimerFeature() { Period = updatePeriod };
            synthesisInfos = new List<CollisionInfo>();
            infoChannel = new Channel<PhysicsInfo>(capacity: 10);
            curGravity = Vector2.Zero;
            curMovement = Vector2.Zero;
            defNormal = Vector2.Zero;
            curGravocity = Vector2.Zero;
            grdCounter = 0;
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
            physicsFeature.Grounded = grdCounter > 0;

            // Update the normal direction from ground if user decides to change gravity.
            if (curGravity != physicsFeature.Gravity)
            {
                if (physicsFeature.Gravity == Vector2.Zero)
                {
                    defNormal = Vector2.Zero;
                    curGravocity = Vector2.Zero;
                }
                else
                {
                    defNormal = -Vector2.Normalize(physicsFeature.Gravity);
                    curGravocity = -defNormal;
                }
                curGravity = physicsFeature.Gravity;
                grdCounter = 0;
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
                // Update the position based on current velocity based on gravity and movement.
                physicsFeature.Position += (curGravocity + curMovement);

                // Update current velocity based on gravity.
                curGravocity += physicsFeature.Gravity;

                // Limit the speed based on the gravity to the maximum speed based on gravity.
                var curGravspeed = curGravocity.Length();
                if (curGravspeed > physicsFeature.MaxGravspeed)
                    curGravocity = physicsFeature.MaxGravspeed * -defNormal;

                // Update current velocity based on movement. 
                // This operation only occurs when in the air.
                if (grdCounter == 0)
                    curMovement = physicsFeature.Movement;
                else
                    grdCounter--;

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

                    // Correct current movement.
                    {
                        // Correct current horizontal movement.
                        // The idea is, so long as the incline isn't too steep, the horizontal movement will rotate with the incline.
                        // This state also determines whether the physics feature is considered grounded or not.
                        Vector2 horMovement;
                        if (Vector2.Dot(defNormal, info.Normal) > 0.25f)
                        {
                            horMovement = physicsFeature.Movement.X * info.Normal.GetPerpendicular(); // horizontal movement rotates with ground.
                            grdCounter = 10; // increasing ground counter implies the physics feature is grounded.
                            curGravocity = -defNormal; // velocity based on gravity is reset back to negative default normal.
                        }
                        else
                        {
                            horMovement = Vector2.Zero; // squash horizontal movement if colliding with wall.
                        }

                        // Correct current vertical movement.
                        // The idea is, so long as there isn't something in the way, vertical movement is free to occur.
                        Vector2 verMovement;
                        if (Vector2.Dot(-physicsFeature.Movement.Y * defNormal, info.Normal) >= 0)
                        {
                            verMovement = -physicsFeature.Movement.Y * defNormal;
                        }
                        else
                        {
                            verMovement = Vector2.Zero;
                        }

                        // Combination of horizontal and vertical movement defines the current movement.
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
                }
            }

            timerFeature.Update(timeElapsed);
        }
        IList<PhysicsInterface> ManagerInterface<PhysicsInterface>.Features { get => throw new NotImplementedException("For the PhysicsManager, pass feature into constructor."); }
        void ManagerInterface<PhysicsInterface>.DestroyFeature(PhysicsInterface feature) => throw new NotImplementedException("For the PhysicsManager, pass feature into constructor.");
        void ManagerInterface<PhysicsInterface>.SetupFeature(PhysicsInterface feature) => throw new NotImplementedException("For the PhysicsManager, pass feature into constructor.");
    }
}
