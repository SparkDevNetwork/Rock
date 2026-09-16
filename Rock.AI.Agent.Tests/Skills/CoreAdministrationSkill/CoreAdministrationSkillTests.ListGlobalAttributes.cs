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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region ListGlobalAttributes

    [TestMethod]
    public void ListGlobalAttributes_WithNone_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListGlobalAttributes();

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListGlobalAttributes_WithGlobalAttribute_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        MockData.CreateAttribute( rockContext, "OrganizationName", "Organization Name" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListGlobalAttributes();

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListGlobalAttributes_WithPartialName_ReturnsMatching()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        MockData.CreateAttribute( rockContext, "OrganizationName", "Organization Name" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListGlobalAttributes( partialName: "Organization" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListGlobalAttributes_WithCategoryIdKey_FiltersByCategory()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        MockData.CreateAttribute( rockContext, "OrganizationName", "Organization Name" );

        var category = new Category
        {
            Id = 8,
            Guid = new Guid( "6b0000c0-0000-4000-8000-0000000000c0" ),
            Name = "Attribute Category",
            EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id
        };

        rockContext.Set<Category>().Add( category );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // The seeded global attribute is in no category, so the category filter
        // excludes it. This exercises the categoryIdKey resolution and filter branch.
        var result = skill.ListGlobalAttributes( categoryIdKey: IdHasher.Instance.GetHash( category.Id ) );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    #endregion
}
