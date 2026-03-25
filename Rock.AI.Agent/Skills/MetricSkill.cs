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
using Rock.Attribute;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// This skill provides access to metric related data.
/// </summary>

[Description( "This skill provides access to metric related data." )]
[AgentUsage( "Metrics are specific terms in Rock to refer to items created to track custom values over time." )]
[CategoryField( "Categories",
    Description = "The categories to use when searching for metrics. Only metrics that are direct children of these categories will be included.",
    IsRequired = true,
    EntityType = typeof( Model.MetricCategory ),
    AllowMultiple = true,
    Key = ConfigurationKey.Categories,
    Order = 0 )]
[AgentSkillGuid( "88855c70-9e92-4cbc-8432-b0dd2d50a33f" )]
[EntityTypeGuid( "b8ae26b9-cc3f-48f1-8e36-f8afc0be1167" )]
internal sealed partial class MetricSkill : AgentSkillComponent
{
    #region Keys

    private static class ConfigurationKey
    {
        public const string Categories = "Categories";
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
    /// The constructor for the Metric Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public MetricSkill( ILogger<MetricSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
