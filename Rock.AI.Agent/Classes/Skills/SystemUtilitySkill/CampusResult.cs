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

using System.Collections.Generic;
using System.Text.Json.Serialization;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.SystemUtilitySkill
{
    /// <summary>
    /// Represents a campus, including identity, type/status, location, service times, and custom attributes.
    /// </summary>
    public class CampusResult
    {
        /// <summary>
        /// The Id of the campus.s
        /// </summary>
        [JsonIgnore]
        public int Id { get; set; }

        /// <summary>
        /// Opaque campus identifier (Id Key). Use this value with other APIs instead of the numeric Id.
        /// </summary>
        public string CampusIdKey { get; set; }

        /// <summary>
        /// Indicates whether the campus is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Short campus code or abbreviation used for display.
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Display value of the campus type (e.g., Physical, Online).
        /// </summary>
        public string CampusType { get; set; }

        /// <summary>
        /// Display value of the campus status (e.g., Open, Inactive).
        /// </summary>
        public string CampusStatus { get; set; }

        /// <summary>
        /// Primary geographic location information for the campus.
        /// </summary>
        public LocationResult Location { get; set; }

        /// <summary>
        /// Schedules for the campus.
        /// </summary>
        public List<CampusScheduleResult> CampusSchedules { get; set; }

        /// <summary>
        /// The group id of the campus team.
        /// </summary>
        [JsonIgnore]
        public int? CampusTeamGroupId { get; set; }

        /// <summary>
        /// The phone number of the campus.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The URL to the campus.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The campus's team members.
        /// </summary>
        public List<CampusTeamMemberResult> CampusTeamMembers { get; set; } = new List<CampusTeamMemberResult>();

        /// <summary>
        /// Arbitrary campus attributes represented as a key/value dictionary.
        /// Keys are attribute keys; values are formatted strings.
        /// </summary>
        public List<AttributeResult> Attributes { get; set; } = new List<AttributeResult>();
    }
}
