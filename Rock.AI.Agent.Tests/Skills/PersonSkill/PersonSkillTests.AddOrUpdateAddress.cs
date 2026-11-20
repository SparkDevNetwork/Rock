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

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.PersonSkill;

public partial class PersonSkillTests
{
    #region AddOrUpdateAddress

    [TestMethod]
    public void AddOrUpdateAddress_OtherPersonWithViewOnly_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var locationType = MockData.CreateDefinedValue( rockContext, Guid.NewGuid(), "Home", 2 );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var target = CreatePersonWithEmail( rockContext, "Target" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.AddOrUpdateAddress( IdHasher.Instance.GetHash( target.Id ), IdHasher.Instance.GetHash( locationType.Id ), street1: "1 P9V2H Way", city: "Phoenix", state: "AZ", postalCode: "85001" );

        AssertNotAuthorized( result );
        Assert.AreEqual( 0, rockContext.Set<GroupLocation>().Count() );
        Assert.AreEqual( 0, rockContext.Set<Location>().Count() );
    }

    [TestMethod]
    public void AddOrUpdateAddress_SharedLocationChangedToExistingAddress_RepointsOnlyThisFamily()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var homeType = SeedAddressTypes( rockContext );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var neighbor = CreatePersonWithEmail( rockContext, "Neighbor" );
        var callerFamily = SeedFamily( rockContext, 101, caller );
        var neighborFamily = SeedFamily( rockContext, 102, neighbor );

