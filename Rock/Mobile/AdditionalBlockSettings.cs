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
namespace Rock.Mobile
{
    /// <summary>
    /// Stores additional block settings for use with Mobile blocks.
    /// </summary>
    public class AdditionalBlockSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether show on tablets.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block should be shown on tablets; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOnTablet { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show on phones.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block should be shown on phones; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOnPhone { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether network is required for this block to display.
        /// </summary>
        /// <value>
        ///   <c>true</c> if network is required for this block to display; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresNetwork { get; set; }

        /// <summary>
        /// Gets or sets the content when there is no network.
        /// </summary>
        /// <value>
        /// The content when there is no network.
        /// </value>
        public string NoNetworkContent { get; set; }

        /// <summary>
        /// Gets or sets the CSS styles specific to this block.
        /// </summary>
        /// <value>
        /// The CSS styles specific to this block.
        /// </value>
        public string CssStyles { get; set; }

        /// <summary>
        /// Gets or sets the duration of the cache.
        /// </summary>
        /// <value>
        /// The duration of the cache.
        /// </value>
        public int CacheDuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to process lava on the server.
        /// </summary>
        /// <value>
        ///   <c>true</c> if lava should be processed on the server; otherwise, <c>false</c>.
        /// </value>
        public bool ProcessLavaOnServer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to process lava on the client.
        /// </summary>
        /// <value>
        ///   <c>true</c> if lava should be processed on the client; otherwise, <c>false</c>.
        /// </value>
        public bool ProcessLavaOnClient { get; set; }
    }
}
