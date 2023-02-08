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
        public RoomInterface Current { get; private set; } = null;
        public List<RoomInterface> Features { get; private set; } = new List<RoomInterface>(); 
        private void goTo(string nextIdentifier, float timeElapsed)
        {
            // End the previous room.
            Current.StartChannel.Enqueue(StartAction.End);
            Current.Update(timeElapsed);

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
            Current.Update(timeElapsed);
        }
        public void Update(float timeElapsed)
        {
            if (Current == null)
            {
                if (Features.Count >= 1)
                {
                    Current = Features.First();
                    Current.StartChannel.Enqueue(StartAction.Start);
                    Current.Update(timeElapsed);
                }
            }
            else
            {
                if (Current.GoToChannel.Count > 0)
                {
                    string nextIdentifier = Current.GoToChannel.Dequeue();
                    goTo(nextIdentifier, timeElapsed);
                }
                else
                {
                    Current.Update(timeElapsed);
                }
            }
        }
        public void Draw(Matrix? transformMatrix = null)
        {
            if (Current != null)
                Current.Draw(transformMatrix);
        }
    }
}
