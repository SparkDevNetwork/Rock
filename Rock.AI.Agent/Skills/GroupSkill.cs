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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.Attribute;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// This skill provides access to group related data.
/// </summary>

[Description( "This skill provides access to group related data." )]
[AgentPurpose( "Provides access to groups and group membership for people." )]

[GroupTypesField( "Group Types",
        Description = "The group types that will be managed by this skill.",
        IsRequired = true,
        EnhancedSelection = true,
        Key = ConfigurationKey.GroupTypes,
        Order = 0 )]

[AgentSkillGuid( "fa40a5e9-df52-4645-b3ed-cf9bbf79b12f" )]
[EntityTypeGuid( "ec39756f-44ba-4000-bb75-4335ddc95bc6" )]
internal sealed partial class GroupSkill : AgentSkillComponent
{
    #region Keys

    private static class ConfigurationKey
    {
        public const string GroupTypes = "GroupTypes";
    }

    #endregion

    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// The constructor for the Group Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public GroupSkill( ILogger<GroupSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Methods

    private IEnumerable<GroupTypeCache> GetConfiguredGroupTypes()
    {
        var groupTypeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.GroupTypes, string.Empty )
            .SplitDelimitedValues()
            .AsGuidList();

        if ( groupTypeGuids.Count == 0 )
        {
            return [];
        }

        return GroupTypeCache.GetMany( groupTypeGuids, AgentRequestContext.RockContext )
            .Where( gt => gt.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) );
    }

    #endregion
}
