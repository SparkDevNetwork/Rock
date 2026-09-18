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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.GroupSkill;
using Rock.Enums.AI.Agent;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class GroupSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single group type in full configuration detail.
    /// </summary>
    /// <remarks>
    /// This is the detail partner of <see cref="LookupGroupTypes"/>, which returns
    /// identity and roles only. The same availability gating applies: an internal
    /// audience may read any group type it is authorized to view, while a public
    /// audience is limited to the group types this skill is configured to manage.
    /// </remarks>
    [Description( "Gets a single group type in full configuration detail." )]
    [AgentPurpose( "Retrieves the settings of one group type." )]
    [AgentToolPrerequisite( "Call LookupGroupTypes to determine the groupTypeIdKey." )]
    [AgentToolGuid( "2846FCA1-4B7B-4623-903A-2293C17436A3" )]
    public AgentToolResult GetGroupType( string groupTypeIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var groupType = helper.GetRequiredEntity<Rock.Model.GroupType>( groupTypeIdKey );

        if ( groupType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( LookupGroupTypes )} function to determine the available group types." );
        }

        // A public audience may only see the group types this skill is configured to
        // manage, matching LookupGroupTypes. An internal audience relies on the VIEW
        // check GetRequiredEntity already performed.
        var isInternal = AgentRequestContext.AudienceType == AudienceType.Internal;

        if ( !isInternal )
        {
            var availableGroupTypeIds = GetAvailableGroupTypes().Select( gt => gt.Id ).ToList();

            if ( !availableGroupTypeIds.Contains( groupType.Id ) )
            {
                return Error( "You do not have access to this group type." )
                    .WithInstructions( $"Call the {nameof( LookupGroupTypes )} function to determine the available group types." );
            }
        }

        var groupTypeCache = GroupTypeCache.Get( groupType.Id, AgentRequestContext.RockContext );

        if ( groupTypeCache == null )
        {
            return Error( "You do not have access to this group type." );
        }

        var purpose = groupTypeCache.GroupTypePurposeValue;
        var inheritedGroupType = groupTypeCache.InheritedGroupType;
        var defaultRole = groupTypeCache.DefaultGroupRoleId.HasValue
            ? groupTypeCache.Roles.FirstOrDefault( r => r.Id == groupTypeCache.DefaultGroupRoleId.Value )
            : null;

        var result = new GroupTypeDetailResult
        {
            Id = groupTypeCache.Id,
            Guid = groupTypeCache.Guid,
            Name = groupTypeCache.Name,
            Description = groupTypeCache.Description,
            GroupTerm = groupTypeCache.GroupTerm,
            GroupMemberTerm = groupTypeCache.GroupMemberTerm,
            IconCssClass = groupTypeCache.IconCssClass,
            GroupTypeColor = groupTypeCache.GroupTypeColor,
            IsSystem = groupTypeCache.IsSystem,
            Purpose = KeyNameResult.FromCache( purpose ),
            InheritedGroupType = KeyNameResult.FromCache( inheritedGroupType ),
            TakesAttendance = groupTypeCache.TakesAttendance,
            AttendanceRule = groupTypeCache.AttendanceRule,
            ShowInGroupList = groupTypeCache.ShowInGroupList,
            ShowInNavigation = groupTypeCache.ShowInNavigation,
            AllowMultipleLocations = groupTypeCache.AllowMultipleLocations,
            EnableLocationSchedules = groupTypeCache.EnableLocationSchedules,
            AllowedScheduleTypes = groupTypeCache.AllowedScheduleTypes,
            GroupCapacityRule = groupTypeCache.GroupCapacityRule,
            EnableRSVP = groupTypeCache.EnableRSVP,
            EnableGroupHistory = groupTypeCache.EnableGroupHistory,
            AllowGroupSync = groupTypeCache.AllowGroupSync,
            AllowAnyChildGroupType = groupTypeCache.AllowAnyChildGroupType,
            DefaultGroupRole = KeyNameResult.FromCache( defaultRole ),
            Roles = groupTypeCache.Roles
                .Select( r => KeyNameResult.FromCache( r ) )
                .ToList(),
            ChildGroupTypes = groupTypeCache.ChildGroupTypes
                .Select( ct => KeyNameResult.FromCache( ct ) )
                .ToList()
        };

        return Success( result )
            .WithHistoryContent( KeyNameResult.FromCache( groupTypeCache ) );
    }

    #endregion
}
