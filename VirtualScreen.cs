using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class VirtualScreen : DrawInterface
    {
        private Rectangle destinationRectangle;
        private GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;
        public VirtualScreen(SpriteBatch spriteBatch, int screenScalar = 2)
        {
            if (screenScalar < 1)
                throw new ArgumentOutOfRangeException("screenScalar must be greater than or equal to 1.");
            this.spriteBatch = spriteBatch;
            graphicsDevice = spriteBatch.GraphicsDevice;
            renderTarget = new RenderTarget2D(
                graphicsDevice: graphicsDevice,
                width: graphicsDevice.Viewport.Width,
                height: graphicsDevice.Viewport.Height);
            destinationRectangle = new Rectangle(x: 0, y: 0, width: graphicsDevice.Viewport.Width * screenScalar, height: graphicsDevice.Viewport.Height * screenScalar);
        }

        public void BeginCapture() => graphicsDevice.SetRenderTarget(renderTarget);
        public void EndCapture() => graphicsDevice.SetRenderTarget(null);
        public void Draw(Matrix? _ = null)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            //spriteBatch.Draw(texture: renderTarget, destinationRectangle: destinationRectangle, color: Color.White);
            spriteBatch.Draw(texture: renderTarget, position: Vector2.Zero, sourceRectangle: null, color: Color.White, rotation: 0, origin: Vector2.Zero, scale: 2, effects: SpriteEffects.None, layerDepth: 0);
            spriteBatch.End();
        }

    }
}
