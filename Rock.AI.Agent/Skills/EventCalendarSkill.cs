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
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// This skill provides access to working with event calendars and event items.
/// </summary>

[Description( "This skill provides access to working with event calendars and event items." )]
[AgentUsage( "This skill provides access to working with event calendars and event items." )]

[CustomCheckboxListField( "Calendars",
    Description = "The calendars that will be available to work with.",
    ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [EventCalendar]",
    IsRequired = false,
    Key = ConfigurationKey.Calendars,
    Order = 0 )]

[AgentSkillGuid( "985a7a4b-a94d-47e8-a056-350aa54f796e" )]
[EntityTypeGuid( "23fc8746-dc29-42ef-9846-0148784b7b8e" )]
internal sealed partial class EventCalendarSkill : AgentSkillComponent
{
    #region Keys

    private static class ConfigurationKey
    {
        public const string Calendars = "Calendars";
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
    /// The constructor for the Event Calendar Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public EventCalendarSkill( ILogger<EventCalendarSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Methods

    private IEnumerable<EventCalendarCache> GetConfiguredCalendars()
    {
        var groupTypeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.Calendars, string.Empty )
            .SplitDelimitedValues()
            .AsGuidList();

        if ( groupTypeGuids.Count == 0 )
        {
            return [];
        }

        return EventCalendarCache.GetMany( groupTypeGuids, AgentRequestContext.RockContext )
            .Where( gt => gt.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) );
    }

    #endregion
}
