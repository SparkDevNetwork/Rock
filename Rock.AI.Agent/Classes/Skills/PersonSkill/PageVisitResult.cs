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
using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Lightweight result model for a person's page-visit aggregate.
    /// </summary>
    internal class PageVisitResult
    {
        [JsonIgnore]
        internal int SiteId { get; set; }

        /// <summary>
        /// Internal numeric identifier of the site containing the page.
        /// </summary>
        public string SiteIdKey { get; set; }

        /// <summary>
        /// Display name of the page.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Total number of visits counted in the selected range.
        /// </summary>
        public int VisitCount { get; set; }

        /// <summary>
        /// Timestamp of the most recent visit, or <c>null</c> if none found.
        /// </summary>
        public DateTime? LastVisit { get; set; }
    }
}
