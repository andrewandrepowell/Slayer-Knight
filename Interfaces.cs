using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Utility
{
    public enum StartAction { Start, End }
    public interface StartInterface
    {
        public bool Started { get; }
        public Queue<StartAction> StartQueue { get; } // user -> feature

    }
    public interface DestroyInterface
    {
        public Queue<object> DestroyQueue { get; } // user -> feature
        public bool Destroyed { get; }
    }
    public interface DrawInterface
    {
        void Draw(Matrix? transformMatrix = null);
    }
    public interface UpdateInterface
    {
        void Update(float timeElapsed);
    }
}
