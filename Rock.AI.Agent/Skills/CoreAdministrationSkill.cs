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

using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// Provides read access to Rock's core configuration metadata: defined types and
/// values, entity types, categories, field types, attribute definitions, and
/// system communications.
/// </summary>
/// <remarks>
/// <para>
/// Nothing in this skill is specific to any one domain. It exists so that a
/// workflow builder, a page builder, a report builder, and a Lava authoring skill
/// can share one implementation of the reference lookups they all need.
/// </para>
/// <para>
/// Two rules apply to every tool here. A <c>List</c> or <c>Lookup</c> returns
/// identity only, meaning IdKey, Name, and whatever is needed to choose between
/// rows; everything else belongs to the matching <c>Get</c>. And a fully qualified
/// class name is only ever output, never a parameter, so a misspelled class name
/// cannot be expressed.
/// </para>
/// </remarks>
[Description( "Provides access to Rock's core configuration metadata: defined types and values, entity types, categories, field types, attributes, and system communications." )]
[AgentSkillGuid( "6DBD6867-2E0B-4D2E-9BF9-B34B77E4E94B" )]
[EntityTypeGuid( "55EB1E6F-EFBF-4E9C-BA11-DBC7147DA342" )]
internal sealed partial class CoreAdministrationSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// The constructor for the Core Administration Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public CoreAdministrationSkill( ILogger<CoreAdministrationSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
