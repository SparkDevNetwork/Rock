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

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    [Description( "Provides common, non-domain-specific helper functions that can be used across multiple skills." )]
    [AgentPurpose( "Provides common, non-domain-specific helper functions that can be used across multiple skills." )]
    [AgentPurpose( "These include utilities for working with dates, times, and simple data conversions." )]
    [AgentUsage( "ALWAYS call DetermineDateRange for natural-language date/time prompts (e.g., \"past 3 years\", \"yesterday\", \"Q2\", \"this week\", specific dates)." )]
    [AgentSkillGuid( "3406D2DC-6718-45A2-99D3-1DAA32BF2EFD" )]
    [EntityTypeGuid( "35CD02D0-1FF7-4256-B495-FBBFBC9A2C9C" )]
    internal sealed partial class SystemUtilitySkill : AgentSkillComponent
    {
        #region Fields

        private readonly ILogger<SystemUtilitySkill> _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemUtilitySkill"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public SystemUtilitySkill( ILogger<SystemUtilitySkill> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion
    }
}
