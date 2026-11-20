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
    #region DeleteLocation

    [TestMethod]
    public void DeleteLocation_WhenUnused_DeletesIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var room = SeedLocation( rockContext, 50, name: "Room 101" );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteLocation( IdHasher.Instance.GetHash( room.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<Location>().Any( l => l.Id == room.Id ) );
    }

    [TestMethod]
    public void DeleteLocation_WithChildLocation_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var building = SeedLocation( rockContext, 50, name: "Main Building" );
        SeedLocation( rockContext, 51, name: "Room 101", parentLocationId: building.Id );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteLocation( IdHasher.Instance.GetHash( building.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<Location>().Any( l => l.Id == building.Id ) );
    }

    [TestMethod]
    public void DeleteLocation_WhenUsedByFamily_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var home = SeedLocation( rockContext, 50, street1: "1 Q7W Way", city: "Phoenix" );

        rockContext.Set<GroupLocation>().Add( new GroupLocation
        {
            Id = 60,
            Guid = Guid.NewGuid(),
            GroupId = 70,
            LocationId = home.Id,
            Location = home
        } );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteLocation( IdHasher.Instance.GetHash( home.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "group or family" ) ) );
        Assert.IsTrue( rockContext.Set<Location>().Any( l => l.Id == home.Id ) );
    }

    [TestMethod]
    public void DeleteLocation_WhenAssignedToDevice_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var room = SeedLocation( rockContext, 50, name: "Room 101" );

        var kiosk = new Device
        {
            Id = 60,
            Guid = Guid.NewGuid(),
            Name = "Kiosk 1"
        };

        kiosk.Locations.Add( room );
        rockContext.Set<Device>().Add( kiosk );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteLocation( IdHasher.Instance.GetHash( room.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "device" ) ) );
        Assert.IsTrue( rockContext.Set<Location>().Any( l => l.Id == room.Id ) );
    }

    [TestMethod]
    public void DeleteLocation_WhenStoredInAddressAttribute_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var address = SeedLocation( rockContext, 50, street1: "1 Q7W Way", city: "Phoenix" );

        var addressFieldType = MockData.CreateFieldType( rockContext, SystemGuid.FieldType.ADDRESS.AsGuid(), "Address", "Rock.Field.Types.AddressFieldType" );
        var attribute = MockData.CreateAttribute( rockContext, "MeetingAddress", "Meeting Address", addressFieldType.Id );

        rockContext.Set<AttributeValue>().Add( new AttributeValue
        {
            Id = 80,
            Guid = Guid.NewGuid(),
            AttributeId = attribute.Id,
            Attribute = attribute,
            EntityId = 90,
            Value = address.Guid.ToString()
        } );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteLocation( IdHasher.Instance.GetHash( address.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "attribute value" ) ) );
        Assert.IsTrue( rockContext.Set<Location>().Any( l => l.Id == address.Id ) );
    }

    [TestMethod]
    public void DeleteLocation_WhenAttributeDefaultValue_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var room = SeedLocation( rockContext, 50, name: "Room 101" );

        var locationFieldType = MockData.CreateFieldType( rockContext, SystemGuid.FieldType.LOCATION.AsGuid(), "Location", "Rock.Field.Types.LocationFieldType" );
        MockData.CreateAttribute( rockContext, "DefaultRoom", "Default Room", locationFieldType.Id, defaultValue: room.Guid.ToString() );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteLocation( IdHasher.Instance.GetHash( room.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "default value" ) ) );
        Assert.IsTrue( rockContext.Set<Location>().Any( l => l.Id == room.Id ) );
    }

    [TestMethod]
    public void DeleteLocation_WithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var room = SeedLocation( rockContext, 50, name: "Room 101" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteLocation( IdHasher.Instance.GetHash( room.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.IsTrue( rockContext.Set<Location>().Any( l => l.Id == room.Id ) );
    }

    #endregion
}
