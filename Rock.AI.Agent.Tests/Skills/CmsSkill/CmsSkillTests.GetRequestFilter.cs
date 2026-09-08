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
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CmsSkill;

public partial class CmsSkillTests
{
    #region GetRequestFilter

    [TestMethod]
    public void GetRequestFilter_WithValidFilter_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = SeedSite( rockContext, 620, "Filter Site" );

        var requestFilter = SeedRequestFilter( rockContext, 600, "Mobile Visitors", "MOBILE" );
        requestFilter.SiteId = site.Id;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetRequestFilter( IdHasher.Instance.GetHash( requestFilter.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetRequestFilter_WithoutSite_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        // No SiteId, so the result's Site null branch runs.
        var requestFilter = SeedRequestFilter( rockContext, 600, "All Sites Filter", "ALL" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetRequestFilter( IdHasher.Instance.GetHash( requestFilter.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetRequestFilter_WithMissingFilter_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetRequestFilter( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
