using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Utility
{
    public static class ColorExtensions
    {
        public static Color Add(this Color color, Color color2)
        {
            return new Color(
                r: Math.Min(color.R + color2.R, 255),
                g: Math.Min(color.G + color2.G, 255),
                b: Math.Min(color.B + color2.B, 255),
                alpha: Math.Min(color.A + color2.A, 255));
        }

        public static Color[] Extract(this Color[] image, Size size, Rectangle region)
        {
            if ((size.Width * size.Height) != image.Length)
                throw new ArgumentException($"Length of image {image.Length} should equal the product of size Width {size.Width} Height {size.Height}.");
            if (region.X < 0 || (region.X + region.Width) > size.Width || region.Y < 0 || (region.Y + region.Height) > size.Height)
                throw new ArgumentException($"Region {region} should fit in image.");
            Color[] extracted = new Color[image.Length];
            int bottom = region.Y + region.Height;
            int right = region.X + region.Width;
            int index = 0;
            for (int row = region.Y; row < bottom; row++)
                for (int col = region.X; col < right; col++)
                    extracted[index++] = image[col + row * size.Width];
            return extracted;
        }
    }
}
