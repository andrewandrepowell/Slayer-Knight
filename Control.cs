using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace Utility
{
    public enum ControlState { Pressed, Held, Released }
    public enum ControlAction { MoveLeft, MoveRight, MoveUp, MoveDown, Jump, Attack, Special }
    public struct ControlInfo
    {
        public ControlAction Action { get; private set; }
        public ControlState State { get; private set; }
        public ControlInfo(ControlAction action, ControlState state)
        {
            Action = action;
            State = state;
        }
    }
    public class ControlFeature
    {
        public bool Activated { get; set; } = false;
        public Channel<ControlInfo> InfoChannel { get; private set; } = new Channel<ControlInfo>(capacity: 100); // manager -> feature
    }
    public class ControlManager : UpdateInterface
    {
        readonly static Dictionary<KeyState, ControlState> keyStateControlStateMap = new Dictionary<KeyState, ControlState>() 
        {
            { KeyState.Pressed, ControlState.Pressed },
            { KeyState.Held, ControlState.Held },
            { KeyState.Released, ControlState.Released }
        };
        public KeyboardFeature KeyboardFeatureObject { get; set; } = null;
        public Dictionary<Keys, ControlAction> KeyActionMap { get; private set; } = new Dictionary<Keys, ControlAction>();
        public List<ControlFeature> Features { get; private set; } = new List<ControlFeature>();
        public void Update(float timeElapsed)
        {
            if (KeyboardFeatureObject != null)
            {
                while (KeyboardFeatureObject.InfoChannel.Count > 0)
                {
                    var keyboardInfo = KeyboardFeatureObject.InfoChannel.Dequeue();
                    ControlAction action;
                    if (KeyActionMap.TryGetValue(key: keyboardInfo.Key, value: out action))
                    {
                        ControlState state = keyStateControlStateMap[keyboardInfo.State];
                        foreach (var feature in Features.Where(x => x.Activated))
                            feature.InfoChannel.Enqueue(new ControlInfo(action: action, state: state));
                    }

                }
            }
        }
    }
}
