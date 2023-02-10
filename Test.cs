using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Utility;

namespace SlayerKnight
{
    internal class TestLevelFeature : UpdateInterface, DrawInterface
    {
        private LevelFeature levelFeature;
        private RoomManager roomManager;
        private KeyboardManager keyboardManager;
        private KeyboardFeature keyboardFeature;
        private ControlManager controlManager;
        public TestLevelFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            string levelIdentifier,
            string environmentVisualAsset,
            string environmentMaskAsset,
            Size environmentGridSize,
            Color environmentStartColor,
            Color environmentIncludeColor,
            Color environmentExcludeColor)
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
            levelFeature = new LevelFeature(
                contentManager: contentManager,
                spriteBatch: spriteBatch,
                controlManager: controlManager,
                keyboardManager: keyboardManager,
                roomIdentifier: levelIdentifier,
                environmentVisualAsset: environmentVisualAsset,
                environmentMaskAsset: environmentMaskAsset,
                environmentGridSize: environmentGridSize,
                environmentStartColor: environmentStartColor,
                environmentIncludeColor: environmentIncludeColor,
                environmentExcludeColor: environmentExcludeColor);
            roomManager = new RoomManager();
            roomManager.Features.Add(levelFeature);
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
