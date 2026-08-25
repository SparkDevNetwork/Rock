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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// This skill provides access to benevolence requests.
/// </summary>

[Description( "This skill provides access to benevolence requests." )]
[AgentPurpose( "Provides access to benevolence requests for people." )]

[CustomCheckboxListField( "Benevolence Types",
        Description = "Specifies which benevolence types to enable for use with the tools in this skill.",
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [BenevolenceType]",
        IsRequired = false,
        Key = ConfigurationKey.BenevolenceTypes,
        Order = 0 )]

[AgentSkillGuid( "d7340fae-917c-4a96-8958-99ec8361328a" )]
[EntityTypeGuid( "43f23f97-2360-4089-ad6e-c1ddcdf4665b" )]
internal sealed partial class BenevolenceSkill : AgentSkillComponent
{
    #region Keys

    private static class ConfigurationKey
    {
        public const string BenevolenceTypes = "BenevolenceTypes";
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
    /// The constructor for the Benevolence Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public BenevolenceSkill( ILogger<BenevolenceSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Methods

    private IEnumerable<Model.BenevolenceType> GetConfiguredBenevolenceTypes()
    {
        var benevolenceTypeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.BenevolenceTypes, string.Empty )
            .SplitDelimitedValues()
            .AsGuidList();

        if ( benevolenceTypeGuids.Count == 0 )
        {
            return [];
        }

        return new BenevolenceTypeService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( bt => benevolenceTypeGuids.Contains( bt.Guid )
                && bt.IsActive )
            .ToList()
            .Where( bt => bt.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) );
    }

    #endregion
}
