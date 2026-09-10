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

using Rock.AI.Agent.Classes;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CmsSkill;

public partial class CmsSkillTests
{
    #region UpdateRequestFilter

    [TestMethod]
    public void UpdateRequestFilter_WithValidData_UpdatesFilter()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = SeedSite( rockContext, 610, "Filter Site" );

        var requestFilter = SeedRequestFilter( rockContext, 600, "Original Name", "FILTER_KEY" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdateRequestFilter(
            requestFilterIdKey: IdHasher.Instance.GetHash( requestFilter.Id ),
            name: "Renamed Filter",
            siteIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( site.Id ) },
            isActive: true );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed Filter", requestFilter.Name );
        Assert.AreEqual( site.Id, requestFilter.SiteId );
        Assert.IsTrue( requestFilter.IsActive );
    }

    [TestMethod]
    public void UpdateRequestFilter_WithMissingFilter_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdateRequestFilter(
            requestFilterIdKey: IdHasher.Instance.GetHash( 999 ),
            name: "Renamed" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void UpdateRequestFilter_WithInvalidSite_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var requestFilter = SeedRequestFilter( rockContext, 600, "Original Name", "FILTER_KEY" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // A site key that resolves to no site makes the navigation update record an
        // error, exercising the post-update error guard.
        var result = skill.UpdateRequestFilter(
            requestFilterIdKey: IdHasher.Instance.GetHash( requestFilter.Id ),
            siteIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( 999 ) } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
