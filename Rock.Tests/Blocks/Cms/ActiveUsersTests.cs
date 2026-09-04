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

using Rock.Blocks.Cms;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Tests.Shared.TestFramework;
using Rock.ViewModels.Blocks.Cms.ActiveUsers;
using Rock.Web.Cache;

namespace Rock.Tests.Blocks.Cms;

/// <summary>
/// Mocked-database tests for the Active Users block's switch from
/// <see cref="UserLogin"/> (<c>IsOnLine</c> / <c>LastActivityDateTime</c>) to
/// <see cref="PersonSession"/> as the source of truth for who is currently
/// active.
/// </summary>
/// <remarks>
/// <para>
/// Tests instantiate a real <see cref="ActiveUsers"/> block (with the minimum
/// <c>BlockCache</c> / <c>PageCache</c> scaffolding it needs to read its
/// attributes) and call the now-<c>internal</c>
/// <see cref="ActiveUsers.GetActiveUsers(int, int)"/> directly, asserting on the
/// <see cref="ActiveUserBag"/> list the block actually produces — not on a
/// re-implemented copy of the query.
/// </para>
/// <para>
/// A person only surfaces in the block when their most-recent page view (within
/// the last 24 hours) is on the configured site, so each test seeds the full
/// <see cref="Person"/> → <see cref="PersonAlias"/> → <see cref="PersonSession"/>
/// graph plus an <see cref="Interaction"/> → <see cref="InteractionComponent"/>
/// → <see cref="InteractionChannel"/> chain. Navigation properties are wired up
/// by hand because the mocked <see cref="RockContext"/> does not auto-load
/// Includes.
/// </para>
/// </remarks>
[TestClass]
public class ActiveUsersTests
{
    private const int SiteId = 100;
    private const int OtherSiteId = 200;

    /// <summary>
    /// The block returns a person who has an active <see cref="PersonSession"/>
    /// with a recent <c>LastActivityDateTime</c> even when their
    /// <see cref="UserLogin"/> has stale activity and <c>IsOnLine = false</c>.
    /// This is the regression guard against the legacy UserLogin-based read.
    /// </summary>
    [TestMethod]
    public void GetActiveUsers_IncludesPersonWithRecentSession_DespiteStaleUserLogin()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        const int targetPersonId = 42;
        var staleDate = RockDateTime.Now.AddDays( -90 );
        var recentDate = RockDateTime.Now.AddMinutes( -2 );

#pragma warning disable 618 // Intentionally seeding the obsolete UserLogin.LastActivityDateTime/IsOnLine to prove the PersonSession-based query ignores legacy data.
        rockContext.Set<UserLogin>().Add( new UserLogin
        {
            Id = 1,
            UserName = "stalecharlie",
            PersonId = targetPersonId,
            LastActivityDateTime = staleDate,
            IsOnLine = false,
        } );
#pragma warning restore 618

        var personAlias = SeedPerson( rockContext, targetPersonId, personAliasId: 7, nickName: "Charlie", lastName: "Decker" );
        SeedActiveSession( rockContext, sessionId: 1, personAlias, recentDate );
        SeedSiteInteraction( rockContext, interactionId: 1, personAlias, SiteId, recentDate, interactionSessionId: 500, title: "Home" );

        var block = BuildActiveUsersBlock( rockContext );

        var result = block.GetActiveUsers( SiteId, pageViewCount: 5 );

        var entry = result.SingleOrDefault( u => u.FullName == "Charlie Decker" );
        Assert.IsNotNull( entry, "Person with a recent PersonSession should appear in the active-users list." );
        Assert.IsTrue( entry.IsRecent, "Activity 2 minutes ago should be flagged recent." );
    }

    /// <summary>
    /// A person whose only <see cref="PersonSession"/> rows are marked
    /// <see cref="PersonSession.IsActive"/> = <c>false</c> does NOT appear in
    /// the list, even if their <see cref="UserLogin"/> would have qualified
    /// under the legacy <c>IsOnLine</c> read.
    /// </summary>
    [TestMethod]
    public void GetActiveUsers_ExcludesPersonWithOnlyInactiveSessions()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        const int targetPersonId = 99;

#pragma warning disable 618 // Intentionally seeding the obsolete UserLogin.LastActivityDateTime/IsOnLine to prove the PersonSession-based query ignores legacy data.
        rockContext.Set<UserLogin>().Add( new UserLogin
        {
            Id = 1,
            UserName = "onlinedave",
            PersonId = targetPersonId,
            LastActivityDateTime = RockDateTime.Now,
            IsOnLine = true,
        } );
