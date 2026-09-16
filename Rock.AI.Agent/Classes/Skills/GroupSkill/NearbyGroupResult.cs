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

using System;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.GroupSkill;

/// <summary>
/// A single group returned from a group finder proximity search, trimmed to the
/// fields useful to a language model.
/// </summary>
internal class NearbyGroupResult : EntityResultBase
{
    /// <summary>
    /// The name of the group.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The group type of the group.
    /// </summary>
    public KeyNameResult GroupType { get; set; }

    /// <summary>
    /// The campus the group belongs to, or <c>null</c> when the group is not
    /// tied to a campus.
    /// </summary>
    public KeyNameResult Campus { get; set; }

    /// <summary>
    /// How the group meets (in person, online, or hybrid), when specified.
    /// </summary>
    public MeetingStyle? MeetingStyle { get; set; }

    /// <summary>
    /// The day of the week the group meets, when it has a weekly schedule.
    /// </summary>
    public DayOfWeek? MeetingDay { get; set; }

    /// <summary>
    /// The friendly time of day the group meets (e.g. "6:30 PM"), when it has a
    /// weekly schedule.
    /// </summary>
    public string MeetingTime { get; set; }

    /// <summary>
    /// The full street address of the meeting location, when one is available.
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// The straight-line ("as the crow flies") distance in miles between the
    /// origin and the group's location.
    /// </summary>
    public double? StraightLineDistanceInMiles { get; set; }

    /// <summary>
    /// The travel distance in miles for the requested travel mode. Only present
    /// when a travel mode was requested.
    /// </summary>
    public double? TravelDistanceInMiles { get; set; }

    /// <summary>
    /// The travel time in minutes for the requested travel mode. Only present
    /// when a travel mode was requested.
    /// </summary>
    public int? TravelTimeInMinutes { get; set; }

    /// <summary>
    /// The travel mode used to calculate <see cref="TravelDistanceInMiles"/> and
    /// <see cref="TravelTimeInMinutes"/>.
    /// </summary>
    public Rock.Enums.Geography.TravelMode? TravelMode { get; set; }
}
