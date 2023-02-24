using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;
using MonoGame.Extended.Serialization;
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
        public AnimatedSprite Sprite { get => (this as FeatureInterface<AnimatorManager>).ManagerObject.GetSprite(this); }
        public SpriteSheetAnimation Play(string animation, Action onCompleted = null) =>
            (this as FeatureInterface<AnimatorManager>).ManagerObject.Play(
                feature: this, 
                animation: animation, 
                onCompleted: onCompleted);
        public AnimatorFeature(string identifier)
        {
            Identifier = identifier;
        }
        AnimatorManager FeatureInterface<AnimatorManager>.ManagerObject { get; set; }
    }
    public class AnimatorManager : UpdateInterface, DrawInterface, ManagerInterface<AnimatorFeature>
    {
        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private Dictionary<AnimatorFeature, AnimatedSprite> mapFeatureSprite = new Dictionary<AnimatorFeature, AnimatedSprite>();
        public Vector2 CurrentOffset { get; set; } = Vector2.Zero;
        public IList<AnimatorFeature> Features { get; private set; }
        public Vector2 Position { get; set; } = Vector2.Zero;
        public AnimatedSprite GetSprite(AnimatorFeature feature) => mapFeatureSprite[feature];
        public AnimatorFeature CurrentFeature { get; private set; } = null;
        public AnimatedSprite CurrentSprite { get; private set; } = null;
        public SpriteSheetAnimation CurrentSpriteSheetAnimation { get; private set; } = null;
        public SpriteSheetAnimation Play(AnimatorFeature feature, string animation, Action onCompleted = null)
        {
            CurrentOffset = feature.Offset;
            CurrentFeature = feature;
            CurrentSprite = mapFeatureSprite[feature];
            CurrentSpriteSheetAnimation = CurrentSprite.Play(name: animation, onCompleted: onCompleted);
            return CurrentSpriteSheetAnimation;
        }
        public AnimatorManager(ContentManager contentManager, SpriteBatch spriteBatch)
        {
            Features = new DirectlyManagedList<AnimatorFeature, AnimatorManager>(this);
            this.contentManager = contentManager;
            this.spriteBatch = spriteBatch;
        }

        void ManagerInterface<AnimatorFeature>.DestroyFeature(AnimatorFeature feature)
        {
            mapFeatureSprite.Remove(feature);
        }

        void ManagerInterface<AnimatorFeature>.SetupFeature(AnimatorFeature feature)
        {

            var spriteSheet = contentManager.Load<SpriteSheet>(feature.Identifier, new JsonContentLoader());
            var sprite = new AnimatedSprite(spriteSheet);
            mapFeatureSprite.Add(feature, sprite);
        }

        public void Update(float timeElapsed)
        {
            if (CurrentSprite != null)
            {
                CurrentSprite.Update(timeElapsed);
            }
        }

        public void Draw(Matrix? transformMatrix = null)
        {
            if (CurrentSprite != null)
            {
                spriteBatch.Begin(transformMatrix: transformMatrix);
                spriteBatch.Draw(sprite: CurrentSprite, position: Position + CurrentOffset);
                spriteBatch.End();
            }
        }
    }
}
