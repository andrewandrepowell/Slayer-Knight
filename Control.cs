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
    public interface ControlInterface
    {
        public ControlFeature ControlFeatureObject { get; }
    }
    public class ControlFeature
    {
        private const int MaxActions = 100;
        public object Parent { get; set; } = null;
        public bool Activated { get; set; } = false;
        public Queue<ControlInfo> InfoQueue { get; private set; } = new Queue<ControlInfo>(capacity: MaxActions); // manager -> feature
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
                while (KeyboardFeatureObject.InfoQueue.Count > 0)
                {
                    var keyboardInfo = KeyboardFeatureObject.InfoQueue.Dequeue();
                    ControlAction action;
                    if (KeyActionMap.TryGetValue(key: keyboardInfo.Key, value: out action))
                    {
                        ControlState state = keyStateControlStateMap[keyboardInfo.State];
                        foreach (var feature in Features.Where(x => x.Activated))
                            feature.InfoQueue.Enqueue(new ControlInfo(action: action, state: state));
                    }

                }
            }
        }
    }
}
