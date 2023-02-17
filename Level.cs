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

namespace SlayerKnight
{ 
    internal class LevelFeature : RoomInterface
    {
        private SpriteBatch spriteBatch;
        private ContentManager contentManager;
        private CollisionManager collisionManager;
        private OrthographicCamera orthographicCamera;
        private ControlManager controlManager;
        private KeyboardManager keyboardManager;
        private List<ComponentInterface> componentFeatures;
        private List<DestroyInterface> destroyFeatures;
        private string environmentVisualAsset;
        private string environmentMaskAsset;
        private Size environmentGridSize;
        private Texture2D environmentVisualTexture;
        private Color environmentStartColor;
        private Color environmentIncludeColor;
        private Color environmentExcludeColor;
        public bool Started { get; private set; }
        public ChannelInterface<string> GoToChannel { get; private set; }
        public string Identifier { get; private set; }
        public LevelFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
            KeyboardManager keyboardManager,
            ControlManager controlManager,
            string roomIdentifier, 
            string environmentVisualAsset, 
            string environmentMaskAsset,
            Size environmentGridSize,
            Color environmentStartColor,
            Color environmentIncludeColor,
            Color environmentExcludeColor)
        {
            Identifier = roomIdentifier;
            Started = false;
            GoToChannel = new Channel<string>();
            collisionManager = new CollisionManager();
            orthographicCamera = new OrthographicCamera(graphicsDevice: spriteBatch.GraphicsDevice);
            componentFeatures = new List<ComponentInterface>();
            destroyFeatures = new List<DestroyInterface>();
            this.spriteBatch = spriteBatch;
            this.contentManager = contentManager;
            this.keyboardManager = keyboardManager;
            this.controlManager = controlManager;
            this.environmentVisualAsset = environmentVisualAsset;
            this.environmentMaskAsset = environmentMaskAsset;
            this.environmentGridSize = environmentGridSize;
            this.environmentStartColor = environmentStartColor;
            this.environmentIncludeColor = environmentIncludeColor;
            this.environmentExcludeColor = environmentExcludeColor;
        }
        private void add(ComponentInterface component)
        {
            componentFeatures.Add(component);
            if (component is CollisionInterface collisionFeature)
                collisionManager.Features.Add(collisionFeature);
            if (component is ControlInterface controlHolder)
                controlManager.Features.Add(controlHolder.ControlFeatureObject);
            if (component is KeyboardInterface keyboardHolder)
                keyboardManager.Features.Add(keyboardHolder.keyboardFeatureObject);
        }
        private void remove(ComponentInterface component)
        {
            componentFeatures.Remove(component);
            if (component is CollisionInterface collisionFeature)
                collisionManager.Features.Remove(collisionFeature);
            if (component is ControlInterface controlFeature)
                controlManager.Features.Remove(controlFeature.ControlFeatureObject);
            if (component is KeyboardInterface keyboardHolder)
                keyboardManager.Features.Remove(keyboardHolder.keyboardFeatureObject);
        }
        public void Start()
        {
            // Can't start something that has already been started.
            if (Started)
                throw new Exception("There should never be case where the level is started when already started.");

            // Load static visual textures.
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
                            roomIdentifier: Identifier,
                            goToOutput: GoToChannel);
                        if (componentFeature != null)
                            add(componentFeature);

                        // If the mask has at least one visible pixel--i.e. alpha not equal to 0--then determine grid vertices, 
                        // and create wall feature..
                        else if (gridMask.Any(x => x.A != 0))
                        {
                            var gridVertices = CollisionManager.GetVertices(
                                maskData: gridMask, size: environmentGridSize,
                                startColor: environmentStartColor, includeColor: environmentIncludeColor, excludeColor: environmentExcludeColor);
                            var wallFeature = new WallComponentFeature(
                                position: gridPosition,
                                size: environmentGridSize,
                                mask: gridMask,
                                vertices: gridVertices);
                            add(wallFeature);
                        }
                    }

                // The environment mask asset is no long needed now that the 
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

            // Unload assets.
            contentManager.UnloadAsset(environmentVisualAsset);

            // Remove all the component features. 
            // Don't remove components that implement the destroy interface immediately, instead set them to get destroyed and then get cleaned up later.  
            foreach (var feature in componentFeatures.ToList())
            {
                if (feature is DestroyInterface destroyFeature)
                    destroyFeature.Destroy();
                else remove(feature);   
            }

            // Let the world know the level has ended.
            Started = false;
        }
        public void Update(float timeElapsed)
        {
            // Update the components.
            foreach (var component in componentFeatures)
                component.Update(timeElapsed);

            // Remove any destroyed components.
            destroyFeatures.Clear();
            foreach (var component in componentFeatures.OfType<DestroyInterface>())
                if (component.Destroyed)
                    destroyFeatures.Add(component);
            foreach (var destroy in destroyFeatures)
               remove(destroy as ComponentInterface);
        }
        public void Draw(Matrix? _ = null)
        {
            Matrix transformMatrix = orthographicCamera.GetViewMatrix();

            if (Started)
            {
                // Draw the environment visual texture.
                spriteBatch.Begin(transformMatrix: transformMatrix);
                spriteBatch.Draw(texture: environmentVisualTexture, position: Vector2.Zero, color: Color.White);
                spriteBatch.End();
            }

            // Draw all the other components.
            if (componentFeatures.Count > 0)
            {
                int minDrawLevel = componentFeatures.Select(x => x.DrawLevel).Min();
                int maxDrawLevel = componentFeatures.Select(x => x.DrawLevel).Max();
                for (int curr = minDrawLevel; curr <= maxDrawLevel; curr++)
                    foreach (var component in componentFeatures)
                        if (component.DrawLevel == curr)
                            component.Draw(transformMatrix);
            }
        }
    }
}
