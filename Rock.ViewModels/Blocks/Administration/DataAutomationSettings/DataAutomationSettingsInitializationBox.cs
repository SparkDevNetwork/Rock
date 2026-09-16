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

namespace Rock.ViewModels.Blocks.Administration.DataAutomationSettings
{
    /// <summary>
    /// The information required to render the Data Automation Settings block.
    /// </summary>
    public class DataAutomationSettingsInitializationBox
    {
        /// <summary>
        /// Gets or sets the current data automation settings.
        /// </summary>
        public DataAutomationSettingsBag Settings { get; set; }

        /// <summary>
        /// Gets or sets the group types that take attendance, used by the
        /// reactivate and inactivate group-type selectors. Each value is a group
        /// type unique identifier.
        /// </summary>
        public List<ListItemBag> AttendanceGroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the person attributes available for selection. Each value
        /// is an attribute unique identifier.
        /// </summary>
        public List<ListItemBag> PersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets the options for the tie-breaker used when the campuses
        /// calculated from attendance and giving differ.
        /// </summary>
        public List<ListItemBag> MostAttendanceOrGivingOptions { get; set; }

        /// <summary>
        /// Gets or sets the options for the "based on" criteria on an ignore
        /// campus change rule.
        /// </summary>
        public List<ListItemBag> CampusChangeBasedOnOptions { get; set; }

        /// <summary>
        /// Gets or sets the known relationships group type unique identifier used
        /// to scope the parent and sibling relationship pickers.
        /// </summary>
        public Guid? KnownRelationshipGroupTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Update Family Campus
        /// section is visible. It is hidden when only a single campus exists.
        /// </summary>
        public bool IsUpdateFamilyCampusVisible { get; set; }
    }
}
