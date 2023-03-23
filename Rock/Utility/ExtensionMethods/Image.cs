// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Rock
{
    /// <summary>
    /// Handy <see cref="Image"/> extensions.
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region IImageProcessingContext Extensions

        /// <summary>
        /// Applies rounded corners to an image.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="cornerRadius"></param>
        /// <returns></returns>
        public static IImageProcessingContext ApplyRoundedCorners( this IImageProcessingContext ctx, float cornerRadius )
        {
            // Source: https://github.com/SixLabors/Samples/blob/main/ImageSharp/AvatarWithRoundedCorner/Program.cs

            // First create a square
            var rect = new RectangularPolygon( -0.5f, -0.5f, cornerRadius, cornerRadius );

            // Then cut out of the square a circle so we are left with a corner
            IPath cornerTopLeft = rect.Clip( new EllipsePolygon( cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius ) );

            var size = ctx.GetCurrentSize();

            float rightPos = size.Width - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = size.Height - cornerTopLeft.Bounds.Height + 1;

            // Move it across the width of the image - the width of the shape
            IPath cornerTopRight = cornerTopLeft.RotateDegree( 90 ).Translate( rightPos, 0 );
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree( -90 ).Translate( 0, bottomPos );
            IPath cornerBottomRight = cornerTopLeft.RotateDegree( 180 ).Translate( rightPos, bottomPos );

            var corners = new PathCollection( cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight );

            ctx.SetGraphicsOptions( new GraphicsOptions()
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // Enforces that any part of this shape that has color is punched out of the background
            } );

            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach ( var c in corners )
            {
                ctx = ctx.Fill( Color.Red, c );
            }
            return ctx;
        }

        /// <summary>
        /// Gives the corners a circle effect in a way where the image looks round on the edges (a full circle if the image is square).
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static IImageProcessingContext ApplyCircleCorners( this IImageProcessingContext ctx )
        {
            var size = ctx.GetCurrentSize();

            // 1/2 of the height + 2 px (to make up for the internal logic of rounded corners)
            ctx.ApplyRoundedCorners( ( size.Height / 2 ) + 2 );

            return ctx;
        }

        #endregion IImageProcessingContext Extensions

        #region Image Extensions

        /// <summary>
        /// Resizes the source image. Crops the resized image to fit the bounds of its container.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Image CropResize( this Image source, int width, int height )
        {
            var resizeOptions = new ResizeOptions() { Mode = ResizeMode.Crop, Size = new SixLabors.ImageSharp.Size( width, height ), Sampler = KnownResamplers.Lanczos3 };
            source.Mutate( i => i.Resize( resizeOptions ) );

            return source;
        }

        #endregion Image Extensions
    }
}
