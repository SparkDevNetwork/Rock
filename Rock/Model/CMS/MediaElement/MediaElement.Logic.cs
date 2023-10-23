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


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using Rock.Media;

namespace Rock.Model
{
    public partial class MediaElement
    {
        #region Properties

        /// <summary>
        /// Gets the default file URL to use for media playback. This value is
        /// calculated at run time but also stored on the database so it is
        /// available in SQL and LINQ queries as well.
        /// </summary>
        /// <value>
        /// The default file URL or an empty string if one is not available.
        /// </value>
        [DataMember]
        [MaxLength( 2048 )]
        public string DefaultFileUrl
        {
            get
            {
                // OrderByDescending is correct when doing a boolean, true > false.
                return FileData.OrderByDescending( f => f.Quality == MediaElementQuality.HLS )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.UltraHD )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.HD )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.SD )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.Embed )
                    .ThenByDescending( f => f.Quality == MediaElementQuality.Audio )
                    .ThenByDescending( f => f.Width )
                    .FirstOrDefault()?.Link ?? string.Empty;
            }
            private set
            {
                // Make EF happy to use this property as a mapped column but
                // do not allow anybody to try and update the value.
            }
        }

        /// <summary>
        /// Gets the default thumbnail URL. This value is calculated at run
        /// time but also stored on the database so it is available in SQL
        /// and LINQ queries as well.
        /// </summary>
        /// <value>
        /// The default thumbnail URL or an empty string if one is not available.
        /// </value>
        [DataMember]
        [MaxLength( 2048 )]
        public string DefaultThumbnailUrl
        {
            get
            {
                return ThumbnailData.OrderByDescending( t => t.Width )
                    .FirstOrDefault()?.Link ?? string.Empty;
            }
            private set
            {
                // Make EF happy to use this property as a mapped column but
                // do not allow anybody to try and update the value.
            }
        }

        /// <summary>
        /// Gets or sets the media element file data. This contains all the
        /// information about the different file URLs available for the user
        /// to stream or download.
        /// </summary>
        /// <value>
        /// The media element file data.
        /// </value>
        [NotMapped]
        public virtual List<MediaElementFileData> FileData { get; set; } = new List<MediaElementFileData>();

        /// <summary>
        /// Gets or sets the thumbnail data.
        /// </summary>
        /// <value>
        /// The thumbnail data.
        /// </value>
        [NotMapped]
        public virtual List<MediaElementThumbnailData> ThumbnailData { get; set; } = new List<MediaElementThumbnailData>();

        #endregion
    }
}
