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
    public class ControlFeature : FeatureInterface<ControlManager>
    {
        public bool Activated { get; set; } = false;
        ControlManager FeatureInterface<ControlManager>.ManagerObject { get; set; }
        public bool GetNext(out ControlInfo info) => (this as FeatureInterface<ControlManager>).ManagerObject.GetNext(this, out info);
    }
    public class ControlManager : UpdateInterface, ManagerInterface<ControlFeature>
    {
        readonly private static Dictionary<KeyState, ControlState> mapKeyStateControlState = new Dictionary<KeyState, ControlState>() 
        {
            { KeyState.Pressed, ControlState.Pressed },
            { KeyState.Held, ControlState.Held },
            { KeyState.Released, ControlState.Released }
        };
        private Dictionary<ControlFeature, Channel<ControlInfo>> mapFeatureChannel;
        public KeyboardFeature KeyboardFeatureObject { get; set; }
        public Dictionary<Keys, ControlAction> KeyActionMap { get; private set; }
        public IList<ControlFeature> Features { get; private set; }
        public bool GetNext(ControlFeature feature, out ControlInfo info)
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
        public ControlManager()
        {
            mapFeatureChannel = new Dictionary<ControlFeature, Channel<ControlInfo>>();
            KeyboardFeatureObject = default;
            KeyActionMap = new Dictionary<Keys, ControlAction>();
            Features = new DirectlyManagedList<ControlFeature, ControlManager>(this);
        }
        public void Update(float timeElapsed)
        {
            if (KeyboardFeatureObject != null)
            {
                while (KeyboardFeatureObject.GetNext(out var keyboardInfo))
                {
                    ControlAction action;
                    if (KeyActionMap.TryGetValue(key: keyboardInfo.Key, value: out action))
                    {
                        ControlState state = mapKeyStateControlState[keyboardInfo.State];
                        foreach ((var feature, var channel) in mapFeatureChannel.Where(x => x.Key.Activated))
                            channel.Enqueue(new ControlInfo(action: action, state: state));
                    }

                }
            }
        }

        void ManagerInterface<ControlFeature>.SetupFeature(ControlFeature feature)
        {
            mapFeatureChannel.Add(feature, new Channel<ControlInfo>(capacity: 100));
        }

        void ManagerInterface<ControlFeature>.DestroyFeature(ControlFeature feature)
        {
            mapFeatureChannel.Remove(feature);
        }
    }
}
