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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rock.Media
{
    /// <summary>
    /// This class is used to store and retrieve thumbnail data
    /// </summary>
    [Serializable]
    public class MediaElementThumbnailData
    {
        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        /// <value>
        /// The link.
        /// </value>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes.
        /// </summary>
        /// <value>
        /// The size in bytes.
        /// </value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the link with play button.
        /// </summary>
        /// <value>
        /// The with play button.
        /// </value>
        public string LinkWithPlayButton { get; set; }

        /// <summary>
        /// Gets the size of the formatted file size.
        /// </summary>
        /// <value>
        /// The size of the formatted file size.
        /// </value>
        [JsonIgnore]
        public string FormattedFileSize
        {
            get
            {
                if ( Size == 0 )
                {
                    return string.Empty;
                }

                return Size.FormatAsMemorySize();
            }
        }

        /// <summary>
        /// Gets the dimension.
        /// </summary>
        /// <value>
        /// The dimension.
        /// </value>
        [JsonIgnore]
        public string Dimension
        {
            get
            {
                if ( Width == 0 && Height == 0 )
                {
                    return string.Empty;
                }

                return string.Format( "{0}x{1}", Width, Height );
            }
        }
    }
}
