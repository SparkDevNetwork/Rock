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

namespace Rock.AI.Agent.Tests.Skills.CmsSkill;

public partial class CmsSkillTests
{
    #region LookupRequestFilters

    [TestMethod]
    public void LookupRequestFilters_WithNone_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.LookupRequestFilters();

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void LookupRequestFilters_WithFilters_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = SeedSite( rockContext, 630, "Filter Site" );

        var requestFilter = SeedRequestFilter( rockContext, 600, "Mobile Visitors", "MOBILE" );
        requestFilter.SiteId = site.Id;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.LookupRequestFilters();

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    /// <summary>
    /// Seeds a request filter and returns it.
    /// </summary>
    internal static RequestFilter SeedRequestFilter( Data.RockContext rockContext, int id, string name, string key )
    {
        var requestFilter = new RequestFilter
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            RequestFilterKey = key,
            IsActive = true
        };

        rockContext.Set<RequestFilter>().Add( requestFilter );

        return requestFilter;
    }

    #endregion
}
