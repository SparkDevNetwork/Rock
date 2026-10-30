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
using System.ComponentModel;

using Rock.Enums;

#if NET
namespace Rock.Enums.Group
#else
namespace Rock.Model
#endif
{
    /// <summary>
    /// The types of notifications a group schedule coordinator can receive about changes to scheduled individuals.
    /// </summary>
    [Flags]
#if !NET
    [EnumDomain( "Group" )]
#endif
    public enum ScheduleCoordinatorNotificationType
    {
        /// <summary>
        /// Don't sent any notifications.
        /// </summary>
        None = 0,

        /// <summary>
        /// Send a notification when a person accepts a schedule.
        /// </summary>
        Accept = 1,

        /// <summary>
        /// Send a notification when a person declines a schedule.
        /// </summary>
        Decline = 2,

        /// <summary>
        /// Send a notification when a person self-schedules (e.g. using the Group Schedule Toolbox to sign up for additional times).
        /// </summary>
        [Description( "Self-Schedule" )]
        SelfSchedule = 4
    }
}
