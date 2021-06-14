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

using Newtonsoft.Json;

namespace Rock.Media
{
    /// <summary>
    /// This class is used to store and retrieve media element file data
    /// </summary>
    [Serializable]
    public class MediaElementFileData
    {
        /// <summary>
        /// Gets or sets the public name.
        /// </summary>
        /// <value>
        /// The public name.
        /// </value>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        /// <value>
        /// The link.
        /// </value>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the type of the quality.
        /// </summary>
        /// <value>
        /// The type of the quality.
        /// </value>
        public MediaElementQuality Quality { get; set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public string Format { get; set; }

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
        /// Gets or sets the frame per second.
        /// </summary>
        /// <value>
        /// The frame per second.
        /// </value>
        public int FPS { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes.
        /// </summary>
        /// <value>
        /// The size in bytes.
        /// </value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow download].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow download]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowDownload { get; set; }

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
                return string.Format( "{0}x{1}", Width, Height );
            }
        }
    }
}
