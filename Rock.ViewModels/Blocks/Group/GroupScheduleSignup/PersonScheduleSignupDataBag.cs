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

namespace Rock.ViewModels.Blocks.Group.GroupScheduleSignup
{
    /// <summary>
    /// Gets or sets a class representing the schedule sign up information.
    /// </summary>
    public class PersonScheduleSignupDataBag
    {
        /// <summary>
        /// Gets or sets a Guid representing the GroupId of the attendance.
        /// </summary>
        public Guid GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets an integer representing the GroupOrder of the attendance.
        /// </summary>
        public int GroupOrder { get; set; }

        /// <summary>
        /// Gets or sets a string representing the group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets a Guid representing the default LocationId
        /// </summary>
        public Guid LocationGuid { get; set; }

        /// <summary>
        /// Gets or sets a Guid representing the default ScheduleId
        /// </summary>
        public Guid ScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets a DateTime representing the schedule date time.
        /// </summary>
        public DateTimeOffset ScheduledDateTime { get; set; }

        /// <summary>
        /// Gets or sets a string representing the schedule name.
        /// </summary>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets a string representing the location name.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets an integer representing the location order.
        /// </summary>
        public int LocationOrder { get; set; }

        /// <summary>
        /// Gets or sets a boolean representing whether or not the max amount of scheduled people has been hit.
        /// </summary>
        public bool MaxScheduled { get; set; }

        /// <summary>
        /// Gets or sets an integer representing how many people are left to schedule.
        /// </summary>
        public int PeopleNeeded { get; set; }
    }
}
