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
#if WEBFORMS
using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Tests.Shared.TestFramework;
using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Security;

/// <summary>
/// Mocked-database tests for the legacy <c>FormsAuthenticationTicket</c>
/// upgrade path on <see cref="PersonSessionService"/>. The tests target
/// <c>UpgradeLegacyTicket</c> directly so they can synthesize a ticket
/// without booting <c>HttpContext</c>; the public
/// <c>UpgradeLegacyCookieForRequest</c> shim is untested for the same
/// reason the BeginRequest / PostAuthenticateRequest shims are (no
/// WebForms request lifecycle inside the test harness — see plan
/// Guardrails).
/// </summary>
/// <remarks>
/// Each test in this file is <c>#if WEBFORMS</c>-only and skipped on
/// .NET Core builds, because <c>FormsAuthenticationTicket</c> itself is
/// a System.Web type that only exists on the WebForms target.
/// </remarks>
[TestClass]
public class PersonSessionServiceLegacyUpgradeTests
{
    /// <summary>
    /// A non-impersonation legacy ticket whose <c>Name</c> resolves to a
    /// known <see cref="UserLogin"/> upgrades to a new <see cref="PersonSession"/>
    /// with <c>CreationSource = Legacy</c> and <c>IssuedDateTime</c>
    /// equal to <c>ticket.IssueDate</c>. The latter is what makes the
    /// <c>RejectAuthenticationCookiesIssuedBefore</c> kill switch correct
    /// for upgraded sessions.
    /// </summary>
    [TestMethod]
    public void UpgradeLegacyTicket_CreatesLegacySession_ForValidNonImpersonationTicket()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedUserLogin( rockContext, userLoginId: 7, userName: "ted", personId: 100, primaryAliasId: 200 );

        var ticketIssueDate = RockDateTime.Now.AddDays( -10 );
        var ticket = BuildTicket( "ted", ticketIssueDate, isImpersonated: false );
        var requestContext = BuildRequestContext( new TrackingResponseContext() );

        var service = new PersonSessionService( rockContext );
#pragma warning disable CS0618 // Type or member is obsolete
        var session = service.UpgradeLegacyTicket( ticket, requestContext );
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.IsNotNull( session );
        Assert.AreEqual( PersonSessionCreationSource.Legacy, session.CreationSource );
        Assert.AreEqual( 7, session.UserLoginId );
        Assert.AreEqual( ticketIssueDate, session.IssuedDateTime );
        Assert.IsTrue( session.IsActive );
        Assert.IsTrue( session.IsPersistent );
    }

    /// <summary>
    /// Presenting the same legacy ticket twice (e.g., a client that does
    /// not honor <c>Set-Cookie</c>) resolves to the existing row both
    /// times via the composite key <c>(UserLoginId, IssuedDateTime,
    /// CreationSource = Legacy)</c>. Prevents row spam from misbehaving
    /// clients.
    /// </summary>
    [TestMethod]
    public void UpgradeLegacyTicket_ResolvesExistingRow_OnRepeatedCall()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedUserLogin( rockContext, userLoginId: 7, userName: "ted", personId: 100, primaryAliasId: 200 );

        var ticketIssueDate = RockDateTime.Now.AddDays( -10 );
        var ticket = BuildTicket( "ted", ticketIssueDate, isImpersonated: false );

        var service = new PersonSessionService( rockContext );

#pragma warning disable CS0618 // Type or member is obsolete
        var firstSession = service.UpgradeLegacyTicket( ticket, BuildRequestContext( new TrackingResponseContext() ) );
        var secondSession = service.UpgradeLegacyTicket( ticket, BuildRequestContext( new TrackingResponseContext() ) );
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.IsNotNull( firstSession );
        Assert.IsNotNull( secondSession );
        Assert.AreEqual( firstSession.Guid, secondSession.Guid );

        var totalLegacyRows = rockContext.Set<PersonSession>()
            .Count( s => s.CreationSource == PersonSessionCreationSource.Legacy );
        Assert.AreEqual( 1, totalLegacyRows );
    }

    /// <summary>
    /// A legacy ticket whose <c>UserData.IsImpersonated</c> is <c>true</c>
    /// is dropped: no <see cref="PersonSession"/> is created, the helper
    /// returns null, and the cookie is expired via the response. Silently
    /// upgrading impersonation cookies into long-lived sessions would
    /// extend impersonation past its intended lifetime.
    /// </summary>
    [TestMethod]
    public void UpgradeLegacyTicket_ReturnsNullAndExpiresCookie_ForImpersonatedTicket()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedUserLogin( rockContext, userLoginId: 7, userName: "ted", personId: 100, primaryAliasId: 200 );

        var ticket = BuildTicket( "ted", RockDateTime.Now.AddDays( -10 ), isImpersonated: true );
        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( response );

        var service = new PersonSessionService( rockContext );
