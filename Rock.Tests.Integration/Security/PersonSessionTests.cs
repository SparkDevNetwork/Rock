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
using System.Collections.Specialized;
using System.Linq;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.Constants;
using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Integration.Security;

/// <summary>
/// Full integration tests for the <see cref="PersonSession"/> entity. These
/// verify behavior that depends on the real save pipeline firing the
/// <c>PersonSession.SaveHook</c>; plain-POCO assertions live in
/// <c>Rock.Tests.Model.PersonSessionTests</c>.
/// </summary>
[TestClass]
public class PersonSessionTests : DatabaseTestsBase
{
    /// <summary>
    /// Saving a <see cref="PersonSession"/> with
    /// <see cref="PersonSession.IsActive"/> = <c>false</c> must stamp
    /// <c>InactiveDateTime</c> automatically via the save hook. The two
    /// columns are kept in lockstep so a caller cannot leave the row in a
    /// contradictory state.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void SaveHook_StampsInactiveDateTime_WhenIsActiveFlipsFalse()
    {
        var sessionGuid = Guid.NewGuid();
        int personAliasId;

        using ( var rockContext = new RockContext() )
        {
            var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            Assert.IsNotNull( tedDecker, "Ted Decker test data is required for this test." );
            Assert.IsNotNull( tedDecker.PrimaryAliasId, "Ted Decker must have a primary alias." );
            personAliasId = tedDecker.PrimaryAliasId.Value;

            // Create the session in the active state and confirm the save
            // hook leaves InactiveDateTime null.
            var session = new PersonSession
            {
                Guid = sessionGuid,
                PersonAliasId = personAliasId,
                IsActive = true,
                IssuedDateTime = RockDateTime.Now,
                LastActivityDateTime = RockDateTime.Now,
                IsPersistent = false,
                CreationSource = PersonSessionCreationSource.Component,
            };

            rockContext.Set<PersonSession>().Add( session );
            rockContext.SaveChanges();
        }

        using ( var rockContext = new RockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );
            Assert.IsTrue( session.IsActive );
            Assert.IsNull( session.InactiveDateTime, "InactiveDateTime should be null while the session is active." );

            // Flip the flag. The save hook is what stamps InactiveDateTime
            // — callers cannot write the column directly (private setter,
            // compile-time enforced).
            session.IsActive = false;
            rockContext.SaveChanges();
        }

