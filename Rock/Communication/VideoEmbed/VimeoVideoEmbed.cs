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
using Newtonsoft.Json;
using RestSharp;

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
        public override string RegexFilter => @"vimeo.com";

        /*
            11/20/2020 - JME 
            The original expression from the PR was "(http|https)?:\/\/(www\.)?vimeo.com\/(?:channels\/(?:\w+\/)?|groups\/([^\/]*)\/videos\/|)(\d+)(?:|\/\?)" (from https://regexr.com/3begm)
            but this didn't allow https://player.vimeo.com. Since we no longer need to worry about match groups using the new oEmbedded API we updated this
            to be more forgiving to accept any type of Vimeo link.
        */

        /// <summary>
        /// Generates the thumbnail from video url
        /// </summary>
        /// <param name="videoUrl">The url of the video</param>
        /// <returns>Thumbnail url</returns>
        public override string GetThumbnail( string videoUrl )
        {
            /*  
                11/20/2020 - JME 
                Re-wrote the retrival of the meta data from Vimeo from the deprecated XML API ( syntax: http://vimeo.com/api/v2/video/{0}.xml )
                to the newer oEmbeded API https://developer.vimeo.com/api/oembed/videos.
            */

            var restClient = new RestClient( "https://vimeo.com" );
            var restRequest = new RestRequest( "api/oembed.json", Method.GET );
            restRequest.AddParameter( "url", videoUrl );
            restRequest.AddParameter( "width", "1280" ); // Tell Vimeo what size thumbnail we'd like

            var restResponse = restClient.Execute<VimeoResponse>( restRequest );

            // Return if video was not found.
            if ( restResponse.Data.IsNull() )
            {
                return string.Empty;
            }

            string imageUrl = restResponse.Data.ThumbnailUrl;

            // Download thumbnail image from Vimeo
            using ( WebClient webClient = new WebClient() )
            {
                byte[] data = webClient.DownloadData( imageUrl );

                using ( MemoryStream mem = new MemoryStream( data ) )
                {
                    using ( var thumbnail = Image.FromStream( mem ) )
                    {
                        return OverlayImage( thumbnail, $"Vimeo_{restResponse.Data.VideoId}", HttpContext.Current.Server.MapPath( "~/Assets/Images/vimeo-overlay.png" ) );
                    }
                }
            }
        }
    }

    #region POCOS
    /// <summary>
    /// POCO for the Vimeo response.
    /// </summary>
    public class VimeoResponse {

        // Note there's more in the return, but we only care about thumbnails.

        /// <summary>
        /// Gets or sets the video identifier.
        /// </summary>
        /// <value>
        /// The video identifier.
        /// </value>
        [JsonProperty( "video_id" )]
        public int VideoId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the thumbnail URL.
        /// </summary>
        /// <value>
        /// The thumbnail URL.
        /// </value>
        [JsonProperty( "thumbnail_url" )]
        public string ThumbnailUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the width of the thumbnail.
        /// </summary>
        /// <value>
        /// The width of the thumbnail.
        /// </value>
        [JsonProperty( "thumbnail_width" )]
        public int ThumbnailWidth { get; set; } = 0;

        /// <summary>
        /// Gets or sets the height of the thumbnail.
        /// </summary>
        /// <value>
        /// The height of the thumbnail.
        /// </value>
        [JsonProperty( "thumbnail_height" )]
        public int ThumbnailHeight { get; set; } = 0;
    }

    #endregion
}
