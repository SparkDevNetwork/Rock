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
using Rock.Model;

namespace Rock.ViewModels.Blocks.Engagement.StreakTypeDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class StreakTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a description of the Streak Type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the attendance association
        /// </summary>
        public StreakStructureType? StructureType { get; set; }

        /// <summary>
        /// This determines whether the streak type will write attendance records when marking someone as present or
        /// if it will just update the enrolled individual’s map.
        /// </summary>
        public bool EnableAttendance { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the Streak Type. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the timespan that each map bit represents (Rock.Model.StreakOccurrenceFrequency).
        /// </summary>
        public StreakOccurrenceFrequency OccurrenceFrequency { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this streak type requires explicit enrollment. If not set, a person can be
        /// implicitly enrolled through attendance.
        /// </summary>
        public bool RequiresEnrollment { get; set; }

        /// <summary>
        /// Gets or sets the System.DateTime associated with the least significant bit of all maps in this streak type.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the Streaks that are of this streak type.
        /// </summary>
        public List<ListItemBag> Streaks { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the StreakTypeExclusions
        /// that are of this streak type.
        /// </summary>
        public List<ListItemBag> StreakTypeExclusions { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Entity associated with attendance for this streak type. If not set, this streak type
        /// will account for any attendance record.
        /// </summary>
        public ListItemBag StructureEntity { get; set; }

        /// <summary>
        /// Gets or sets the structure settings JSON.
        /// </summary>
        public Boolean IncludeChildAccounts { get; set; }

        /// <summary>
        /// Gets or sets the first day of the week when the frequency is set to weekly
        /// </summary>
        public int FirstDayOfWeek { get; set; }

        /// <summary>
        /// The Guid for the Interaction Channel associated with the Interaction Component.
        /// This is set only if the linked activity is Interaction Component
        /// </summary>
        public ListItemBag InteractionComponentInteractionChannel { get; set; }

        /// <summary>
        /// The text display of structure type in view mode in the frontend
        /// </summary>
        public string StructureTypeDisplay { get; set; }
    }
}
