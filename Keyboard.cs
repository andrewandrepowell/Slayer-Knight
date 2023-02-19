using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;


namespace Utility
{
    public enum KeyState { Pressed, Held, Released }
    public struct KeyInfo
    {
        public Keys Key { get; private set; }
        public KeyState State { get; private set; }
        public KeyInfo(Keys key, KeyState state)
        {
            Key = key;
            State = state;
        }
    }
    public interface KeyboardInterface
    {
        public KeyboardFeature keyboardFeatureObject { get; }
    }
    public class KeyboardFeature : FeatureInterface<KeyboardManager>
    {
        public bool Activated { get; set; } = false; 
        KeyboardManager FeatureInterface<KeyboardManager>.ManagerObject { get; set; } = default;
        public bool GetNext(out KeyInfo info) => (this as FeatureInterface<KeyboardManager>).ManagerObject.GetNext(this, out info);
    }
    public class KeyboardManager : UpdateInterface, ManagerInterface<KeyboardFeature>
    {
        private Keys[] previousPressedKeys;
        private Dictionary<KeyboardFeature, Channel<KeyInfo>> mapFeatureChannel;
        public bool GetNext(KeyboardFeature feature, out KeyInfo info)
        {
            var channel = mapFeatureChannel[feature];
            info = default;
            if (channel.Count > 0)
            {
                info = channel.Dequeue();
                return true;
            }
            return false;
        }
        public IList<KeyboardFeature> Features { get; private set; }
        public KeyboardManager()
        {
            previousPressedKeys = Array.Empty<Keys>();
            mapFeatureChannel = new Dictionary<KeyboardFeature, Channel<KeyInfo>>();
            Features = new DirectlyManagedList<KeyboardFeature, KeyboardManager>(this);
        }
        public void Update(float timeElapsed)
        {
            // Acquire keys, determine states, push keys and state through queue.
            var keyboardState = KeyboardExtended.GetState();
            var pressedKeys = keyboardState.GetPressedKeys();
            foreach ((var feature, var channel) in mapFeatureChannel.Where(x => x.Key.Activated))
            {
                foreach (var key in pressedKeys.Where(x => !previousPressedKeys.Contains(x)))
                    channel.Enqueue(new KeyInfo(key: key, state: KeyState.Pressed));
                foreach (var key in pressedKeys.Where(x => previousPressedKeys.Contains(x)))
                    channel.Enqueue(new KeyInfo(key: key, state: KeyState.Held));
                foreach (var key in previousPressedKeys.Where(x => !pressedKeys.Contains(x)))
                    channel.Enqueue(new KeyInfo(key: key, state: KeyState.Released));
            }
            previousPressedKeys = pressedKeys;
        }

        public void SetupFeature(KeyboardFeature feature)
        {
            mapFeatureChannel.Add(feature, new Channel<KeyInfo>(capacity: 100));
        }

        public void DestroyFeature(KeyboardFeature feature)
        {
            mapFeatureChannel.Remove(feature);
        }
    }
}
