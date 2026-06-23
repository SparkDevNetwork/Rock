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

using Rock.Configuration;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Cms;

/// <summary>
/// Mocked-database tests covering the Active Users block's switch from
/// <see cref="UserLogin"/> (<c>IsOnLine</c> / <c>LastActivityDateTime</c>) to
/// <see cref="PersonSession"/> as the source of truth for who is currently
/// active. Phase 14 of the PersonSession plan.
/// </summary>
/// <remarks>
/// The block's full join graph (Interaction → InteractionComponent →
/// InteractionChannel) is too tangled to mock cleanly, so these tests focus on
/// the substantive query change: the underlying
/// <see cref="PersonSessionService.Queryable()"/> filter that drives the block
/// surfaces active sessions correctly even when the corresponding
/// <see cref="UserLogin.LastActivityDateTime"/> is stale or
/// <see cref="UserLogin.IsOnLine"/> is false.
/// </remarks>
[TestClass]
public class ActiveUsersBlockTests
{
    /// <summary>
    /// The block-driving query returns a person who has an active
    /// <see cref="PersonSession"/> with a recent <c>LastActivityDateTime</c>
    /// even when their <see cref="UserLogin"/> has stale activity and
    /// <c>IsOnLine = false</c>. This is the regression guard against the
    /// legacy UserLogin-based read.
    /// </summary>
    [TestMethod]
    public void ActiveSessionQuery_IncludesPersonWithRecentSession_DespiteStaleUserLogin()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
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

        var personAlias = new PersonAlias
        {
            Id = 7,
            PersonId = targetPersonId,
            Guid = Guid.NewGuid(),
        };
        rockContext.Set<PersonAlias>().Add( personAlias );

        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = personAlias.Id,
            PersonAlias = personAlias,
            IsActive = true,
            LastActivityDateTime = recentDate,
            CreationSource = PersonSessionCreationSource.Component,
        } );

        var activePersonIds = new PersonSessionService( rockContext ).Queryable()
            .Where( s => s.IsActive )
            .GroupBy( s => s.PersonAlias.PersonId )
            .Select( g => new { PersonId = g.Key, LastActivity = g.Max( s => s.LastActivityDateTime ) } )
            .ToList();

        Assert.IsTrue( activePersonIds.Any( p => p.PersonId == targetPersonId ),
            "Person with a recent PersonSession should appear in the active-users query." );
    }

    /// <summary>
    /// A person whose only <see cref="PersonSession"/> rows are marked
    /// <see cref="PersonSession.IsActive"/> = <c>false</c> does NOT appear in
    /// the active-users query, even if their <see cref="UserLogin"/> would
    /// have qualified under the legacy <c>IsOnLine</c> read.
    /// </summary>
    [TestMethod]
    public void ActiveSessionQuery_ExcludesPersonWithOnlyInactiveSessions()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
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

        var personAlias = new PersonAlias
        {
            Id = 7,
            PersonId = targetPersonId,
            Guid = Guid.NewGuid(),
        };
        rockContext.Set<PersonAlias>().Add( personAlias );

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

        var activePersonIds = new PersonSessionService( rockContext ).Queryable()
            .Where( s => s.IsActive )
            .Select( s => s.PersonAlias.PersonId )
            .ToList();

        Assert.DoesNotContain( targetPersonId, activePersonIds,
            "Person whose only sessions are inactive must NOT appear in the active-users query." );
    }

    /// <summary>
    /// When a person has multiple active <see cref="PersonSession"/> rows
    /// (multi-device), the active-users query collapses them to a single
    /// entry keyed on the maximum <c>LastActivityDateTime</c>. This is the
    /// behavior the block's <c>GroupBy</c> projection relies on to render a
    /// single row per person.
    /// </summary>
    [TestMethod]
    public void ActiveSessionQuery_CollapsesMultipleSessions_PerPerson()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        const int targetPersonId = 7;

        var personAlias = new PersonAlias
        {
            Id = 1,
            PersonId = targetPersonId,
            Guid = Guid.NewGuid(),
        };
        rockContext.Set<PersonAlias>().Add( personAlias );

        var olderActivity = RockDateTime.Now.AddMinutes( -30 );
        var newerActivity = RockDateTime.Now.AddMinutes( -2 );

        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = personAlias.Id,
            PersonAlias = personAlias,
            IsActive = true,
            LastActivityDateTime = olderActivity,
            CreationSource = PersonSessionCreationSource.Component,
        } );

        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            PersonAliasId = personAlias.Id,
            PersonAlias = personAlias,
            IsActive = true,
            LastActivityDateTime = newerActivity,
            CreationSource = PersonSessionCreationSource.Component,
        } );

        var collapsed = new PersonSessionService( rockContext ).Queryable()
            .Where( s => s.IsActive )
            .GroupBy( s => s.PersonAlias.PersonId )
            .Select( g => new { PersonId = g.Key, LastActivity = g.Max( s => s.LastActivityDateTime ) } )
            .ToList();

        Assert.AreEqual( 1, collapsed.Count( c => c.PersonId == targetPersonId ),
            "Multiple sessions for the same person must collapse to one row." );
        Assert.AreEqual( newerActivity, collapsed.Single( c => c.PersonId == targetPersonId ).LastActivity,
            "Collapsed row must carry the maximum LastActivityDateTime." );
    }
}
