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
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Configuration;
using Rock.Data;
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

    // Descriptions are kept in the same case as the filter text. SQL matches
    // without regard to case by collation, but the in-memory mock does not.
    private const string ModifyStepDescription = "Unable to save the record during modifystep processing.";

    [TestMethod]
    public void ListExceptionInstances_WithPartialDescriptionPrefix_ReturnsMatchingInstance()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedExceptionInstance( rockContext, 50, ModifyStepDescription );
        SeedExceptionInstance( rockContext, 51, "Something else went wrong." );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptionInstances( description: "Unable to save" );

        CollectionAssert.AreEqual( new[] { 50 }, GetExceptionInstanceItems( result ).Select( e => e.Id ).ToArray() );
    }

    [TestMethod]
    public void ListExceptionInstances_WithFullDescription_ReturnsMatchingInstance()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedExceptionInstance( rockContext, 50, ModifyStepDescription );
        SeedExceptionInstance( rockContext, 51, "Something else went wrong." );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptionInstances( description: ModifyStepDescription );

        CollectionAssert.AreEqual( new[] { 50 }, GetExceptionInstanceItems( result ).Select( e => e.Id ).ToArray() );
    }

    [TestMethod]
    public void ListExceptionInstances_WithTextNotAtStartOfDescription_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedExceptionInstance( rockContext, 50, ModifyStepDescription );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptionInstances( description: "modifystep" );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListExceptionInstances_WithLongDescription_ReturnsCutDescription()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var longDescription = new string( 'x', 1000 );
        SeedExceptionInstance( rockContext, 50, longDescription );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptionInstances();
        var description = GetExceptionInstanceItems( result ).Single().Description;

        Assert.AreEqual( 258, description.Length );
        Assert.AreEqual( longDescription.Substring( 0, 255 ) + "...", description );
    }

    [TestMethod]
    public void ListExceptionInstances_WithShortDescription_ReturnsDescriptionUnchanged()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedExceptionInstance( rockContext, 50, ModifyStepDescription );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListExceptionInstances();

        Assert.AreEqual( ModifyStepDescription, GetExceptionInstanceItems( result ).Single().Description );
    }

    /// <summary>
    /// Seeds an outermost exception from yesterday with the given description.
    /// </summary>
    /// <remarks>
    /// The Site and Page navigation are set because the tool projects their Guid
    /// and Name, which an in-memory mock evaluates eagerly.
    /// </remarks>
    private static void SeedExceptionInstance( RockContext rockContext, int id, string description )
    {
        var site = new Site { Id = id, Guid = Guid.NewGuid(), Name = "Main Site" };
        var page = new Page { Id = id, Guid = Guid.NewGuid(), InternalName = "Home" };

        rockContext.Set<Site>().Add( site );
        rockContext.Set<Page>().Add( page );
        rockContext.Set<ExceptionLog>().Add( new ExceptionLog
        {
            Id = id,
            Guid = Guid.NewGuid(),
            ExceptionType = "System.InvalidOperationException",
            Description = description,
            CreatedDateTime = RockDateTime.Now.AddDays( -1 ),
            ParentId = null,
            SiteId = site.Id,
            Site = site,
            PageId = page.Id,
            Page = page
        } );
    }

    /// <summary>
    /// Gets the exception instances in a successful list result.
    /// </summary>
    private static ExceptionInstanceResult[] GetExceptionInstanceItems( AgentToolResult result )
    {
        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var page = ( PaginatedResult<ExceptionInstanceResult> ) result.GetContent();

        return page.Items.ToArray();
    }

    #endregion
}
