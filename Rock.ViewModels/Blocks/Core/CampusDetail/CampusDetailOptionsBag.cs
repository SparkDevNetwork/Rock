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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.CampusDetail
{
    /// <summary>
    /// Class CampusDetailOptionsBag.
    /// </summary>
    public class CampusDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is multi time zone supported.
        /// </summary>
        /// <value><c>true</c> if this instance is multi time zone supported; otherwise, <c>false</c>.</value>
        public bool IsMultiTimeZoneSupported { get; set; }

        /// <summary>
        /// Gets or sets the time zone options.
        /// </summary>
        /// <value>The time zone options.</value>
        public List<ListItemBag> TimeZoneOptions { get; set; }
    }
}
