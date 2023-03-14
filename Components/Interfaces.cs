using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlayerKnight.Components
{
    internal interface DamagingInterface
    {
        public object ParentObject { get; }
        public bool Active { get; }
    }
}
