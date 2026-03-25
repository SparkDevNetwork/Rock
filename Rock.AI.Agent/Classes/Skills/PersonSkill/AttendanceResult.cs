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

namespace Rock.AI.Agent.Classes.Skills.PersonSkill;

/// <summary>
/// A single attendance record for a person.
/// </summary>
internal class AttendanceResult : EntityResultBase
{
    /// <summary>
    /// The type of group that was attended.
    /// </summary>
    public KeyNameResult GroupType { get; set; }

    /// <summary>
    /// The group that was attended.
    /// </summary>
    public KeyNameResult Group { get; set; }

    /// <summary>
    /// The location that they were physically in.
    /// </summary>
    public KeyNameResult Location { get; set; }

    /// <summary>
    /// The schedule that was attended.
    /// </summary>
    public KeyNameResult Schedule { get; set; }

    /// <summary>
    /// The campus that was attended.
    /// </summary>
    public KeyNameResult Campus { get; set; }

    /// <summary>
    /// The date and time the attendance record is for.
    /// </summary>
    public DateTime? StartDateTime { get; set; }
}
