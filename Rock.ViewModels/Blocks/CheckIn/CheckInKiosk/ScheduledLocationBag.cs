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

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// Represents a single GroupLocation that has been scheduled. This includes
    /// all the information required to display this item for modification.
    /// </summary>
    public class ScheduledLocationBag
    {
        /// <summary>
        /// The encrypted identifier of the group location to be modified.
        /// </summary>
        public string GroupLocationId { get; set; }

        /// <summary>
        /// The path to the group that should be scheduled. This includes
        /// any parent groups in the text.
        /// </summary>
        public string GroupPath { get; set; }

        /// <summary>
        /// The path to the area that contains the group.
        /// </summary>
        public string AreaPath { get; set; }

        /// <summary>
        /// The name of the location.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// The path to the location which includes all ancestor locations.
        /// </summary>
        public string LocationPath { get; set; }

        /// <summary>
        /// The encrypted schedule identifiers of all schedules that are currently
        /// active for this group location.
        /// </summary>
        public List<string> ScheduleIds { get; set; }
    }
}
