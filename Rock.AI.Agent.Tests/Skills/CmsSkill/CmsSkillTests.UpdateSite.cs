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

namespace Rock.AI.Agent.Tests.Skills.CmsSkill;

public partial class CmsSkillTests
{
    #region UpdateSite

    [TestMethod]
    public void UpdateSite_WithValidData_UpdatesSite()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = SeedSite( rockContext, 400, "Original Site" );

        var defaultPage = new Page
        {
            Id = 401,
            Guid = new Guid( "c0000001-0000-4000-8000-000000000401" ),
            InternalName = "Default Page",
            LayoutId = 1
        };
        var loginPage = new Page
        {
            Id = 402,
            Guid = new Guid( "c0000001-0000-4000-8000-000000000402" ),
            InternalName = "Login Page",
            LayoutId = 1
        };
        rockContext.Set<Page>().Add( defaultPage );
        rockContext.Set<Page>().Add( loginPage );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdateSite(
            siteIdKey: IdHasher.Instance.GetHash( site.Id ),
            name: new SetOrClear<string> { Value = "Renamed Site" },
            description: new SetOrClear<string> { Value = "A renamed site." },
            isActive: true,
            theme: new SetOrClear<string> { Value = "Rock" },
            externalUrl: new SetOrClear<string> { Value = "https://external.example" },
            defaultPageIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( defaultPage.Id ) },
            loginPageIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( loginPage.Id ) } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed Site", site.Name );
        Assert.AreEqual( "A renamed site.", site.Description );
        Assert.IsTrue( site.IsActive );
        Assert.AreEqual( "Rock", site.Theme );
        Assert.AreEqual( "https://external.example", site.ExternalUrl );
        Assert.AreEqual( defaultPage.Id, site.DefaultPageId );
        Assert.AreEqual( loginPage.Id, site.LoginPageId );
    }

    [TestMethod]
    public void UpdateSite_WithMissingSite_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdateSite(
            siteIdKey: IdHasher.Instance.GetHash( 999 ),
            name: new SetOrClear<string> { Value = "Renamed" } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void UpdateSite_ClearingName_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = SeedSite( rockContext, 400, "Original Site" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdateSite(
            siteIdKey: IdHasher.Instance.GetHash( site.Id ),
            name: new SetOrClear<string> { ClearValue = true } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "cannot be cleared" ) ) );
    }

    [TestMethod]
    public void UpdateSite_WithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = SeedSite( rockContext, 400, "Original Site" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdateSite(
            siteIdKey: IdHasher.Instance.GetHash( site.Id ),
            name: new SetOrClear<string> { Value = "Renamed" } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void UpdateSite_WithInvalidDefaultPage_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = SeedSite( rockContext, 400, "Original Site" );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // A page key that resolves to no page makes the navigation update record an
        // error, exercising the post-update error guard.
        var result = skill.UpdateSite(
            siteIdKey: IdHasher.Instance.GetHash( site.Id ),
            defaultPageIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( 999 ) } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    /// <summary>
    /// Seeds a site into the mocked context and returns it.
    /// </summary>
    internal static Site SeedSite( Data.RockContext rockContext, int id, string name )
    {
        var site = new Site
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            IsActive = true,
            // A domain keeps SiteCache building from falling back to the empty
            // PublicApplicationRoot global attribute, which would throw.
            SiteDomains = new System.Collections.Generic.List<SiteDomain>
            {
                new SiteDomain { Domain = "rock.example", Order = 0 }
            }
        };

        rockContext.Set<Site>().Add( site );

        return site;
    }

    #endregion
}
