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

namespace Rock.AI.Agent.Tests.Skills.CmsSkill;

public partial class CmsSkillTests
{
    #region GetPageAvailableAttributes

    [TestMethod]
    public void GetPageAvailableAttributes_WithExistingPage_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var page = new Page
        {
            Id = 900,
            Guid = new Guid( "b2000001-0000-4000-8000-000000000001" ),
            InternalName = "Home",
            LayoutId = 1
        };

        rockContext.Set<Page>().Add( page );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetPageAvailableAttributes( pageIdKey: IdHasher.Instance.GetHash( page.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetPageAvailableAttributes_WithParentPageBeforeCreation_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var parentPage = new Page
        {
            Id = 901,
            Guid = new Guid( "b2000002-0000-4000-8000-000000000002" ),
            InternalName = "Parent",
            LayoutId = 1
        };

        rockContext.Set<Page>().Add( parentPage );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetPageAvailableAttributes( parentPageIdKey: IdHasher.Instance.GetHash( parentPage.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetPageAvailableAttributes_WithLayoutBeforeCreation_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = SeedSite( rockContext, 640, "Layout Site" );

        var layout = new Layout
        {
            Id = 640,
            Guid = new Guid( "b2000003-0000-4000-8000-000000000640" ),
            Name = "Full Width",
            SiteId = site.Id
        };
        rockContext.Set<Layout>().Add( layout );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetPageAvailableAttributes( layoutIdKey: IdHasher.Instance.GetHash( layout.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetPageAvailableAttributes_WithSiteBeforeCreation_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = SeedSite( rockContext, 641, "New Page Site" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetPageAvailableAttributes( siteIdKey: IdHasher.Instance.GetHash( site.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetPageAvailableAttributes_WithInvalidParentPage_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetPageAvailableAttributes( parentPageIdKey: IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
