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

using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
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
    #region AddOrUpdateAuthorizationForEntity

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_AddWithValidRule_CreatesRule()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.VIEW,
            allowOrDeny: AllowOrDeny.Allow,
            specialRole: SpecialRole.AllUsers );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<Auth>().Any( a => a.Action == Authorization.VIEW && a.SpecialRole == SpecialRole.AllUsers ) );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_WithInvalidEntityType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( 999999 ),
            action: Authorization.VIEW,
            allowOrDeny: AllowOrDeny.Allow,
            specialRole: SpecialRole.AllUsers );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_WithUnsupportedAction_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: "NotARealAction",
            allowOrDeny: AllowOrDeny.Allow,
            specialRole: SpecialRole.AllUsers );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_AddWithoutSubject_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.VIEW,
            allowOrDeny: AllowOrDeny.Allow );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "exactly one subject" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_AddWithMultipleSubjects_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.VIEW,
            allowOrDeny: AllowOrDeny.Allow,
            personIdKey: IdHasher.Instance.GetHash( 1 ),
            specialRole: SpecialRole.AllUsers );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "exactly one subject" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_WithoutAdministerAccess_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.VIEW,
            allowOrDeny: AllowOrDeny.Allow,
            specialRole: SpecialRole.AllUsers );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_AddWithPersonSubject_CreatesRule()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        var person = MockData.CreatePerson( rockContext );

        // Administer to reach the tool, and view so the person can be resolved.
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.VIEW,
            allowOrDeny: AllowOrDeny.Allow,
            personIdKey: IdHasher.Instance.GetHash( person.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<Auth>().Any( a => a.PersonAliasId == person.PrimaryAliasId && a.Action == Authorization.VIEW ) );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_AddWithGroupSubject_CreatesRule()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );

        // A security role group so RoleCache resolves it as a valid subject.
        var role = new Rock.Model.Group
        {
            Id = 750,
            Guid = new System.Guid( "c0000001-0000-4000-8000-000000000750" ),
            Name = "Administrators",
            IsSecurityRole = true
        };
        rockContext.Set<Rock.Model.Group>().Add( role );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.VIEW,
            allowOrDeny: AllowOrDeny.Allow,
            groupIdKey: IdHasher.Instance.GetHash( role.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<Auth>().Any( a => a.GroupId == role.Id && a.Action == Authorization.VIEW ) );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_AddWithNonRoleGroup_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );

        // A regular group (not a security role) cannot be used in a rule.
        var group = new Rock.Model.Group
        {
            Id = 751,
            Guid = new System.Guid( "c0000001-0000-4000-8000-000000000751" ),
            Name = "Regular Group",
            GroupTypeId = 999,
            IsSecurityRole = false
        };
        rockContext.Set<Rock.Model.Group>().Add( group );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.VIEW,
            allowOrDeny: AllowOrDeny.Allow,
            groupIdKey: IdHasher.Instance.GetHash( group.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "security role" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_UpdateExistingRule_ChangesAllowOrDeny()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        // An existing all-users View allow to update by its authIdKey.
        var auth = MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.VIEW, "A", 0, SpecialRole.AllUsers, order: 0 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.VIEW,
            allowOrDeny: AllowOrDeny.Deny,
            authIdKey: IdHasher.Instance.GetHash( auth.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "D", rockContext.Set<Auth>().Single( a => a.Id == auth.Id ).AllowOrDeny );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_DenyingAdministrateToSelf_IsRefused()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // Denying Administrate to all users, as an anonymous caller, would remove
        // the caller's own ability to administer the entity.
        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.ADMINISTRATE,
            allowOrDeny: AllowOrDeny.Deny,
            specialRole: SpecialRole.AllUsers );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "administer this entity" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_DenyingAdministrateAsKnownPerson_IsRefused()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        var person = MockData.CreatePerson( rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson: person ) );

        // A known caller denying Administrate to all users: the retain-access guard
        // resolves the caller's aliases and finds the deny would apply to them.
        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.ADMINISTRATE,
            allowOrDeny: AllowOrDeny.Deny,
            specialRole: SpecialRole.AllUsers );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "administer this entity" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateAuthorizationForEntity_DenyingAllUsersWhileSelfExplicitlyAllowed_Proceeds()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        var person = MockData.CreatePerson( rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        // An explicit Administrate allow for this person, ordered ahead of the deny
        // being added, so the retain-access guard sees the caller keeps access.
        MockAuthorizationHelper.AddRule( rockContext, entityType.Id, Authorization.ADMINISTRATE, "A", 0, SpecialRole.None, personAliasId: person.PrimaryAliasId, order: 0 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson: person ) );

        var result = skill.AddOrUpdateAuthorizationForEntity(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            action: Authorization.ADMINISTRATE,
            allowOrDeny: AllowOrDeny.Deny,
            specialRole: SpecialRole.AllUsers,
            order: 1 );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    #endregion
}
