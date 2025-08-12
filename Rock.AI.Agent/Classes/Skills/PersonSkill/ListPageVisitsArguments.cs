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
using System.ComponentModel;

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Arguments for listing a person's page visits.
    /// </summary>
    public class ListPageVisitsArguments
    {
        /// <summary>
        /// Optional earliest date to include (inclusive). Set to <c>null</c> to leave the start unbounded.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional latest date to include (inclusive). Set to <c>null</c> to leave the end unbounded.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Optional site filter. The ID Key of the site to analyze.
        /// </summary>
        [Description( "Optional. The ID Key of site to analyze." )]
        public string SiteKey { get; set; }

        /// <summary>
        /// Required. The stable key identifying the person whose visits are being requested.
        /// </summary>
        public string PersonKey { get; set; }

        /// <summary>
        /// The 1-based page number to return. Defaults to <c>1</c>.
        /// </summary>
        [Description( "Optional. The page number to return, starting at 1. Defaults to 1." )]
        public int PageNumber { get; set; } = 1;
    }
}