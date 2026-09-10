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
    #region DeleteCategory

    [TestMethod]
    public void DeleteCategory_WithValidCategory_DeletesIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var category = new Category
        {
            Id = 10,
            Guid = new Guid( "1a000001-0000-4000-8000-000000000001" ),
            Name = "Deletable",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( category );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteCategory( IdHasher.Instance.GetHash( category.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<Category>().Any( c => c.Id == category.Id ) );
    }

    [TestMethod]
    public void DeleteCategory_WithMissingCategory_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteCategory( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void DeleteCategory_WithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var category = new Category
        {
            Id = 10,
            Guid = new Guid( "1a000002-0000-4000-8000-000000000002" ),
            Name = "Protected",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Category>().Add( category );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteCategory( IdHasher.Instance.GetHash( category.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void DeleteCategory_WithSystemCategory_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var category = new Category
        {
            Id = 10,
            Guid = new Guid( "1a000003-0000-4000-8000-000000000003" ),
            Name = "System",
            EntityTypeId = entityType.Id,
            IsSystem = true
        };

        rockContext.Set<Category>().Add( category );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteCategory( IdHasher.Instance.GetHash( category.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<Category>().Any( c => c.Id == category.Id ) );
    }

    [TestMethod]
    public void DeleteCategory_WithChildCategories_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<WorkflowType>( true, rockContext );

        var parent = new Category
        {
            Id = 10,
            Guid = new Guid( "1a000004-0000-4000-8000-000000000004" ),
            Name = "Parent",
            EntityTypeId = entityType.Id
        };

        var child = new Category
        {
            Id = 11,
            Guid = new Guid( "1a000005-0000-4000-8000-000000000005" ),
            Name = "Child",
            EntityTypeId = entityType.Id,
            ParentCategoryId = parent.Id
        };

        rockContext.Set<Category>().Add( parent );
        rockContext.Set<Category>().Add( child );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteCategory( IdHasher.Instance.GetHash( parent.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "child categories" ) ) );
    }

    #endregion
}
