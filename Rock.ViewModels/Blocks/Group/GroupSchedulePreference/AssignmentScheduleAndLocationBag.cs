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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupSchedulePreference
{
    /// <summary>
    /// Gets or sets a class representing data to pass cleanly into mobile.
    /// </summary>
    public class AssignmentScheduleAndLocationBag
    {
        /// <summary>
        /// Gets or sets a guid representing the group member assignment ID.
        /// </summary>
        public Guid GroupMemberAssignmentGuid { get; set; }

        /// <summary>
        /// Gets or sets a list of schedule keys and values.
        /// </summary>
        public ListItemBag ScheduleListItem { get; set; }

        /// <summary>
        /// Gets or sets a list of location keys and values.
        /// </summary>
        public ListItemBag LocationListItem { get; set; }
    }
}
