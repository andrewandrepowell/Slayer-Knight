using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class TimerFeature : UpdateInterface
    {
        private Channel<object> channel = new Channel<object>();
        private float current = 0.0f;
        public bool Repeat { get; set; } = false;
        public bool Activated { get; set; } = false;
        public float Period { get; set; } = 1.0f;
        public int Count { get; set; } = 0;
        public bool GetNext()
        {
            if (channel.Count > 0)
            {
                channel.Dequeue();
                return true;
            }
            return false;
        }

        public void Update(float timeElapsed)
        {
            if (Activated)
            {
                current += timeElapsed;
                if (current >= Period)
                {
                    channel.Enqueue(null);
                    current -= Period;
                    Count++;
                    if (!Repeat)
                        Activated = false;
                }
            }
            else
            {
                current = 0;
            }
        }
    }
}
