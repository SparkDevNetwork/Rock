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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Rock.Communication.VideoEmbed
{
    /// <summary>
    /// Generates a thumbnail for YouTube videos
    /// </summary>
    [Export( typeof( VideoEmbedComponent ) )]
    [ExportMetadata( "ComponentName", "YouTube" )]
    [Description( "Generates YouTube video thumbnails for email." )]
    public class YoutubeEmbed : VideoEmbedComponent
    {
        /// <summary>
        /// Regex to check if is a YouTube video and extract information
        /// </summary>
        public override string RegexFilter => @"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";

        /// <summary>
        /// Generates the thumbnail from video url
        /// </summary>
        /// <param name="videoUrl">The url of the video</param>
        /// <returns>Thumbnail url</returns>
        public override string GetThumbnail( string videoUrl )
        {
            var match = Regex.Match( videoUrl, RegexFilter );
            var videoId = match.Groups[5].Value;

            // The best quality thumbnail will be the maxresdefault.jpg. If you're thumbnail comes back 4:3 with black letterboxing this image
            // wasn't available. The only work around is to have YouTube regenerate the thumbnail for you. You'll need to be account owner
            // to be able to do this.
            var imageUrl = string.Format( "https://img.youtube.com/vi/{0}/maxresdefault.jpg", videoId );

            using ( WebClient client = new WebClient() )
            {
                byte[] data;
                try
                {
                    data = client.DownloadData( imageUrl );
                }
                catch
                {
                    //not as good of a thumbnail
                    imageUrl = string.Format( "https://img.youtube.com/vi/{0}/0.jpg", videoId );
                    data = client.DownloadData( imageUrl );
                }
                using ( MemoryStream mem = new MemoryStream( data ) )
                {
                    using ( var thumbnail = Image.FromStream( mem ) )
                    {
                        return OverlayImage( thumbnail, "YouTube_" + videoId, HttpContext.Current.Server.MapPath( "~/Assets/Images/youtube-overlay.png" ) );
                    }
                }
            }
        }
    }
}
