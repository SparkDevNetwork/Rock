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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.GroupSkill;

/// <summary>
/// A single scheduled attendance record.
/// </summary>
internal class ScheduledAttendanceResult : EntityResultBase
{
    /// <summary>
    /// The person that is scheduled.
    /// </summary>
    public PersonResult Person { get; set; }

    /// <summary>
    /// The group they are scheduled for.
    /// </summary>
    public GroupResult Group { get; set; }

    /// <summary>
    /// The location they are scheduled for.
    /// </summary>
    public KeyNameResult Location { get; set; }

    /// <summary>
    /// The date and time they are scheduled for.
    /// </summary>
    public DateTime? ScheduledDate { get; set; }

    /// <summary>
    /// If the scheduled date is in the past and the group type is configured
    /// to take attendance, this will indicate whether or not the person
    /// attended.
    /// </summary>
    public bool? Attended { get; set; }

    /// <summary>
    /// The RSVP state.
    /// </summary>
    public RSVP? ConfirmationState { get; set; }
}
