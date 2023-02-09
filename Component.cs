using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SlayerKnight
{
    internal interface ComponentInterface : UpdateInterface, DrawInterface
    {
        public int DrawLevel { get; }
    }
    internal static class ComponentManager
    {
        public static object GetComponentFeature(string identifier)
        {
            return null;
        }
    }
}
