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
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Configuration;
using Rock.Enums.Net;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;

namespace Rock.Tests.Security;

/// <summary>
/// Tests for the cookie I/O surface of <see cref="PersonSessionService"/>:
/// encode / decode round-trip, tamper detection, wire-format guarantees, and
/// the <c>ResolveSessionForRequest</c> read-side lifecycle (cookie reissue,
/// cookie expiry on rejection, absent-cookie handling). Plain-unit tests
/// where possible; mocked-database tests where session-state assertions
/// require a <c>RockContext</c>.
/// </summary>
[TestClass]
public class PersonSessionServiceCookieTests
{
    #region Encode / Decode

    /// <summary>
    /// A payload encoded via <c>GetCookieValue</c> + the internal
    /// <c>TryDecodeCookie</c> round-trips back to the same session Guid and
    /// payload version. The <c>iat</c> field captures the encode-time
    /// timestamp.
    /// </summary>
    [TestMethod]
    public void GetCookieValue_TryDecodeCookie_RoundTripsPayload()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };

        var before = RockDateTime.Now;
        var cookieValue = service.GetCookieValue( session );
        var after = RockDateTime.Now;

        Assert.IsFalse( cookieValue.IsNullOrWhiteSpace() );

        PersonSessionService.TryDecodeCookie( cookieValue, out var decoded, out var metadata );
        Assert.IsNotNull( decoded );
        Assert.AreEqual( session.Guid, decoded.SessionGuid );
        Assert.AreEqual( PersonSessionService.CookiePayloadVersion, decoded.Version );
        Assert.IsTrue( decoded.IssuedAt >= before && decoded.IssuedAt <= after,
            "Cookie iat should fall in the window between encode start and end." );

        Assert.IsNotNull( metadata );
        Assert.IsFalse( metadata.DecryptedWithOldKey, "Fresh cookie should decrypt with the current key." );
        Assert.AreEqual( PersonSessionService.CookiePayloadVersion, metadata.PayloadVersion );
    }

    /// <summary>
    /// A tampered cookie value (one character mutated mid-string) decrypts
    /// to <c>null</c> via the encrypt-then-MAC authentication step, and
    /// <c>TryDecodeCookie</c> returns false. The auth pipeline silently
    /// treats this as an absent cookie.
    /// </summary>
    [TestMethod]
    public void TryDecodeCookie_ReturnsFalse_ForTamperedCookie()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };

        var cookieValue = service.GetCookieValue( session );

        // Flip a character roughly in the middle of the base64 payload to
        // hit the ciphertext (not the V2 footer or the trailing tag).
        var midpoint = cookieValue.Length / 2;
        var tamperedChar = cookieValue[midpoint] == 'A' ? 'B' : 'A';
        var tamperedCookie = cookieValue.Substring( 0, midpoint ) + tamperedChar + cookieValue.Substring( midpoint + 1 );

        PersonSessionService.TryDecodeCookie( tamperedCookie, out var decoded, out var metadata );
        Assert.IsNull( decoded, "Tampered ciphertext should fail HMAC verification and decode to null." );
        Assert.IsNull( metadata );
    }

    /// <summary>
    /// The plaintext payload emitted by <c>GetCookieValue</c> is a pure
    /// System.Text.Json object with exactly three short-named properties
    /// (<c>v</c>, <c>sid</c>, <c>iat</c>) and no Newtonsoft.Json-style type
    /// discriminator (<c>$type</c>) or property casing override. Locks down
    /// the wire format so a future Newtonsoft re-introduction would visibly
    /// break the test.
    /// </summary>
    [TestMethod]
    public void GetCookieValue_WireFormat_HasNoNewtonsoftArtifacts()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };

        var cookieValue = service.GetCookieValue( session );

        // Decrypt directly to inspect the plaintext wire bytes.
        var plaintext = Rock.Security.Encryption.DecryptString( cookieValue, isLegacyAllowed: false );
        Assert.IsFalse( plaintext.IsNullOrWhiteSpace() );

        using var document = JsonDocument.Parse( plaintext );
        var root = document.RootElement;
        Assert.AreEqual( JsonValueKind.Object, root.ValueKind );

        var propertyNames = root.EnumerateObject().Select( p => p.Name ).ToList();
        CollectionAssert.AreEquivalent( new[] { "v", "sid", "iat" }, propertyNames,
            "Cookie payload should contain exactly v/sid/iat with no extras." );

        Assert.DoesNotContain( "$type", plaintext, "Cookie payload should not contain Newtonsoft type discriminators." );
        Assert.DoesNotContain( "Version", plaintext, "Cookie payload should use short key names, not PascalCase property names." );
        Assert.DoesNotContain( "SessionGuid", plaintext, "Cookie payload should use short key names, not PascalCase property names." );
        Assert.DoesNotContain( "IssuedAt", plaintext, "Cookie payload should use short key names, not PascalCase property names." );
    }

    #endregion Encode / Decode

    #region ResolveSessionForRequest — absent / undecodable / inactive

    /// <summary>
    /// A request with no <c>.ROCK</c> cookie present returns null and does
    /// not mutate the response.
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_AbsentCookie_ReturnsNullAndDoesNotMutateResponse()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue: null, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNull( result );
        Assert.IsEmpty( response.AddedCookies );
        Assert.IsEmpty( response.RemovedCookies );
    }

    /// <summary>
    /// A request bearing a legacy <c>FormsAuthenticationTicket</c>-format
    /// cookie (no <c>"V2"</c> footer) is returned as null without mutating
    /// the response.
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_LegacyFormatCookie_ReturnsNullAndLeavesCookieAlone()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        // A legacy FormsAuthenticationTicket value is base64 but has no V2
        // footer; the decoder bails before any session lookup.
        var legacyLookingValue = Convert.ToBase64String( [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08] );

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( legacyLookingValue, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNull( result );
        Assert.IsEmpty( response.AddedCookies );
        Assert.IsEmpty( response.RemovedCookies );
    }

    /// <summary>
    /// A request whose cookie decodes to a <see cref="PersonSession"/> that
    /// is marked inactive returns null and expires the cookie on the
    /// response.
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_InactiveSession_ReturnsNullAndExpiresCookie()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = false, // inactive
            IsPersistent = false,
        };
        rockContext.Set<PersonSession>().Add( session );

        var cookieValue = service.GetCookieValue( session );

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNull( result );
        Assert.HasCount( 1, response.RemovedCookies );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.RemovedCookies[0].Name );
    }

    /// <summary>
    /// A request whose cookie decodes to a <see cref="PersonSession"/> past
    /// its <c>ExpiresDateTime</c> returns null and expires the cookie.
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_ExpiredSession_ReturnsNullAndExpiresCookie()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            ExpiresDateTime = RockDateTime.Now.AddHours( -1 ), // expired one hour ago
            IsPersistent = false,
        };
        rockContext.Set<PersonSession>().Add( session );

        var cookieValue = service.GetCookieValue( session );

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNull( result );
        Assert.HasCount( 1, response.RemovedCookies );
    }

    #endregion ResolveSessionForRequest — absent / undecodable / inactive

    #region ResolveSessionForRequest — happy path + reissue

    /// <summary>
    /// A young cookie (iat just stamped, well inside the half-life window)
    /// against a valid active session returns the session AND does NOT
    /// reissue the cookie on the response.
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_YoungCookie_ReturnsSessionWithNoReissue()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        // ResolveSessionForRequest will trigger an attempt to create an Attribute
        // of field type Text to save the security settings.
        rockContext.Set<FieldType>().Add( new FieldType
        {
            Id = 1,
            Guid = SystemGuid.FieldType.TEXT.AsGuid(),
        } );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( session );

        var cookieValue = service.GetCookieValue( session );

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNotNull( result );
        Assert.AreEqual( session.Guid, result.Guid );
        Assert.IsEmpty( response.AddedCookies, "Young cookie should not trigger a reissue." );
        Assert.IsEmpty( response.RemovedCookies );
    }

    /// <summary>
    /// A cookie at or past the half-life (default 15 days for the 30-day
    /// FormsAuthentication.Timeout) triggers a reissue: the response gets a
    /// fresh <c>Set-Cookie</c> with a new <c>iat</c>, the same <c>sid</c>,
    /// and a refreshed <c>Expires</c>. <see cref="PersonSession.IssuedDateTime"/>
    /// is left untouched.
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_CookiePastHalfLife_TriggersReissueWithoutChangingIssuedDateTime()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        // ResolveSessionForRequest will trigger an attempt to create an Attribute
        // of field type Text to save the security settings.
        rockContext.Set<FieldType>().Add( new FieldType
        {
            Id = 1,
            Guid = SystemGuid.FieldType.TEXT.AsGuid(),
        } );

        var originalIssuedDateTime = RockDateTime.Now.AddDays( -20 );
        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
            IssuedDateTime = originalIssuedDateTime,
        };
        rockContext.Set<PersonSession>().Add( session );

        // Manually mint a cookie whose iat is past the half-life — the
        // half-life of PersonSessionService.AuthCookieTimeout.
        var stalePayload = new PersonSessionCookiePayload
        {
            Version = PersonSessionService.CookiePayloadVersion,
            SessionGuid = session.Guid,
            IssuedAt = RockDateTime.Now.AddDays( -( ( PersonSessionService.AuthCookieTimeout.TotalDays / 2 ) + 5 ) ),
        };
        var stalePlaintext = JsonSerializer.Serialize( stalePayload );
        var staleCookieValue = Rock.Security.Encryption.EncryptString( stalePlaintext );

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( staleCookieValue, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNotNull( result );
        Assert.AreEqual( session.Guid, result.Guid );
        Assert.HasCount( 1, response.AddedCookies, "Stale cookie should trigger exactly one reissue." );

        // Decode the reissued cookie and verify its iat advanced AND its sid
        // matches the original session.
        var reissued = response.AddedCookies[0];
        PersonSessionService.TryDecodeCookie( reissued.Value, out var reissuedDecoded, out _ );
        Assert.IsNotNull( reissuedDecoded );
        Assert.AreEqual( session.Guid, reissuedDecoded.SessionGuid, "Reissue must preserve the same sid." );
        Assert.IsTrue( reissuedDecoded.IssuedAt > stalePayload.IssuedAt, "Reissue must advance iat." );

        // PersonSession.IssuedDateTime must NOT have changed (kill-switch
        // correctness is preserved across reissue).
        Assert.AreEqual( originalIssuedDateTime, session.IssuedDateTime,
            "Reissue must not touch PersonSession.IssuedDateTime." );
    }

    /// <summary>
    /// A cookie whose payload <c>v</c> is older than the current
    /// <see cref="PersonSessionService.CookiePayloadVersion"/> triggers a
    /// reissue regardless of <c>iat</c> age, migrating the payload forward.
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_OlderPayloadVersion_TriggersReissue()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        // ResolveSessionForRequest will trigger an attempt to create an Attribute
        // of field type Text to save the security settings.
        rockContext.Set<FieldType>().Add( new FieldType
        {
            Id = 1,
            Guid = SystemGuid.FieldType.TEXT.AsGuid(),
        } );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( session );

        // Mint a cookie at version 0 (one less than the current version).
        // The iat is fresh so half-life is not what's triggering reissue.
        var oldVersionPayload = new PersonSessionCookiePayload
        {
            Version = PersonSessionService.CookiePayloadVersion - 1,
            SessionGuid = session.Guid,
            IssuedAt = RockDateTime.Now,
        };
        var oldVersionPlaintext = JsonSerializer.Serialize( oldVersionPayload );
        var oldVersionCookie = Rock.Security.Encryption.EncryptString( oldVersionPlaintext );

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( oldVersionCookie, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNotNull( result );
        Assert.HasCount( 1, response.AddedCookies, "Older payload version should trigger reissue." );

        PersonSessionService.TryDecodeCookie( response.AddedCookies[0].Value, out var reissuedDecoded, out _ );

        Assert.AreEqual( PersonSessionService.CookiePayloadVersion, reissuedDecoded.Version,
            "Reissued cookie must use the current payload version." );
    }

    /// <summary>
    /// A non-persistent (session-cookie) <see cref="PersonSession"/> emits a
    /// cookie with no <c>Expires</c> attribute. The cookie dies with the
    /// browser, matching the legacy behavior.
    /// </summary>
    [TestMethod]
    public void SetAuthCookie_NonPersistentSession_HasNoExpiresAttribute()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = false,
        };

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue: null, response );

        service.SetAuthCookie( session, requestContext );

        Assert.HasCount( 1, response.AddedCookies );
        Assert.IsNull( response.AddedCookies[0].Expires,
            "A non-persistent session's cookie should have no Expires attribute." );
    }

    /// <summary>
    /// A persistent <see cref="PersonSession"/> emits a cookie with an
    /// <c>Expires</c> attribute capped by the MIN-formula:
    /// <c>MIN(session.ExpiresDateTime ?? MaxValue, Now + AuthCookieTimeout)</c>.
    /// When <see cref="PersonSession.ExpiresDateTime"/> is null, the cap is
    /// just <c>Now + AuthCookieTimeout</c>.
    /// </summary>
    [TestMethod]
    public void SetAuthCookie_PersistentSession_HasExpiresFromMinFormula()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
            ExpiresDateTime = null, // no session-level expiry, so the cap = Now + Timeout
        };

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue: null, response );

        var before = RockDateTime.Now;
        service.SetAuthCookie( session, requestContext );
        var after = RockDateTime.Now;

        Assert.HasCount( 1, response.AddedCookies );
        var expires = response.AddedCookies[0].Expires;
        Assert.IsNotNull( expires );

        var expectedLow = before.Add( PersonSessionService.AuthCookieTimeout );
        var expectedHigh = after.Add( PersonSessionService.AuthCookieTimeout );
        Assert.IsTrue( expires.Value >= expectedLow && expires.Value <= expectedHigh,
            $"Expires {expires.Value:o} should be in window [{expectedLow:o}, {expectedHigh:o}]." );
    }

    #endregion ResolveSessionForRequest — happy path + reissue

    #region Test infrastructure

    /// <summary>
    /// Builds a <see cref="RockRequestContext"/> backed by a Moq <see cref="IRequest"/>
    /// that surfaces the supplied <paramref name="cookieValue"/> as the
    /// <c>.ROCK</c> cookie (when non-null). The response is the caller-supplied
    /// <see cref="TrackingResponseContext"/> so the test can inspect cookie
    /// writes / removes after the call.
    /// </summary>
    private static RockRequestContext BuildRequestContext( string cookieValue, IRockResponseContext response )
    {
        var headers = new NameValueCollection( StringComparer.OrdinalIgnoreCase );
        var cookies = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        if ( cookieValue.IsNotNullOrWhiteSpace() )
        {
            cookies[PersonSessionService.AuthCookieName] = cookieValue;
        }

        var requestMock = new Mock<IRequest>( MockBehavior.Strict );
        requestMock.SetupGet( r => r.RemoteAddress ).Returns( IPAddress.Loopback );
        requestMock.SetupGet( r => r.RequestUri ).Returns( ( Uri ) null );
        requestMock.SetupGet( r => r.Method ).Returns( "GET" );
        requestMock.SetupGet( r => r.QueryString ).Returns( [] );
        requestMock.SetupGet( r => r.RouteData ).Returns( new Dictionary<string, object>() );
        requestMock.SetupGet( r => r.Headers ).Returns( headers );
        requestMock.SetupGet( r => r.Cookies ).Returns( cookies );
        requestMock.SetupGet( r => r.CookiesValuesAreUrlDecoded ).Returns( false );

        return new RockRequestContext( requestMock.Object, response, currentUser: null );
    }

    /// <summary>
    /// Captures cookie writes and removals so tests can assert on what
    /// <see cref="PersonSessionService"/> emitted via the response context.
    /// </summary>
    private class TrackingResponseContext : IRockResponseContext
    {
        public List<BrowserCookie> AddedCookies { get; } = [];
        public List<BrowserCookie> RemovedCookies { get; } = [];

        public void AddCookie( BrowserCookie cookie ) => AddedCookies.Add( cookie );
        public void RemoveCookie( BrowserCookie cookie ) => RemovedCookies.Add( cookie );

        public void AddBreadCrumb( IBreadCrumb breadcrumb ) { }
        public void AddHtmlElement( string id, string name, string content, Dictionary<string, string> attributes, ResponseElementLocation location ) { }
        public void RedirectToUrl( string url, bool permanent = false ) { }
        public void SetHttpHeader( string name, string value ) { }
        public void SetPageTitle( string title ) { }
        public void SetBrowserTitle( string title ) { }
    }

    #endregion Test infrastructure
}
