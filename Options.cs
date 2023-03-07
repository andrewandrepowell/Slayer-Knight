using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public interface OptionsInterface : FeatureInterface<OptionsManager>
    {
        public void Update();
    }
    public static class OptionsExtended
    {

    }
    public class OptionsManager : ManagerInterface<OptionsInterface>
    {
        private float soundVolume = 0.5f;
        private float musicVolume = 0.5f;
        public IList<OptionsInterface> Features { get; private set; }
        public float SoundVolume
        {
            get => soundVolume;
            set
            {
                if (value < 0 || value > 1)
                    throw new Exception("Volume must be between 0 and 1.");
                soundVolume = value;
            }
        }
        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                if (value < 0 || value > 1)
                    throw new Exception("Volume must be between 0 and 1.");
                musicVolume = value;
            }
        }
        public void Update()
        {
            foreach (var feature in Features)
                feature.Update();
        }
        public OptionsManager()
        {
            Features = new DirectlyManagedList<OptionsInterface, OptionsManager>(this);
        }

        void ManagerInterface<OptionsInterface>.DestroyFeature(OptionsInterface feature)
        {
            
        }

        void ManagerInterface<OptionsInterface>.SetupFeature(OptionsInterface feature)
        {
            feature.Update();
        }
    }
}
