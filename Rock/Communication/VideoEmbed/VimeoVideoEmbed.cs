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
using System.Xml;

namespace Rock.Communication.VideoEmbed
{
    /// <summary>
    /// Generates a thumbnail for Vimeo videos
    /// </summary>
    [Export( typeof( VideoEmbedComponent ) )]
    [ExportMetadata( "ComponentName", "Vimeo" )]
    [Description( "Generates Vimeo video thumbnails for email." )]
    public class VimeoVideoEmbed : VideoEmbedComponent
    {
        /// <summary>
        /// Regex to check if is a Vimeo video and extract information
        /// </summary>
        public override string RegexFilter => @"(http|https)?:\/\/(www\.)?vimeo.com\/(?:channels\/(?:\w+\/)?|groups\/([^\/]*)\/videos\/|)(\d+)(?:|\/\?)";

        /// <summary>
        /// Generates the thumbnail from video url
        /// </summary>
        /// <param name="videoUrl">The url of the video</param>
        /// <returns>Thumbnail url</returns>
        public override string GetThumbnail( string videoUrl )
        {
            var match = Regex.Match( videoUrl, RegexFilter );
            var videoId = match.Groups[4].Value;
            var apiURL = string.Format( "http://vimeo.com/api/v2/video/{0}.xml", videoId );

            XmlDocument doc = new XmlDocument();
            doc.Load( apiURL );
            XmlElement root = doc.DocumentElement;
            string imageUrl = root.FirstChild.SelectSingleNode( "thumbnail_large" ).ChildNodes[0].Value;

            using ( WebClient client = new WebClient() )
            {
                byte[] data = client.DownloadData( imageUrl );

                using ( MemoryStream mem = new MemoryStream( data ) )
                {
                    using ( var thumbnail = Image.FromStream( mem ) )
                    {
                        return OverlayImage( thumbnail, "Vimeo_" + videoId + ".png", HttpContext.Current.Server.MapPath( "~/Assets/Images/vimeo-overlay.png" ) );
                    }
                }
            }
        }
    }
}
