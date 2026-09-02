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
    #region DeleteAuthorizationForEntity

    [TestMethod]
    public void DeleteAuthorizationForEntity_WithValidRule_DeletesIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var auth = MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.VIEW, "A" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteAuthorizationForEntity(
            authIdKey: IdHasher.Instance.GetHash( auth.Id ),
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<Auth>().Any( a => a.Id == auth.Id ) );
    }

    [TestMethod]
    public void DeleteAuthorizationForEntity_WithInvalidEntityType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteAuthorizationForEntity(
            authIdKey: IdHasher.Instance.GetHash( 100 ),
            entityTypeIdKey: IdHasher.Instance.GetHash( 999999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void DeleteAuthorizationForEntity_WithMissingRule_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteAuthorizationForEntity(
            authIdKey: IdHasher.Instance.GetHash( 12345 ),
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void DeleteAuthorizationForEntity_WithRuleOnDifferentEntity_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        var otherType = EntityTypeCache.Get<DefinedType>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        // The rule belongs to a different entity type than the one supplied.
        var auth = MockAuthorizationHelper.AddRule( rockContext, otherType.Id, Authorization.VIEW, "A" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteAuthorizationForEntity(
            authIdKey: IdHasher.Instance.GetHash( auth.Id ),
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "does not identify a rule on this entity" ) ) );
    }

    [TestMethod]
    public void DeleteAuthorizationForEntity_WithoutAdministerAccess_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        var auth = MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.VIEW, "A" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteAuthorizationForEntity(
            authIdKey: IdHasher.Instance.GetHash( auth.Id ),
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void DeleteAuthorizationForEntity_RemovingAdministrateFromSelf_IsRefused()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        // The only Administrate-granting rule on the entity. Deleting it, as an
        // anonymous caller, would remove the caller's own access.
        var auth = MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.ADMINISTRATE, "A" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteAuthorizationForEntity(
            authIdKey: IdHasher.Instance.GetHash( auth.Id ),
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "administer this entity" ) ) );
    }

    [TestMethod]
    public void DeleteAuthorizationForEntity_RemovingPersonRuleWhileInheritedAllows_Proceeds()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        var person = MockData.CreatePerson( rockContext );

        // The default grants Administrate to all users; the entity adds an explicit
        // person rule on top. Deleting the entity rule still leaves the caller with
        // access through the inherited default, so the retain-access guard walks the
        // parent authority and allows the deletion.
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );
        var auth = MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.ADMINISTRATE, "A", 0, SpecialRole.None, personAliasId: person.PrimaryAliasId, order: 0 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson: person ) );

        var result = skill.DeleteAuthorizationForEntity(
            authIdKey: IdHasher.Instance.GetHash( auth.Id ),
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<Auth>().Any( a => a.Id == auth.Id ) );
    }

    #endregion
}
