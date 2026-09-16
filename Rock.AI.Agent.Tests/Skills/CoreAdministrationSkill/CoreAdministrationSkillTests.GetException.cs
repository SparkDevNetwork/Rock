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
    #region GetException

    [TestMethod]
    public void GetException_WithValidException_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var exceptionLog = new ExceptionLog
        {
            Id = 50,
            Guid = new Guid( "7a000001-0000-4000-8000-000000000001" ),
            ExceptionType = "System.InvalidOperationException",
            Description = "Something went wrong.",
            CreatedDateTime = RockDateTime.Now.AddDays( -1 )
        };

        rockContext.Set<ExceptionLog>().Add( exceptionLog );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetException( IdHasher.Instance.GetHash( exceptionLog.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetException_WithFullyPopulatedException_MapsAllReferences()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var site = new Site
        {
            Id = 1,
            Guid = new Guid( "7a000010-0000-4000-8000-000000000010" ),
            Name = "Main Site",
            // A domain keeps SiteCache building from throwing on an empty root.
            SiteDomains = new System.Collections.Generic.List<SiteDomain>
            {
                new SiteDomain { Domain = "rock.example", Order = 0 }
            }
        };

        var page = new Page
        {
            Id = 1,
            Guid = new Guid( "7a000011-0000-4000-8000-000000000011" ),
            InternalName = "Home",
            LayoutId = 1
        };

        rockContext.Set<Site>().Add( site );
        rockContext.Set<Page>().Add( page );

        // A parent and a child so the ParentException and InnerExceptions branches run.
        rockContext.Set<ExceptionLog>().Add( new ExceptionLog
        {
            Id = 49,
            Guid = new Guid( "7a000012-0000-4000-8000-000000000012" ),
            ExceptionType = "System.Exception",
            CreatedDateTime = RockDateTime.Now.AddDays( -1 )
        } );

        rockContext.Set<ExceptionLog>().Add( new ExceptionLog
        {
            Id = 51,
            Guid = new Guid( "7a000013-0000-4000-8000-000000000013" ),
            ExceptionType = "System.NullReferenceException",
            ParentId = 50,
            CreatedDateTime = RockDateTime.Now.AddDays( -1 )
        } );

        var exceptionLog = new ExceptionLog
        {
            Id = 50,
            Guid = new Guid( "7a000014-0000-4000-8000-000000000014" ),
            ExceptionType = "System.InvalidOperationException",
            Description = "Something went wrong.",
            Source = "Rock",
            StatusCode = "500",
            StackTrace = "at Rock.Foo()",
            PageUrl = "/page/1",
            CreatedDateTime = RockDateTime.Now.AddDays( -1 ),
            SiteId = site.Id,
            PageId = page.Id,
            ParentId = 49,
            // The navigation is set directly because the mock does not resolve it
            // from a foreign key id the way EF change tracking would.
            CreatedByPersonAlias = new PersonAlias
            {
                Id = 1,
                Person = new Rock.Model.Person { Id = 1, Guid = new Guid( "7a000015-0000-4000-8000-000000000015" ), FirstName = "Test", LastName = "Person" }
            }
        };

        rockContext.Set<ExceptionLog>().Add( exceptionLog );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetException( IdHasher.Instance.GetHash( exceptionLog.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetException_WithMissingException_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetException( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
