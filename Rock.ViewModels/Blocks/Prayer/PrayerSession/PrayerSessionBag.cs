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

namespace Rock.ViewModels.Blocks.Prayer.PrayerSession
{
    /// <summary>
    /// The initial data for the Prayer Session block's welcome step.
    /// </summary>
    public class PrayerSessionBag
    {
        /// <summary>
        /// Gets or sets the active prayer categories available for selection, each
        /// labeled with its current request count.
        /// </summary>
        public List<ListItemBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets the category values that should be pre-selected from the
        /// person's saved preferences.
        /// </summary>
        public List<string> SelectedCategoryValues { get; set; }

        /// <summary>
        /// Gets or sets the campus that should be pre-selected from the person's
        /// saved preferences.
        /// </summary>
        public ListItemBag SelectedCampus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any active prayer categories
        /// exist. When <c>false</c> the block shows a "no active prayer requests"
        /// message instead of the welcome step.
        /// </summary>
        public bool HasActiveCategories { get; set; }
    }
}
