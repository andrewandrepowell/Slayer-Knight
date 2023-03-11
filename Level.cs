using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;
using SlayerKnight.Components;
using System.Linq.Expressions;
using System.Collections;

namespace SlayerKnight
{ 
    internal interface LevelInterface : RoomInterface
    {
        public Size ScreenSize { get; }
        public Size LevelSize { get; }
        public OrthographicCamera CameraObject { get; }
        public IList<ComponentInterface> Features { get; }
    }
    internal class LevelFeature : LevelInterface
    {
        public class ComponentList : IList<ComponentInterface>
        {
            private LevelFeature levelFeature;
            private List<ComponentInterface> componentFeatures = new List<ComponentInterface>();
            private void setup(ComponentInterface componentFeature)
            {
                if (componentFeature is CollisionInterface collisionFeature)
                    levelFeature.collisionManager.Features.Add(collisionFeature);
                if (componentFeature is HasControlInterface controlHolder)
                    levelFeature.controlManager.Features.Add(controlHolder.ControlFeatureObject);
                if (componentFeature is HasKeyboardInterface keyboardHolder)
                    levelFeature.keyboardManager.Features.Add(keyboardHolder.keyboardFeatureObject);
                if (componentFeature is HasSoundInterface soundHolder)
                    levelFeature.optionsManager.Features.Add(soundHolder.SoundManagerObject);
            }
            private void destroy(ComponentInterface componentFeature)
            {
                if (componentFeature is CollisionInterface collisionFeature)
                    levelFeature.collisionManager.Features.Remove(collisionFeature);
                if (componentFeature is HasControlInterface controlFeature)
                    levelFeature.controlManager.Features.Remove(controlFeature.ControlFeatureObject);
                if (componentFeature is HasKeyboardInterface keyboardHolder)
                    levelFeature.keyboardManager.Features.Remove(keyboardHolder.keyboardFeatureObject);
                if (componentFeature is HasSoundInterface soundHolder)
                    levelFeature.optionsManager.Features.Remove(soundHolder.SoundManagerObject);
            }
            public ComponentList(LevelFeature levelFeature) => this.levelFeature = levelFeature;
            public ComponentInterface this[int index] { get => componentFeatures[index]; set => throw new NotImplementedException(); }
            public int Count => componentFeatures.Count;
            public bool IsReadOnly => (componentFeatures as ICollection<ComponentInterface>).IsReadOnly;
            public void Add(ComponentInterface item)
            {
                if (componentFeatures.Contains(item))
                    throw new Exception("Duplicate components are not allowed.");
                setup(item);
                componentFeatures.Add(item);
            }
            public void Clear()
            {
                foreach (var componentFeature in componentFeatures.ToList())
                {
                    componentFeatures.Remove(componentFeature);
                    destroy(componentFeature);
                }
            }
            public bool Contains(ComponentInterface item) => componentFeatures.Contains(item);
            public void CopyTo(ComponentInterface[] array, int arrayIndex)
            {
                foreach (var item in array)
                {
                    if (componentFeatures.Contains(item))
                        throw new Exception("Duplicate components are not allowed.");
                    setup(item);
                }
                componentFeatures.CopyTo(array, arrayIndex);
            }
            public IEnumerator<ComponentInterface> GetEnumerator() => componentFeatures.GetEnumerator();
            public int IndexOf(ComponentInterface item) => componentFeatures.IndexOf(item);
            public void Insert(int index, ComponentInterface item)
            {
                if (componentFeatures.Contains(item))
                    throw new Exception("Duplicate components are not allowed.");
                setup(item);
                componentFeatures.Insert(index, item);
            }
            public bool Remove(ComponentInterface item)
            {
                var removed = componentFeatures.Remove(item);
                if (removed)
                    destroy(item);
                return removed;
            }
            public void RemoveAt(int index)
            {
                var item = this[index];
                componentFeatures.RemoveAt(index);
                destroy(item);
            }
            IEnumerator IEnumerable.GetEnumerator() => componentFeatures.GetEnumerator();
        }
        private SpriteBatch spriteBatch;
        private ContentManager contentManager;
        private CollisionManager collisionManager;
        private ControlManager controlManager;
        private OptionsManager optionsManager;
        private KeyboardManager keyboardManager;
        private List<DestroyInterface> destroyFeatures;
        private string environmentVisualAsset;
        private string environmentMaskAsset;
        private string backgroundVisualAsset;
        private Size environmentGridSize;
        private Texture2D environmentVisualTexture;
        private Texture2D backgroundVisualTexture;
        private Color environmentStartColor;
        private Color environmentIncludeColor;
        private Color environmentExcludeColor;
        public IList<ComponentInterface> Features { get; private set; }
        public bool Started { get; private set; }
        public string Identifier { get; private set; }
        public OrthographicCamera CameraObject { get; private set; }
        public Size LevelSize { get => environmentVisualTexture.Bounds.Size; }
        public Size ScreenSize { get; private set; }
        RoomManager FeatureInterface<RoomManager>.ManagerObject { get; set; }
        public LevelFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            KeyboardManager keyboardManager,
            ControlManager controlManager,
            OptionsManager optionsManager,
            string roomIdentifier, 
            string environmentVisualAsset, 
            string environmentMaskAsset,
            string backgroundVisualAsset,
            Size environmentGridSize,
            Color environmentStartColor,
            Color environmentIncludeColor,
            Color environmentExcludeColor,
            Size screenSize)
        {
            Features = new ComponentList(this);
            Identifier = roomIdentifier;
            Started = false;
            collisionManager = new CollisionManager();
            destroyFeatures = new List<DestroyInterface>();
            CameraObject = new OrthographicCamera(graphicsDevice: spriteBatch.GraphicsDevice);
            ScreenSize = screenSize;
            this.spriteBatch = spriteBatch;
            this.contentManager = contentManager;
            this.keyboardManager = keyboardManager;
            this.controlManager = controlManager;
            this.optionsManager = optionsManager;
            this.environmentVisualAsset = environmentVisualAsset;
            this.environmentMaskAsset = environmentMaskAsset;
            this.backgroundVisualAsset = backgroundVisualAsset;
            this.environmentGridSize = environmentGridSize;
            this.environmentStartColor = environmentStartColor;
            this.environmentIncludeColor = environmentIncludeColor;
            this.environmentExcludeColor = environmentExcludeColor;
        }
        public void Start()
        {
            // Can't start something that has already been started.
            if (Started)
                throw new Exception("There should never be case where the level is started when already started.");

            // Load static visual textures.
            backgroundVisualTexture = contentManager.Load<Texture2D>(backgroundVisualAsset);
            environmentVisualTexture = contentManager.Load<Texture2D>(environmentVisualAsset);

            // Generate features from environment mask.
            {
                // Load the mask as a texture.
                var maskTexture = contentManager.Load<Texture2D>(environmentMaskAsset);

                // Verify parameters are consistent.
                if (environmentVisualTexture.Width != maskTexture.Width || environmentVisualTexture.Height != maskTexture.Height)
                    throw new ArgumentException("Dimensions of environment visual and environment mask should be the same.");
                if (maskTexture.Width % environmentGridSize.Width != 0 || maskTexture.Height % environmentGridSize.Height != 0)
                    throw new ArgumentException("Dimensions of environment mask should be a multiple of environment grid size.");

                // Get the raw color data.
                var maskArray = new Color[maskTexture.Width * maskTexture.Height];
                var maskSize = new Size(width: maskTexture.Width, height: maskTexture.Height);
                maskTexture.GetData(maskArray);

                // Check each wall in the environment mask.
                int gridRows = maskTexture.Height / environmentGridSize.Height;
                int gridCols = maskTexture.Width / environmentGridSize.Width;
                int gridLength = environmentGridSize.Width * environmentGridSize.Height;
                for (int gridRow = 0; gridRow < gridRows; gridRow++)
                    for (int gridCol = 0; gridCol < gridCols; gridCol++)
                    {
                        // Get the position and mask of the grid.
                        var gridPosition = new Vector2(x: gridCol * environmentGridSize.Width, y: gridRow * environmentGridSize.Height);
                        var gridMask = maskArray.Extract(size: maskSize, region: new Rectangle(location: gridPosition.ToPoint(), size: environmentGridSize));

                        // Attempt to generate component. If the component exists, add it to the component feature list.
                        ComponentInterface componentFeature = ComponentManager.GetComponentFeature(
                            identifier: gridMask[0],
                            contentManager: contentManager,
                            spriteBatch: spriteBatch,
                            position: gridPosition,
                            levelFeature: this);
                        if (componentFeature != null)
                            Features.Add(componentFeature);

                        // If the mask has at least one visible pixel--i.e. alpha not equal to 0--then determine grid vertices, 
                        // and create wall feature..
                        else if (gridMask.Any(x => x.A != 0))
                        {
                            var wallFeature = ComponentManager.GetWallComponentFeature(
                                mask: gridMask,
                                size: environmentGridSize,
                                start: environmentStartColor,
                                include: environmentIncludeColor,
                                exclude: environmentExcludeColor,
                                position: gridPosition);
                            if (wallFeature == null)
                                throw new Exception("Uncreognizable wall component.");
                            Features.Add(wallFeature);
                        }
                    }

                // The environment mask asset is no longer needed now that the 
                // collision manager has been updated with all the grid features.
                contentManager.UnloadAsset(environmentMaskAsset);
            }

            // Let the world know the level has started.
            Started = true;
        }
        public void End()
        {
            // Can't end something that hasn't been started.
            if (!Started)
                throw new Exception("There should never be a case where the level is ended but is not started.");

            // Remove all the component features. 
            Features.Clear();

            // Unload all assets.
            contentManager.Unload();

            // Let the world know the level has ended.
            Started = false;
        }
        public void Update(float timeElapsed)
        {
            // Update the state of each component.
            foreach (var component in Features)
            {
                // Perform the update.
                component.Update(timeElapsed);

                // Any destroyed interfaces should be added to the list to get removed later.
                if (component is DestroyInterface destroy && destroy.Destroyed)
                    destroyFeatures.Add(destroy);
            }

            // Remove any destroyed components.
            foreach (var destroy in destroyFeatures)
                Features.Remove(destroy as ComponentInterface);
            destroyFeatures.Clear();
        }
        public void Draw(Matrix? _ = null)
        {
            Matrix transformMatrix = CameraObject.GetViewMatrix();

            if (Started)
            {
                // Draw the environment visual texture.
                spriteBatch.Begin(transformMatrix: transformMatrix);
                spriteBatch.Draw(texture: backgroundVisualTexture, position: CameraObject.Position, color: Color.White);
                spriteBatch.Draw(texture: environmentVisualTexture, position: Vector2.Zero, color: Color.White);
                spriteBatch.End();
            }

            // Draw all the other components.
            if (Features.Count > 0)
            {
                int minDrawLevel = Features.Select(x => x.DrawLevel).Min();
                int maxDrawLevel = Features.Select(x => x.DrawLevel).Max();
                for (int curr = minDrawLevel; curr <= maxDrawLevel; curr++)
                    foreach (var component in Features)
                        if (component.DrawLevel == curr)
                            component.Draw(transformMatrix);
            }
        }
    }
}
