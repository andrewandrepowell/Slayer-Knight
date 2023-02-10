using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using SlayerKnight.Components;

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
            SpriteBatch spriteBatch,
            Vector2 position,
            string roomIdentifier,
            OutputInterface<string> goToOutput)
        {
            if (TestComponentFeature.Identifier == identifier)
            {
                var componentFeature = new TestComponentFeature(
                    contentManager: contentManager,
                    spriteBatch: spriteBatch,
                    roomIdentifier: roomIdentifier,
                    goToOutput: goToOutput)
                {  
                    Position = position 
                };
                return componentFeature;
            }
            return null;
        }
    }
}
