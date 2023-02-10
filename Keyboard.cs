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
    public class KeyboardFeature
    {
        public bool Activated { get; set; } = false; // feature -> manager
        public Channel<KeyInfo> InfoChannel { get; private set; } = new Channel<KeyInfo>(capacity: 100); // manager -> feature
    }
    public class KeyboardManager : UpdateInterface
    {
        private Keys[] previousPressedKeys = Array.Empty<Keys>();
        public List<KeyboardFeature> Features { get; private set; } = new List<KeyboardFeature>();
        public void Update(float timeElapsed)
        {
            // Acquire keys, determine states, push keys and state through queue.
            var keyboardState = KeyboardExtended.GetState();
            var pressedKeys = keyboardState.GetPressedKeys();
            foreach (var feature in Features.Where(x => x.Activated))
            {
                foreach (var key in pressedKeys.Where(x => !previousPressedKeys.Contains(x)))
                    feature.InfoChannel.Enqueue(new KeyInfo(key: key, state: KeyState.Pressed));
                foreach (var key in pressedKeys.Where(x => previousPressedKeys.Contains(x)))
                    feature.InfoChannel.Enqueue(new KeyInfo(key: key, state: KeyState.Held));
                foreach (var key in previousPressedKeys.Where(x => !pressedKeys.Contains(x)))
                    feature.InfoChannel.Enqueue(new KeyInfo(key: key, state: KeyState.Released));
            }
            previousPressedKeys = pressedKeys;
        }
    }
}
