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
/// This skill provides access to step related data.
/// </summary>

[Description( "This skill provides access to step related data." )]
[AgentUsage( "Steps and Programs are a way to guide users through a series of requirements, such as becoming a church member.")]
[AgentSkillGuid( "644caff4-73ef-43a3-9864-1b08614036c0" )]
[EntityTypeGuid( "4490b637-10f7-4912-8008-af6a061587c1" )]
internal sealed partial class StepSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// The constructor for the Step Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public StepSkill( ILogger<StepSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
