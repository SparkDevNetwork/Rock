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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
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
    #region FindNearbyGroupsForMe - Guards and validation

    [TestMethod]
    public void FindNearbyGroupsForMe_WithNoCurrentPerson_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString(), currentPerson: null );

        var result = skill.FindNearbyGroupsForMe();

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroupsForMe_WithNoConfiguredGroupTypes_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = new Person { Id = 42, Guid = Guid.NewGuid(), FirstName = "Test", LastName = "Person" };

        var skill = CreateFinderSkill( scope.App, rockContext, configuredGroupTypes: string.Empty, currentPerson: person );

        var result = skill.FindNearbyGroupsForMe();

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroupsForMe_WithGroupTypeOutsideConfiguredSet_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var person = new Person { Id = 42, Guid = Guid.NewGuid(), FirstName = "Test", LastName = "Person" };

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString(), currentPerson: person );

        var result = skill.FindNearbyGroupsForMe( groupTypeIdKey: IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroupsForMe_WithUnresolvableGroupTypeIdKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );

        var person = new Person { Id = 42, Guid = Guid.NewGuid(), FirstName = "Test", LastName = "Person" };

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString(), currentPerson: person );

        // A group type id key that cannot be decoded to any id.
        var result = skill.FindNearbyGroupsForMe( groupTypeIdKey: "not-a-valid-id-key" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void FindNearbyGroupsForMe_WhenPersonHasNoMappedAddress_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        AddFinderGroupType( rockContext );
        AddFamilyGroupType( rockContext );

        var person = new Person { Id = 42, Guid = Guid.NewGuid(), FirstName = "Test", LastName = "Person" };
        rockContext.Set<Person>().Add( person );

        var skill = CreateFinderSkill( scope.App, rockContext, FinderGroupTypeGuid.ToString(), currentPerson: person );

        var result = skill.FindNearbyGroupsForMe();

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion

    #region FindNearbyGroupsForMe - Search behavior

    [ConditionalTestMethod]
    public void FindNearbyGroupsForMe_ReturnsGroupsNearCurrentPerson()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = AddFinderGroupType( rockContext );

        // The current person's mapped home is at (33.60, -112.30).
        var person = AddPersonWithMappedLocation( rockContext, 42, 33.60, -112.30 );

        AddGroup( rockContext, CreateGroup( 10, "Near Group", groupType.Id, 33.61, -112.30 ) );
        AddGroup( rockContext, CreateGroup( 11, "Far Group", groupType.Id, 34.10, -112.30 ) );

        var skill = CreateFinderSkill( scope.App, rockContext, groupType.Guid.ToString(), currentPerson: person );

        var result = skill.FindNearbyGroupsForMe();

        var results = GetNearbyGroups( result );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.HasCount( 2, results );
        Assert.AreEqual( "Near Group", results[0].Name );
    }

    [ConditionalTestMethod]
    public void FindNearbyGroupsForMe_WithGroupTypeIdKey_NarrowsToThatType()
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

        var person = AddPersonWithMappedLocation( rockContext, 42, 33.60, -112.30 );

        AddGroup( rockContext, CreateGroup( 10, "Wanted", wantedType.Id, 33.61, -112.30 ) );
        AddGroup( rockContext, CreateGroup( 11, "Other", otherType.Id, 33.61, -112.30 ) );

        var configured = $"{wantedType.Guid},{otherType.Guid}";
        var skill = CreateFinderSkill( scope.App, rockContext, configured, currentPerson: person );

        var result = skill.FindNearbyGroupsForMe( groupTypeIdKey: IdHasher.Instance.GetHash( wantedType.Id ) );

        var results = GetNearbyGroups( result );

        Assert.HasCount( 1, results );
        Assert.AreEqual( "Wanted", results[0].Name );
    }

    #endregion

    #region Support

    /// <summary>
    /// Adds the Family group type (required for resolving a person's mapped
    /// address through <c>PersonService.GetGeopoints</c>).
    /// </summary>
    private static GroupType AddFamilyGroupType( RockContext rockContext )
    {
        var familyGroupType = new GroupType
        {
            Id = 1,
            Guid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(),
            Name = "Family",
            GroupTerm = "Family",
            GroupMemberTerm = "Member"
        };

        rockContext.Set<GroupType>().Add( familyGroupType );

        return familyGroupType;
    }

    /// <summary>
    /// Adds a person with a family whose mapped location is at the given
    /// coordinates, so the finder tools can resolve their home as the origin.
    /// </summary>
    private static Person AddPersonWithMappedLocation( RockContext rockContext, int personId, double latitude, double longitude )
    {
        var familyGroupType = AddFamilyGroupType( rockContext );

        var person = new Person
        {
            Id = personId,
            Guid = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Person",
            // A primary alias is required for the person to resolve through
            // AgentToolHelper.GetOptionalEntity (used by the personIdKey path).
            PrimaryAliasId = personId
        };

        rockContext.Set<Person>().Add( person );

        var location = new Location
        {
            Id = 900,
            Guid = Guid.NewGuid(),
            GeoPoint = new GeographyPoint( latitude, longitude ).ToDatabase()
        };

        var familyGroup = new Group
        {
            Id = 500,
            Guid = Guid.NewGuid(),
            Name = "Test Family",
            GroupTypeId = familyGroupType.Id,
            IsActive = true
        };

        var familyGroupLocation = new GroupLocation
        {
            Id = 5000,
            GroupId = familyGroup.Id,
            Group = familyGroup,
            LocationId = location.Id,
            Location = location,
            IsMappedLocation = true
        };

        familyGroup.GroupLocations = new List<GroupLocation> { familyGroupLocation };

        var member = new GroupMember
        {
            Id = 5001,
            PersonId = person.Id,
            Person = person,
            GroupId = familyGroup.Id,
            Group = familyGroup,
            GroupMemberStatus = GroupMemberStatus.Active
        };

        rockContext.Set<Group>().Add( familyGroup );
        rockContext.Set<GroupMember>().Add( member );

        return person;
    }

    #endregion
}
