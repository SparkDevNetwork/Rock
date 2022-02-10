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
using System.ComponentModel;

namespace Rock.Model
{
    /// <summary>
    /// Represents the attendance association of a <see cref="StreakType"/>.
    /// </summary>
    public enum StreakStructureType
    {
        /// <summary>
        /// The <see cref="StreakType"/> is associated with any attendance record.
        /// </summary>
        [Description( "Attendance: Any" )]
        AnyAttendance = 0,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with attendance to a single group.
        /// </summary>
        [Description( "Attendance: Group" )]
        Group = 1,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with attendance to groups of a given type.
        /// </summary>
        [Description( "Attendance: Group Type" )]
        GroupType = 2,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with attendance to groups within group types of a common purpose (defined type).
        /// </summary>
        [Description( "Attendance: Group Type Purpose" )]
        GroupTypePurpose = 3,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with attendance specified by a check-in configuration.
        /// </summary>
        [Description( "Attendance: Check-In Configuration" )]
        CheckInConfig = 4,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with interactions in a certain channel.
        /// </summary>
        [Description( "Interaction: Channel" )]
        InteractionChannel = 5,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with interactions in a certain component.
        /// </summary>
        [Description( "Interaction: Component" )]
        InteractionComponent = 6,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with interactions over a certain.
        /// </summary>
        [Description( "Interaction: Medium" )]
        InteractionMedium = 7,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with financial transactions.
        /// </summary>
        [Description( "Financial Transaction" )]
        FinancialTransaction = 8
    }
}
