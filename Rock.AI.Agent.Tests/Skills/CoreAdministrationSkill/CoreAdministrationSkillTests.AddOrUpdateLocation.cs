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

using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region AddOrUpdateLocation

    [TestMethod]
    public void AddOrUpdateLocation_AddExistingAddress_ReusesLocation()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocationCountries( rockContext );
        var existing = SeedLocation( rockContext, 50, street1: "1 Q7W Way", city: "Phoenix", state: "AZ", postalCode: "85001", country: "US" );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            street1: "1 Q7W Way",
            city: "Phoenix",
            state: "AZ",
            postalCode: "85001",
            country: "US" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( 1, rockContext.Set<Location>().Count() );
        Assert.AreEqual( existing.Guid, ( ( LocationDetailResult ) result.GetContent() ).Guid );
        Assert.IsTrue( result.GetInstructions().Any( i => i.Contains( "reused" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateLocation_AddExistingAddressWithOtherValues_LeavesLocationUnchanged()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocationCountries( rockContext );
        var existing = SeedLocation( rockContext, 50, street1: "1 Q7W Way", city: "Phoenix", state: "AZ", postalCode: "85001", country: "US" );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            street1: "1 Q7W Way",
            city: "Phoenix",
            state: "AZ",
            postalCode: "85001",
            country: "US",
            isActive: false );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( existing.Guid, ( ( LocationDetailResult ) result.GetContent() ).Guid );
        Assert.IsTrue( existing.IsActive );
        Assert.IsTrue( result.GetInstructions().Any( i => i.Contains( "were not applied" ) ) );
    }

    [TestMethod]
    [Ignore( "A hidden match is treated as no match, so the new row is verified with LocationService.Verify, which creates its own RockContext. The mocked database cannot supply one. Check this path on a live site." )]
    public void AddOrUpdateLocation_AddExistingAddressWithoutViewAccess_CreatesNewLocation()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocationCountries( rockContext );
        var existing = SeedLocation( rockContext, 50, street1: "1 Q7W Way", city: "Phoenix", state: "AZ", postalCode: "85001", country: "US" );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );
        MockAuthorizationHelper.DenyAllUsers<Location>( rockContext, Authorization.VIEW, existing.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            street1: "1 Q7W Way",
            city: "Phoenix",
            state: "AZ",
            postalCode: "85001",
            country: "US" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( 2, rockContext.Set<Location>().Count() );
        Assert.AreNotEqual( existing.Guid, ( ( LocationDetailResult ) result.GetContent() ).Guid );
        Assert.IsFalse( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateLocation_AddAddressWithInvalidAttributeKey_ReturnsErrorBeforeLookup()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocationCountries( rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // No location has this address, so without the early return the
        // lookup would go on to call the address verification service.
        var result = skill.AddOrUpdateLocation(
            street1: "1 Q7W Way",
            city: "Phoenix",
            state: "AZ",
            postalCode: "85001",
            country: "US",
            attributeValues: new List<AttributeValueResult>
            {
                new AttributeValueResult { Key = "NoSuchAttribute", Value = "x" }
            } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "'NoSuchAttribute' does not exist" ) ) );
        Assert.IsFalse( result.GetErrorMessages().Any( m => m.Contains( "could not be checked" ) ) );
        Assert.AreEqual( 0, rockContext.Set<Location>().Count() );
    }

    [TestMethod]
    public void AddOrUpdateLocation_UpdateAddressWithInvalidAttributeKey_ReturnsErrorAndLeavesAddress()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocationCountries( rockContext );
        var home = SeedLocation( rockContext, 50, street1: "1 Q7W Way", city: "Phoenix" );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            locationIdKey: IdHasher.Instance.GetHash( home.Id ),
            street1: "2 Q7W Way",
            attributeValues: new List<AttributeValueResult>
            {
                new AttributeValueResult { Key = "NoSuchAttribute", Value = "x" }
            } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "'NoSuchAttribute' does not exist" ) ) );
        Assert.IsNull( home.StandardizeAttemptedDateTime );
    }

    [TestMethod]
    public void AddOrUpdateLocation_AddAddressWithUnknownCountry_ReturnsValidationError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocationCountries( rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            street1: "1 Q7W Way",
            city: "Phoenix",
            country: "ZZ" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "\"ZZ\" is invalid" ) ) );
        Assert.AreEqual( 0, rockContext.Set<Location>().Count() );
    }

    [TestMethod]
    public void AddOrUpdateLocation_AddWithoutNameOrAddress_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation( isActive: true );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "name or at least one address part" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateLocation_AddNamedLocationWithParent_CreatesChild()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var building = SeedLocation( rockContext, 50, name: "Main Building" );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            name: new SetOrClear<string> { Value = "Room 101" },
            parentLocationIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( building.Id ) } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var room = rockContext.Set<Location>().SingleOrDefault( l => l.Name == "Room 101" );

        Assert.IsNotNull( room );
        Assert.AreEqual( building.Id, room.ParentLocationId );
        Assert.IsTrue( room.IsActive );
    }

    [TestMethod]
    public void AddOrUpdateLocation_UpdateName_UpdatesLocation()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var room = SeedLocation( rockContext, 50, name: "Room 101" );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            locationIdKey: IdHasher.Instance.GetHash( room.Id ),
            name: new SetOrClear<string> { Value = "Room 102" },
            isActive: false );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Room 102", room.Name );
        Assert.IsFalse( room.IsActive );
        Assert.AreEqual( 1, rockContext.Set<Location>().Count() );
    }

    [TestMethod]
    public void AddOrUpdateLocation_AddWithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation( name: new SetOrClear<string> { Value = "Room 101" } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateLocation_UpdateWithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var room = SeedLocation( rockContext, 50, name: "Room 101" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            locationIdKey: IdHasher.Instance.GetHash( room.Id ),
            name: new SetOrClear<string> { Value = "Room 102" } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.AreEqual( "Room 101", room.Name );
    }

    [TestMethod]
    public void AddOrUpdateLocation_UpdateNamedLocationWithBlankName_ReturnsError()
    {
        var blankNames = new[]
        {
            new SetOrClear<string> { Value = "" },
            new SetOrClear<string> { Value = "   " },
            new SetOrClear<string> { ClearValue = true }
        };

        foreach ( var blankName in blankNames )
        {
            using var scope = TestHelper.CreateScopedRockApp();
            var rockContext = scope.App.CreateRockContext();

            var room = SeedLocation( rockContext, 50, name: "Room 101" );
            MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

            var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

            var result = skill.AddOrUpdateLocation(
                locationIdKey: IdHasher.Instance.GetHash( room.Id ),
                name: blankName );

            Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
            Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "cannot be cleared" ) ) );
            Assert.AreEqual( "Room 101", room.Name );
        }
    }

    [TestMethod]
    public void AddOrUpdateLocation_UpdateName_TrimsName()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var room = SeedLocation( rockContext, 50, name: "Room 101" );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            locationIdKey: IdHasher.Instance.GetHash( room.Id ),
            name: new SetOrClear<string> { Value = "  Room 102  " } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Room 102", room.Name );
    }

    [TestMethod]
    public void AddOrUpdateLocation_AddNamedLocation_TrimsName()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation( name: new SetOrClear<string> { Value = " Main Building " } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( 1, rockContext.Set<Location>().Count( l => l.Name == "Main Building" ) );
    }

    [TestMethod]
    public void AddOrUpdateLocation_AddWithAddressAsParent_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var home = SeedLocation( rockContext, 50, street1: "1 Q7W Way", city: "Phoenix" );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            name: new SetOrClear<string> { Value = "Room 101" },
            parentLocationIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( home.Id ) } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "must be a named location" ) ) );
        Assert.AreEqual( 1, rockContext.Set<Location>().Count() );
    }

    [TestMethod]
    public void AddOrUpdateLocation_MoveUnderParentWithoutEditAccess_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var mainBuilding = SeedLocation( rockContext, 50, name: "Main Building" );
        var lockedBuilding = SeedLocation( rockContext, 51, name: "Locked Building" );
        var room = SeedLocation( rockContext, 52, name: "Room 101", parentLocationId: mainBuilding.Id );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );
        MockAuthorizationHelper.DenyAllUsers<Location>( rockContext, Authorization.EDIT, lockedBuilding.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateLocation(
            locationIdKey: IdHasher.Instance.GetHash( room.Id ),
            parentLocationIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( lockedBuilding.Id ) } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized to add a location to the selected parent" ) ) );
    }

    #endregion
}
