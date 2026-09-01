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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region AddOrUpdateDefinedValue

    [TestMethod]
    public void AddOrUpdateDefinedValue_AddWithValidData_CreatesDefinedValue()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedType = SeedDefinedType( rockContext, 20 );

        // A category so the categoryIdKey resolves and the result's Category maps.
        var category = new Category
        {
            Id = 6,
            Guid = new Guid( "5a0000c0-0000-4000-8000-0000000000c0" ),
            Name = "Value Category",
            EntityTypeId = EntityTypeCache.Get<DefinedValue>( true, rockContext ).Id
        };

        rockContext.Set<Category>().Add( category );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // Every optional property is set so each update branch is exercised.
        var result = skill.AddOrUpdateDefinedValue(
            definedTypeIdKey: IdHasher.Instance.GetHash( definedType.Id ),
            value: "New Value",
            description: new SetOrClear<string> { Value = "A description." },
            categoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( category.Id ) },
            isActive: true );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var created = rockContext.Set<DefinedValue>().SingleOrDefault();
        Assert.IsNotNull( created );
        Assert.AreEqual( "New Value", created.Value );
        Assert.AreEqual( definedType.Id, created.DefinedTypeId );
        Assert.AreEqual( category.Id, created.CategoryId );
    }

    [TestMethod]
    public void AddOrUpdateDefinedValue_AddWithoutDefinedType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedValue( value: "Orphan Value" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "definedTypeIdKey" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateDefinedValue_AddWithoutValue_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedType = SeedDefinedType( rockContext, 20 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedValue(
            definedTypeIdKey: IdHasher.Instance.GetHash( definedType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "value" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateDefinedValue_AddWithInvalidDefinedType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedValue(
            definedTypeIdKey: IdHasher.Instance.GetHash( 999 ),
            value: "New Value" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateDefinedValue_UpdateWithValidData_UpdatesDefinedValue()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedType = SeedDefinedType( rockContext, 20 );

        var definedValue = new DefinedValue
        {
            Id = 30,
            Guid = new Guid( "5a000001-0000-4000-8000-000000000001" ),
            Value = "Original Value",
            DefinedTypeId = definedType.Id
        };

        rockContext.Set<DefinedValue>().Add( definedValue );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedValue(
            definedValueIdKey: IdHasher.Instance.GetHash( definedValue.Id ),
            value: "Renamed Value" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed Value", definedValue.Value );
    }

    [TestMethod]
    public void AddOrUpdateDefinedValue_UpdateToDifferentDefinedType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedType = SeedDefinedType( rockContext, 20 );
        var otherType = SeedDefinedType( rockContext, 21 );

        var definedValue = new DefinedValue
        {
            Id = 30,
            Guid = new Guid( "5a000002-0000-4000-8000-000000000002" ),
            Value = "Original Value",
            DefinedTypeId = definedType.Id
        };

        rockContext.Set<DefinedValue>().Add( definedValue );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedValue(
            definedValueIdKey: IdHasher.Instance.GetHash( definedValue.Id ),
            definedTypeIdKey: IdHasher.Instance.GetHash( otherType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "different defined type" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateDefinedValue_UpdateWithMissingDefinedValue_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedValue(
            definedValueIdKey: IdHasher.Instance.GetHash( 999 ),
            value: "Renamed Value" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    /// <summary>
    /// Seeds a defined type with the given id into the mocked context.
    /// </summary>
    private static DefinedType SeedDefinedType( Data.RockContext rockContext, int id )
    {
        var definedType = new DefinedType
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = $"Type {id}"
        };

        rockContext.Set<DefinedType>().Add( definedType );

        return definedType;
    }

    #endregion
}
