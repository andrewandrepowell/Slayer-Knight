using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class TimerFeature : UpdateInterface
    {
        private float current = 0.0f;
        public object Parent { get; set; } = null;
        public bool Repeat { get; set; } = false;
        public bool Activated { get; set; } = false;
        public float Period { get; set; } = 1.0f;
        public Queue<object> RunQueue { get; private set; } = new Queue<object>(); // feature -> user

        public void Update(float timeElapsed)
        {
            if (Activated)
            {
                current += timeElapsed;
                if (current >= Period)
                {
                    RunQueue.Enqueue(null);
                    if (Repeat)
                    {
                        current -= Period;
                    }
                    else
                    {
                        Activated = false;
                        current = 0;
                    }
                }
            }
            else
            {
                current = 0;
            }
        }
    }
}
