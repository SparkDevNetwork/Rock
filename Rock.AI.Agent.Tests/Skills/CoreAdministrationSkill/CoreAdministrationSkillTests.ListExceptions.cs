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

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region ListExceptions

    [TestMethod]
    public void ListExceptions_WithNoExceptions_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptions();

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListExceptions_WithExceptions_ReturnsSummary()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<ExceptionLog>().Add( new ExceptionLog
        {
            Id = 50,
            Guid = new Guid( "7b000001-0000-4000-8000-000000000001" ),
            ExceptionType = "System.InvalidOperationException",
            Description = "Something went wrong.",
            CreatedDateTime = RockDateTime.Now.AddDays( -1 ),
            ParentId = null
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptions();

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListExceptions_WithStartAfterEnd_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptions(
            startDateTime: RockDateTime.Now,
            endDateTime: RockDateTime.Now.AddDays( -1 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListExceptions_WithRangeExceedingMaximum_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptions(
            startDateTime: RockDateTime.Now.AddDays( -40 ),
            endDateTime: RockDateTime.Now );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListExceptions_WithFilters_ReturnsSummary()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = new Site { Id = 1, Guid = new Guid( "7b000010-0000-4000-8000-000000000010" ), Name = "Main Site" };
        var page = new Page { Id = 1, Guid = new Guid( "7b000011-0000-4000-8000-000000000011" ), InternalName = "Home", LayoutId = 1 };

        rockContext.Set<Site>().Add( site );
        rockContext.Set<Page>().Add( page );
        rockContext.Set<ExceptionLog>().Add( new ExceptionLog
        {
            Id = 50,
            Guid = new Guid( "7b000012-0000-4000-8000-000000000012" ),
            ExceptionType = "System.InvalidOperationException",
            Description = "Something went wrong.",
            CreatedDateTime = RockDateTime.Now.AddDays( -1 ),
            ParentId = null,
            SiteId = site.Id,
            PageId = page.Id
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // Exercises the partial-type, site, and page filter branches.
        var result = skill.ListExceptions(
            partialExceptionType: "InvalidOperation",
            siteIdKey: IdHasher.Instance.GetHash( site.Id ),
            pageIdKey: IdHasher.Instance.GetHash( page.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    #endregion
}
