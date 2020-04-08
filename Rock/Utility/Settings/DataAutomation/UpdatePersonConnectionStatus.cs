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

namespace Rock.Utility.Settings.DataAutomation
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdatePersonConnectionStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the connection status value identifier dataview identifier mapping.
        /// </summary>
        /// <value>
        /// The connection status value identifier dataview identifier mapping.
        /// </value>
        public Dictionary<int, int?> ConnectionStatusValueIdDataviewIdMapping { get; set; } = new Dictionary<int, int?>();
    }
}
