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

using System.Collections.Generic;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.EventCalendarSkill;

/// <summary>
/// Represents a single EventItem.
/// </summary>
internal class EventItemResult : EntityResultBase
{
    /// <summary>
    /// The name of the event item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The summary that describes the event.
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// Determines if the event has been approved.
    /// </summary>
    public bool? IsApproved { get; set; }

    /// <summary>
    /// The person that approved the event.
    /// </summary>
    public PersonResult ApprovedByPerson { get; set; }

    /// <summary>
    /// The audiences used for filtering when searching for events.
    /// </summary>
    public List<KeyNameResult> Audiences { get; set; }

    /// <summary>
    /// The calendars this event will be displayed on.
    /// </summary>
    public List<KeyNameResult> Calendars { get; set; }
}