        // Both families point at the same address row, as they would after
        // the family editor found an existing match with LocationService.Get().
        var sharedLocation = SeedLocation( rockContext, 201, "1 P9V2H Way", "Phoenix", "AZ", "85001" );
        var targetLocation = SeedLocation( rockContext, 202, "2 P9V2H Road", "Tempe", "AZ", "85281" );
        var callerGroupLocation = SeedGroupLocation( rockContext, 301, callerFamily, sharedLocation, homeType.Id );
        var neighborGroupLocation = SeedGroupLocation( rockContext, 302, neighborFamily, sharedLocation, homeType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.AddOrUpdateAddress( IdHasher.Instance.GetHash( caller.Id ), IdHasher.Instance.GetHash( homeType.Id ), street1: "2 P9V2H Road", city: "Tempe", state: "AZ", postalCode: "85281" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        // Only the caller's family moves to the new address row.
        Assert.AreSame( targetLocation, callerGroupLocation.Location );
        Assert.AreSame( sharedLocation, neighborGroupLocation.Location );

        // The shared row itself is never edited.
        Assert.AreEqual( "1 P9V2H Way", sharedLocation.Street1 );
        Assert.AreEqual( string.Empty, sharedLocation.Street2 );
        Assert.AreEqual( "Phoenix", sharedLocation.City );
        Assert.AreEqual( "AZ", sharedLocation.State );
        Assert.AreEqual( "85001", sharedLocation.PostalCode );

        // No new address row is made for an address that already exists.
        Assert.AreEqual( 2, rockContext.Set<Location>().Count( l => l.Id != 0 ) );

        // The old address is kept as a previous address for the caller's family.
        var previousAddress = rockContext.Set<GroupLocation>()
            .Single( gl => gl.GroupLocationTypeValueId == PreviousAddressTypeId );
        Assert.AreEqual( callerFamily.Id, previousAddress.GroupId );
        Assert.AreEqual( "1 P9V2H Way", previousAddress.Location.Street1 );
        Assert.AreNotSame( sharedLocation, previousAddress.Location );
    }

    [TestMethod]
    public void AddOrUpdateAddress_SameAddressSent_LeavesSharedLocationAlone()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var homeType = SeedAddressTypes( rockContext );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var neighbor = CreatePersonWithEmail( rockContext, "Neighbor" );
        var callerFamily = SeedFamily( rockContext, 101, caller );
        var neighborFamily = SeedFamily( rockContext, 102, neighbor );
        var sharedLocation = SeedLocation( rockContext, 201, "1 P9V2H Way", "Phoenix", "AZ", "85001" );
        var callerGroupLocation = SeedGroupLocation( rockContext, 301, callerFamily, sharedLocation, homeType.Id );
        var neighborGroupLocation = SeedGroupLocation( rockContext, 302, neighborFamily, sharedLocation, homeType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.AddOrUpdateAddress( IdHasher.Instance.GetHash( caller.Id ), IdHasher.Instance.GetHash( homeType.Id ), street1: "1 P9V2H Way", city: "Phoenix", isMailingLocation: false );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreSame( sharedLocation, callerGroupLocation.Location );
        Assert.AreSame( sharedLocation, neighborGroupLocation.Location );
        Assert.AreEqual( "1 P9V2H Way", sharedLocation.Street1 );
        Assert.AreEqual( "Phoenix", sharedLocation.City );
        Assert.IsFalse( callerGroupLocation.IsMailingLocation );
        Assert.IsTrue( neighborGroupLocation.IsMailingLocation );

        // Street1 did not change, so no previous address is made.
        Assert.AreEqual( 0, rockContext.Set<GroupLocation>().Count( gl => gl.GroupLocationTypeValueId == PreviousAddressTypeId ) );
    }

    [TestMethod]
    public void AddOrUpdateAddress_OnlyFlagsSent_DoesNotCreatePreviousAddress()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var homeType = SeedAddressTypes( rockContext );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var callerFamily = SeedFamily( rockContext, 101, caller );
        var location = SeedLocation( rockContext, 201, "1 P9V2H Way", "Phoenix", "AZ", "85001" );
        var callerGroupLocation = SeedGroupLocation( rockContext, 301, callerFamily, location, homeType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.AddOrUpdateAddress( IdHasher.Instance.GetHash( caller.Id ), IdHasher.Instance.GetHash( homeType.Id ), isMailingLocation: false );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreSame( location, callerGroupLocation.Location );
        Assert.IsFalse( callerGroupLocation.IsMailingLocation );

        // No street1 was sent, so the street did not change and no previous
        // address is made.
        Assert.AreEqual( 0, rockContext.Set<GroupLocation>().Count( gl => gl.GroupLocationTypeValueId == PreviousAddressTypeId ) );
    }

    [TestMethod]
    public void AddOrUpdateAddress_NewAddressTypeMatchingExistingRow_PointsAtExistingRow()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var homeType = SeedAddressTypes( rockContext );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var callerFamily = SeedFamily( rockContext, 101, caller );
        var existingLocation = SeedLocation( rockContext, 201, "1 P9V2H Way", "Phoenix", "AZ", "85001" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.AddOrUpdateAddress( IdHasher.Instance.GetHash( caller.Id ), IdHasher.Instance.GetHash( homeType.Id ), street1: "1 P9V2H Way", city: "Phoenix", state: "AZ", postalCode: "85001", country: "US" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var groupLocation = rockContext.Set<GroupLocation>().Single( gl => gl.GroupLocationTypeValueId == homeType.Id );
        Assert.AreEqual( callerFamily.Id, groupLocation.GroupId );
        Assert.AreSame( existingLocation, groupLocation.Location );
        Assert.IsTrue( groupLocation.IsMailingLocation );
        Assert.IsTrue( groupLocation.IsMappedLocation );
        Assert.AreEqual( 1, rockContext.Set<Location>().Count() );
    }

    [TestMethod]
    [Ignore( "LocationService.Get() saves a brand new address through its own new RockContext, which the mocked database cannot run. Verify on a live site instead." )]
    public void AddOrUpdateAddress_SharedLocationChangedToNewAddress_CreatesNewRowAndRepointsOnlyThisFamily()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var homeType = SeedAddressTypes( rockContext );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var neighbor = CreatePersonWithEmail( rockContext, "Neighbor" );
        var callerFamily = SeedFamily( rockContext, 101, caller );
        var neighborFamily = SeedFamily( rockContext, 102, neighbor );
        var sharedLocation = SeedLocation( rockContext, 201, "1 P9V2H Way", "Phoenix", "AZ", "85001" );
        var callerGroupLocation = SeedGroupLocation( rockContext, 301, callerFamily, sharedLocation, homeType.Id );
        var neighborGroupLocation = SeedGroupLocation( rockContext, 302, neighborFamily, sharedLocation, homeType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.AddOrUpdateAddress( IdHasher.Instance.GetHash( caller.Id ), IdHasher.Instance.GetHash( homeType.Id ), street1: "3 P9V2H Lane", city: "Mesa", state: "AZ", postalCode: "85201" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreNotSame( sharedLocation, callerGroupLocation.Location );
        Assert.AreEqual( "3 P9V2H Lane", callerGroupLocation.Location.Street1 );
        Assert.AreSame( sharedLocation, neighborGroupLocation.Location );
        Assert.AreEqual( "1 P9V2H Way", sharedLocation.Street1 );
    }

    #endregion

    #region AddOrUpdateAddress Support

    private const int CountriesDefinedTypeId = 11;
    private const int LocationTypesDefinedTypeId = 12;
    private const int PreviousAddressTypeId = 9002;

    /// <summary>
    /// Seeds the Home and Previous address types and the "US" country, which
    /// the address requirement check reads, and returns the Home type.
    /// </summary>
    private static DefinedValue SeedAddressTypes( RockContext rockContext )
    {
        var countriesType = SeedDefinedType( rockContext, CountriesDefinedTypeId, SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid() );
        var locationTypesType = SeedDefinedType( rockContext, LocationTypesDefinedTypeId, SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() );

        var country = MockData.CreateDefinedValue( rockContext, Guid.NewGuid(), "US", countriesType.Id );
        country.IsActive = true;

        var homeType = MockData.CreateDefinedValue( rockContext, SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid(), "Home", locationTypesType.Id );

        var previousType = new DefinedValue
        {
            Id = PreviousAddressTypeId,
            Guid = SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid(),
            Value = "Previous",
            DefinedTypeId = locationTypesType.Id,
            DefinedType = locationTypesType
        };

        rockContext.Set<DefinedValue>().Add( previousType );

        return homeType;
    }

    /// <summary>
    /// Seeds a family group and makes it the person's primary family. The
    /// mocked context does no navigation fixup, so both sides are wired here.
    /// </summary>
    private static Group SeedFamily( RockContext rockContext, int id, Rock.Model.Person person )
    {
        var family = new Group
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = $"{person.LastName} Family",
            GroupLocations = new List<GroupLocation>()
        };

        rockContext.Set<Group>().Add( family );

        person.PrimaryFamilyId = family.Id;
        person.PrimaryFamily = family;

        return family;
    }

    /// <summary>
    /// Seeds an address row. Street2 is an empty string, not null, because
    /// LocationService.Get() trims missing parts to empty strings and treats
    /// any other difference as a casing fix that it saves through a new
    /// RockContext, which the mocked database cannot run.
    /// </summary>
    private static Location SeedLocation( RockContext rockContext, int id, string street1, string city, string state, string postalCode )
    {
        var location = new Location
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Street1 = street1,
            Street2 = string.Empty,
            City = city,
            State = state,
            PostalCode = postalCode,
            Country = "US"
        };

        rockContext.Set<Location>().Add( location );

        return location;
    }

    /// <summary>
    /// Seeds a family address that points at the given address row.
    /// </summary>
    private static GroupLocation SeedGroupLocation( RockContext rockContext, int id, Group family, Location location, int locationTypeValueId )
    {
        var groupLocation = new GroupLocation
        {
            Id = id,
            Guid = Guid.NewGuid(),
            GroupId = family.Id,
            Group = family,
            LocationId = location.Id,
            Location = location,
            GroupLocationTypeValueId = locationTypeValueId,
            IsMailingLocation = true,
            IsMappedLocation = true
        };

        rockContext.Set<GroupLocation>().Add( groupLocation );
        family.GroupLocations.Add( groupLocation );

        return groupLocation;
    }

    #endregion
}
