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
        private TestComponentFeature testComponentFeature;
        private LevelFeature levelFeature;
        private RoomManager roomManager;
        private KeyboardManager keyboardManager;
        private KeyboardFeature keyboardFeature;
        private KeyboardFeature controlKeyboardFeature;
        private ControlManager controlManager;
        public TestLevelFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            string levelIdentifier,
            string testComponentMaskAsset,
            string environmentVisualAsset,
            string environmentMaskAsset,
            Size environmentGridSize,
            Color environmentStartColor,
            Color environmentIncludeColor,
            Color environmentExcludeColor)
        {
            testComponentFeature = new TestComponentFeature(
                contentManager: contentManager,
                spriteBatch: spriteBatch,
                maskAsset: testComponentMaskAsset);
            levelFeature = new LevelFeature(
                contentManager: contentManager,
                spriteBatch: spriteBatch,
                roomIdentifier: levelIdentifier,
                environmentVisualAsset: environmentVisualAsset,
                environmentMaskAsset: environmentMaskAsset,
                environmentGridSize: environmentGridSize,
                environmentStartColor: environmentStartColor,
                environmentIncludeColor: environmentIncludeColor,
                environmentExcludeColor: environmentExcludeColor);
            roomManager = new RoomManager();
            roomManager.Features.Add(levelFeature);
            keyboardFeature = new KeyboardFeature() { Activated = true };
            controlKeyboardFeature = new KeyboardFeature() { Activated = true };
            keyboardManager = new KeyboardManager();
            keyboardManager.Features.Add(keyboardFeature);
            keyboardManager.Features.Add(controlKeyboardFeature);
            controlManager = new ControlManager();
            controlManager.KeyboardFeatureObject = controlKeyboardFeature;
            controlManager.Features.Add(testComponentFeature.ControlFeatureObject);
            controlManager.KeyActionMap.Add(Keys.Left, ControlAction.MoveLeft);
            controlManager.KeyActionMap.Add(Keys.Right, ControlAction.MoveRight);
            controlManager.KeyActionMap.Add(Keys.Up, ControlAction.MoveUp);
            controlManager.KeyActionMap.Add(Keys.Down, ControlAction.MoveDown);
        }
        public void Draw(Matrix? transformMatrix = null)
        {
            testComponentFeature.Draw();
            roomManager.Draw();
        }
        public void Update(float timeElapsed)
        {
            while (keyboardFeature.InfoChannel.Count > 0)
            {
                var info = keyboardFeature.InfoChannel.Dequeue();
                //Console.WriteLine($"DEBUG: {info.Key} {info.State}");

                if (info.State == Utility.KeyState.Pressed)
                {
                    switch (info.Key)
                    {
                        case Keys.Z:
                            levelFeature.StartChannel.Enqueue(StartAction.Start);
                            break;
                        case Keys.X:
                            levelFeature.StartChannel.Enqueue(StartAction.End);
                            break;
                    }
                }
            }

            testComponentFeature.Update(timeElapsed);
            roomManager.Update(timeElapsed);
            keyboardManager.Update(timeElapsed);
            controlManager.Update(timeElapsed);
        }
    }
    internal class TestComponentFeature : UpdateInterface, DrawInterface
    {
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private string maskAsset;
        private Texture2D maskTexture;
        private TimerFeature controlTimer;
        private const float controlPeriod = .01f;
        public bool Started { get; private set; } // feature -> user
        public Vector2 Position { get; set; } // feature <-> user
        public Size Size { get; private set; } // feature -> user
        public ControlFeature ControlFeatureObject { get; private set; } // feature -> user
        public Queue<StartAction> StartQueue { get; private set; } // user -> feature
        public TestComponentFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            string maskAsset)
        {
            Started = false;
            ControlFeatureObject = new ControlFeature();
            controlTimer = new TimerFeature() { Period = controlPeriod, Repeat = true };
            Position = Vector2.Zero;
            StartQueue = new Queue<StartAction>();

            this.contentManager = contentManager;
            this.spriteBatch = spriteBatch;
            this.maskAsset = maskAsset;
            start();
        }
        private void start()
        {
            if (Started)
                throw new Exception("Should not already be started.");
            maskTexture = contentManager.Load<Texture2D>(maskAsset);
            ControlFeatureObject.Activated = true;
            controlTimer.Activated = true;
            Started = true;
        }
        private void end()
        {
            if (!Started)
                throw new Exception("Should be started.");
            contentManager.UnloadAsset(maskAsset);
            ControlFeatureObject.Activated = false;
            controlTimer.Activated = false;
            Started = false;
        }
        public void Draw(Matrix? transformMatrix = null)
        {
            if (Started)
            {
                spriteBatch.Begin(transformMatrix: transformMatrix);
                spriteBatch.Draw(texture: maskTexture, position: Position, color: Color.White);
                spriteBatch.End();
            }
        }

        public void Update(float timeElapsed)
        {
            if (StartQueue.Count > 0)
            {
                var state = StartQueue.Dequeue();
                switch (state)
                {
                    case StartAction.Start:
                        start();
                        break;
                    case StartAction.End:
                        end();
                        break;
                }
            }

            if (Started)
            {
                while (ControlFeatureObject.InfoChannel.Count > 1)
                    ControlFeatureObject.InfoChannel.Dequeue();

                if (controlTimer.RunChannel.Count > 0)
                {
                    controlTimer.RunChannel.Dequeue();
                    float xMove = 0;
                    float yMove = 0;
                    if (ControlFeatureObject.InfoChannel.Count > 0)
                    {
                        var info = ControlFeatureObject.InfoChannel.Dequeue();
                        switch (info.Action)
                        {
                            case ControlAction.MoveUp:
                                yMove = -4;
                                break;
                            case ControlAction.MoveDown:
                                yMove = +4;
                                break;
                            case ControlAction.MoveLeft:
                                xMove = -4;
                                break;
                            case ControlAction.MoveRight:
                                xMove = +4;
                                break;
                        }
                    }
                    //if (xMove !=0 || yMove != 0)
                    //    Console.WriteLine($"DEBUG: {Position}");
                    Position += new Vector2(x: xMove, y: yMove);
                }

                controlTimer.Update(timeElapsed);
            }
        }
    }
}
