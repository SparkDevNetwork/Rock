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
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Security;

/// <summary>
/// Mocked-database unit tests for Phase 12 of the PersonSession spec
/// (Pattern B / user-token impersonation). Covers the 11-row
/// <c>ProcessImpersonationToken</c> matrix plus the per-request
/// <c>RevalidateUserTokenSession</c> hook.
/// </summary>
[TestClass]
public class PersonSessionServiceImpersonationTokenTests
{
    #region ProcessImpersonationToken matrix

    /// <summary>
    /// Matrix row 1: anonymous request with a valid token creates a new
    /// <see cref="PersonSessionCreationSource.UserToken"/> session for the
    /// token's target person and asks the caller to redirect.
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_Anonymous_ValidToken_CreatesUserTokenSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var rckipid = SeedPersonToken( rockContext, personAliasId: 100, tokenId: 7 );

        var requestContext = new RockRequestContext();
        var service = new PersonSessionService( rockContext );

        var result = service.ProcessImpersonationToken( rckipid, requestContext );

        Assert.IsNotNull( result );
        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNotNull( result.Session );
        Assert.AreEqual( PersonSessionCreationSource.UserToken, result.Session.CreationSource );
        Assert.AreEqual( 100, result.Session.PersonAliasId );
    }

    /// <summary>
    /// Matrix row 2: anonymous request with an invalid token leaves the
    /// caller anonymous and asks for redirect-required (so the rckipid still
    /// comes out of the URL).
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_Anonymous_InvalidToken_NoSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        // Encrypt a random token string that has no matching PersonToken row.
        var rckipid = Encryption.EncryptString( "no-matching-token" );

        var requestContext = new RockRequestContext();
        var service = new PersonSessionService( rockContext );

        var result = service.ProcessImpersonationToken( rckipid, requestContext );

        Assert.IsNotNull( result );
        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNull( result.Session );
    }

    /// <summary>
    /// Matrix row 3: current session is <see cref="PersonSessionCreationSource.UserToken"/>
    /// for X and the incoming token matches the source token of that session.
    /// No new session row is created, <c>TimesUsed</c> is NOT incremented,
    /// and the caller is asked to redirect to a clean URL.
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_UserTokenForSamePerson_SameToken_NoChange()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var tokenGuid = Guid.NewGuid();
        var rckipid = SeedPersonToken( rockContext, personAliasId: 100, tokenId: 7, tokenGuid: tokenGuid );
        var personToken = rockContext.Set<PersonToken>().Single( pt => pt.Id == 7 );
        var initialTimesUsed = personToken.TimesUsed;

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };
        currentSession.SetAdditionalSettings( new PersonSessionUserTokenSettings
        {
            OriginatingPersonTokenGuid = tokenGuid,
        } );
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.ProcessImpersonationToken( rckipid, requestContext );

        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNotNull( result.Session );
        Assert.AreEqual( currentSession.Guid, result.Session.Guid );
        Assert.AreEqual( initialTimesUsed, personToken.TimesUsed, "TimesUsed must not increment when the incoming token matches the current session's source token." );
    }

    /// <summary>
    /// Matrix row 4: current session is <see cref="PersonSessionCreationSource.UserToken"/>
    /// for X but the incoming token is for Y. The current session is marked
    /// inactive and a new <see cref="PersonSessionCreationSource.UserToken"/>
    /// session is created for Y.
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_UserTokenForDifferentPerson_MarksInactiveAndCreatesNew()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );
        SeedPerson( rockContext, personId: 51, personAliasId: 200 );

        var rckipidForY = SeedPersonToken( rockContext, personAliasId: 200, tokenId: 7 );

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };
        currentSession.SetAdditionalSettings( new PersonSessionUserTokenSettings
        {
            OriginatingPersonTokenGuid = Guid.NewGuid(),
        } );
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.ProcessImpersonationToken( rckipidForY, requestContext );

        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNotNull( result.Session );
        Assert.AreEqual( PersonSessionCreationSource.UserToken, result.Session.CreationSource );
        Assert.AreEqual( 200, result.Session.PersonAliasId );
        Assert.IsFalse( currentSession.IsActive, "The prior UserToken session must be marked inactive when the token targets a different person." );
    }

    /// <summary>
    /// Matrix rows 5, 6, 7: an active <see cref="PersonSessionCreationSource.Impersonation"/>
    /// session is abandoned (marked inactive) regardless of whether the
    /// incoming token targets the admin, the impersonated person, or a
    /// third person.
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_FromImpersonationSession_AbandonsImpersonation()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );
        SeedPerson( rockContext, personId: 52, personAliasId: 300 );

        var rckipidForThird = SeedPersonToken( rockContext, personAliasId: 300, tokenId: 7 );

        var impersonationSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( impersonationSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( impersonationSession );

        var service = new PersonSessionService( rockContext );
        var result = service.ProcessImpersonationToken( rckipidForThird, requestContext );

        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNotNull( result.Session );
        Assert.AreEqual( PersonSessionCreationSource.UserToken, result.Session.CreationSource );
        Assert.AreEqual( 300, result.Session.PersonAliasId );
        Assert.IsFalse( impersonationSession.IsActive, "Impersonation must be abandoned (session marked inactive) when an rckipid arrives." );
    }

    /// <summary>
    /// Matrix row 8: current session is <see cref="PersonSessionCreationSource.Component"/>
    /// for X and the incoming token targets the same person. The session is
    /// unchanged, but <c>TimesUsed</c> still increments per the strict
    /// literal spec wording (the Component session references no token, so
    /// the incoming token "differs from the token referenced by the current
    /// session").
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_ComponentForSamePerson_NoSessionChange_TimesUsedIncrements()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var rckipid = SeedPersonToken( rockContext, personAliasId: 100, tokenId: 7 );
        var personToken = rockContext.Set<PersonToken>().Single( pt => pt.Id == 7 );
        var initialTimesUsed = personToken.TimesUsed;

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.ProcessImpersonationToken( rckipid, requestContext );

        Assert.IsTrue( result.IsRedirectRequired );
        Assert.AreEqual( currentSession.Guid, result.Session.Guid );
        Assert.IsTrue( currentSession.IsActive, "Component session for the same person must remain active." );
        Assert.AreEqual( initialTimesUsed + 1, personToken.TimesUsed, "TimesUsed must increment when the current session references no token." );
    }

    /// <summary>
    /// Matrix row 9: current session is <see cref="PersonSessionCreationSource.Component"/>
    /// for X but the incoming token targets Y. The Component session is
    /// marked inactive and a new <see cref="PersonSessionCreationSource.UserToken"/>
    /// session is created for Y.
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_ComponentForDifferentPerson_MarksInactiveAndCreatesNew()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );
        SeedPerson( rockContext, personId: 51, personAliasId: 200 );

        var rckipidForY = SeedPersonToken( rockContext, personAliasId: 200, tokenId: 7 );

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.ProcessImpersonationToken( rckipidForY, requestContext );

        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNotNull( result.Session );
        Assert.AreEqual( PersonSessionCreationSource.UserToken, result.Session.CreationSource );
        Assert.AreEqual( 200, result.Session.PersonAliasId );
        Assert.IsFalse( currentSession.IsActive );
    }

    /// <summary>
    /// Matrix row 10: an expired token marks any current session inactive
    /// (regardless of CreationSource) and leaves the caller anonymous.
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_ExpiredToken_MarksCurrentInactive_NoNewSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var rckipid = SeedPersonToken(
            rockContext,
            personAliasId: 100,
            tokenId: 7,
            expireDateTime: RockDateTime.Now.AddMinutes( -5 ) );

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.ProcessImpersonationToken( rckipid, requestContext );

        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNull( result.Session );
        Assert.IsFalse( currentSession.IsActive, "Expired token must mark the current session inactive." );
    }

    /// <summary>
    /// A token that has been used up to (or beyond) its <c>UsageLimit</c> is
    /// rejected by <c>ProcessImpersonationToken</c> the same way an expired
    /// token is. <c>TimesUsed &gt;= UsageLimit</c> is the rejection
    /// threshold, not <c>&gt;</c>: a <c>UsageLimit=1</c> token with
    /// <c>TimesUsed=1</c> is exhausted, not still usable for one more click.
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_OverUsageLimitToken_MarksCurrentInactive_NoNewSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var rckipid = SeedPersonToken(
            rockContext,
            personAliasId: 100,
            tokenId: 7,
            usageLimit: 1,
            timesUsed: 1 );

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.ProcessImpersonationToken( rckipid, requestContext );

        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNull( result.Session );
        Assert.IsFalse( currentSession.IsActive );
    }

    /// <summary>
    /// A page-scoped token presented on a different page during the rckipid
    /// arrival flow is treated as invalid (matrix rule 1): current session
    /// marked inactive, no new session, redirect required.
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_PageScopedToken_OnWrongPage_TreatedAsInvalid()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var rckipid = SeedPersonToken(
            rockContext,
            personAliasId: 100,
            tokenId: 7,
            pageId: 42 );

        var requestContext = new RockRequestContext();
        var service = new PersonSessionService( rockContext );

        // Token is scoped to page 42; request targets page 99.
        var result = service.ProcessImpersonationToken( rckipid, requestContext, currentPageId: 99 );

        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNull( result.Session );
    }

    /// <summary>
    /// A page-scoped token presented to a non-page context (such as an API
    /// endpoint, where <c>currentPageId</c> is null) is treated as invalid.
    /// The scope's intent is to confine the token to a specific page;
    /// "no page at all" must fail the check rather than bypass it.
    /// </summary>
    [TestMethod]
    public void ProcessImpersonationToken_PageScopedToken_OnNonPageContext_TreatedAsInvalid()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedSystemData( rockContext );
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var rckipid = SeedPersonToken(
            rockContext,
            personAliasId: 100,
            tokenId: 7,
            pageId: 42 );

        var requestContext = new RockRequestContext();
        var service = new PersonSessionService( rockContext );

        // Token is scoped to page 42; caller passes no page (API request).
        var result = service.ProcessImpersonationToken( rckipid, requestContext, currentPageId: null );

        Assert.IsTrue( result.IsRedirectRequired );
        Assert.IsNull( result.Session );
    }

    #endregion ProcessImpersonationToken matrix

    #region RevalidateUserTokenSession

    /// <summary>
    /// A non-UserToken current session is a no-op for the per-request
    /// re-validation hook: there is no source token to check.
    /// </summary>
    [TestMethod]
    public void RevalidateUserTokenSession_ComponentSession_ReturnsOk()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.RevalidateUserTokenSession( requestContext, currentPageId: null );

        Assert.AreEqual( UserTokenRevalidationResult.Ok, result );
    }

    /// <summary>
    /// The per-request hook marks the session inactive when the source
    /// <c>PersonToken</c> row has been deleted between requests
    /// (revocation), and signals <c>SessionRevoked</c> to the caller.
    /// </summary>
    [TestMethod]
    public void RevalidateUserTokenSession_TokenDeleted_MarksInactiveAndReturnsRevoked()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        // No PersonToken row in the database; the session's
        // OriginatingPersonTokenGuid refers to a nonexistent token.
        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };
        currentSession.SetAdditionalSettings( new PersonSessionUserTokenSettings
        {
            OriginatingPersonTokenGuid = Guid.NewGuid(),
        } );
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.RevalidateUserTokenSession( requestContext, currentPageId: null );

        Assert.AreEqual( UserTokenRevalidationResult.SessionRevoked, result );
        Assert.IsFalse( currentSession.IsActive );
    }

    /// <summary>
    /// The per-request hook returns <c>PageScopeMiss</c> (without touching
    /// the session) when the source token is page-scoped and the current
    /// request targets a different page. The recipient can still return to
    /// the in-scope page; only this request is refused.
    /// </summary>
    [TestMethod]
    public void RevalidateUserTokenSession_PageScopeMismatch_ReturnsPageScopeMiss_SessionStaysActive()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var tokenGuid = Guid.NewGuid();
        SeedPersonToken( rockContext, personAliasId: 100, tokenId: 7, tokenGuid: tokenGuid, pageId: 42 );

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };
        currentSession.SetAdditionalSettings( new PersonSessionUserTokenSettings
        {
            OriginatingPersonTokenGuid = tokenGuid,
        } );
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.RevalidateUserTokenSession( requestContext, currentPageId: 99 );

        Assert.AreEqual( UserTokenRevalidationResult.PageScopeMiss, result );
        Assert.IsTrue( currentSession.IsActive, "Page-scope misses must NOT deactivate the session; the recipient can still return to the in-scope page." );
    }

    /// <summary>
    /// A page-scoped UserToken session re-validated with no current page
    /// (such as an API call made from JavaScript or an Obsidian block on
    /// the in-scope page) returns <c>Ok</c>. The page-scope check is for
    /// navigation between pages, not for API calls a legitimate in-scope
    /// page makes to render itself.
    /// </summary>
    [TestMethod]
    public void RevalidateUserTokenSession_PageScopedToken_NoCurrentPage_ReturnsOk()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var tokenGuid = Guid.NewGuid();
        SeedPersonToken( rockContext, personAliasId: 100, tokenId: 7, tokenGuid: tokenGuid, pageId: 42 );

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };
        currentSession.SetAdditionalSettings( new PersonSessionUserTokenSettings
        {
            OriginatingPersonTokenGuid = tokenGuid,
        } );
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.RevalidateUserTokenSession( requestContext, currentPageId: null );

        Assert.AreEqual( UserTokenRevalidationResult.Ok, result );
        Assert.IsTrue( currentSession.IsActive );
    }

    /// <summary>
    /// The per-request hook marks the session inactive and signals
    /// <c>SessionRevoked</c> when the source token's <c>ExpireDateTime</c>
    /// has passed since the session was issued.
    /// </summary>
    [TestMethod]
    public void RevalidateUserTokenSession_TokenExpired_MarksInactiveAndReturnsRevoked()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        SeedPerson( rockContext, personId: 50, personAliasId: 100 );

        var tokenGuid = Guid.NewGuid();
        SeedPersonToken(
            rockContext,
            personAliasId: 100,
            tokenId: 7,
            tokenGuid: tokenGuid,
            expireDateTime: RockDateTime.Now.AddMinutes( -5 ) );

        var currentSession = new PersonSession
        {
            Id = 30,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };
        currentSession.SetAdditionalSettings( new PersonSessionUserTokenSettings
        {
            OriginatingPersonTokenGuid = tokenGuid,
        } );
        rockContext.Set<PersonSession>().Add( currentSession );

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( currentSession );

        var service = new PersonSessionService( rockContext );
        var result = service.RevalidateUserTokenSession( requestContext, currentPageId: null );

        Assert.AreEqual( UserTokenRevalidationResult.SessionRevoked, result );
        Assert.IsFalse( currentSession.IsActive );
    }

    #endregion RevalidateUserTokenSession

    #region Test Helpers

    /// <summary>
    /// Seeds a Person and primary <see cref="PersonAlias"/> into the mocked
    /// context. Tests use known integer ids so cross-entity references are
    /// trivial to wire up.
    /// </summary>
    private static void SeedPerson( RockContext rockContext, int personId, int personAliasId )
    {
        var person = new Person
        {
            Id = personId,
            PrimaryAliasId = personAliasId,
            Aliases = [],
        };
        var alias = new PersonAlias { Id = personAliasId, PersonId = personId, Person = person };
        person.Aliases.Add( alias );
        rockContext.Set<Person>().Add( person );
        rockContext.Set<PersonAlias>().Add( alias );
    }

    /// <summary>
    /// Seeds a <see cref="PersonToken"/> row into the mocked context and
    /// returns the encrypted <c>rckipid</c> string the caller can pass to
    /// <see cref="PersonSessionService.ProcessImpersonationToken"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="PersonToken.Token"/> property has a private setter,
    /// so this helper uses reflection to stamp a generated token value.
    /// The encrypted form of that same value is what
    /// <see cref="PersonTokenService.GetByImpersonationToken"/> decrypts and
    /// matches against during the lookup.
    /// </remarks>
    private static string SeedPersonToken(
        RockContext rockContext,
        int personAliasId,
        int tokenId,
        Guid? tokenGuid = null,
        DateTime? expireDateTime = null,
        int? usageLimit = null,
        int timesUsed = 0,
        int? pageId = null )
    {
        var rawToken = Encryption.GenerateUniqueToken();
        var rckipid = Encryption.EncryptString( rawToken );

        var alias = rockContext.Set<PersonAlias>().SingleOrDefault( pa => pa.Id == personAliasId );

        var personToken = new PersonToken
        {
            Id = tokenId,
            Guid = tokenGuid ?? Guid.NewGuid(),
            PersonAliasId = personAliasId,
            PersonAlias = alias,
            ExpireDateTime = expireDateTime,
            UsageLimit = usageLimit,
            TimesUsed = timesUsed,
            PageId = pageId,
        };

        typeof( PersonToken )
            .GetProperty( nameof( PersonToken.Token ), BindingFlags.Public | BindingFlags.Instance )
            .SetValue( personToken, rawToken );

        rockContext.Set<PersonToken>().Add( personToken );

        return rckipid;
    }

    private static void SeedSystemData( RockContext rockContext )
    {
        rockContext.Set<FieldType>().Add( new FieldType
        {
            Id = 1,
            Guid = SystemGuid.FieldType.TEXT.AsGuid(),
        } );
    }

    #endregion Test Helpers
}
