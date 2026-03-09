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

using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// This skill provides access to streaks and achievement related data.
    /// </summary>

    [Description( "This skill provides access to streaks and achievement related data." )]
    [AgentSkillGuid( "7224d53b-7846-4e72-a454-e8877eef3edf" )]
    [EntityTypeGuid( "ae0913f4-94f4-4afe-bcfc-e94bd39c24f0" )]
    internal sealed partial class StreakSkill : AgentSkillComponent
    {
        #region Fields

        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor for the Streak Skill.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public StreakSkill( ILogger<ConnectionSkill> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion
    }
}
