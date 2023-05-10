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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupSchedulePreference
{

    /// <summary>
    /// A class representing the content we need to pass into mobile.
    /// </summary>
    public class PreferencePageContentBag
    {
        /// <summary>
        /// Gets or sets an integer representing the previously selected offset of reminder days.
        /// </summary>
        public int SelectedOffset { get; set; }

        /// <summary>
        /// Gets or sets an integer representing the index of the selected schedule.
        /// </summary>
        public Guid SelectedSchedule { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DateTime"/> representing the selected start date.
        /// </summary>
        public DateTimeOffset? SelectedStartDate { get; set; }

        /// <summary>
        /// Gets or sets a list of schedule keys and values. 
        /// </summary>
        public List<ListItemBag> ListItems { get; set; }

        /// <summary>
        /// Gets or sets a list of schedule assignments and locations.
        /// </summary>
        public List<AssignmentScheduleAndLocationBag> AssignmentScheduleAndLocations { get; set; }
    }
}
