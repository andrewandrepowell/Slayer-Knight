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
            LevelInterface levelFeature)
        {
            if (TestComponent.Identifier == identifier)
            {
                var componentFeature = new TestComponent(
                    contentManager: contentManager,
                    spriteBatch: spriteBatch,
                    levelFeature: levelFeature)
                {  
                    Position = position 
                };
                return componentFeature;
            }
            else if (KnightComponent.Identifier == identifier)
            {
                var componentFeature = new KnightComponent(
                    contentManager: contentManager,
                    spriteBatch: spriteBatch,
                    levelFeature: levelFeature)
                {
                    Position = position
                };
                return componentFeature;
            }
            else if (SnailComponent.Identifier == identifier)
            {
                var componentFeature = new SnailComponent(
                    contentManager: contentManager,
                    spriteBatch: spriteBatch,
                    levelFeature: levelFeature)
                {
                    Position = position
                };
                return componentFeature;
            }
            return null;
        }
    }
}
