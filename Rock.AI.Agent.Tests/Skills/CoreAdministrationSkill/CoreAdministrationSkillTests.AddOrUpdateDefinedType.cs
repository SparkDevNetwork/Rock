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
    #region AddOrUpdateDefinedType

    [TestMethod]
    public void AddOrUpdateDefinedType_AddWithValidData_CreatesDefinedType()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        // A category so the categoryIdKey resolves and the result's Category maps.
        var category = new Category
        {
            Id = 5,
            Guid = new Guid( "4a0000c0-0000-4000-8000-0000000000c0" ),
            Name = "Type Category",
            EntityTypeId = EntityTypeCache.Get<DefinedType>( true, rockContext ).Id
        };

        rockContext.Set<Category>().Add( category );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // Every optional property is set so each update branch is exercised.
        var result = skill.AddOrUpdateDefinedType(
            name: "New Type",
            description: new SetOrClear<string> { Value = "A description." },
            categoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( category.Id ) },
            helpText: new SetOrClear<string> { Value = "Some help." },
            isActive: true,
            categorizedValuesEnabled: true,
            enableSecurityOnValues: true );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var created = rockContext.Set<DefinedType>().SingleOrDefault();
        Assert.IsNotNull( created );
        Assert.AreEqual( "New Type", created.Name );
        Assert.AreEqual( category.Id, created.CategoryId );
    }

    [TestMethod]
    public void AddOrUpdateDefinedType_AddWithoutName_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedType();

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "name" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateDefinedType_AddWithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedType( name: "Unauthorized Type" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateDefinedType_UpdateWithValidData_UpdatesDefinedType()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedType = new DefinedType
        {
            Id = 20,
            Guid = new Guid( "4a000001-0000-4000-8000-000000000001" ),
            Name = "Original Type"
        };

        rockContext.Set<DefinedType>().Add( definedType );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedType(
            definedTypeIdKey: IdHasher.Instance.GetHash( definedType.Id ),
            name: "Renamed Type" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed Type", definedType.Name );
    }

    [TestMethod]
    public void AddOrUpdateDefinedType_UpdateWithMissingDefinedType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateDefinedType(
            definedTypeIdKey: IdHasher.Instance.GetHash( 999 ),
            name: "Renamed Type" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
