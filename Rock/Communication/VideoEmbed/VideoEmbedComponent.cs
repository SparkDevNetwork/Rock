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
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Communication.VideoEmbed
{
    /// <summary>
    /// Used for embeded video links in communications.
    /// </summary>
    /// <seealso cref="Rock.Extension.Component" />
    public abstract class VideoEmbedComponent : Component
    {
        private const int _scaleWidth = 1280; // 640 x2 for retina

        /// <summary>
        /// Regular expression to detect if this component applies to the url.
        /// </summary>
        public abstract string RegexFilter { get; }

        /// <summary>
        /// Generates a preview image for a video and returns its url
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <returns></returns>
        public abstract string GetThumbnail( string videoUrl );

        /// <summary>
        /// Adds the play button on thumbnail and returns its url
        /// </summary>
        /// <param name="image"></param>
        /// <param name="fileName"></param>
        /// <param name="overlay"></param>
        /// <returns></returns>
        public string OverlayImage( Image image, string fileName, string overlay )
        {
            var rockContext = new RockContext();
            var binaryFileTypeService = new BinaryFileTypeService( rockContext );
            var binaryFileType = binaryFileTypeService.Get( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_IMAGE.AsGuid() );
            var binaryFileService = new BinaryFileService( rockContext );

            // If a thumbnail of the video has already been made, lets not make another
            var preMadeThumbnail = binaryFileService.Queryable()
                .Where( f => f.FileName == fileName && f.BinaryFileTypeId == binaryFileType.Id )
                .FirstOrDefault();

            if ( preMadeThumbnail != null )
            {
                return string.Format( "{0}/GetImage.ashx/Thumbnail{1}.png?guid={1}&filename={2}",
                    GlobalAttributesCache.Value( "PublicApplicationRoot" ).Trim( '/' ),
                    preMadeThumbnail.Guid,
                    preMadeThumbnail.FileName );
            }


            image = ScaleImage( image );
            var overlayImg = Image.FromFile( overlay );

            var size = Math.Min( image.Width, image.Height );

            overlayImg = new Bitmap( overlayImg, size, size );
            var img = new Bitmap( image.Width, image.Height );
            using ( Graphics gr = Graphics.FromImage( img ) )
            {
                gr.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                gr.DrawImage( image, new Point( 0, 0 ) );
                gr.DrawImage( overlayImg, new Point( ( image.Width / 2 ) - ( size / 2 ), 0 ) );

                var stream = new System.IO.MemoryStream();
                img.Save( stream, ImageFormat.Png );
                stream.Position = 0;

                var binaryImage = new BinaryFile
                {
                    FileName = fileName + ".png",
                    Guid = Guid.NewGuid(),
                    BinaryFileTypeId = binaryFileType.Id,
                    MimeType = "image/png",
                    IsTemporary = false,
                    ContentStream = stream
                };
                binaryFileService.Add( binaryImage );
                rockContext.SaveChanges();

                return string.Format( "{0}/GetImage.ashx/Thumbnail{1}.png?guid={1}&filename={2}",
                   GlobalAttributesCache.Value( "PublicApplicationRoot" ).Trim( '/' ),
                   binaryImage.Guid,
                   binaryImage.FileName );
            }
        }

        /// <summary>
        /// Resizes thumbnail to the scaled width constant. 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static Image ScaleImage( Image image )
        {
            var ratio = ( double ) _scaleWidth / image.Width;
            var newHeight = ( int ) ( image.Height * ratio );
            var newImage = new Bitmap( _scaleWidth, newHeight );

            using ( var graphics = Graphics.FromImage( newImage ) )
            {
                graphics.DrawImage( image, 0, 0, _scaleWidth, newHeight );
            }

            return newImage;
        }
    }
}
