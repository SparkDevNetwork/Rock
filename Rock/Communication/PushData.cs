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

namespace Rock.Communication
{
    /// <summary>
    /// This class is used to hold the Rock specific settings in for Rock Push Message.
    /// </summary>
    public class PushData
    {
        /// <summary>
        /// Gets or sets the mobile page identifier.
        /// </summary>
        /// <value>
        /// The mobile page identifier.
        /// </value>
        public int? MobilePageId { get; set; }
        /// <summary>
        /// Gets or sets the mobile page query string.
        /// </summary>
        /// <value>
        /// The mobile page query string.
        /// </value>
        public Dictionary<string, string> MobilePageQueryString { get; set; }
        /// <summary>
        /// Gets or sets the mobile application identifier.
        /// </summary>
        /// <value>
        /// The mobile application identifier.
        /// </value>
        public int? MobileApplicationId { get; set; }
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }
    }
}
