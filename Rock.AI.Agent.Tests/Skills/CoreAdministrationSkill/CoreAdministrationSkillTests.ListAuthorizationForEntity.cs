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

using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    #region ListAuthorizationForEntity

    [TestMethod]
    public void ListAuthorizationForEntity_WithAdministerAccess_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListAuthorizationForEntity( IdHasher.Instance.GetHash( entityType.Id ), action: Authorization.VIEW );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListAuthorizationForEntity_WithInvalidEntityType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListAuthorizationForEntity( IdHasher.Instance.GetHash( 999999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListAuthorizationForEntity_WithUnsupportedAction_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListAuthorizationForEntity( IdHasher.Instance.GetHash( entityType.Id ), action: "NotARealAction" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "action" ) ) );
    }

    [TestMethod]
    public void ListAuthorizationForEntity_WithoutAdministerAccess_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListAuthorizationForEntity( IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListAuthorizationForEntity_WithRules_ReturnsRulesWithResolvedSubjects()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );

        // A person (via its alias) and a security-role group for the person and
        // group subjects, so BuildSubject resolves both.
        var person = MockData.CreatePerson( rockContext );
        var group = new Rock.Model.Group
        {
            Id = 710,
            Guid = new System.Guid( "c0000001-0000-4000-8000-000000000710" ),
            Name = "Security Role"
        };
        rockContext.Set<Rock.Model.Group>().Add( group );

        // Three View rules on the entity type's own default: all-users, a person,
        // and a group. These populate itemAuths so the loop and SubjectKey run.
        MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.VIEW, "A", 0, SpecialRole.AllUsers, order: 0 );
        MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.VIEW, "A", 0, SpecialRole.None, personAliasId: person.PrimaryAliasId, order: 1 );
        MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.VIEW, "A", 0, SpecialRole.None, groupId: group.Id, order: 2 );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListAuthorizationForEntity( IdHasher.Instance.GetHash( entityType.Id ), action: Authorization.VIEW );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListAuthorizationForEntity_WithoutAction_ReturnsAllActions()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );

        // A rule so at least one action's list is non-empty while every supported
        // action is iterated.
        MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.VIEW, "A", 0, SpecialRole.AllUsers );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // No action supplied, so the tool iterates every supported action.
        var result = skill.ListAuthorizationForEntity( IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListAuthorizationForEntity_WithValidEntityIdKey_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );

        var category = new Category
        {
            Id = 720,
            Guid = new System.Guid( "c0000001-0000-4000-8000-000000000720" ),
            Name = "Secured Category",
            EntityTypeId = entityType.Id
        };
        rockContext.Set<Category>().Add( category );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // A specific entity is named, so TryGetAdministrableEntity resolves it
        // rather than the entity type's default.
        var result = skill.ListAuthorizationForEntity(
            IdHasher.Instance.GetHash( entityType.Id ),
            entityIdKey: IdHasher.Instance.GetHash( category.Id ),
            action: Authorization.VIEW );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListAuthorizationForEntity_WithInvalidEntityIdKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // A well-formed entity key that resolves to no entity of that type.
        var result = skill.ListAuthorizationForEntity(
            IdHasher.Instance.GetHash( entityType.Id ),
            entityIdKey: IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
