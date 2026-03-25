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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.EventCalendarSkill;

/// <summary>
/// Represents a single Event Item Occurrence.
/// </summary>
internal class EventItemOccurrenceResult : EntityResultBase
{
    /// <summary>
    /// The event item that this occurrence is associated with.
    /// </summary>
    public EventItemResult EventItem { get; set; }

    /// <summary>
    /// The next start date and time of this occurrence if it is a repeating
    /// schedule.
    /// </summary>
    public DateTime? NextStartDateTime { get; set; }

    /// <summary>
    /// The campus this occurrence is tied to.
    /// </summary>
    public CampusResult Campus { get; set; }

    /// <summary>
    /// The contact person for this event item occurrence.
    /// </summary>
    public PersonResult ContactPerson { get; set; }

    /// <summary>
    /// The description of where this event item occurrence is happening.
    /// </summary>
    public string LocationDescription { get; set; }

    /// <summary>
    /// The contact phone number for this event item occurrence.
    /// </summary>
    public string ContactPhoneNumber { get; set; }

    /// <summary>
    /// The contact email address for this event item occurrence.
    /// </summary>
    public string ContactEmail { get; set; }

    /// <summary>
    /// The description of the schedule for this event item occurrence. This
    /// may describe either a single date or a recurring schedule.
    /// </summary>
    public string ScheduleDescription { get; set; }
}
