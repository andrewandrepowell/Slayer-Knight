using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public class AnimatorFeature : FeatureInterface<AnimatorManager>
    {
        public string Identifier { get; private set; }
        public Vector2 Offset { get; set; } = Vector2.Zero;
        AnimatorManager FeatureInterface<AnimatorManager>.ManagerObject { get; set; }
        public AnimatorFeature(string identifier)
        {
            Identifier = identifier;
        }
    }
    public class AnimatorManager : UpdateInterface, DrawInterface, ManagerInterface<AnimatorFeature>
    {
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private Dictionary<AnimatorFeature, AnimatedSprite> mapFeatureSprite = new Dictionary<AnimatorFeature, AnimatedSprite>();
        private AnimatedSprite curSprite = null;
        private Vector2 curOffset = Vector2.Zero;
        public IList<AnimatorFeature> Features { get; private set; }
        public Vector2 Position { get; set; } = Vector2.Zero;
        public AnimatedSprite GetSprite(AnimatorFeature feature) => mapFeatureSprite[feature];
        public void Play(AnimatorFeature feature, string animation, Action onCompleted = null)
        {
            curSprite = mapFeatureSprite[feature];
            curSprite.Play(name: animation, onCompleted: onCompleted);
            curOffset = feature.Offset;
        }
        public AnimatorManager(ContentManager contentManager, SpriteBatch spriteBatch)
        {
            Features = new DirectlyManagedList<AnimatorFeature, AnimatorManager>(this);
            this.contentManager = contentManager;
            this.spriteBatch = spriteBatch;
        }

        void ManagerInterface<AnimatorFeature>.DestroyFeature(AnimatorFeature feature)
        {
            contentManager.UnloadAsset(feature.Identifier);
            mapFeatureSprite.Remove(feature);
        }

        void ManagerInterface<AnimatorFeature>.SetupFeature(AnimatorFeature feature)
        {

            var spriteSheet = contentManager.Load<SpriteSheet>(feature.Identifier);
            var sprite = new AnimatedSprite(spriteSheet);
            mapFeatureSprite.Add(feature, sprite);
        }

        public void Update(float timeElapsed)
        {
            if (curSprite != null)
            {
                curSprite.Update(timeElapsed);
            }
        }

        public void Draw(Matrix? transformMatrix = null)
        {
            if (curSprite != null)
            {
                spriteBatch.Begin(transformMatrix: transformMatrix);
                spriteBatch.Draw(sprite: curSprite, position: Position + curOffset);
                spriteBatch.End();
            }
        }
    }
}