        using ( var rockContext = new RockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );
            Assert.IsFalse( session.IsActive );
            Assert.IsNotNull( session.InactiveDateTime, "InactiveDateTime should be stamped once IsActive flips to false." );
        }
    }

    /// <summary>
    /// A session created via <c>StartComponentSession</c> + Add + SaveChanges
    /// then deactivated also gets <c>InactiveDateTime</c> stamped by the save
    /// hook. Re-verifies the save-hook stamping invariant now that callers can
    /// go through the central creation path end-to-end.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void StartComponentSession_SavedThenDeactivated_StampsInactiveDateTime()
    {
        Guid sessionGuid;

        using ( var rockContext = new RockContext() )
        {
            var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            Assert.IsNotNull( tedDecker?.PrimaryAliasId, "Ted Decker test data with a primary alias is required for this test." );

            var userLogin = new UserLoginService( rockContext )
                .Queryable()
                .FirstOrDefault( ul => ul.PersonId == tedDecker.Id );
            Assert.IsNotNull( userLogin, "Ted Decker must have at least one UserLogin in the seed data." );

            // Component sessions need an AuthenticationComponent EntityType.
            // Any active EntityType implementing AuthenticationComponent will
            // do; the database FK only checks that the EntityType row exists.
            var anyEntityTypeId = new EntityTypeService( rockContext )
                .Queryable()
                .Select( et => et.Id )
                .First();

            var session = new PersonSessionService( rockContext )
                .StartComponentSession(
                    requestContext: null,
                    personAliasId: tedDecker.PrimaryAliasId.Value,
                    userLoginId: userLogin.Id,
                    authComponentEntityTypeId: anyEntityTypeId,
                    isPersistent: false );

            rockContext.Set<PersonSession>().Add( session );
            rockContext.SaveChanges();
            sessionGuid = session.Guid;
        }

        using ( var rockContext = new RockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );
            Assert.IsTrue( session.IsActive );
            Assert.IsNull( session.InactiveDateTime );
            Assert.IsNotNull( session.LastStepUpAuthenticationDateTime );

            session.IsActive = false;
            rockContext.SaveChanges();
        }

        using ( var rockContext = new RockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );
            Assert.IsFalse( session.IsActive );
            Assert.IsNotNull( session.InactiveDateTime, "InactiveDateTime should be stamped on a Component session that flips to inactive." );
        }
    }

    /// <summary>
    /// <c>PersonSessionService.SignOut</c> end-to-end against the real save
    /// pipeline: the current session is marked inactive AND its
    /// <c>InactiveDateTime</c> is stamped by the save hook. The mocked-database
    /// SignOut test cannot assert the stamp (the save hook does not run there),
    /// so this is the integration coverage for that half of the post-condition.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void SignOut_MarksSessionInactiveAndStampsInactiveDateTime()
    {
        Guid sessionGuid;

        using ( var rockContext = new RockContext() )
        {
            var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            Assert.IsNotNull( tedDecker?.PrimaryAliasId, "Ted Decker test data with a primary alias is required for this test." );

            var session = new PersonSession
            {
                Guid = Guid.NewGuid(),
                PersonAliasId = tedDecker.PrimaryAliasId.Value,
                IsActive = true,
                IssuedDateTime = RockDateTime.Now,
                IsPersistent = true,
                CreationSource = PersonSessionCreationSource.Component,
            };

            rockContext.Set<PersonSession>().Add( session );
            rockContext.SaveChanges();
            sessionGuid = session.Guid;
        }

        // Sign out against a fresh service/context so the real save pipeline
        // (and the PersonSession.SaveHook that stamps InactiveDateTime) runs.
        using ( var rockContext = new RockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );

            var requestContext = new RockRequestContext( new TrackingResponseContext() );
            requestContext.SetPersonSession( session );

            new PersonSessionService( rockContext ).SignOut( requestContext );

            Assert.IsNull( requestContext.PersonSession, "SignOut should detach the session from the request context." );
        }

        using ( var rockContext = new RockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );
            Assert.IsFalse( session.IsActive, "SignOut should mark the current session inactive." );
            Assert.IsNotNull( session.InactiveDateTime, "SignOut should stamp InactiveDateTime via the save hook." );
        }
    }

    /// <summary>
    /// First call to <c>FindOrCreateApiKeySession</c> creates a new row;
    /// second call for the same <c>UserLogin</c> reuses it. Exercises the
    /// real EF SaveChanges path and the filtered unique index in a fresh
    /// database so the find-or-create round-trip is end-to-end verified.
    /// </summary>
    /// <remarks>
    /// The unique-constraint-violation retry leg inside
    /// <see cref="PersonSessionService.FindOrCreateApiKeySession"/> (catch
    /// <c>DbUpdateException</c> → re-run <c>FindActiveApiKeySession</c>) is
    /// NOT verified by this test, nor by any other test in this suite. A
    /// deterministic test would have to interleave a concurrent INSERT
    /// between this caller's <c>FindActiveApiKeySession</c> and
    /// <c>SaveChanges</c>, and the harness has no clean seam for that.
    /// <c>Task.Run</c>-based concurrency is best-effort: on a warm
    /// thread-pool the first task typically completes its full upsert
    /// before the second task's SELECT even runs, so the assertion passes
    /// without exercising the retry branch. The retry leg is held by code
    /// inspection (it mirrors the
    /// <c>FindOrCreateLegacyUpgradeSession</c> retry leg) and would be
    /// covered cleanly only by a future EF / SqlServer interceptor seam.
    /// </remarks>
    [TestMethod]
    [IsolatedTestDatabase]
    public void FindOrCreateApiKeySession_SecondCall_ReusesExistingRow()
    {
        using var rockContext = new RockContext();
        var service = new PersonSessionService( rockContext );
        var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
        Assert.IsNotNull( tedDecker?.PrimaryAliasId, "Ted Decker test data with a primary alias is required for this test." );

        var userLogin = new UserLoginService( rockContext )
            .Queryable()
            .FirstOrDefault( ul => ul.PersonId == tedDecker.Id );
        Assert.IsNotNull( userLogin, "Ted Decker must have at least one UserLogin in the seed data." );

        var first = service.FindOrCreateApiKeySession( requestContext: null, userLogin );
        Assert.IsNotNull( first );
        Assert.AreEqual( PersonSessionCreationSource.ApiKey, first.CreationSource );
        Assert.IsTrue( first.IsActive );
        Assert.IsTrue( first.IsPersistent );

        var second = service.FindOrCreateApiKeySession( requestContext: null, userLogin );
        Assert.AreEqual( first.Id, second.Id );

        // And only one row exists in the database for this UserLogin's
        // ApiKey session (no duplicates from the upsert path).
        var activeApiKeyRowCount = rockContext.Set<PersonSession>()
            .Count( s => s.UserLoginId == userLogin.Id
                && s.CreationSource == PersonSessionCreationSource.ApiKey
                && s.IsActive );

        Assert.AreEqual( 1, activeApiKeyRowCount );
    }

    #region UA → InteractionDeviceType resolution

    /// <summary>
    /// A brand-new User-Agent string resolved through
    /// <c>PopulateNewSession</c> stamps
    /// <see cref="PersonSession.InteractionDeviceTypeId"/> on the new row and
    /// creates exactly one <see cref="InteractionDeviceType"/> row in the
    /// database. The find-or-create pattern matches what
    /// <see cref="InteractionService.GetInteractionDeviceTypeId"/> uses for
    /// real interaction tracking.
    /// </summary>
    /// <remarks>
    /// The spec's "concurrent first-request creates" guarantee leans on
    /// <see cref="InteractionService"/>'s process-wide cache rather than a DB
    /// unique constraint — this test exercises the single-thread create path,
    /// which is the primary correctness property. Validating concurrency
    /// would require harnessing parallel threads against the same DB and is
    /// out of scope for this basic coverage; the same behavior is
    /// what <see cref="InteractionService"/> ships with today.
    /// </remarks>
    [TestMethod]
    [IsolatedTestDatabase]
    public void PopulateNewSession_NewUserAgent_StampsDeviceTypeAndCreatesSingleRow()
    {
        // Use a deliberately uncommon UA string so we can be confident no
        // seed data already has a matching InteractionDeviceType row.
        const string userAgent = "RockPersonSessionTestAgent/1.0 (PersonSessionUnitTest; rv:1) Gecko/20260101 Firefox/120.0";

        using var rockContext = new RockContext();
        var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
        Assert.IsNotNull( tedDecker?.PrimaryAliasId, "Ted Decker test data with a primary alias is required for this test." );

        var userLogin = new UserLoginService( rockContext )
            .Queryable()
            .FirstOrDefault( ul => ul.PersonId == tedDecker.Id );
        Assert.IsNotNull( userLogin, "Ted Decker must have at least one UserLogin in the seed data." );

        var anyEntityTypeId = new EntityTypeService( rockContext )
            .Queryable()
            .Select( et => et.Id )
            .First();

        // Confirm the test UA has no pre-existing InteractionDeviceType
        // row. If this fails the seed data is contaminating the test.
        var preCount = new InteractionDeviceTypeService( rockContext )
            .Queryable()
            .Count( idt => idt.DeviceTypeData == userAgent );
        Assert.AreEqual( 0, preCount, "Test UA must be unique to the test." );

        var requestContext = BuildRequestContextWithUserAgent( userAgent );

        var session = new PersonSessionService( rockContext )
            .StartComponentSession(
                requestContext,
                tedDecker.PrimaryAliasId.Value,
                userLogin.Id,
                anyEntityTypeId,
                isPersistent: false );

        rockContext.Set<PersonSession>().Add( session );
        rockContext.SaveChanges();

        Assert.IsNotNull( session.InteractionDeviceTypeId, "PopulateNewSession should stamp InteractionDeviceTypeId from the request's UA." );

        var deviceTypeRowCount = new InteractionDeviceTypeService( rockContext )
            .Queryable()
            .Count( idt => idt.DeviceTypeData == userAgent );

        Assert.AreEqual( 1, deviceTypeRowCount, "Exactly one InteractionDeviceType row should exist for the new UA." );
    }

    /// <summary>
    /// Two calls to <c>StartComponentSession</c> with the same User-Agent
    /// reuse the existing <see cref="InteractionDeviceType"/> row rather than
    /// creating a duplicate. The two <see cref="PersonSession"/> rows both
    /// point at the same <c>InteractionDeviceTypeId</c>.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void PopulateNewSession_SameUserAgentTwice_ReusesExistingDeviceTypeRow()
    {
        const string userAgent = "RockPersonSessionReuseAgent/2.0 (PersonSessionUnitTest; rv:1) Gecko/20260101 Firefox/121.0";

        using var rockContext = new RockContext();
        var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
        Assert.IsNotNull( tedDecker?.PrimaryAliasId, "Ted Decker test data with a primary alias is required for this test." );

        var userLogin = new UserLoginService( rockContext )
            .Queryable()
            .FirstOrDefault( ul => ul.PersonId == tedDecker.Id );
        Assert.IsNotNull( userLogin, "Ted Decker must have at least one UserLogin in the seed data." );

        var anyEntityTypeId = new EntityTypeService( rockContext )
            .Queryable()
            .Select( et => et.Id )
            .First();

        var requestContext = BuildRequestContextWithUserAgent( userAgent );
        var service = new PersonSessionService( rockContext );

        var firstSession = service.StartComponentSession(
            requestContext,
            tedDecker.PrimaryAliasId.Value,
            userLogin.Id,
            anyEntityTypeId,
            isPersistent: false );
        rockContext.Set<PersonSession>().Add( firstSession );
        rockContext.SaveChanges();

        var secondSession = service.StartComponentSession(
            requestContext,
            tedDecker.PrimaryAliasId.Value,
            userLogin.Id,
            anyEntityTypeId,
            isPersistent: false );
        rockContext.Set<PersonSession>().Add( secondSession );
        rockContext.SaveChanges();

        Assert.IsNotNull( firstSession.InteractionDeviceTypeId );
        Assert.IsNotNull( secondSession.InteractionDeviceTypeId );
        Assert.AreEqual( firstSession.InteractionDeviceTypeId, secondSession.InteractionDeviceTypeId,
            "Both sessions should share the same InteractionDeviceType row for an identical UA." );

        var deviceTypeRowCount = new InteractionDeviceTypeService( rockContext )
            .Queryable()
            .Count( idt => idt.DeviceTypeData == userAgent );

        Assert.AreEqual( 1, deviceTypeRowCount, "Only one InteractionDeviceType row should exist for the shared UA." );
    }

    /// <summary>
    /// Builds a minimum-viable <see cref="RockRequestContext"/> populated
    /// with the supplied User-Agent header. Uses Moq to stub
    /// <see cref="IRequest"/> so the underlying
    /// <see cref="ClientInformation"/> constructor parses the UA into
    /// <c>BrowserInfo</c> via <see cref="IUserAgentParser"/>.
    /// </summary>
    /// <remarks>
    /// <c>RequestUri</c> is intentionally returned as null: the
    /// <see cref="IRequest"/> constructor on <see cref="RockRequestContext"/>
    /// only calls <c>UrlProxySafe()</c> when <c>RequestUri</c> is non-null,
    /// so leaving it null avoids needing to stub the extension method's
    /// header-reading behavior.
    /// </remarks>
    /// <param name="userAgent">The User-Agent string to surface via the request's Headers collection.</param>
    /// <returns>A <see cref="RockRequestContext"/> whose <c>ClientInformation.UserAgent</c> and <c>BrowserInfo</c> are populated.</returns>
    private static RockRequestContext BuildRequestContextWithUserAgent( string userAgent )
    {
        var headers = new NameValueCollection( StringComparer.OrdinalIgnoreCase )
        {
            { "USER-AGENT", userAgent },
        };

        var requestMock = new Mock<IRequest>( MockBehavior.Strict );
        requestMock.SetupGet( r => r.RemoteAddress ).Returns( IPAddress.Loopback );
        requestMock.SetupGet( r => r.RequestUri ).Returns( ( Uri ) null );
        requestMock.SetupGet( r => r.Method ).Returns( "GET" );
        requestMock.SetupGet( r => r.QueryString ).Returns( [] );
        requestMock.SetupGet( r => r.RouteData ).Returns( new Dictionary<string, object>() );
        requestMock.SetupGet( r => r.Headers ).Returns( headers );
        requestMock.SetupGet( r => r.Cookies ).Returns( new Dictionary<string, string>() );
        requestMock.SetupGet( r => r.CookiesValuesAreUrlDecoded ).Returns( false );

        return new RockRequestContext( requestMock.Object, new NullRockResponseContext(), currentUser: null );
    }

    #endregion UA → InteractionDeviceType resolution

    #region InteractionSession.PersonSessionId upsert

    /// <summary>
    /// First-time browser-session presented by an authenticated request
    /// inserts a brand-new <see cref="InteractionSession"/> row with
    /// <see cref="InteractionSession.PersonSessionId"/> already populated.
    /// Covers the "stamp at creation" path from the spec's
    /// InteractionSession sync table.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void GetInteractionSessionId_InsertPath_StampsPersonSessionIdAtInsert()
    {
        using var rockContext = new RockContext();
        var personSession = CreatePersistedComponentPersonSession( rockContext );
        var browserSessionId = Guid.NewGuid();

        var interactionService = new InteractionService( rockContext );
        var interactionSessionId = interactionService.GetInteractionSessionId(
            browserSessionId,
            ipAddress: "127.0.0.1",
            interactionDeviceTypeId: null,
            interactionDateKey: null,
            interactionSessionLocationId: null,
            personSessionId: personSession.Id );

        var row = new InteractionSessionService( rockContext ).Get( interactionSessionId );
        Assert.IsNotNull( row, "Insert path should have created an InteractionSession row." );
        Assert.AreEqual( browserSessionId, row.Guid );
        Assert.AreEqual( personSession.Id, row.PersonSessionId,
            "Newly inserted InteractionSession row should carry the supplied PersonSessionId." );
    }

    /// <summary>
    /// Anonymous browser-session row whose
    /// <see cref="InteractionSession.PersonSessionId"/> is null gets its
    /// <c>PersonSessionId</c> set when an authenticated request later
    /// presents the same <c>RockSessionId</c>. Covers the "adopt by update
    /// at login" and "adopt by update at legacy upgrade" paths.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void GetInteractionSessionId_UpdatePath_AdoptsExistingRowWhenPersonSessionIdNull()
    {
        using var rockContext = new RockContext();
        var personSession = CreatePersistedComponentPersonSession( rockContext );
        var browserSessionId = Guid.NewGuid();

        var interactionService = new InteractionService( rockContext );

        // Anonymous interaction creates the row with PersonSessionId = null.
        var firstId = interactionService.GetInteractionSessionId(
            browserSessionId,
            ipAddress: "127.0.0.1",
            interactionDeviceTypeId: null,
            interactionDateKey: null,
            interactionSessionLocationId: null,
            personSessionId: null );

        // Authenticated request presents the same RockSessionId.
        var secondId = interactionService.GetInteractionSessionId(
            browserSessionId,
            ipAddress: "127.0.0.1",
            interactionDeviceTypeId: null,
            interactionDateKey: null,
            interactionSessionLocationId: null,
            personSessionId: personSession.Id );

        Assert.AreEqual( firstId, secondId, "Same RockSessionId should resolve to the same InteractionSession row." );

        // Force a re-read so we observe the UPDATE that ran on a separate
        // ADO.NET connection inside SqlQuery<int>.
        using ( var verifyContext = new RockContext() )
        {
            var row = new InteractionSessionService( verifyContext ).Get( firstId );
            Assert.AreEqual( personSession.Id, row.PersonSessionId,
                "Existing row should have been adopted by the authenticated request." );
        }
    }

    /// <summary>
    /// The adopt-by-update path only touches the row whose
    /// <c>Guid</c> matches the presented <c>browserSessionId</c>; other
    /// <see cref="InteractionSession"/> rows in the table are untouched.
    /// Regression guard against an UPDATE missing its WHERE clause.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void GetInteractionSessionId_UpdatePath_DoesNotTouchOtherRows()
    {
        using var rockContext = new RockContext();
        var personSession = CreatePersistedComponentPersonSession( rockContext );

        var interactionService = new InteractionService( rockContext );

        var targetBrowserSessionId = Guid.NewGuid();
        var bystanderBrowserSessionId = Guid.NewGuid();

        var targetId = interactionService.GetInteractionSessionId(
            targetBrowserSessionId,
            ipAddress: "127.0.0.1",
            interactionDeviceTypeId: null,
            personSessionId: null );

        var bystanderId = interactionService.GetInteractionSessionId(
            bystanderBrowserSessionId,
            ipAddress: "127.0.0.2",
            interactionDeviceTypeId: null,
            personSessionId: null );

        // Adopt the target row.
        var resolvedTargetId = interactionService.GetInteractionSessionId(
            targetBrowserSessionId,
            ipAddress: "127.0.0.1",
            interactionDeviceTypeId: null,
            personSessionId: personSession.Id );

        Assert.AreEqual( targetId, resolvedTargetId );

        using ( var verifyContext = new RockContext() )
        {
            var sessionService = new InteractionSessionService( verifyContext );
            var target = sessionService.Get( targetId );
            var bystander = sessionService.Get( bystanderId );

            Assert.AreEqual( personSession.Id, target.PersonSessionId,
                "Target row should reflect the adoption." );
            Assert.IsNull( bystander.PersonSessionId,
                "Bystander row should not have been touched by the UPDATE." );
        }
    }

    /// <summary>
    /// Creates a <see cref="PersonSessionCreationSource.Component"/>
    /// <see cref="PersonSession"/> row for Ted Decker and persists it. Used
    /// by the upsert tests to get a valid <c>PersonSession.Id</c> the SQL
    /// path can FK against.
    /// </summary>
    /// <param name="rockContext">The <see cref="RockContext"/> to persist into.</param>
    /// <returns>The persisted <see cref="PersonSession"/>.</returns>
    private static PersonSession CreatePersistedComponentPersonSession( RockContext rockContext )
    {
        var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );
        Assert.IsNotNull( tedDecker?.PrimaryAliasId, "Ted Decker test data with a primary alias is required for this test." );

        var userLogin = new UserLoginService( rockContext )
            .Queryable()
            .FirstOrDefault( ul => ul.PersonId == tedDecker.Id );
        Assert.IsNotNull( userLogin, "Ted Decker must have at least one UserLogin in the seed data." );

        var anyEntityTypeId = new EntityTypeService( rockContext )
            .Queryable()
            .Select( et => et.Id )
            .First();

        var session = new PersonSessionService( rockContext )
            .StartComponentSession(
                requestContext: null,
                personAliasId: tedDecker.PrimaryAliasId.Value,
                userLoginId: userLogin.Id,
                authComponentEntityTypeId: anyEntityTypeId,
                isPersistent: false );

        rockContext.Set<PersonSession>().Add( session );
        rockContext.SaveChanges();

        return session;
    }

    #endregion InteractionSession.PersonSessionId upsert
}
