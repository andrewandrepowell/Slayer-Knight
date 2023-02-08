using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace SlayerKnight
{
    internal class WallFeature : CollisionInterface
    {
        public WallFeature(
            Vector2 position,
            Size size,
            Color[] mask,
            List<Vector2> vertices)
        {
            Position = position;
            Size = size;
            CollisionMask = mask;
            CollisionVertices = vertices;
        }
        public Vector2 Position { get; private set; }
        public Size Size { get; private set; }
        public bool Collidable { get => true; }
        public bool Static { get => true; }
        public Color[] CollisionMask { get; private set; }
        public List<Vector2> CollisionVertices { get; private set; }
        public Channel<CollisionInfo> CollisionInfoChannel { get => throw new NotImplementedException(); }
        public bool Destroyed { get => false; }
        public Channel<object> DestroyChannel { get => throw new NotImplementedException(); }
    }
    internal class LevelFeature : RoomInterface
    {
        private bool environmentMaskLoaded;
        private SpriteBatch spriteBatch;
        private ContentManager contentManager;
        private CollisionManager collisionManager;
        private OrthographicCamera orthographicCamera;
        private string environmentVisualAsset;
        private string environmentMaskAsset;
        private Size environmentGridSize;
        private Texture2D environmentVisualTexture;
        private Color environmentStartColor;
        private Color environmentIncludeColor;
        private Color environmentExcludeColor;
        public bool Started { get; private set; }
        public Channel<string> GoToChannel { get; private set; }
        public Channel<StartAction> StartChannel { get; private set; }
        public string Identifier { get; private set; }
        public LevelFeature(
            ContentManager contentManager,
            SpriteBatch spriteBatch,
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
            StartChannel = new Channel<StartAction>();
            collisionManager = new CollisionManager();
            orthographicCamera = new OrthographicCamera(graphicsDevice: spriteBatch.GraphicsDevice);
            environmentMaskLoaded = false;
            this.spriteBatch = spriteBatch;
            this.contentManager = contentManager;
            this.environmentVisualAsset = environmentVisualAsset;
            this.environmentMaskAsset = environmentMaskAsset;
            this.environmentGridSize = environmentGridSize;
            this.environmentStartColor = environmentStartColor;
            this.environmentIncludeColor = environmentIncludeColor;
            this.environmentExcludeColor = environmentExcludeColor;
        }
        private void start()
        {
            // Can't start something that has already been started.
            if (Started)
                throw new Exception("There should never be case where the level is started when already started.");

            // Load static visual textures.
            environmentVisualTexture = contentManager.Load<Texture2D>(environmentVisualAsset);

            // Initilize environmental mask collision features.
            // This operation only needs to happen once.
            if (!environmentMaskLoaded)
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
                        // Get the position and mask of the wall.
                        var wallPosition = new Point(x: gridCol * environmentGridSize.Width, y: gridRow * environmentGridSize.Height);
                        var wallMask = maskArray.Extract(size: maskSize, region: new Rectangle(location: wallPosition, size: environmentGridSize));

                        // If the mask has at least one visible pixel--i.e. alpha not equal to 0--then determine grid vertices, 
                        // and create wall feature..
                        if (wallMask.Any(x => x.A != 0))
                        {
                            var gridVertices = CollisionManager.GetVertices(
                                maskData: wallMask, size: environmentGridSize, 
                                startColor: environmentStartColor, includeColor: environmentIncludeColor, excludeColor: environmentExcludeColor);
                            var wallFeature = new WallFeature(
                                position: wallPosition.ToVector2(),
                                size: environmentGridSize,
                                mask: wallMask,
                                vertices: gridVertices);
                            collisionManager.Features.Add(wallFeature);
                        }
                    }

                // The environment mask asset is no long needed now that the 
                // collision manager has been updated with all the grid features.
                contentManager.UnloadAsset(environmentMaskAsset);

                // Prevent this operation from occurring again.
                environmentMaskLoaded = true;
            }

            // Let the world know the level has started.
            Started = true;
        }
        private void end()
        {
            // Can't end something that hasn't been started.
            if (!Started)
                throw new Exception("There should never be a case where the level is ended but is not started."); 

            contentManager.UnloadAsset(environmentVisualAsset);

            // Let the world know the level has ended.
            Started = false;
        }
        public void Update(float timeElapsed)
        {
            // If the level is activated, start the level.
            if (StartChannel.Count > 0)
            {
                var action = StartChannel.Dequeue();
                switch (action)
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
                collisionManager.Update(timeElapsed);
            }    
        }
        public void Draw(Matrix? _ = null)
        {
            if (Started)
            {
                Matrix transformMatrix = orthographicCamera.GetViewMatrix();
                spriteBatch.Begin(transformMatrix: transformMatrix);
                spriteBatch.Draw(texture: environmentVisualTexture, position: Vector2.Zero, color: Color.White);
                spriteBatch.End();
            }
        }
    }
}
