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
using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.GroupSkill;

/// <summary>
/// A single group type in full configuration detail.
/// </summary>
/// <remarks>
/// This is the detail partner of the group type lookup, which returns identity and
/// roles only. It carries the configuration an administrator would recognize from
/// the group type screen, not the deeper relationship and chat internals, which no
/// authoring task needs.
/// </remarks>
internal class GroupTypeDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the group type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the group type.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The noun used for one group of this type, such as Group or Team.
    /// </summary>
    public string GroupTerm { get; set; }

    /// <summary>
    /// The noun used for one member of a group of this type, such as Member.
    /// </summary>
    public string GroupMemberTerm { get; set; }

    /// <summary>
    /// The CSS class of the icon shown for the group type.
    /// </summary>
    public string IconCssClass { get; set; }

    /// <summary>
    /// The color associated with the group type, as a hex value.
    /// </summary>
    public string GroupTypeColor { get; set; }

    /// <summary>
    /// Indicates that the group type is part of Rock's core configuration and
    /// cannot be deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// The purpose the group type serves, when one is assigned.
    /// </summary>
    public KeyNameResult Purpose { get; set; }

    /// <summary>
    /// The group type this one inherits its configuration from, when set.
    /// </summary>
    public KeyNameResult InheritedGroupType { get; set; }

    /// <summary>
    /// Indicates that groups of this type record attendance.
    /// </summary>
    public bool TakesAttendance { get; set; }

    /// <summary>
    /// How attendance is recorded for groups of this type.
    /// </summary>
    public AttendanceRule AttendanceRule { get; set; }

    /// <summary>
    /// Indicates that groups of this type appear in the group list.
    /// </summary>
    public bool ShowInGroupList { get; set; }

    /// <summary>
    /// Indicates that groups of this type appear in navigation.
    /// </summary>
    public bool ShowInNavigation { get; set; }

    /// <summary>
    /// Indicates that a group of this type may have more than one location.
    /// </summary>
    public bool AllowMultipleLocations { get; set; }

    /// <summary>
    /// Indicates that locations on groups of this type may have schedules, when
    /// set.
    /// </summary>
    public bool? EnableLocationSchedules { get; set; }

    /// <summary>
    /// The schedule types allowed for groups of this type.
    /// </summary>
    public ScheduleType AllowedScheduleTypes { get; set; }

    /// <summary>
    /// How capacity is enforced for groups of this type.
    /// </summary>
    public GroupCapacityRule GroupCapacityRule { get; set; }

    /// <summary>
    /// Indicates that RSVP is enabled for groups of this type.
    /// </summary>
    public bool EnableRSVP { get; set; }

    /// <summary>
    /// Indicates that group history is tracked for groups of this type.
    /// </summary>
    public bool EnableGroupHistory { get; set; }

    /// <summary>
    /// Indicates that groups of this type may be synced from a data view.
    /// </summary>
    public bool AllowGroupSync { get; set; }

    /// <summary>
    /// The default role a new member is given, when one is configured.
    /// </summary>
    public KeyNameResult DefaultGroupRole { get; set; }

    /// <summary>
    /// The roles available in groups of this type.
    /// </summary>
    public List<KeyNameResult> Roles { get; set; }

    /// <summary>
    /// Indicates that a group of this type may contain a child group of any type.
    /// When true, <see cref="ChildGroupTypes"/> is not restrictive.
    /// </summary>
    public bool AllowAnyChildGroupType { get; set; }

    /// <summary>
    /// The group types allowed as children of a group of this type.
    /// </summary>
    public List<KeyNameResult> ChildGroupTypes { get; set; }
}
