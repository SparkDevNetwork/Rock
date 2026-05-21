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

namespace Rock.Tests.Integration.Security;

/// <summary>
/// Full integration tests for the <see cref="PersonSession"/> entity. These
/// verify behavior that depends on the real save pipeline firing the
/// <c>PersonSession.SaveHook</c>; plain-POCO assertions live in
/// <c>Rock.Tests.Security.PersonSessionTests</c>.
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
    /// hook. Re-verifies the Phase 1 invariant now that callers can go
    /// through the central creation path end-to-end.
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
    /// First call to <c>FindOrCreateApiKeySession</c> creates a new row;
    /// second call for the same <c>UserLogin</c> reuses it. Exercises the
    /// real EF SaveChanges path and the filtered unique index in a fresh
    /// database so the find-or-create round-trip is end-to-end verified.
    /// </summary>
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
    /// out of scope for the basic Phase 3 coverage; the same behavior is
    /// what <see cref="InteractionService"/> ships with today.
    /// </remarks>
    [TestMethod]
    [IsolatedTestDatabase]
    public void PopulateNewSession_NewUserAgent_StampsDeviceTypeAndCreatesSingleRow()
    {
        // Use a deliberately uncommon UA string so we can be confident no
        // seed data already has a matching InteractionDeviceType row.
        const string userAgent = "RockPhaseThreeTestAgent/1.0 (PersonSessionUnitTest; rv:1) Gecko/20260101 Firefox/120.0";

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
        const string userAgent = "RockPhaseThreeReuseAgent/2.0 (PersonSessionUnitTest; rv:1) Gecko/20260101 Firefox/121.0";

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
}
