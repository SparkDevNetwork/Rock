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

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// Min / Desired / Max capacity entry for a (Group Location, Schedule) pair.
    /// </summary>
    public class GroupLocationScheduleConfigBag
    {
        /// <summary>
        /// Gets or sets the Guid of the schedule this capacity entry is for.
        /// </summary>
        public Guid ScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets the minimum capacity for this (location, schedule) pair.
        /// </summary>
        public int? MinimumCapacity { get; set; }

        /// <summary>
        /// Gets or sets the desired capacity for this (location, schedule) pair.
        /// </summary>
        public int? DesiredCapacity { get; set; }

        /// <summary>
        /// Gets or sets the maximum capacity for this (location, schedule) pair.
        /// </summary>
        public int? MaximumCapacity { get; set; }
    }
}
