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

using Rock.ViewModels.Blocks.WebFarm.WebFarmNodeDetail;
using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.WebFarm.WebFarmSettings
{
    /// <summary>
    /// Contains details on the web farm settings.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class WebFarmSettingsBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }
        /// <summary>
        /// Gets or sets the web farm key.
        /// </summary>
        /// <value>
        /// The web farm key.
        /// </value>
        public string WebFarmKey { get; set; }
        /// <summary>
        /// Gets or sets the lower polling limit.
        /// </summary>
        /// <value>
        /// The lower polling limit.
        /// </value>
        public int? LowerPollingLimit { get; set; }
        /// <summary>
        /// Gets or sets the upper polling limit.
        /// </summary>
        /// <value>
        /// The upper polling limit.
        /// </value>
        public int? UpperPollingLimit { get; set; }
        /// <summary>
        /// Gets or sets the maximum polling wait seconds.
        /// </summary>
        /// <value>
        /// The maximum polling wait seconds.
        /// </value>
        public int? MaxPollingWaitSeconds { get; set; }
        /// <summary>
        /// Gets or sets the minimum polling difference.
        /// </summary>
        /// <value>
        /// The minimum polling difference.
        /// </value>
        public int? MinimumPollingDifference { get; set; }

        /// <summary>
        /// Gets or sets the nodes.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        public List<WebFarmNodeBag> Nodes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is in memory transport.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in memory transport; otherwise, <c>false</c>.
        /// </value>
        public bool IsInMemoryTransport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the web farm has a valid key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the web farm has a valid key; otherwise, <c>false</c>.
        /// </value>
        public bool HasValidKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the web farm is running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the web farm instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; set; }
    }
}
