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

public partial class GroupSkillTests
{
    #region FindNearbyGroups - Guards and validation

    [TestMethod]
    public void FindNearbyGroups_WithPublicAudience_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString(),
            audienceType: AudienceType.Public );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroups_WithNoConfiguredGroupTypes_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateFinderSkill( scope.App, rockContext, configuredGroupTypes: string.Empty );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroups_WithGroupTypeOutsideConfiguredSet_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString() );

        // A group type id that is not part of the configured finder set.
        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", groupTypeIdKey: IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroups_WithUnresolvableGroupTypeIdKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString() );

        // A group type id key that cannot be decoded to any id.
        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", groupTypeIdKey: "not-a-valid-id-key" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroups_WithNoOriginAndNoPerson_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString() );

        var result = skill.FindNearbyGroups();

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroups_WithInvalidEarliestTime_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", earliestTime: "not a time" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroups_WithInvalidPersonIdKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString() );

        var result = skill.FindNearbyGroups( personIdKey: IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion

    #region FindNearbyGroups - Search behavior

    [ConditionalTestMethod]
    public void FindNearbyGroups_OrdersResultsByDistanceFromOrigin()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        // Origin at (33.60, -112.30). Near group ~0.75 mi, far group ~34 mi.
        AddGroup( rockContext, CreateGroup( 10, "Near Group", groupType.Id, 33.61, -112.30 ) );
        AddGroup( rockContext, CreateGroup( 11, "Far Group", groupType.Id, 34.10, -112.30 ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30" );

        var results = GetNearbyGroups( result );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.HasCount( 2, results );
        Assert.AreEqual( "Near Group", results[0].Name );
        Assert.AreEqual( "Far Group", results[1].Name );
        Assert.IsLessThan( results[1].StraightLineDistanceInMiles.Value, results[0].StraightLineDistanceInMiles.Value );

        // These groups have no schedule, so the schedule-derived fields project as null.
        Assert.IsNull( results[0].MeetingDay );
        Assert.IsNull( results[0].MeetingTime );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithGroupTypeIdKey_NarrowsToThatType()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var wantedType = AddFinderGroupType( rockContext );
        var otherType = new GroupType
        {
            Id = 71,
            Guid = new Guid( "9a000001-0000-4000-8000-000000000002" ),
            Name = "Serving Team",
            GroupTerm = "Group",
            GroupMemberTerm = "Member"
        };
        rockContext.Set<GroupType>().Add( otherType );

        AddGroup( rockContext, CreateGroup( 10, "Wanted", wantedType.Id, 33.61, -112.30 ) );
        AddGroup( rockContext, CreateGroup( 11, "Other", otherType.Id, 33.61, -112.30 ) );

        // Both types are configured for the finder.
        var configured = $"{wantedType.Guid},{otherType.Guid}";
        var skill = CreateFinderSkill( scope.App, rockContext, configured );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", groupTypeIdKey: IdHasher.Instance.GetHash( wantedType.Id ) );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 1, results );
        Assert.AreEqual( "Wanted", results[0].Name );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithCampusIdKey_NarrowsToThatCampus()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        var campus = new Campus { Id = 5, Guid = Guid.NewGuid(), Name = "Main Campus" };
        rockContext.Set<Campus>().Add( campus );

        AddGroup( rockContext, CreateGroup( 10, "On Campus", groupType.Id, 33.61, -112.30, campusId: campus.Id ) );
        AddGroup( rockContext, CreateGroup( 11, "Other Campus", groupType.Id, 33.61, -112.30, campusId: 6 ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", campusIdKey: IdHasher.Instance.GetHash( campus.Id ) );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 1, results );
        Assert.AreEqual( "On Campus", results[0].Name );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithMaxDistanceMiles_ExcludesGroupsBeyondIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        AddGroup( rockContext, CreateGroup( 10, "Near Group", groupType.Id, 33.61, -112.30 ) );
        AddGroup( rockContext, CreateGroup( 11, "Far Group", groupType.Id, 34.10, -112.30 ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        // 5 miles excludes the ~34 mi far group but keeps the ~0.75 mi near group.
        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", maxDistanceMiles: 5 );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 1, results );
        Assert.AreEqual( "Near Group", results[0].Name );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithMeetingStyle_FiltersToMatchingStyle()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        AddGroup( rockContext, CreateGroup( 10, "In Person", groupType.Id, 33.61, -112.30, meetingStyle: MeetingStyle.InPerson ) );
        AddGroup( rockContext, CreateGroup( 11, "Online", groupType.Id, 33.62, -112.30, meetingStyle: MeetingStyle.Online ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", meetingStyle: MeetingStyle.Online );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 1, results );
        Assert.AreEqual( "Online", results[0].Name );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithDaysOfWeek_FiltersToMatchingDay()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        AddGroup( rockContext, CreateGroup( 10, "Monday Group", groupType.Id, 33.61, -112.30, weeklyDay: DayOfWeek.Monday ) );
        AddGroup( rockContext, CreateGroup( 11, "Tuesday Group", groupType.Id, 33.62, -112.30, weeklyDay: DayOfWeek.Tuesday ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", daysOfWeek: "Monday" );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 1, results );
        Assert.AreEqual( "Monday Group", results[0].Name );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_HidesNonPublicGroupsByDefault()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        AddGroup( rockContext, CreateGroup( 10, "Public", groupType.Id, 33.61, -112.30, isPublic: true ) );
        AddGroup( rockContext, CreateGroup( 11, "Private", groupType.Id, 33.62, -112.30, isPublic: false ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30" );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 1, results );
        Assert.AreEqual( "Public", results[0].Name );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithIncludeNonPublicGroups_ReturnsNonPublicGroups()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        AddGroup( rockContext, CreateGroup( 10, "Public", groupType.Id, 33.61, -112.30, isPublic: true ) );
        AddGroup( rockContext, CreateGroup( 11, "Private", groupType.Id, 33.62, -112.30, isPublic: false ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", includeNonPublicGroups: true );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 2, results );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_HidesOverCapacityGroupsByDefault()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        AddGroup( rockContext, CreateGroup( 10, "Has Room", groupType.Id, 33.61, -112.30, groupCapacity: 5, activeMemberCount: 1 ) );
        AddGroup( rockContext, CreateGroup( 11, "Full", groupType.Id, 33.62, -112.30, groupCapacity: 2, activeMemberCount: 2 ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30" );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 1, results );
        Assert.AreEqual( "Has Room", results[0].Name );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithIncludeOverCapacityGroups_ReturnsFullGroups()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        AddGroup( rockContext, CreateGroup( 10, "Has Room", groupType.Id, 33.61, -112.30, groupCapacity: 5, activeMemberCount: 1 ) );
        AddGroup( rockContext, CreateGroup( 11, "Full", groupType.Id, 33.62, -112.30, groupCapacity: 2, activeMemberCount: 2 ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", includeGroupsOverCapacity: true );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 2, results );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithMaxResultsAboveCap_ClampsAndReportsCap()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        AddGroup( rockContext, CreateGroup( 10, "Near Group", groupType.Id, 33.61, -112.30 ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", maxResults: 100 );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var instructions = result.GetInstructions();

        Assert.IsNotNull( instructions );
        Assert.IsTrue( instructions.Any( i => i.Contains( "25" ) ) );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithTimeRange_FiltersToGroupsWithinIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        AddGroup( rockContext, CreateGroup( 10, "Evening", groupType.Id, 33.61, -112.30, weeklyTime: new TimeSpan( 18, 0, 0 ) ) );
        AddGroup( rockContext, CreateGroup( 11, "Late Night", groupType.Id, 33.62, -112.30, weeklyTime: new TimeSpan( 21, 0, 0 ) ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        // 10:00 AM - 8:00 PM keeps the 6 PM group, excludes the 9 PM group.
        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", earliestTime: "10:00 AM", latestTime: "8:00 PM" );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 1, results );
        Assert.AreEqual( "Evening", results[0].Name );
        Assert.AreEqual( "6:00 PM", results[0].MeetingTime );
    }

    [TestMethod]
    public void FindNearbyGroups_WithInvalidLatestTime_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30", latestTime: "not a time" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithPersonIdKeyAndNoOrigin_UsesPersonMappedAddress()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        // The target person's mapped home is at (33.60, -112.30).
        var person = AddPersonWithMappedLocation( rockContext, 42, 33.60, -112.30 );

        AddGroup( rockContext, CreateGroup( 10, "Near Group", groupType.Id, 33.61, -112.30 ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString() );

        var result = skill.FindNearbyGroups( personIdKey: IdHasher.Instance.GetHash( person.Id ) );

        var results = GetNearbyGroups( result );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.HasCount( 1, results );
        Assert.AreEqual( "Near Group", results[0].Name );
    }

    [TestMethod]
    public void FindNearbyGroups_WhenResolvedPersonHasNoMappedAddress_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );
        AddFamilyGroupType( rockContext );

        var person = new Person
        {
            Id = 42,
            Guid = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Person",
            PrimaryAliasId = 42
        };
        rockContext.Set<Person>().Add( person );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString() );

        var result = skill.FindNearbyGroups( personIdKey: IdHasher.Instance.GetHash( person.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroups_WithNoMatchingGroups_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString() );

        var result = skill.FindNearbyGroups( origin: "33.60,-112.30" );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    #endregion
}
