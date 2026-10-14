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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
using Rock.AI.Agent.Classes.Skills.GroupSkill;
using Rock.AI.Agent.Skills;
using Rock.AI.Agent.Tests.TestFramework;
using Rock.Configuration;
using Rock.Core.Geography.Classes;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.GroupSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="GroupSkill"/>.
/// </summary>
[TestClass]
[MethodIgnoreIf( nameof( IsSqlSpatialSupported ), "Requires the SQL Server spatial native library, which does not support ARM." )]
public partial class GroupSkillTests
{
    #region GetGroupType

    [TestMethod]
    public void GetGroupType_WithValidGroupType_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = new GroupType
        {
            Id = 70,
            Guid = new Guid( "9a000001-0000-4000-8000-000000000001" ),
            Name = "Small Group",
            GroupTerm = "Group",
            GroupMemberTerm = "Member"
        };

        rockContext.Set<GroupType>().Add( groupType );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetGroupType( IdHasher.Instance.GetHash( groupType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetGroupType_WithMissingGroupType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetGroupType( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion

    #region Support

    /// <summary>
    /// A stable group type guid used by the group finder tests.
    /// </summary>
    private static readonly Guid FinderGroupTypeGuid = new Guid( "9a000001-0000-4000-8000-000000000001" );

    /// <summary>
    /// Predicate for <see cref="MethodIgnoreIfAttribute"/>: the finder search
    /// tests evaluate spatial distances in memory, which requires the SQL Server
    /// spatial native library that is unavailable on ARM.
    /// </summary>
    /// <returns><c>true</c> when the spatial tests can run; otherwise <c>false</c>.</returns>
    public static bool IsSqlSpatialSupported()
    {
        return SqlServerSpatialSupport.IsSupported;
    }

    private static Rock.AI.Agent.Skills.GroupSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.GroupSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    /// <summary>
    /// Creates a GroupSkill primed with the group finder configuration, so the
    /// finder tools resolve their dedicated <c>FinderGroupTypes</c> setting.
    /// </summary>
    private static Rock.AI.Agent.Skills.GroupSkill CreateFinderSkill(
        System.IServiceProvider serviceProvider,
        RockContext rockContext,
        string configuredGroupTypes,
        Rock.Model.Person currentPerson = null,
        AudienceType audienceType = AudienceType.Internal )
    {
        var configuration = new Dictionary<string, string>
        {
            ["FinderGroupTypes"] = configuredGroupTypes
        };

        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.GroupSkill>(
            serviceProvider,
            configuration,
            CreateRequestContext( rockContext, currentPerson, audienceType ) );
    }

    /// <summary>
    /// Adds the standard finder group type to the context and returns it.
    /// </summary>
    private static GroupType AddFinderGroupType( RockContext rockContext )
    {
        var groupType = new GroupType
        {
            Id = 70,
            Guid = FinderGroupTypeGuid,
            Name = "Small Group",
            GroupTerm = "Group",
            GroupMemberTerm = "Member"
        };

        rockContext.Set<GroupType>().Add( groupType );

        return groupType;
    }

    /// <summary>
    /// Adds a group and its wired-up navigation graph to the context.
    /// </summary>
    private static void AddGroup( RockContext rockContext, Group group )
    {
        rockContext.Set<Group>().Add( group );
    }

    /// <summary>
    /// Creates a group with a single mapped location at the given coordinates,
    /// wiring the navigation properties the proximity query and filters walk.
    /// </summary>
    private static Group CreateGroup(
        int id,
        string name,
        int groupTypeId,
        double latitude,
        double longitude,
        bool isPublic = true,
        int? campusId = null,
        MeetingStyle? meetingStyle = null,
        DayOfWeek? weeklyDay = null,
        TimeSpan? weeklyTime = null,
        int? groupCapacity = null,
        int activeMemberCount = 0 )
    {
        var location = new Location
        {
            Id = id * 10,
            Guid = Guid.NewGuid(),
            GeoPoint = new GeographyPoint( latitude, longitude ).ToDatabase()
        };

        var group = new Group
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            GroupTypeId = groupTypeId,
            IsActive = true,
            IsPublic = isPublic,
            CampusId = campusId,
            MeetingStyle = meetingStyle,
            GroupCapacity = groupCapacity,
            Members = new List<GroupMember>()
        };

        for ( var i = 0; i < activeMemberCount; i++ )
        {
            group.Members.Add( new GroupMember
            {
                Id = ( id * 1000 ) + i,
                GroupMemberStatus = GroupMemberStatus.Active
            } );
        }

        if ( weeklyDay.HasValue || weeklyTime.HasValue )
        {
            group.Schedule = new Schedule
            {
                WeeklyDayOfWeek = weeklyDay,
                WeeklyTimeOfDay = weeklyTime
            };
        }

        var groupLocation = new GroupLocation
        {
            Id = id * 100,
            GroupId = group.Id,
            Group = group,
            LocationId = location.Id,
            Location = location
        };

        group.GroupLocations = new List<GroupLocation> { groupLocation };

        return group;
    }

    /// <summary>
    /// Extracts the strongly-typed results from a finder tool result.
    /// </summary>
    private static List<NearbyGroupResult> GetNearbyGroups( AgentToolResult result )
    {
        return result.GetResults()?.Cast<NearbyGroupResult>().ToList() ?? new List<NearbyGroupResult>();
    }

    #endregion
}
