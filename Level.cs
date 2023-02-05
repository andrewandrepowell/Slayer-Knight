using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SlayerKnight
{
    internal class LevelFeature : UpdateInterface, DrawInterface, RoomInterface
    {
        private bool environmentMaskLoaded;
        private ContentManager contentManager;
        private CollisionManager collisionManager;
        private string environmentVisualAsset;
        private string environmentMaskAsset;
        private Size environmentGridSize;
        private Texture2D environmentVisualTexture;
        private Color environmentStartColor;
        private Color environmentIncludeColor;
        private Color environmentExcludeColor;
        public RoomFeature RoomFeatureObject { get; private set; }
        public bool Started { get; private set; }
        public LevelFeature(
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
            collisionManager = new CollisionManager();
            environmentMaskLoaded = false;
            this.contentManager = contentManager;
            this.environmentVisualAsset = environmentVisualAsset;
            this.environmentMaskAsset = environmentMaskAsset;
            this.environmentGridSize = environmentGridSize;
            this.environmentStartColor = environmentStartColor;
            this.environmentIncludeColor = environmentIncludeColor;
            this.environmentExcludeColor = environmentExcludeColor;
        }
        public void Start()
        {
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

                // Check each grid in the environment mask.
                int gridRows = maskTexture.Height / environmentGridSize.Height;
                int gridCols = maskTexture.Width / environmentGridSize.Width;
                int gridLength = environmentGridSize.Width * environmentGridSize.Height;
                for (int gridRow = 0; gridRow < gridRows; gridRow++)
                    for (int gridCol = 0; gridCol < gridCols; gridCol++)
                    {
                        // Get the position and mask of the grid.
                        var gridPosition = new Point(x: gridCol * environmentGridSize.Width, y: gridRow * environmentGridSize.Height);
                        var gridMask = maskArray.Extract(size: environmentGridSize, region: new Rectangle(location: gridPosition, size: environmentGridSize));

                        // If the mask has at least one visible pixel--i.e. alpha not equal to 0--then determine grid vertices, 
                        // create grid collision feature, and add the feature to the collision manager.
                        if (gridMask.Any(x => x.A != 0))
                        {
                            var gridVertices = CollisionManager.GetVertices(
                                maskData: gridMask, size: environmentGridSize, 
                                startColor: environmentStartColor, includeColor: environmentIncludeColor, excludeColor: environmentExcludeColor);
                            var gridCollisionFeature = new CollisionFeature()
                            {
                                Position = gridPosition.ToVector2(),
                                Size = environmentGridSize,
                                Collidable = true,
                                Static = true,
                                CollisionMask = gridMask,
                                CollisionVertices = gridVertices
                            };
                            collisionManager.Features.Add(gridCollisionFeature);
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
        public void End()
        {
            contentManager.UnloadAsset(environmentVisualAsset);
            Started = false;
        }
        public void Update(float timeElapsed)
        {
            if (Started)
            {
                collisionManager.Update(timeElapsed);
            }    
        }
        public void Draw(Matrix? transformMatrix)
        {

        }
    }
}