#pragma warning disable CS0618 // Type or member is obsolete
        var session = service.UpgradeLegacyTicket( ticket, requestContext );
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.IsNull( session );
        Assert.AreEqual( 0, rockContext.Set<PersonSession>().Count() );
        Assert.HasCount( 1, response.RemovedCookies );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.RemovedCookies[0].Name );
    }

    /// <summary>
    /// A legacy ticket whose <c>IsPersistent</c> is <c>false</c> is dropped:
    /// no <see cref="PersonSession"/> is created, the helper returns null,
    /// and the cookie is expired. The user unchecked "remember me" at login,
    /// so the legacy cookie was a transient session cookie that would have
    /// died with the browser. Every <c>PersonSession</c> created by the
    /// upgrade path is stamped <c>IsPersistent = true</c>, so silently
    /// upgrading a transient ticket would contradict the user's original
    /// choice. The recipient re-authenticates on the new format with
    /// whatever persistence they prefer at that point.
    /// </summary>
    [TestMethod]
    public void UpgradeLegacyTicket_ReturnsNullAndExpiresCookie_ForNonPersistentTicket()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedUserLogin( rockContext, userLoginId: 7, userName: "ted", personId: 100, primaryAliasId: 200 );

        var ticket = BuildTicket( "ted", RockDateTime.Now.AddDays( -10 ), isImpersonated: false, isPersistent: false );
        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( response );

        var service = new PersonSessionService( rockContext );
#pragma warning disable CS0618 // Type or member is obsolete
        var session = service.UpgradeLegacyTicket( ticket, requestContext );
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.IsNull( session );
        Assert.AreEqual( 0, rockContext.Set<PersonSession>().Count() );
        Assert.HasCount( 1, response.RemovedCookies );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.RemovedCookies[0].Name );
    }

    /// <summary>
    /// A legacy ticket whose <c>Name</c> does not match any existing
    /// <see cref="UserLogin"/> (deleted between issuance and presentation)
    /// is dropped: no <see cref="PersonSession"/> is created, the helper
    /// returns null, and the stale cookie is expired.
    /// </summary>
    [TestMethod]
    public void UpgradeLegacyTicket_ReturnsNullAndExpiresCookie_WhenUserLoginIsUnknown()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        // No UserLogin seeded; the ticket points at a deleted account.
        var ticket = BuildTicket( "ghost", RockDateTime.Now.AddDays( -10 ), isImpersonated: false );
        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( response );

        var service = new PersonSessionService( rockContext );
#pragma warning disable CS0618 // Type or member is obsolete
        var session = service.UpgradeLegacyTicket( ticket, requestContext );
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.IsNull( session );
        Assert.AreEqual( 0, rockContext.Set<PersonSession>().Count() );
        Assert.HasCount( 1, response.RemovedCookies );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.RemovedCookies[0].Name );
    }

    /// <summary>
    /// An upgraded session leaves <c>LastStepUpAuthenticationDateTime</c>
    /// and <c>LastMultiFactorAuthenticationDateTime</c> null, so the
    /// session reports <see cref="AuthenticationStrength.Authenticated"/>
    /// — NOT <c>Elevated</c> or <c>MultiFactor</c> — until the user next
    /// authenticates. The legacy ticket carries no timestamp for either,
    /// so honoring its <c>IsTwoFactorAuthenticated</c> bit at upgrade
    /// would amount to granting an indefinite MFA window for free.
    /// </summary>
    [TestMethod]
    public void UpgradeLegacyTicket_LeavesRecencyNull_OnUpgradedSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedUserLogin( rockContext, userLoginId: 7, userName: "ted", personId: 100, primaryAliasId: 200 );

        var ticket = BuildTicket( "ted", RockDateTime.Now.AddDays( -10 ), isImpersonated: false );
        var requestContext = BuildRequestContext( new TrackingResponseContext() );

        var service = new PersonSessionService( rockContext );
