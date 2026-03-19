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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionOperationalSnapshot
{
    /// <summary>
    /// Filter configuration and supported values.
    /// </summary>
    public class FiltersBag
    {
        /// <summary>
        /// Gets or sets the available date range filter options.
        /// </summary>
        public List<ListItemBag> DateRanges { get; set; }

        /// <summary>
        /// Gets or sets the default date range value used when no preference is stored.
        /// </summary>
        public string DefaultDateRangeValue { get; set; }
    }
}
