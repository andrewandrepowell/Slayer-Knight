using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public interface RoomInterface : IdentityInterface, StartInterface, UpdateInterface, DrawInterface
    {
        public Channel<string> GoToChannel { get; }
    }
    public class RoomManager : UpdateInterface, DrawInterface
    {
        private Dictionary<RoomInterface, RoomInterface> previousMap = new Dictionary<RoomInterface, RoomInterface>();
        public RoomInterface Current { get; private set; } = null; // user -> current
        public List<RoomInterface> Features { get; private set; } = new List<RoomInterface>(); // user -> current
        public Channel<string> GoToChannel { get; private set; } = new Channel<string>(); // user -> manager
        private void goTo(string nextIdentifier)
        {
            // End the previous room.
            Current.StartChannel.Enqueue(StartAction.End);

            // If the nextIdentifier is specified, then go to the specified room.
            if (nextIdentifier != null)
            {
                RoomInterface nextRoom = Features.Find(x => x.Identifier == nextIdentifier);
                if (nextRoom == null)
                    throw new Exception($"Current {Current} with Identifier {Current.Identifier} attempted to go to nonexistent nextRoom with Identifier {nextIdentifier}.");
                previousMap[nextRoom] = Current;
                Current = nextRoom;
            }

            // If the nextIdentifier is null, then go to previous room.
            else
            {
                if (!previousMap.TryGetValue(Current, out RoomInterface previous))
                    throw new Exception($"Current {Current} with Identifier {Current.Identifier} does not have a Previous to go to.");
                Current = previous;
            }

            // Start the new current room.
            Current.StartChannel.Enqueue(StartAction.Start);
        }
        public void Update(float timeElapsed)
        {
            // Go to next room or previous room based on identifier from user.
            if (GoToChannel.Count > 0)
            {
                string nextIdentifier = GoToChannel.Dequeue();
                goTo(nextIdentifier);
            }

            // Set the Current once a RoomFeature is added to the Feature list.
            if (Current == null && Features.Count >= 1)
            {
                Current = Features.First();
                Current.StartChannel.Enqueue(StartAction.Start);
            }

            // Perform the following operations once a Current has been established.
            if (Current != null)
            {
                // Go to next room or previous room based on identifier.
                if (Current.GoToChannel.Count > 0)
                {
                    string nextIdentifier = Current.GoToChannel.Dequeue();
                    goTo(nextIdentifier);
                }
            }

            // Update all the rooms.
            foreach (var feature in Features)
                feature.Update(timeElapsed);
        }
        public void Draw(Matrix? transformMatrix = null)
        {
            // Draw all the rooms.
            foreach (var feature in Features)
                feature.Draw(transformMatrix);
        }
    }
}
