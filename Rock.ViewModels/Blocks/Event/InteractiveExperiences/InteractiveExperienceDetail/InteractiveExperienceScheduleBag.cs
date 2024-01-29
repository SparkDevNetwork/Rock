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

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail
{
    /// <summary>
    /// A bag that represents a single schedule and the one or more campuses
    /// that are associated with the schedule.
    /// </summary>
    public class InteractiveExperienceScheduleBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of this schedule.
        /// </summary>
        /// <value>The unique identifier of this schedule.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>The schedule.</value>
        public ListItemBag Schedule { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes before the schedule starts that
        /// the experience should be enabled.
        /// </summary>
        /// <value>The number of minutes before the schedule to enable the experience.</value>
        public int? EnableMinutesBefore { get; set; }

        /// <summary>
        /// Gets or sets the campuses that are tied to this schedule.
        /// </summary>
        /// <value>The campuses that are tied to this schedule.</value>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the data view to use for filtering.
        /// </summary>
        /// <value>The data view to use for filtering.</value>
        public ListItemBag DataView { get; set; }

        /// <summary>
        /// Gets or sets the group to use for filtering.
        /// </summary>
        /// <value>The group to use for filtering.</value>
        public ListItemBag Group { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
