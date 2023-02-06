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
        public CollisionFeature CollisionFeatureObject { get; private set; }
        public WallFeature(
            Vector2 position,
            Size size,
            Color[] mask,
            List<Vector2> vertices)
        {
            CollisionFeatureObject = new CollisionFeature()
            {
                Parent = this,
                Position = position,
                Size = size,
                Collidable = true,
                Static = true,
                CollisionMask = mask,
                CollisionVertices = vertices
            };
        }
    }
    internal interface LevelInterface
    {
        public LevelFeature LevelFeatureObject { get; }
    }
    internal class LevelFeature : UpdateInterface, DrawInterface, RoomInterface
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
        public RoomFeature RoomFeatureObject { get; private set; }
        public bool Started { get; private set; }
        public Queue<string> GoToQueue { get; private set; } // user -> feature
        public LevelFeature(
            SpriteBatch spriteBatch,
            ContentManager contentManager, 
            string roomIdentifier, 
            string environmentVisualAsset, 
            string environmentMaskAsset,
            Size environmentGridSize,
            Color environmentStartColor,
            Color environmentIncludeColor,
            Color environmentExcludeColor)
        {
            Started = false;
            RoomFeatureObject = new RoomFeature()
            {
                Identifier = roomIdentifier,
                UpdateObject = this,
                DrawObject = this
            };
            GoToQueue = new Queue<string>();
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
                        var wallMask = maskArray.Extract(size: environmentGridSize, region: new Rectangle(location: wallPosition, size: environmentGridSize));

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
                            collisionManager.Features.Add(wallFeature.CollisionFeatureObject);
                        }
                    }

                // The environment mask asset is no long needed now that the 
                // collision manager has been updated with all the grid features.
                contentManager.UnloadAsset(environmentMaskAsset);

                // Prevent this operation from occurring again.
                environmentMaskLoaded = true;
            }
            Started = true;
        }
        private void end()
        {
            // Can't end something that hasn't been started.
            if (!Started)
                throw new Exception("There should never be a case where the level is ended but is not started."); 

            contentManager.UnloadAsset(environmentVisualAsset);
            Started = false;
        }
        public void Update(float timeElapsed)
        {
            // If the level is activated, start the level.
            if (RoomFeatureObject.ActiveQueue.Count > 0)
            {
                RoomFeatureObject.ActiveQueue.Dequeue();
                start();
            }

            // If a new room is selected, end the level and then go to next room.
            if (GoToQueue.Count > 0)
            {
                var nextIdentifier = GoToQueue.Dequeue();
                end();
                RoomFeatureObject.GoToQueue.Enqueue(nextIdentifier);
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
