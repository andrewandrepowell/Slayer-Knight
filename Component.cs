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
using MonoGame.Extended;

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

        public static ComponentInterface GetWallComponentFeature(
            Color[] mask,
            Size size,
            Color start,
            Color include,
            Color exclude,
            Vector2 position)
        {
            if (mask.Length != size.Width * size.Height)
                throw new Exception("Length of mask isn't consistent with size.");
            var vertices = CollisionManager.GetVertices(
                maskData: mask, size: size,
                startColor: start, includeColor: include, excludeColor: exclude);
            var ignoreColors = new Color[] { start, include, exclude };
            Color identifier;
            try
            {
                identifier = mask.Where(p => !ignoreColors.Contains(p) && p.A != 0).First();
            }
            catch (InvalidOperationException)
            {
                throw new Exception("There was no wall pixel in wall grid.");
            }

            if (identifier == WallComponent.Identifier)
            {
                return new WallComponent(
                    position: position,
                    size: size,
                    mask: mask,
                    vertices: vertices);
            }
            else if (identifier == MobWallComponent.Identifier)
            {
                return new MobWallComponent(
                    position: position,
                    size: size,
                    mask: mask,
                    vertices: vertices);
            }
            return null;
        }
    }
}
