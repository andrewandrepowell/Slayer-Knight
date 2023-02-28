using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Utility;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace SlayerKnight
{
    internal class TestLevelFeature : UpdateInterface, DrawInterface
    {
        private static Size environmentGridSize = new Size(width: 100, height: 100);
        private static Color environmentStartColor = new Color(r: 255, g: 255, b: 0, alpha: 255);
        private static Color environmentIncludeColor = new Color(r: 255, g: 0, b: 0, alpha: 255);
        private static Color environmentExcludeColor = new Color(r: 0, g: 255, b: 0, alpha: 255);
        private LevelFeature firstLevelFeature;
        private LevelFeature secondLevelFeature;
        private RoomManager roomManager;
        private KeyboardManager keyboardManager;
        private KeyboardFeature keyboardFeature;
        private ControlManager controlManager;
        public TestLevelFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch)
        {
            keyboardFeature = new KeyboardFeature() { Activated = true };
            keyboardManager = new KeyboardManager();
            keyboardManager.Features.Add(keyboardFeature);
            controlManager = new ControlManager();
            controlManager.KeyboardFeatureObject = keyboardFeature;
            controlManager.KeyActionMap.Add(Keys.Left, ControlAction.MoveLeft);
            controlManager.KeyActionMap.Add(Keys.Right, ControlAction.MoveRight);
            controlManager.KeyActionMap.Add(Keys.Up, ControlAction.MoveUp);
            controlManager.KeyActionMap.Add(Keys.Down, ControlAction.MoveDown);
            controlManager.KeyActionMap.Add(Keys.Space, ControlAction.Jump);
            controlManager.KeyActionMap.Add(Keys.W, ControlAction.Dash);
            var screenSize = new Size(
                width: spriteBatch.GraphicsDevice.Viewport.Bounds.Width,
                height: spriteBatch.GraphicsDevice.Viewport.Bounds.Height) / 2;
            firstLevelFeature = new LevelFeature(
                contentManager: contentManager,
                spriteBatch: spriteBatch,
                controlManager: controlManager,
                keyboardManager: keyboardManager,
                roomIdentifier: "first_level",
                environmentVisualAsset: "kingdom/kingdom_environment_visual_asset_0",
                environmentMaskAsset: "kingdom/kingdom_environment_mask_asset_0",
                environmentGridSize: new Size(width: 16, height: 16),
                environmentStartColor: environmentStartColor,
                environmentIncludeColor: environmentIncludeColor,
                environmentExcludeColor: environmentExcludeColor,
                screenSize: screenSize);
            secondLevelFeature = new LevelFeature(
                contentManager: contentManager,
                spriteBatch: spriteBatch,
                controlManager: controlManager,
                keyboardManager: keyboardManager,
                roomIdentifier: "second_level",
                environmentVisualAsset: "test/test_environment_visual_asset_1",
                environmentMaskAsset: "test/test_environment_mask_asset_1",
                environmentGridSize: environmentGridSize,
                environmentStartColor: environmentStartColor,
                environmentIncludeColor: environmentIncludeColor,
                environmentExcludeColor: environmentExcludeColor,
                screenSize: screenSize);
            roomManager = new RoomManager();
            roomManager.Features.Add(firstLevelFeature);
            roomManager.Features.Add(secondLevelFeature);
        }
        public void Draw(Matrix? transformMatrix = null)
        {
            roomManager.Draw();
        }
        public void Update(float timeElapsed)
        {
            roomManager.Update(timeElapsed);
            keyboardManager.Update(timeElapsed);
            controlManager.Update(timeElapsed);
        }
    
    }
}
