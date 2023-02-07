using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public interface RoomInterface
    {
        public RoomFeature RoomFeatureObject { get; }
    }
    public class RoomFeature : StartInterface
    {
        public string Identifier { get; set; } = ""; // feature -> manager
        public RoomFeature Previous { get; set; } = null; // manager -> feature
        public UpdateInterface UpdateObject { get; set; } = null; // feature -> manager
        public DrawInterface DrawObject { get; set; } = null; // feature -> manager
        public bool Started { get; set; } = false; // manager -> feature
        public Queue<string> GoToQueue { get; private set; } = new Queue<string>(capacity: 1); // feature -> manager
        public Queue<StartAction> StartQueue => new Queue<StartAction>(capacity: 1); // manager -> feature
    }
    public class RoomManager : UpdateInterface, DrawInterface
    {
        public RoomFeature Current { get; private set; } = null;
        public List<RoomFeature> Features { get; private set; } = new List<RoomFeature>();
        public Queue<string> GoToQueue { get; private set; } = new Queue<string>(); // user -> manager
        private void goTo(string nextIdentifier)
        {
            // End the previous room.
            Current.StartQueue.Enqueue(StartAction.End);

            // If the nextIdentifier is specified, then go to the specified room.
            if (nextIdentifier != null)
            {
                RoomFeature nextRoom = Features.Find(x => x.Identifier == nextIdentifier);
                if (nextRoom == null)
                    throw new Exception($"Current {Current} with Identifier {Current.Identifier} attempted to go to nonexistent nextRoom with Identifier {nextIdentifier}.");
                nextRoom.Previous = Current;
                Current = nextRoom;
            }

            // If the nextIdentifier is null, then go to previous room.
            else
            {
                if (Current.Previous == null)
                    throw new Exception($"Current {Current} with Identifier {Current.Identifier} does not have a Previous to go to.");
                Current = Current.Previous;
            }

            // Start the new current room.
            Current.StartQueue.Enqueue(StartAction.Start);
        }
        public void Update(float timeElapsed)
        {
            // Go to next room or previous room based on identifier from user.
            if (GoToQueue.Count > 0)
            {
                string nextIdentifier = GoToQueue.Dequeue();
                goTo(nextIdentifier);
            }

            // Set the Current once a RoomFeature is added to the Feature list.
            if (Current == null && Features.Count >= 1)
            {
                Current = Features.First();
                Current.StartQueue.Enqueue(StartAction.Start);
            }

            // Perform the following operations once a Current has been established.
            if (Current != null)
            {
                // Go to next room or previous room based on identifier.
                if (Current.GoToQueue.Count > 0)
                {
                    string nextIdentifier = Current.GoToQueue.Dequeue();
                    goTo(nextIdentifier);
                }
            }

            // Update all the rooms.
            foreach (var feature in Features)
                feature.UpdateObject.Update(timeElapsed);
        }
        public void Draw(Matrix? transformMatrix)
        {
            // Draw all the rooms.
            foreach (var feature in Features)
                feature.DrawObject.Draw(transformMatrix);
        }
    }
}
