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

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.Constants;
using Rock.Tests.Shared.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Model;

/// <summary>
/// Full integration tests for <see cref="PersonSessionService"/>. These verify
/// behavior that depends on the real save pipeline and the filtered unique
/// indexes; mocked-db, save-free assertions live in
/// <c>Rock.Tests.Model.PersonSessionServiceTests</c>, and entity save-hook
/// behavior lives in <c>PersonSessionTests</c>.
/// </summary>
[TestClass]
public class PersonSessionServiceTests : DatabaseTestsBase
{
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

        using ( var rockContext = RockApp.Current.CreateRockContext() )
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

        using ( var rockContext = RockApp.Current.CreateRockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );
            Assert.IsTrue( session.IsActive );
            Assert.IsNull( session.InactiveDateTime );
            Assert.IsNotNull( session.LastStepUpAuthenticationDateTime );

            session.IsActive = false;
            rockContext.SaveChanges();
        }

        using ( var rockContext = RockApp.Current.CreateRockContext() )
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

        using ( var rockContext = RockApp.Current.CreateRockContext() )
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
        using ( var rockContext = RockApp.Current.CreateRockContext() )
        {
            var session = rockContext.Set<PersonSession>().First( s => s.Guid == sessionGuid );

            var requestContext = new RockRequestContext( new TrackingResponseContext() );
            requestContext.SetPersonSession( session );

            new PersonSessionService( rockContext ).SignOut( requestContext );

            Assert.IsNull( requestContext.PersonSession, "SignOut should detach the session from the request context." );
        }

        using ( var rockContext = RockApp.Current.CreateRockContext() )
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
        using var rockContext = RockApp.Current.CreateRockContext();
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
    /// unique constraint - this test exercises the single-thread create path,
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

        using var rockContext = RockApp.Current.CreateRockContext();
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

        using var rockContext = RockApp.Current.CreateRockContext();
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

    #region FindOrCreate reuse / isolation invariants

    /*
        9/14/26 - DH

        These tests were promoted from Rock.Tests/Model/PersonSessionServiceTests.cs.
        In the mocked-db suite the create/save leg cannot complete, which forced a
        try/catch that swallowed the outcome (and, in some cases, the assertion
        itself), leaving the tests unable to fail. Here the real save pipeline and
        the filtered unique indexes run, so each invariant is asserted positively.

        Reason: Real-DB coverage for the FindOrCreate reuse / isolation invariants.
    */

    /// <summary>
    /// An orphaned <see cref="PersonSessionCreationSource.ApiKey"/> session (its
    /// <see cref="UserLogin"/> was deleted, so the FK cascade SET NULL its
    /// <c>UserLoginId</c>) MUST NOT be returned for a different UserLogin's
    /// lookup. A new session is created for the requesting UserLogin and the
    /// orphan is left untouched.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void FindOrCreateApiKeySession_OrphanedSession_IsNotResurrected()
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var userLogin = CreatePersonWithUserLogin( rockContext );
        var service = new PersonSessionService( rockContext );

        // Orphan: an ApiKey session whose UserLogin was deleted, leaving
        // UserLoginId null (FK ON DELETE SET NULL). It is excluded from the
        // IX_ApiKey_UserLoginId filtered unique index (UserLoginId IS NOT NULL),
        // so it neither matches the lookup nor blocks the new insert.
        var orphan = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = userLogin.Person.PrimaryAliasId.Value,
            UserLoginId = null,
            CreationSource = PersonSessionCreationSource.ApiKey,
            IsActive = true,
            IsPersistent = true,
            IssuedDateTime = RockDateTime.Now,
        };
        rockContext.Set<PersonSession>().Add( orphan );
        rockContext.SaveChanges();

        var resolved = service.FindOrCreateApiKeySession( requestContext: null, userLogin );

        Assert.IsNotNull( resolved );
        Assert.AreNotEqual( orphan.Id, resolved.Id, "Orphaned ApiKey session must not be returned to a different UserLogin." );
        Assert.AreEqual( userLogin.Id, resolved.UserLoginId );
        Assert.AreEqual( PersonSessionCreationSource.ApiKey, resolved.CreationSource );
        Assert.IsTrue( resolved.IsActive );

        var orphanReloaded = service.Get( orphan.Id );
        Assert.IsTrue( orphanReloaded.IsActive, "The orphan row must be left untouched." );
        Assert.IsNull( orphanReloaded.UserLoginId, "The orphan row must remain unowned." );
    }

    /// <summary>
    /// An <see cref="PersonSessionCreationSource.ApiKey"/> session whose
    /// <c>IsActive</c> is still true but whose <see cref="PersonSession.ExpiresDateTime"/>
    /// has passed MUST NOT be handed back. Today ApiKey sessions never receive an
    /// <c>ExpiresDateTime</c>, so this state is only reachable defensively: the
    /// filtered unique index still counts the stale row as the one active ApiKey
    /// session for the UserLogin, so the create-new leg cannot insert a
    /// replacement and the method returns <c>null</c> rather than reusing the
    /// expired row. The invariant asserted here is only "never returns the
    /// expired row"; if ApiKey sessions ever gain real expiration,
    /// <c>FindOrCreateApiKeySession</c> must be updated to deactivate the expired
    /// row before creating a new one, and this test will flag that change.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void FindOrCreateApiKeySession_ExpiredButActiveSession_IsNotReused()
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var userLogin = CreatePersonWithUserLogin( rockContext );
        var service = new PersonSessionService( rockContext );

        var expired = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = userLogin.Person.PrimaryAliasId.Value,
            UserLoginId = userLogin.Id,
            CreationSource = PersonSessionCreationSource.ApiKey,
            IsActive = true,
            IsPersistent = true,
            IssuedDateTime = RockDateTime.Now.AddMinutes( -30 ),
            ExpiresDateTime = RockDateTime.Now.AddMinutes( -5 ),
        };
        rockContext.Set<PersonSession>().Add( expired );
        rockContext.SaveChanges();

        var resolved = service.FindOrCreateApiKeySession( requestContext: null, userLogin );

        Assert.IsTrue( resolved == null || resolved.Id != expired.Id,
            "Expired-but-active ApiKey session must not be reused." );
    }

    /// <summary>
    /// A fresh device login (no session of its own on the request) must NOT
    /// reuse a Component session belonging to a DIFFERENT client of the same
    /// shared Database UserLogin (e.g. the person's web session). A new device
    /// session is created and the web session is left active. This is the
    /// mobile / TV bug where a login reused a 30-minute-old web PersonSession.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void FindOrCreateDeviceComponentSession_FreshLogin_DoesNotReuseAnotherClientSession()
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var userLogin = CreatePersonWithUserLogin( rockContext );
        var service = new PersonSessionService( rockContext );

        var webSession = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = userLogin.Person.PrimaryAliasId.Value,
            UserLoginId = userLogin.Id,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
            IssuedDateTime = RockDateTime.Now.AddMinutes( -30 ),
        };
        rockContext.Set<PersonSession>().Add( webSession );
        rockContext.SaveChanges();

        // A fresh device login presents no session of its own (requestContext null).
        var resolved = service.FindOrCreateDeviceComponentSession( requestContext: null, userLogin );

        Assert.IsNotNull( resolved );
        Assert.AreNotEqual( webSession.Id, resolved.Id, "A fresh device login must not reuse another client's (web) session." );
        Assert.AreEqual( userLogin.Id, resolved.UserLoginId );
        Assert.IsTrue( resolved.IsActive );

        Assert.IsTrue( service.Get( webSession.Id ).IsActive, "The other client's (web) session must be left active." );

        var activeComponentCount = rockContext.Set<PersonSession>()
            .Count( s => s.UserLoginId == userLogin.Id
                && s.CreationSource == PersonSessionCreationSource.Component
                && s.IsActive );
        Assert.AreEqual( 2, activeComponentCount, "Both the web session and the new device session should be active." );
    }

    /// <summary>
    /// When the request already carries a <see cref="PersonSession"/> for a
    /// DIFFERENT <see cref="UserLogin"/> (account switch on the device), that
    /// prior session is marked inactive (with <c>InactiveDateTime</c> stamped by
    /// the save hook) and a new device session is created for the incoming
    /// UserLogin. Covers the "Mobile login as a different person on a device that
    /// already had a session" spec test.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void FindOrCreateDeviceComponentSession_DifferentUserLoginOnRequest_MarksPriorInactiveAndCreatesNew()
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var priorLogin = CreatePersonWithUserLogin( rockContext );
        var newLogin = CreatePersonWithUserLogin( rockContext );
        var service = new PersonSessionService( rockContext );

        var priorSession = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = priorLogin.Person.PrimaryAliasId.Value,
            UserLoginId = priorLogin.Id,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
            IssuedDateTime = RockDateTime.Now,
        };
        rockContext.Set<PersonSession>().Add( priorSession );
        rockContext.SaveChanges();

        var requestContext = new RockRequestContext( new TrackingResponseContext() );
        requestContext.SetPersonSession( priorSession );

        var resolved = service.FindOrCreateDeviceComponentSession( requestContext, newLogin );

        Assert.IsNotNull( resolved );
        Assert.AreEqual( newLogin.Id, resolved.UserLoginId );
        Assert.AreNotEqual( priorSession.Id, resolved.Id );
        Assert.IsTrue( resolved.IsActive );

        var priorReloaded = service.Get( priorSession.Id );
        Assert.IsFalse( priorReloaded.IsActive, "Prior session for the other UserLogin should be marked inactive." );
        Assert.IsNotNull( priorReloaded.InactiveDateTime, "Save hook should stamp InactiveDateTime on the superseded prior session." );
    }

    /// <summary>
    /// The device's own session, when its <c>IsActive</c> is still true but its
    /// <see cref="PersonSession.ExpiresDateTime"/> has passed (Rock Cleanup has
    /// not run yet), must NOT be reused; handing it back would fail the next
    /// request's <c>ResolveSessionForRequest</c> expiration check and sign the
    /// device out. The method creates a fresh session instead and leaves the
    /// expired same-person row alone for Rock Cleanup.
    /// </summary>
    [TestMethod]
    [IsolatedTestDatabase]
    public void FindOrCreateDeviceComponentSession_ExpiredButActiveSession_IsNotReused()
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var userLogin = CreatePersonWithUserLogin( rockContext );
        var service = new PersonSessionService( rockContext );

        var expired = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = userLogin.Person.PrimaryAliasId.Value,
            UserLoginId = userLogin.Id,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
            IssuedDateTime = RockDateTime.Now.AddMinutes( -30 ),
            ExpiresDateTime = RockDateTime.Now.AddMinutes( -5 ),
        };
        rockContext.Set<PersonSession>().Add( expired );
        rockContext.SaveChanges();

        // The device presents its own (now-expired) session on the request.
        var requestContext = new RockRequestContext( new TrackingResponseContext() );
        requestContext.SetPersonSession( expired );

        var resolved = service.FindOrCreateDeviceComponentSession( requestContext, userLogin );

        Assert.IsNotNull( resolved );
        Assert.AreNotEqual( expired.Id, resolved.Id, "Expired-but-active device session must not be reused." );
        Assert.AreEqual( userLogin.Id, resolved.UserLoginId );
        Assert.IsTrue( resolved.IsActive );
        Assert.IsNull( resolved.ExpiresDateTime, "A freshly created device session should have no expiration." );
    }

    /// <summary>
    /// Creates a fresh <see cref="Person"/> (with a primary alias) and a
    /// confirmed Database <see cref="UserLogin"/> for it, persists both, and
    /// returns the reloaded UserLogin with its <c>Person</c> navigation
    /// available. Used by the FindOrCreate invariant tests so each test owns
    /// hermetic identities independent of the sample seed data.
    /// </summary>
    /// <param name="rockContext">The <see cref="RockContext"/> to persist into.</param>
    /// <returns>The persisted, reloaded <see cref="UserLogin"/>.</returns>
    private static UserLogin CreatePersonWithUserLogin( RockContext rockContext )
    {
        var personGuid = Guid.NewGuid();
        var person = new Person
        {
            FirstName = "PersonSessionTest",
            LastName = personGuid.ToString(),
            Email = $"{personGuid}@test.com",
            Guid = personGuid,
        };
        PersonService.SaveNewPerson( person, rockContext );
        rockContext.SaveChanges();

        var userLogin = new UserLogin
        {
            Guid = Guid.NewGuid(),
            UserName = $"pstest-{personGuid}",
            Password = "$2a$11$XTLibmiVyu6SArCqLSSi5OQO3tA8cuMWgPVNIfylx5bICaniAfP5C",
            PersonId = person.Id,
            EntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE ).Id,
            IsConfirmed = true,
        };
        new UserLoginService( rockContext ).Add( userLogin );
        rockContext.SaveChanges();

        var reloaded = new UserLoginService( rockContext )
            .Queryable( "Person" )
            .First( ul => ul.Id == userLogin.Id );

        Assert.IsNotNull( reloaded.Person?.PrimaryAliasId, "Seeded person must have a primary alias." );

        return reloaded;
    }

    #endregion FindOrCreate reuse / isolation invariants
}
