using Microsoft.Xna.Framework;

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
        public ChannelInterface<StartAction> StartChannel { get; }
    }

    public interface DestroyInterface
    {
        public bool Destroyed { get; }
        public ChannelInterface<object> DestroyChannel { get; }
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
