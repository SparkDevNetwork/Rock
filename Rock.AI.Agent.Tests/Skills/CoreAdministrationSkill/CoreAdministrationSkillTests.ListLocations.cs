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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region ListLocations

    [TestMethod]
    public void ListLocations_WithoutFilters_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocation( rockContext, 50, street1: "1 Q7W Way", city: "Phoenix" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // includeInactive alone does not limit the results, so it does not count.
        var result = skill.ListLocations( includeInactive: true );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "At least one filter" ) ) );
    }

    [TestMethod]
    public void ListLocations_ByCity_ReturnsOnlyMatchingLocations()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocationCountries( rockContext );
        var first = SeedLocation( rockContext, 50, street1: "1 Q7W Way", city: "Phoenix" );
        var second = SeedLocation( rockContext, 51, street1: "2 Q7W Way", city: "Phoenix" );
        SeedLocation( rockContext, 52, street1: "3 Q7W Way", city: "Tempe" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListLocations( city: "Phoenix" );
        var page = result.GetContent() as PaginatedResult<LocationResult>;

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsNotNull( page, "Expected a page of location results." );
        CollectionAssert.AreEquivalent( new[] { first.Guid, second.Guid }, page.Items.Select( i => i.Guid.Value ).ToArray() );
        Assert.IsTrue( page.Items.All( i => i.FormattedAddress.Contains( "Phoenix" ) ) );
    }

    [TestMethod]
    public void ListLocations_ByPartialName_ReturnsActiveNamedLocations()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocationCountries( rockContext );
        var building = SeedLocation( rockContext, 50, name: "Main Building" );
        SeedLocation( rockContext, 51, name: "Old Main Building", isActive: false );
        SeedLocation( rockContext, 52, name: "Gym" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListLocations( partialName: "Main" );
        var page = result.GetContent() as PaginatedResult<LocationResult>;

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsNotNull( page, "Expected a page of location results." );
        Assert.HasCount( 1, page.Items );
        Assert.AreEqual( building.Guid, page.Items[0].Guid );
        Assert.AreEqual( "Main Building", page.Items[0].Name );
    }

    /// <summary>
    /// Seeds a location with the given id into the mocked context. Supply a
    /// name for a named location, address parts for an address, or both.
    /// </summary>
    private static Location SeedLocation(
        RockContext rockContext,
        int id,
        string name = null,
        string street1 = null,
        string city = null,
        string state = "AZ",
        string postalCode = "85001",
        string country = "US",
        int? parentLocationId = null,
        bool isActive = true )
    {
        var hasAddress = street1.IsNotNullOrWhiteSpace() || city.IsNotNullOrWhiteSpace();

        var location = new Location
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            Street1 = street1,
            City = city,
            State = hasAddress ? state : null,
            PostalCode = hasAddress ? postalCode : null,
            Country = hasAddress ? country : null,
            ParentLocationId = parentLocationId,
            IsActive = isActive,
            CreatedDateTime = RockDateTime.Now.AddDays( -1 )
        };

        // The mocked context performs no navigation-property fixup.
        if ( parentLocationId.HasValue )
        {
            location.ParentLocation = rockContext.Set<Location>().FirstOrDefault( l => l.Id == parentLocationId.Value );
        }

        rockContext.Set<Location>().Add( location );

        return location;
    }

    /// <summary>
    /// Seeds the Countries defined type with a single US value. Formatting and
    /// validating an address both read this type, and both fail without it.
    /// </summary>
    private static void SeedLocationCountries( RockContext rockContext )
    {
        var countries = new DefinedType
        {
            Id = 40,
            Guid = SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid(),
            Name = "Countries"
        };

        rockContext.Set<DefinedType>().Add( countries );

        MockData.CreateDefinedValue( rockContext, Guid.NewGuid(), "US", countries.Id );
    }

    #endregion
}
