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

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// This skill provides access to LMS data.
/// </summary>

[Description( "This skill provides access to LMS data." )]
[AgentPurpose( "Provides access to the learning management system data." )]
[AgentPurpose( "The learning management system (LMS) typically functions like a set of school courses." )]

[AgentSkillGuid( "57f6c7d7-883b-4793-84ea-0fd21b1f09a9" )]
[EntityTypeGuid( "b808a071-5d86-4d69-936d-7573130dd842" )]
internal sealed partial class LearningManagementSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// The constructor for the Learning Management Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public LearningManagementSkill( ILogger<LearningManagementSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