#pragma warning restore 618

        var personAlias = SeedPerson( rockContext, targetPersonId, personAliasId: 7, nickName: "Dave", lastName: "Decker" );

        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = personAlias.Id,
            PersonAlias = personAlias,
            IsActive = false,
            LastActivityDateTime = RockDateTime.Now.AddMinutes( -2 ),
            CreationSource = PersonSessionCreationSource.Component,
        } );

        // Even with a qualifying page view on the site, an inactive session means no entry.
        SeedSiteInteraction( rockContext, interactionId: 1, personAlias, SiteId, RockDateTime.Now.AddMinutes( -2 ), interactionSessionId: 500, title: "Home" );

        var block = BuildActiveUsersBlock( rockContext );

        var result = block.GetActiveUsers( SiteId, pageViewCount: 5 );

        Assert.IsFalse( result.Any( u => u.FullName == "Dave Decker" ),
            "Person whose only sessions are inactive must NOT appear in the active-users list." );
    }

    /// <summary>
    /// When a person has multiple active <see cref="PersonSession"/> rows
    /// (multi-device), the block collapses them to a single entry, and the
    /// recent indicator is driven by the maximum <c>LastActivityDateTime</c>.
    /// </summary>
    [TestMethod]
    public void GetActiveUsers_CollapsesMultipleSessions_PerPerson()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        const int targetPersonId = 7;
        var olderActivity = RockDateTime.Now.AddMinutes( -30 );
        var newerActivity = RockDateTime.Now.AddMinutes( -2 );

        var personAlias = SeedPerson( rockContext, targetPersonId, personAliasId: 1, nickName: "Multi", lastName: "Device" );
        SeedActiveSession( rockContext, sessionId: 1, personAlias, olderActivity );
        SeedActiveSession( rockContext, sessionId: 2, personAlias, newerActivity );
        SeedSiteInteraction( rockContext, interactionId: 1, personAlias, SiteId, newerActivity, interactionSessionId: 500, title: "Home" );

        var block = BuildActiveUsersBlock( rockContext );

        var result = block.GetActiveUsers( SiteId, pageViewCount: 5 );

        var matches = result.Where( u => u.FullName == "Multi Device" ).ToList();
        Assert.HasCount( 1, matches, "Multiple sessions for the same person must collapse to one entry." );
        Assert.IsTrue( matches[0].IsRecent, "Recent flag must reflect the maximum LastActivityDateTime (2 minutes ago), not the older session." );
    }

    /// <summary>
    /// A person with an active session whose most-recent page view is on a
    /// different site is filtered out — the block only lists people currently
    /// browsing the configured site.
    /// </summary>
    [TestMethod]
    public void GetActiveUsers_ExcludesPersonBrowsingDifferentSite()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        const int targetPersonId = 55;
        var recentDate = RockDateTime.Now.AddMinutes( -2 );

        var personAlias = SeedPerson( rockContext, targetPersonId, personAliasId: 3, nickName: "Else", lastName: "Where" );
        SeedActiveSession( rockContext, sessionId: 1, personAlias, recentDate );
        SeedSiteInteraction( rockContext, interactionId: 1, personAlias, OtherSiteId, recentDate, interactionSessionId: 500, title: "Other Home" );

        var block = BuildActiveUsersBlock( rockContext );

        var result = block.GetActiveUsers( SiteId, pageViewCount: 5 );

        Assert.IsFalse( result.Any( u => u.FullName == "Else Where" ),
            "Person whose latest page view is on a different site must NOT appear." );
    }

    /// <summary>
    /// When <c>pageViewCount</c> is positive, the tooltip page titles are
    /// populated from the user's latest interaction session, ordered
    /// most-recent first.
    /// </summary>
    [TestMethod]
    public void GetActiveUsers_PopulatesPageTitles_ForLatestSession()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        const int targetPersonId = 71;
        const int latestSessionId = 900;
        var newest = RockDateTime.Now.AddMinutes( -1 );
        var middle = RockDateTime.Now.AddMinutes( -2 );

        var personAlias = SeedPerson( rockContext, targetPersonId, personAliasId: 4, nickName: "Page", lastName: "Viewer" );
        SeedActiveSession( rockContext, sessionId: 1, personAlias, newest );
        SeedSiteInteraction( rockContext, interactionId: 1, personAlias, SiteId, newest, latestSessionId, title: "Newest Page" );
        SeedSiteInteraction( rockContext, interactionId: 2, personAlias, SiteId, middle, latestSessionId, title: "Middle Page" );

        var block = BuildActiveUsersBlock( rockContext );

        var result = block.GetActiveUsers( SiteId, pageViewCount: 5 );

        var entry = result.SingleOrDefault( u => u.FullName == "Page Viewer" );
        Assert.IsNotNull( entry );
        CollectionAssert.AreEqual( new[] { "Newest Page", "Middle Page" }, entry.PageTitles,
            "Tooltip titles must come from the latest session, ordered most-recent first." );
    }

    #region Test infrastructure

    /// <summary>
    /// Constructs an <see cref="ActiveUsers"/> block instance ready to call
    /// <see cref="ActiveUsers.GetActiveUsers(int, int)"/>. Seeds the minimum
    /// <see cref="Page"/> / <see cref="BlockType"/> / <see cref="Block"/> rows
    /// the block's <c>BlockCache</c> and <c>PageCache</c> properties need (so
    /// <c>GetAttributeValue</c> resolves), then wires
    /// <see cref="ActiveUsers.RequestContext"/> to a synthetic
    /// <see cref="RockRequestContext"/> with no current person.
    /// </summary>
    private static ActiveUsers BuildActiveUsersBlock( RockContext rockContext )
    {
        var page = new Page
        {
            Id = 1,
        };

        var blockTypeEntityType = EntityTypeCache.Get<ActiveUsers>( true, rockContext );

        var blockType = new BlockType
        {
            Id = 1,
            Name = "Active Users",
            EntityTypeId = blockTypeEntityType.Id,
        };

        var block = new Block
        {
            Id = 1,
            BlockTypeId = blockType.Id,
            PageId = page.Id,
        };

        rockContext.Set<Page>().Add( page );
        rockContext.Set<BlockType>().Add( blockType );
        rockContext.Set<Block>().Add( block );

        return new ActiveUsers
        {
            RockContext = rockContext,
            RequestContext = new RockRequestContext( new NullRockResponseContext() ),
            PageCache = PageCache.Get( page.Id, rockContext ),
            BlockCache = BlockCache.Get( block.Id, rockContext ),
        };
    }

    /// <summary>
    /// Seeds a <see cref="Person"/> + <see cref="PersonAlias"/> graph and
    /// returns the alias. The alias' <see cref="PersonAlias.PersonId"/> drives
    /// both the session GroupBy and the page-view person match; the Person's
    /// name fields feed <see cref="Person.FormatFullName(string, string, int?, int?)"/>.
    /// </summary>
    private static PersonAlias SeedPerson( RockContext rockContext, int personId, int personAliasId, string nickName, string lastName )
    {
        var person = new Person
        {
            Id = personId,
            NickName = nickName,
            LastName = lastName,
        };

        var personAlias = new PersonAlias
        {
            Id = personAliasId,
            PersonId = personId,
            Person = person,
            Guid = Guid.NewGuid(),
        };

        rockContext.Set<Person>().Add( person );
        rockContext.Set<PersonAlias>().Add( personAlias );

        return personAlias;
    }

    /// <summary>
    /// Adds an active <see cref="PersonSession"/> for the given alias with the
    /// supplied last-activity timestamp.
    /// </summary>
    private static void SeedActiveSession( RockContext rockContext, int sessionId, PersonAlias personAlias, DateTime lastActivityDateTime )
    {
        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = sessionId,
            Guid = Guid.NewGuid(),
            PersonAliasId = personAlias.Id,
            PersonAlias = personAlias,
            IsActive = true,
            LastActivityDateTime = lastActivityDateTime,
            CreationSource = PersonSessionCreationSource.Component,
        } );
    }

    /// <summary>
    /// Adds a page-view <see cref="Interaction"/> for the given alias on the
    /// specified site, wiring the
    /// <see cref="Interaction.InteractionComponent"/> →
    /// <see cref="InteractionComponent.InteractionChannel"/> →
    /// <c>ChannelEntityId</c> chain the block uses to decide which site the
    /// user is on. The component's <c>Name</c> doubles as the tooltip page
    /// title.
    /// </summary>
    private static void SeedSiteInteraction( RockContext rockContext, int interactionId, PersonAlias personAlias, int siteId, DateTime interactionDateTime, int interactionSessionId, string title )
    {
        var channel = new InteractionChannel
        {
            Id = siteId,
            ChannelEntityId = siteId,
        };

        var component = new InteractionComponent
        {
            Id = interactionId,
            Name = title,
            InteractionChannelId = channel.Id,
            InteractionChannel = channel,
        };

        rockContext.Set<Interaction>().Add( new Interaction
        {
            Id = interactionId,
            PersonAliasId = personAlias.Id,
            PersonAlias = personAlias,
            InteractionDateTime = interactionDateTime,
            InteractionSessionId = interactionSessionId,
            InteractionComponentId = component.Id,
            InteractionComponent = component,
        } );
    }

    #endregion Test infrastructure
}
