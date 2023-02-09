using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        public static ComponentInterface GetComponentFeature(
            Color identifier,
            ContentManager contentManager,
            SpriteBatch spriteBatch)
        {
            if (Components.TestComponentFeature.Identifier == identifier)
            {
                var componentFeature = new Components.TestComponentFeature(
                    contentManager: contentManager,
                    spriteBatch: spriteBatch);
                return componentFeature;
            }
            return null;
        }
    }
}
