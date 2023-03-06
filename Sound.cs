using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class SoundFeature : FeatureInterface<SoundManager>
    {
        private float volume = 0.5f;
        public string Identifier { get; private set; }
        public float Volume
        {
            get => volume;
            set
            {
                if (value < 0 || value > 1)
                    throw new Exception($"volume {value} should be between 0 and 1 inclusively.");
                volume = value;
            }
        }
        public bool IsLooped { get; private set; }
        public void Play()
        {
            var managerObject = (this as FeatureInterface<SoundManager>).ManagerObject;
            if (managerObject == null)
                throw new Exception("Manager is not associated with feature.");
            managerObject.Play(this);
        }
        public SoundFeature(string identifier)
        {
            Identifier = identifier;
        }
        SoundManager FeatureInterface<SoundManager>.ManagerObject { get; set; }
    }
    public class SoundManager : ManagerInterface<SoundFeature>
    {
        private ContentManager contentManager;
        private Dictionary<SoundFeature, SoundEffectInstance> mapFeatureSoundEffectInstance;
        public IList<SoundFeature> Features { get; private set; }
        public SoundFeature CurrentFeature { get; private set; }
        public SoundEffectInstance CurrentSoundEffectInstance { get; private set; }
        public SoundManager(ContentManager contentManager)
        {
            Features = new DirectlyManagedList<SoundFeature, SoundManager>(this);
            mapFeatureSoundEffectInstance = new Dictionary<SoundFeature, SoundEffectInstance>();
            this.contentManager = contentManager;
        }
        public void Play(SoundFeature feature)
        {
            if (!Features.Contains(feature))
                throw new Exception($"Feature is not associated with manager.");
            CurrentFeature = feature;
            if (CurrentSoundEffectInstance != null)
                CurrentSoundEffectInstance.Stop();
            CurrentSoundEffectInstance = mapFeatureSoundEffectInstance[feature];
            CurrentSoundEffectInstance.Play();
        }
        void ManagerInterface<SoundFeature>.DestroyFeature(SoundFeature feature)
        {
            mapFeatureSoundEffectInstance.Remove(feature);
        }

        void ManagerInterface<SoundFeature>.SetupFeature(SoundFeature feature)
        {
            var soundEffect = contentManager.Load<SoundEffect>(feature.Identifier);
            var soundEffectInstance = soundEffect.CreateInstance();
            soundEffectInstance.Volume = feature.Volume;
            soundEffectInstance.IsLooped = feature.IsLooped;
            mapFeatureSoundEffectInstance.Add(feature, soundEffectInstance);
            if (Features.Count == 0)
            {
                CurrentFeature = feature;
                CurrentSoundEffectInstance = soundEffectInstance;
            }
        }
    }
}
