using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Utility
{
    public interface IdentityInterface
    {
        public string Identifier { get; }
    }
    public enum StartAction { Start, End }
    public interface StartInterface
    {
        public bool Started { get; }
        public Channel<StartAction> StartChannel { get; }

    }
    public interface DestroyInterface
    {
        public bool Destroyed { get; }
        public Channel<object> DestroyChannel { get; }
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
