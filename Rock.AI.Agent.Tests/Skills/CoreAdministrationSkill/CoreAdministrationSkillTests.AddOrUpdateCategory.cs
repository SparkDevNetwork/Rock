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
    #region AddOrUpdateCategory - Add

    [TestMethod]
    public void AddOrUpdateCategory_AddWithValidData_CreatesCategory()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );
        AllowAllUsersToEdit<Category>( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            name: "Test Category" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var created = rockContext.Set<Category>().SingleOrDefault();
        Assert.IsNotNull( created );
        Assert.AreEqual( "Test Category", created.Name );
        Assert.AreEqual( entityType.Id, created.EntityTypeId );

        var content = result.GetContent() as CategoryDetailResult;
        Assert.IsNotNull( content );
        Assert.AreEqual( "Test Category", content.Name );
    }

    [TestMethod]
    public void AddOrUpdateCategory_AddOrdersNewCategoryAfterSiblings()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );
        AllowAllUsersToEdit<Category>( rockContext );

        rockContext.Set<Category>().Add( new Category
        {
            Id = 1,
            Guid = new Guid( "a2c0f5e4-1f9d-4b3a-9c2e-0a1b2c3d4e5f" ),
            Name = "Existing Sibling",
            EntityTypeId = entityType.Id,
            Order = 4
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            name: "New Sibling" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var created = rockContext.Set<Category>().SingleOrDefault( c => c.Name == "New Sibling" );
        Assert.IsNotNull( created );
        Assert.AreEqual( 5, created.Order );
    }

    [TestMethod]
    public void AddOrUpdateCategory_AddWithoutEntityTypeIdKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory( name: "Missing Entity Type" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "entityTypeIdKey" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateCategory_AddWithoutName_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "name" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateCategory_AddWithoutEditAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        // Intentionally not granting EDIT. A new category resolves to the
        // global default, which denies EDIT unless a rule allows it.
        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            name: "Unauthorized Category" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateCategory_AddWithInvalidEntityType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // A well-formed idKey that resolves to no entity type on record.
        var result = skill.AddOrUpdateCategory(
            entityTypeIdKey: IdHasher.Instance.GetHash( 999 ),
            name: "Invalid Entity Type" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateCategory_AddWithValidParent_CreatesCategoryUnderParent()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var parent = new Category
        {
            Id = 1,
            Guid = new Guid( "e60529a8-5d3f-4f7e-b062-4e5f60718293" ),
            Name = "Parent",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( parent );
        AllowAllUsersToEdit<Category>( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            name: "Child",
            parentCategoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( parent.Id ) } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var created = rockContext.Set<Category>().SingleOrDefault( c => c.Name == "Child" );
        Assert.IsNotNull( created );
        Assert.AreEqual( parent.Id, created.ParentCategoryId );

        var content = result.GetContent() as CategoryDetailResult;
        Assert.IsNotNull( content );
        Assert.IsNotNull( content.ParentCategory );
    }

    [TestMethod]
    public void AddOrUpdateCategory_AddWithMissingParentCategory_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // A parent idKey that resolves to no category on record. Setting the
        // navigation property records an error before the save is reached.
        var result = skill.AddOrUpdateCategory(
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            name: "Orphan",
            parentCategoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( 999 ) } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateCategory_AddWithParentOfDifferentEntityType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var childEntityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );
        var parentEntityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        rockContext.Set<Category>().Add( new Category
        {
            Id = 1,
            Guid = new Guid( "b3d1f6e5-2a0e-4c4b-8d3f-1b2c3d4e5f60" ),
            Name = "Parent In Other Type",
            EntityTypeId = parentEntityType.Id
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            entityTypeIdKey: IdHasher.Instance.GetHash( childEntityType.Id ),
            name: "Child",
            parentCategoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( 1 ) } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "different entity type" ) ) );
    }

    #endregion

    #region AddOrUpdateCategory - Update

    [TestMethod]
    public void AddOrUpdateCategory_UpdateWithValidData_UpdatesCategory()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var category = new Category
        {
            Id = 10,
            Guid = new Guid( "c4e2f7d6-3b1f-4d5c-9e40-2c3d4e5f6071" ),
            Name = "Original Name",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( category );
        AllowAllUsersToEdit<Category>( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( category.Id ),
            name: "Renamed" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed", category.Name );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateWithMissingCategory_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( 999 ),
            name: "Renamed" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateToDifferentEntityType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var originalEntityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );
        var otherEntityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        var category = new Category
        {
            Id = 10,
            Guid = new Guid( "d5f30807-4c2f-4e6d-af51-3d4e5f607182" ),
            Name = "Original Name",
            EntityTypeId = originalEntityType.Id
        };

        rockContext.Set<Category>().Add( category );
        AllowAllUsersToEdit<Category>( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( category.Id ),
            entityTypeIdKey: IdHasher.Instance.GetHash( otherEntityType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "different entity type" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateWithMatchingEntityType_UpdatesCategory()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var category = new Category
        {
            Id = 10,
            Guid = new Guid( "f7051a29-5e40-4a7f-b173-4f5061728394" ),
            Name = "Original Name",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( category );
        AllowAllUsersToEdit<Category>( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // Supplying the category's own entity type is allowed and should pass
        // through the "cannot be moved" guard rather than trip it.
        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( category.Id ),
            entityTypeIdKey: IdHasher.Instance.GetHash( entityType.Id ),
            name: "Renamed" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed", category.Name );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateWithInvalidSuppliedEntityType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var category = new Category
        {
            Id = 10,
            Guid = new Guid( "08162b3a-6f51-4b80-a284-5061728394a5" ),
            Name = "Original Name",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( category );
        AllowAllUsersToEdit<Category>( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( category.Id ),
            entityTypeIdKey: IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateMoveUnderEditableParentWithoutEditOnCategory_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var finance = new Category
        {
            Id = 10,
            Guid = new Guid( "1a2b3c4d-0001-4a37-9000-000000000010" ),
            Name = "Finance",
            EntityTypeId = entityType.Id
        };

        var youth = new Category
        {
            Id = 11,
            Guid = new Guid( "1a2b3c4d-0001-4a37-9000-000000000011" ),
            Name = "Youth",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( finance );
        rockContext.Set<Category>().Add( youth );

        // The caller can edit Youth but not Finance, which has no rules of its
        // own. Moving Finance under Youth must not grant EDIT through Youth.
        MockAuthorizationHelper.AllowAllUsers<Category>( rockContext, Authorization.EDIT, youth.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( finance.Id ),
            parentCategoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( youth.Id ) } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized to edit" ) ) );
        Assert.IsNull( finance.ParentCategoryId );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateMoveUnderParentWithoutEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var youth = new Category
        {
            Id = 11,
            Guid = new Guid( "1a2b3c4d-0002-4a37-9000-000000000011" ),
            Name = "Youth",
            EntityTypeId = entityType.Id
        };

        var youthSub = new Category
        {
            Id = 12,
            Guid = new Guid( "1a2b3c4d-0002-4a37-9000-000000000012" ),
            Name = "Youth Sub",
            EntityTypeId = entityType.Id,
            ParentCategoryId = youth.Id,
            ParentCategory = youth
        };

        var missions = new Category
        {
            Id = 13,
            Guid = new Guid( "1a2b3c4d-0002-4a37-9000-000000000013" ),
            Name = "Missions",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( youth );
        rockContext.Set<Category>().Add( youthSub );
        rockContext.Set<Category>().Add( missions );

        // The caller can edit Youth Sub through its own rule, but not Missions.
        MockAuthorizationHelper.AllowAllUsers<Category>( rockContext, Authorization.EDIT, youthSub.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( youthSub.Id ),
            parentCategoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( missions.Id ) } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "selected parent category" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateMoveWithEditOnBoth_MovesCategory()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var youth = new Category
        {
            Id = 11,
            Guid = new Guid( "1a2b3c4d-0003-4a37-9000-000000000011" ),
            Name = "Youth",
            EntityTypeId = entityType.Id
        };

        var youthSub = new Category
        {
            Id = 12,
            Guid = new Guid( "1a2b3c4d-0003-4a37-9000-000000000012" ),
            Name = "Youth Sub",
            EntityTypeId = entityType.Id,
            ParentCategoryId = youth.Id,
            ParentCategory = youth
        };

        var missions = new Category
        {
            Id = 13,
            Guid = new Guid( "1a2b3c4d-0003-4a37-9000-000000000013" ),
            Name = "Missions",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( youth );
        rockContext.Set<Category>().Add( youthSub );
        rockContext.Set<Category>().Add( missions );

        MockAuthorizationHelper.AllowAllUsers<Category>( rockContext, Authorization.EDIT, youthSub.Id );
        MockAuthorizationHelper.AllowAllUsers<Category>( rockContext, Authorization.EDIT, missions.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( youthSub.Id ),
            parentCategoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( missions.Id ) } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( missions.Id, youthSub.ParentCategoryId );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateRenameWithEditThroughParent_UpdatesCategory()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var youth = new Category
        {
            Id = 11,
            Guid = new Guid( "1a2b3c4d-0004-4a37-9000-000000000011" ),
            Name = "Youth",
            EntityTypeId = entityType.Id
        };

        var child = new Category
        {
            Id = 12,
            Guid = new Guid( "1a2b3c4d-0004-4a37-9000-000000000012" ),
            Name = "Child",
            EntityTypeId = entityType.Id,
            ParentCategoryId = youth.Id,
            ParentCategory = youth
        };

        rockContext.Set<Category>().Add( youth );
        rockContext.Set<Category>().Add( child );

        // EDIT on the child is inherited from its stored parent.
        MockAuthorizationHelper.AllowAllUsers<Category>( rockContext, Authorization.EDIT, youth.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( child.Id ),
            name: "Renamed Child" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed Child", child.Name );
        Assert.AreEqual( youth.Id, child.ParentCategoryId );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateWithoutEditOnStoredCategory_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var category = new Category
        {
            Id = 10,
            Guid = new Guid( "1a2b3c4d-0005-4a37-9000-000000000010" ),
            Name = "Original Name",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( category );

        // Intentionally not granting EDIT.
        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( category.Id ),
            name: "Renamed" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "edit that category" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateMoveWithAdministrateOnParent_MovesCategory()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var youth = new Category
        {
            Id = 11,
            Guid = new Guid( "1a2b3c4d-0007-4a37-9000-000000000011" ),
            Name = "Youth",
            EntityTypeId = entityType.Id
        };

        var youthSub = new Category
        {
            Id = 12,
            Guid = new Guid( "1a2b3c4d-0007-4a37-9000-000000000012" ),
            Name = "Youth Sub",
            EntityTypeId = entityType.Id,
            ParentCategoryId = youth.Id,
            ParentCategory = youth
        };

        var missions = new Category
        {
            Id = 13,
            Guid = new Guid( "1a2b3c4d-0007-4a37-9000-000000000013" ),
            Name = "Missions",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( youth );
        rockContext.Set<Category>().Add( youthSub );
        rockContext.Set<Category>().Add( missions );

        // The caller can edit Youth Sub through its own rule and can only
        // administrate Missions, which is enough to move a category under it.
        MockAuthorizationHelper.AllowAllUsers<Category>( rockContext, Authorization.EDIT, youthSub.Id );
        MockAuthorizationHelper.AllowAllUsers<Category>( rockContext, Authorization.ADMINISTRATE, missions.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( youthSub.Id ),
            parentCategoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( missions.Id ) } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( missions.Id, youthSub.ParentCategoryId );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateMoveByGuidUnderEditableParentWithoutEditOnCategory_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var finance = new Category
        {
            Id = 10,
            Guid = new Guid( "1a2b3c4d-0008-4a37-9000-000000000010" ),
            Name = "Finance",
            EntityTypeId = entityType.Id
        };

        var youth = new Category
        {
            Id = 11,
            Guid = new Guid( "1a2b3c4d-0008-4a37-9000-000000000011" ),
            Name = "Youth",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( finance );
        rockContext.Set<Category>().Add( youth );

        // Same as the IdKey case, but both categories are addressed by Guid.
        MockAuthorizationHelper.AllowAllUsers<Category>( rockContext, Authorization.EDIT, youth.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: finance.Guid.ToString(),
            parentCategoryIdKey: new SetOrClear<string> { Value = youth.Guid.ToString() } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "edit that category" ) ) );
        Assert.IsNull( finance.ParentCategoryId );
    }

    [TestMethod]
    public void AddOrUpdateCategory_UpdateMoveToRootWithEditOnCategory_MovesCategory()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var youth = new Category
        {
            Id = 11,
            Guid = new Guid( "1a2b3c4d-0006-4a37-9000-000000000011" ),
            Name = "Youth",
            EntityTypeId = entityType.Id
        };

        var youthSub = new Category
        {
            Id = 12,
            Guid = new Guid( "1a2b3c4d-0006-4a37-9000-000000000012" ),
            Name = "Youth Sub",
            EntityTypeId = entityType.Id,
            ParentCategoryId = youth.Id,
            ParentCategory = youth
        };

        rockContext.Set<Category>().Add( youth );
        rockContext.Set<Category>().Add( youthSub );

        MockAuthorizationHelper.AllowAllUsers<Category>( rockContext, Authorization.EDIT, youthSub.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateCategory(
            categoryIdKey: IdHasher.Instance.GetHash( youthSub.Id ),
            parentCategoryIdKey: new SetOrClear<string> { ClearValue = true } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsNull( youthSub.ParentCategoryId );
    }

    #endregion
}
