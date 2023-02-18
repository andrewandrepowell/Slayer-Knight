using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public static class RoomExtensions
    {
        public static void GoTo(this RoomInterface room, string identifier) => room.ManagerObject.GoTo(identifier);
    }
    public interface RoomInterface : IdentityInterface, StartInterface, UpdateInterface, DrawInterface, DirectlyManagedInterface<RoomManager>
    {
    }
    public class RoomManager : UpdateInterface, DrawInterface, ManagerInterface<RoomInterface>
    {
        private Dictionary<RoomInterface, RoomInterface> previousMap;
        private Channel<string> goToChannel;
        public RoomInterface Current { get; private set; }
        public DirectlyManagedList<RoomInterface, RoomManager> Features { get; private set; }
        public RoomManager()
        {
            previousMap = new Dictionary<RoomInterface, RoomInterface>();
            goToChannel = new Channel<string>();
            Current = null;
            Features = new DirectlyManagedList<RoomInterface, RoomManager>(this);
        }
        public void GoTo(string identifier) => goToChannel.Enqueue(identifier);

        private void goTo(string nextIdentifier, float timeElapsed)
        {
            // End the previous room.
            Current.End();

            // If the nextIdentifier is specified, then go to the specified room.
            if (nextIdentifier != null)
            {
                RoomInterface nextRoom = Features.Where(x => x.Identifier == nextIdentifier).First();
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
            Current.Start();
        }

        public void Update(float timeElapsed)
        {
            if (Current == null)
            {
                if (Features.Count >= 1)
                {
                    Current = Features.First();
                    Current.Start();
                }
            }
            else
            {
                if (goToChannel.Count > 0)
                {
                    string nextIdentifier = goToChannel.Dequeue();
                    goTo(nextIdentifier, timeElapsed);
                }
            }

            foreach (var feature in Features)
                feature.Update(timeElapsed);
        }

        public void Draw(Matrix? transformMatrix = null)
        {
            foreach (var feature in Features)
                feature.Draw(transformMatrix);
        }

        void ManagerInterface<RoomInterface>.SetupFeature(RoomInterface feature)
        {
        }

        void ManagerInterface<RoomInterface>.DestroyFeature(RoomInterface feature)
        {
        }
    }
}
