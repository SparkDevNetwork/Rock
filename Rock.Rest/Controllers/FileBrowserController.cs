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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using ImageResizer;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class FileBrowserController : ApiController
    {
        /// <summary>
        /// Gets the file thumbnail
        /// </summary>
        /// <param name="relativeFilePath">The relative file path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        /// <example>
        ///   <![CDATA[ <img src='api/FileBrowser/GetFileThumbnail?relativeFilePath=External+Site%5cMarketing%5cFunnyCat.gif&width=100&height=100 ]]>
        /// </example>
        [HttpGet]
        [System.Web.Http.Route( "api/FileBrowser/GetFileThumbnail" )]
        public HttpResponseMessage GetFileThumbnail( string relativeFilePath, int? width = 100, int? height = 100 )
        {
            string physicalFilePath = HttpContext.Current.Request.MapPath( relativeFilePath );
            string fullPath = physicalFilePath;

            // default width/height to 100 if they specified a zero or negative param
            width = width <= 0 ? 100 : width;
            height = height <= 0 ? 100 : height;

            // return a 404 if the file doesn't exist
            if ( !File.Exists( fullPath ) )
            {
                throw new HttpResponseException( new System.Net.Http.HttpResponseMessage( HttpStatusCode.NotFound ) );
            }

            string mimeType = System.Web.MimeMapping.GetMimeMapping( physicalFilePath );
            if (mimeType.StartsWith("image/"))
            {
                return ResizeAndSendImage( width, height, fullPath );
            }
            else
            {
                // figure out the extension of the file
                string fileExtension = Path.GetExtension( relativeFilePath ).TrimStart( '.' );
                string virtualThumbnailFilePath = string.Format( "~/Assets/Icons/FileTypes/{0}.png", fileExtension);
                string thumbnailFilePath = HttpContext.Current.Request.MapPath( virtualThumbnailFilePath );
                if (!File.Exists(thumbnailFilePath))
                {
                    virtualThumbnailFilePath = "~/Assets/Icons/FileTypes/other.png";
                    thumbnailFilePath = HttpContext.Current.Request.MapPath( virtualThumbnailFilePath );
                }

                return ResizeAndSendImage( width, height, thumbnailFilePath );
            }
        }

        /// <summary>
        /// Resizes the and send image.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="fullPath">The full path.</param>
        /// <returns></returns>
        private HttpResponseMessage ResizeAndSendImage( int? width, int? height, string fullPath )
        {
            if ( Path.GetExtension( fullPath ).Equals( ".svg", StringComparison.OrdinalIgnoreCase ) )
            {
                HttpResponseMessage result = new HttpResponseMessage( HttpStatusCode.OK );
                result.Content = new StreamContent( new FileStream( fullPath, FileMode.Open, FileAccess.Read ) );
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "image/svg+xml" );
                return result;
            }
            else
            {
                try {
                    using ( Image image = Image.FromFile( fullPath ) )
                    {
                        string mimeType = string.Empty;

                        // try to figure out the MimeType by using the ImageCodeInfo class
                        var codecs = ImageCodecInfo.GetImageEncoders();
                        ImageCodecInfo codecInfo = codecs.FirstOrDefault( a => a.FormatID == image.RawFormat.Guid );
                        if ( codecInfo != null )
                        {
                            mimeType = codecInfo.MimeType;
                        }

                        // load the image into a stream, then use ImageResizer to resize it to the specified width and height (same technique as RockWeb GetImage.ashx.cs)
                        var origImageStream = new MemoryStream();
                        image.Save( origImageStream, image.RawFormat );
                        origImageStream.Position = 0;
                        var resizedStream = new MemoryStream();

                        ImageBuilder.Current.Build( origImageStream, resizedStream, new ResizeSettings { Width = width ?? 100, Height = height ?? 100 } );

                        HttpResponseMessage result = new HttpResponseMessage( HttpStatusCode.OK );
                        resizedStream.Position = 0;
                        result.Content = new StreamContent( resizedStream );
                        result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( mimeType );

                        if ( this.Request.GetQueryString( "timeStamp" ) != null )
                        {
                            // set the CacheControl to 365 days since the Request URL timestamp param will change if the file changes
                            result.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { Public = true, MaxAge = new TimeSpan( 365, 0, 0, 0 ) };
                        }

                        return result;
                    }
                }
                catch 
                {
                    
                    // there was a problem with the image so send the default corrupt image warning back
                    string imagePath = HttpContext.Current.Request.MapPath( "~/Assets/Images/corrupt-image.jpg" );
                    
                    // return a 404 if the file doesn't exist
                    if ( !File.Exists( imagePath ) )
                    {
                        throw new HttpResponseException( new System.Net.Http.HttpResponseMessage( HttpStatusCode.NotFound ) );
                    }

                    using ( Image image = Image.FromFile( imagePath ) )
                    {
                        string mimeType = "image/jpg";
                        var origImageStream = new MemoryStream();
                        image.Save( origImageStream, image.RawFormat );
                        origImageStream.Position = 0;
                        var resizedStream = new MemoryStream();

                        ImageBuilder.Current.Build( origImageStream, resizedStream, new ResizeSettings { Width = width ?? 100, Height = height ?? 100 } );

                        HttpResponseMessage result = new HttpResponseMessage( HttpStatusCode.OK );
                        resizedStream.Position = 0;
                        result.Content = new StreamContent( resizedStream );
                        result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( mimeType );

                        return result;
                    }
                }
            }
        }
    }
}