#pragma warning disable CS0618 // Type or member is obsolete
        var session = service.UpgradeLegacyTicket( ticket, requestContext );
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.IsNotNull( session );
        Assert.IsNull( session.LastStepUpAuthenticationDateTime );
        Assert.IsNull( session.LastMultiFactorAuthenticationDateTime );
        Assert.AreEqual( AuthenticationStrength.Authenticated, session.GetAuthenticationStrength() );
    }

    /// <summary>
    /// The upgraded session's <c>IssuedDateTime</c> equals the legacy
    /// ticket's <c>IssueDate</c>. This is what makes the
    /// <c>RejectAuthenticationCookiesIssuedBefore</c> kill switch correct
    /// for upgraded sessions on subsequent requests — the kill-switch
    /// comparison runs against <c>PersonSession.IssuedDateTime</c>, and
    /// the upgrade path is the only place where that value comes from
    /// outside the system clock. (The kill-switch behavior itself is
    /// exercised by <c>PersonSessionServiceCookieTests</c>; this test
    /// guards the input.)
    /// </summary>
    [TestMethod]
    public void UpgradeLegacyTicket_StampsIssuedDateTime_FromTicketIssueDate()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedUserLogin( rockContext, userLoginId: 7, userName: "ted", personId: 100, primaryAliasId: 200 );

        var ticketIssueDate = RockDateTime.Now.AddDays( -45 );
        var ticket = BuildTicket( "ted", ticketIssueDate, isImpersonated: false );
        var requestContext = BuildRequestContext( new TrackingResponseContext() );

        var service = new PersonSessionService( rockContext );
#pragma warning disable CS0618 // Type or member is obsolete
        var session = service.UpgradeLegacyTicket( ticket, requestContext );
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.IsNotNull( session );
        Assert.AreEqual( ticketIssueDate, session.IssuedDateTime );
    }

    /// <summary>
    /// The successful upgrade path emits a fresh new-format cookie via
    /// the response context so the client switches to new-format on the
    /// next request without being forced to re-authenticate.
    /// </summary>
    [TestMethod]
    public void UpgradeLegacyTicket_EmitsNewFormatCookie_OnSuccess()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedUserLogin( rockContext, userLoginId: 7, userName: "ted", personId: 100, primaryAliasId: 200 );

        var ticket = BuildTicket( "ted", RockDateTime.Now.AddDays( -10 ), isImpersonated: false );
        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( response );

        var service = new PersonSessionService( rockContext );
#pragma warning disable CS0618 // Type or member is obsolete
        var session = service.UpgradeLegacyTicket( ticket, requestContext );
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.IsNotNull( session );
        Assert.HasCount( 1, response.AddedCookies );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.AddedCookies[0].Name );
        Assert.IsFalse( string.IsNullOrEmpty( response.AddedCookies[0].Value ) );
    }

    #region Test infrastructure

    /// <summary>
    /// Seeds a <see cref="UserLogin"/> linked to a <see cref="Person"/>
    /// (with the supplied primary alias id) into the mocked
    /// <c>RockContext</c>. The navigation property is wired manually
    /// because mocked EF does not auto-load <c>Include</c>.
    /// </summary>
    private static void SeedUserLogin( Rock.Data.RockContext rockContext, int userLoginId, string userName, int personId, int primaryAliasId )
    {
        var person = new Person
        {
            Id = personId,
            PrimaryAliasId = primaryAliasId,
        };

        var userLogin = new UserLogin
        {
            Id = userLoginId,
            UserName = userName,
            PersonId = personId,
            Person = person,
        };

        rockContext.Set<Person>().Add( person );
        rockContext.Set<UserLogin>().Add( userLogin );
    }

    /// <summary>
    /// Builds a synthetic legacy <c>FormsAuthenticationTicket</c> with the
    /// minimum fields the upgrade helper reads (<c>Name</c>,
    /// <c>IssueDate</c>, <c>IsPersistent</c>, <c>UserData</c>). Defaults
    /// to <c>isPersistent: true</c> so callers exercising the happy path
    /// do not have to opt in.
    /// </summary>
    private static System.Web.Security.FormsAuthenticationTicket BuildTicket( string name, DateTime issueDate, bool isImpersonated, bool isPersistent = true )
    {
        // The UserData JSON shape that Authorization.GetUserData
        // deserializes. Keys are PascalCase to match the AuthenticationTicketUserData
        // type the helper uses internally.
        var userData = $"{{\"IsImpersonated\":{( isImpersonated ? "true" : "false" )},\"IsTwoFactorAuthenticated\":false}}";

        return new System.Web.Security.FormsAuthenticationTicket(
            version: 1,
            name: name,
            issueDate: issueDate,
            expiration: issueDate.AddDays( 30 ),
            isPersistent: isPersistent,
            userData: userData,
            cookiePath: "/"
        );
    }

    /// <summary>
    /// Builds a minimal <see cref="RockRequestContext"/> backed by the
    /// supplied response. The upgrade helper does not read the cookie
    /// from the request (it receives the ticket directly), so the
    /// request side can be empty.
    /// </summary>
    private static RockRequestContext BuildRequestContext( IRockResponseContext response )
    {
        return new RockRequestContext( response );
    }

    #endregion Test infrastructure
}
#endif
