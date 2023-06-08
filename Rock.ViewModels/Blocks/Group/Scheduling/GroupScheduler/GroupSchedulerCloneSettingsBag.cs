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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The clone settings to indicate how group schedules should be cloned.
    /// </summary>
    public class GroupSchedulerCloneSettingsBag
    {
        /// <summary>
        /// Gets or sets the available source dates.
        /// </summary>
        /// <value>
        /// The available source dates.
        /// </value>
        public List<ListItemBag> AvailableSourceDates { get; set; }

        /// <summary>
        /// Gets or sets the ISO 8601 selected source date.
        /// </summary>
        /// <value>
        /// The ISO 8601 selected source date.
        /// </value>
        public string SelectedSourceDate { get; set; }

        /// <summary>
        /// Gets or sets the available destination dates.
        /// </summary>
        /// <value>
        /// The available destination dates.
        /// </value>
        public List<ListItemBag> AvailableDestinationDates { get; set; }

        /// <summary>
        /// Gets or sets the ISO 8601 selected destination date.
        /// </summary>
        /// <value>
        /// The ISO 8601 selected destination date.
        /// </value>
        public string SelectedDestinationDate { get; set; }

        /// <summary>
        /// Gets or sets the available groups.
        /// </summary>
        /// <value>
        /// The available groups.
        /// </value>
        public List<ListItemBag> AvailableGroups { get; set; }

        /// <summary>
        /// Gets or sets the selected groups.
        /// </summary>
        /// <value>
        /// The selected groups.
        /// </value>
        public List<string> SelectedGroups { get; set; }

        /// <summary>
        /// Gets or sets the available locations.
        /// </summary>
        /// <value>
        /// The available locations.
        /// </value>
        public List<ListItemBag> AvailableLocations { get; set; }

        /// <summary>
        /// Gets or sets the selected locations.
        /// </summary>
        /// <value>
        /// The selected locations.
        /// </value>
        public List<string> SelectedLocations { get; set; }

        /// <summary>
        /// Gets or sets the available schedules.
        /// </summary>
        /// <value>
        /// The available schedules.
        /// </value>
        public List<ListItemBag> AvailableSchedules { get; set; }

        /// <summary>
        /// Gets or sets the selected schedules.
        /// </summary>
        /// <value>
        /// The selected schedules.
        /// </value>
        public List<string> SelectedSchedules { get; set; }
    }
}
