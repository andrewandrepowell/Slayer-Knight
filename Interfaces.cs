using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Utility
{
    public interface FeatureInterface<T>
    {
        public T ManagerObject { get; set; }
    }

    public interface ManagerInterface<T>
    {
        public IList<T> Features { get; }
        public void SetupFeature(T feature);
        public void DestroyFeature(T feature);
    }
    public interface IdentityInterface
    {
        public string Identifier { get; }
    }

    public enum StartAction { Start, End }
    public interface StartInterface
    {
        public bool Started { get; }
        public void Start();
        public void End();
    }

    public interface DestroyInterface
    {
        public bool Destroyed { get; }
        public void Destroy();
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
