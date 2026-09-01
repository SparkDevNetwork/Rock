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
    #region ListExceptionInstances

    [TestMethod]
    public void ListExceptionInstances_WithNoExceptions_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptionInstances();

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListExceptionInstances_WithExceptions_ReturnsInstances()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        // Site and Page navigation are set because the tool projects their Guid
        // and Name, which an in-memory mock evaluates eagerly.
        var site = new Site { Id = 1, Guid = new Guid( "7c000001-0000-4000-8000-000000000001" ), Name = "Main Site" };
        var page = new Page { Id = 1, Guid = new Guid( "7c000002-0000-4000-8000-000000000002" ), InternalName = "Home" };

        rockContext.Set<Site>().Add( site );
        rockContext.Set<Page>().Add( page );
        rockContext.Set<ExceptionLog>().Add( new ExceptionLog
        {
            Id = 50,
            Guid = new Guid( "7c000003-0000-4000-8000-000000000003" ),
            ExceptionType = "System.InvalidOperationException",
            Description = "Something went wrong.",
            CreatedDateTime = RockDateTime.Now.AddDays( -1 ),
            ParentId = null,
            SiteId = site.Id,
            Site = site,
            PageId = page.Id,
            Page = page
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // The type, site, and page filters are exercised here. The description
        // filter is omitted: it runs a SUBSTRING that SQL tolerates but an
        // in-memory mock throws on for a description shorter than the grouping
        // prefix, so it is not exercised against the mock.
        var result = skill.ListExceptionInstances(
            exceptionType: "InvalidOperation",
            siteIdKey: IdHasher.Instance.GetHash( site.Id ),
            pageIdKey: IdHasher.Instance.GetHash( page.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListExceptionInstances_WithStartAfterEnd_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptionInstances(
            startDateTime: RockDateTime.Now,
            endDateTime: RockDateTime.Now.AddDays( -1 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListExceptionInstances_WithRangeExceedingMaximum_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptionInstances(
            startDateTime: RockDateTime.Now.AddDays( -40 ),
            endDateTime: RockDateTime.Now );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
