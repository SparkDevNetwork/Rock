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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.ReportingSkill;

public partial class ReportingSkillTests
{
    #region ListDataViews

    [TestMethod]
    public void ListDataViews_WithNone_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Person>( true, rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListDataViews( IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListDataViews_WithDataViews_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Person>( true, rockContext );
        SeedDataView( rockContext, 1000, "Active Members", entityType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListDataViews( IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListDataViews_WithPartialName_ReturnsMatching()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Person>( true, rockContext );
        SeedDataView( rockContext, 1000, "Active Members", entityType.Id );
        SeedDataView( rockContext, 1001, "Inactive Members", entityType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListDataViews( IdHasher.Instance.GetHash( entityType.Id ), partialName: "Active" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListDataViews_WithCategoryIdKey_ReturnsMatching()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Person>( true, rockContext );

        var category = new Category
        {
            Id = 1050,
            Guid = new System.Guid( "a1000000-0000-4000-8000-000000001051" ),
            Name = "Data View Category",
            EntityTypeId = entityType.Id
        };
        rockContext.Set<Category>().Add( category );

        var dataView = SeedDataView( rockContext, 1000, "Active Members", entityType.Id );
        dataView.CategoryId = category.Id;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListDataViews( IdHasher.Instance.GetHash( entityType.Id ), categoryIdKey: IdHasher.Instance.GetHash( category.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListDataViews_WithInvalidEntityType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListDataViews( IdHasher.Instance.GetHash( 999999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
