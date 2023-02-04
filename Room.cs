using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public interface RoomInterface
    {
        public RoomFeature RoomFeatureObject { get; }
    }
    public class RoomFeature
    {
        public string Identifier { get; set; } = "";
        public RoomFeature Previous { get; set; } = null;
        public UpdateInterface UpdateObject { get; set; } = null;
        public DrawInterface DrawObject { get; set; } = null;
        public bool Destroyed { get; private set; } = false;
        public RoomManager Manager { get; set; } = null;
        public Queue<string> GoToQueue { get; private set; } = new Queue<string>(); // feature -> manager.
        public Queue<object> PreviousQueue { get; private set; } = new Queue<object>(); // feature -> manager.
        public Queue<object> ActiveQueue { get; private set; } = new Queue<object>(); // manager -> active.
        public void Destroy() { Destroyed = true; }

    }
    public class RoomManager : UpdateInterface, DrawInterface
    {
        public RoomFeature Current { get; private set; } = null;
        public List<RoomFeature> Features { get; private set; } = new List<RoomFeature>();
        public void Update(float timeElapsed)
        {
            // Remove any destroyed collision features from the manager.
            Features.Where((x) => x.Destroyed).ToList().ForEach((x) => Features.Remove(x));

            // Set the Current once a RoomFeature is added to the Feature list.
            if (Current == null && Features.Count >= 1)
            {
                Current = Features.First();
                Current.ActiveQueue.Enqueue(null);
            }

            // Perform the following operations once a Current has been established.
            if (Current != null)
            {
                // Go to next room based on identifier.
                if (Current.GoToQueue.Count > 0)
                {
                    string nextIdentifier = Current.GoToQueue.Dequeue();
                    RoomFeature nextRoom = Features.Find(x => x.Identifier == nextIdentifier);
                    if (nextRoom == null)
                        throw new Exception($"Current {Current} with Identifier {Current.Identifier} attempted to go to nonexistent nextRoom with Identifier {nextIdentifier}.");
                    nextRoom.Previous = Current;
                    Current = nextRoom;
                    Current.ActiveQueue.Enqueue(null);
                }

                // Got to previous room.
                else if (Current.PreviousQueue.Count > 0)
                {
                    Current.PreviousQueue.Dequeue();
                    if (Current.Previous == null)
                        throw new Exception($"Current {Current} with Identifier {Current.Identifier} does not have a Previous to go to.");
                    Current = Current.Previous;
                }

                // Update the Current.
                Current.UpdateObject.Update(timeElapsed);
            }      
        }
        public void Draw(Matrix? transformMatrix)
        {
            // Draw the CurrentRoom.
            if (Current != null)
                Current.DrawObject.Draw(transformMatrix);
        }
    }
}
