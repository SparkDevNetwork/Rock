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
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CmsSkill;

public partial class CmsSkillTests
{
    #region ListShortcodes

    [TestMethod]
    public void ListShortcodes_WithNone_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListShortcodes();

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListShortcodes_WithShortcodes_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<LavaShortcode>().Add( new LavaShortcode
        {
            Id = 700,
            Guid = new Guid( "b0000001-0000-4000-8000-000000000001" ),
            Name = "Accordion",
            TagName = "accordion",
            IsActive = true
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListShortcodes();

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListShortcodes_WithInactiveShortcode_ExcludedByDefault()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<LavaShortcode>().Add( new LavaShortcode
        {
            Id = 710,
            Guid = new Guid( "b0000001-0000-4000-8000-000000000710" ),
            Name = "Retired Shortcode",
            TagName = "retired",
            IsActive = false
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListShortcodes();

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListShortcodes_WithInactiveShortcode_IncludedWhenRequested()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<LavaShortcode>().Add( new LavaShortcode
        {
            Id = 710,
            Guid = new Guid( "b0000001-0000-4000-8000-000000000710" ),
            Name = "Retired Shortcode",
            TagName = "retired",
            IsActive = false
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListShortcodes( includeInactive: true );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListShortcodes_WithMatchingPartialName_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<LavaShortcode>().Add( new LavaShortcode
        {
            Id = 700,
            Guid = new Guid( "b0000001-0000-4000-8000-000000000001" ),
            Name = "Accordion",
            TagName = "accordion",
            IsActive = true
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListShortcodes( partialName: "accord" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListShortcodes_WithNonMatchingPartialName_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<LavaShortcode>().Add( new LavaShortcode
        {
            Id = 700,
            Guid = new Guid( "b0000001-0000-4000-8000-000000000001" ),
            Name = "Accordion",
            TagName = "accordion",
            IsActive = true
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListShortcodes( partialName: "no-such-shortcode" );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListShortcodes_WithCategory_ReturnsMatching()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var category = new Category
        {
            Id = 720,
            Guid = new Guid( "b0000001-0000-4000-8000-000000000720" ),
            Name = "Shortcode Category"
        };
        rockContext.Set<Category>().Add( category );

        rockContext.Set<LavaShortcode>().Add( new LavaShortcode
        {
            Id = 721,
            Guid = new Guid( "b0000001-0000-4000-8000-000000000721" ),
            Name = "Categorized",
            TagName = "categorized",
            IsActive = true,
            Categories = new System.Collections.Generic.List<Category> { category }
        } );

        // The category is resolved with a security check, so allow VIEW by default.
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListShortcodes( categoryIdKey: IdHasher.Instance.GetHash( category.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    #endregion
}
