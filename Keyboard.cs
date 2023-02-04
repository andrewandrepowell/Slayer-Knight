using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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
    public class KeyboardFeature
    {
        public bool Activated { get; set; } = false;
        public Queue<KeyInfo> InfoQueue { get; private set; } = new Queue<KeyInfo>(); // manager -> feature
    }
    public class KeyboardManager : UpdateInterface
    {
        private Keys[] previousPressedKeys = new Keys[0];
        public List<KeyboardFeature> Features { get; private set; } = new List<KeyboardFeature>();
        public void Update(float timeElapsed)
        {
            var keyboardState = KeyboardExtended.GetState();
            var pressedKeys = keyboardState.GetPressedKeys();
            foreach (var feature in Features.Where(x => x.Activated))
            {
                foreach (var key in pressedKeys.Where(x => !previousPressedKeys.Contains(x)))
                    feature.InfoQueue.Enqueue(new KeyInfo(key: key, state: KeyState.Pressed));
                foreach (var key in pressedKeys.Where(x => previousPressedKeys.Contains(x)))
                    feature.InfoQueue.Enqueue(new KeyInfo(key: key, state: KeyState.Held));
                foreach (var key in previousPressedKeys.Where(x => !pressedKeys.Contains(x)))
                    feature.InfoQueue.Enqueue(new KeyInfo(key: key, state: KeyState.Released));
            }
            previousPressedKeys = pressedKeys;
        }
    }
}
