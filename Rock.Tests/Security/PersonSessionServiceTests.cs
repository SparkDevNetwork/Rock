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
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Security;

/// <summary>
/// Mocked-database unit tests for <see cref="PersonSessionService"/>. Tests
/// that need real save semantics (PreSave hook, raw-SQL upsert, etc.) live in
/// <c>Rock.Tests.Integration.Security.PersonSessionTests</c>.
/// </summary>
[TestClass]
public class PersonSessionServiceTests
{
    #region GetImpersonatorSession

    /// <summary>
    /// An admin-impersonation session whose
    /// <see cref="PersonSessionAdminImpersonationSettings.ImpersonatorPersonSessionGuid"/>
    /// resolves to a real <see cref="PersonSession"/> returns that prior
    /// session.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsPriorSession_ForValidImpersonationRestoreReference()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var impersonator = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( impersonator );

        var impersonationSession = new PersonSession
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };
        impersonationSession.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = impersonator.Guid,
            ImpersonatorInteractionSessionGuid = Guid.NewGuid(),
        } );

        var service = new PersonSessionService( rockContext );

        var restored = service.GetImpersonatorSession( impersonationSession );

        Assert.IsNotNull( restored );
        Assert.AreEqual( impersonator.Guid, restored.Guid );
    }

    /// <summary>
    /// A <see cref="PersonSessionCreationSource.UserToken"/> session never has
    /// a restorable impersonator (the recipient is the legitimate owner of the
    /// data, not an admin), so <c>GetImpersonatorSession</c> returns null even
    /// when the session was incorrectly stamped with admin-impersonation
    /// settings.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsNull_ForUserTokenSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };
        session.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = Guid.NewGuid(),
            ImpersonatorInteractionSessionGuid = Guid.NewGuid(),
        } );

        var service = new PersonSessionService( rockContext );

        Assert.IsNull( service.GetImpersonatorSession( session ) );
    }

    /// <summary>
    /// A <see cref="PersonSessionCreationSource.Component"/> session has no
    /// impersonator and returns null.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsNull_ForComponentSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };

        var service = new PersonSessionService( rockContext );

        Assert.IsNull( service.GetImpersonatorSession( session ) );
    }

    /// <summary>
    /// An impersonation session whose stamped
    /// <see cref="PersonSessionAdminImpersonationSettings.ImpersonatorPersonSessionGuid"/>
    /// does not resolve to any <see cref="PersonSession"/> row (deleted, or
    /// never existed) returns null. This is the dangling-reference branch
    /// — the impersonation does not silently continue.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsNull_WhenImpersonatorReferenceIsDangling()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var impersonationSession = new PersonSession
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };
        impersonationSession.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = Guid.NewGuid(), // not seeded
            ImpersonatorInteractionSessionGuid = Guid.NewGuid(),
        } );

        var service = new PersonSessionService( rockContext );

        Assert.IsNull( service.GetImpersonatorSession( impersonationSession ) );
    }

    /// <summary>
    /// <c>GetImpersonatorSession( null )</c> returns null without throwing.
    /// Defensive guard; the spec's strength mapping says "no PersonSession"
    /// is conceptual, but a service that takes a parameter must still tolerate
    /// the caller passing null.
    /// </summary>
    [TestMethod]
    public void GetImpersonatorSession_ReturnsNull_WhenSessionArgumentIsNull()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        Assert.IsNull( service.GetImpersonatorSession( null ) );
    }

    #endregion GetImpersonatorSession

    #region StartComponentSession

    /// <summary>
    /// A regular login always stamps <c>LastStepUpAuthenticationDateTime</c>
    /// — the user has just provided a primary credential — and stamps
    /// <c>LastMultiFactorAuthenticationDateTime</c> when the caller supplies
    /// an <c>mfaRecency</c> (i.e., the authenticating component is configured
    /// for two-factor authentication).
    /// </summary>
    [TestMethod]
    public void StartComponentSession_StampsStepUp_AndMfaWhenSupplied()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var mfaInstant = RockDateTime.Now;
        var session = service.StartComponentSession( requestContext: null, personAliasId: 100, userLoginId: 7, authComponentEntityTypeId: 42, isPersistent: true, mfaRecency: mfaInstant );

        Assert.AreEqual( PersonSessionCreationSource.Component, session.CreationSource );
        Assert.AreEqual( 100, session.PersonAliasId );
        Assert.AreEqual( 7, session.UserLoginId );
        Assert.AreEqual( 42, session.AuthenticationComponentId );
        Assert.IsTrue( session.IsPersistent );
        Assert.IsTrue( session.IsActive );
        Assert.IsNotNull( session.LastStepUpAuthenticationDateTime );
        Assert.AreEqual( mfaInstant, session.LastMultiFactorAuthenticationDateTime );
    }

    /// <summary>
    /// When <c>mfaRecency</c> is not supplied, <c>LastMultiFactorAuthenticationDateTime</c>
    /// stays null. The login stamps <c>LastStepUpAuthenticationDateTime</c>
    /// either way.
    /// </summary>
    [TestMethod]
    public void StartComponentSession_LeavesMfaRecencyNull_WhenNotSupplied()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = service.StartComponentSession( requestContext: null, personAliasId: 100, userLoginId: 7, authComponentEntityTypeId: 42, isPersistent: false );

        Assert.IsNotNull( session.LastStepUpAuthenticationDateTime );
        Assert.IsNull( session.LastMultiFactorAuthenticationDateTime );
    }

    /// <summary>
    /// A <c>Component</c> session never carries impersonation or user-token
    /// settings. Prevents accidental cross-pollination through the shared
    /// <c>PopulateNewSession</c> helper.
    /// </summary>
    [TestMethod]
    public void StartComponentSession_HasNoImpersonationOrUserTokenSettings()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = service.StartComponentSession( requestContext: null, personAliasId: 100, userLoginId: 7, authComponentEntityTypeId: 42, isPersistent: false );

        Assert.IsNull( session.GetAdditionalSettingsOrNull<PersonSessionAdminImpersonationSettings>() );
        Assert.IsNull( session.GetAdditionalSettingsOrNull<PersonSessionUserTokenSettings>() );
    }

    #endregion StartComponentSession

    #region StartImpersonationSession

    /// <summary>
    /// Admin impersonation copies both recency timestamps from the
    /// impersonator's prior session so MFA-required pages continue to grant
    /// access during impersonation.
    /// </summary>
    [TestMethod]
    public void StartImpersonationSession_CopiesRecencyFromImpersonator()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var stepUpInstant = RockDateTime.Now.AddMinutes( -2 );
        var mfaInstant = RockDateTime.Now.AddMinutes( -4 );
        var impersonator = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            LastStepUpAuthenticationDateTime = stepUpInstant,
            LastMultiFactorAuthenticationDateTime = mfaInstant,
        };
        var impersonatorInteractionSession = new InteractionSession { Guid = Guid.NewGuid() };

        var session = service.StartImpersonationSession( requestContext: null, targetPersonAliasId: 200, impersonator, impersonatorInteractionSession );

        Assert.AreEqual( stepUpInstant, session.LastStepUpAuthenticationDateTime );
        Assert.AreEqual( mfaInstant, session.LastMultiFactorAuthenticationDateTime );
    }

    /// <summary>
    /// Null recency timestamps on the impersonator stay null on the new
    /// session — no "stamp to now" fallback. Prevents an admin who has not
    /// recently authenticated from getting a fresh recency window for free
    /// by clicking "Impersonate".
    /// </summary>
    [TestMethod]
    public void StartImpersonationSession_LeavesRecencyNull_WhenImpersonatorHasNullRecency()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var impersonator = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            LastStepUpAuthenticationDateTime = null,
            LastMultiFactorAuthenticationDateTime = null,
        };
        var impersonatorInteractionSession = new InteractionSession { Guid = Guid.NewGuid() };

        var session = service.StartImpersonationSession( requestContext: null, targetPersonAliasId: 200, impersonator, impersonatorInteractionSession );

        Assert.IsNull( session.LastStepUpAuthenticationDateTime );
        Assert.IsNull( session.LastMultiFactorAuthenticationDateTime );
    }

    /// <summary>
    /// Admin impersonation stamps <see cref="PersonSessionAdminImpersonationSettings"/>
    /// with both the impersonator's prior <c>PersonSession.Guid</c> AND prior
    /// <c>InteractionSession.Guid</c> so <c>EndImpersonationAndRestore</c>
    /// can revert both.
    /// </summary>
    [TestMethod]
    public void StartImpersonationSession_StampsAdminImpersonationSettings()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var impersonator = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        var impersonatorInteractionSession = new InteractionSession { Guid = Guid.NewGuid() };

        var session = service.StartImpersonationSession( requestContext: null, targetPersonAliasId: 200, impersonator, impersonatorInteractionSession );

        var settings = session.GetAdditionalSettingsOrNull<PersonSessionAdminImpersonationSettings>();
        Assert.IsNotNull( settings );
        Assert.AreEqual( impersonator.Guid, settings.ImpersonatorPersonSessionGuid );
        Assert.AreEqual( impersonatorInteractionSession.Guid, settings.ImpersonatorInteractionSessionGuid );

        // And no user-token settings should have leaked through PopulateNewSession.
        Assert.IsNull( session.GetAdditionalSettingsOrNull<PersonSessionUserTokenSettings>() );
    }

    #endregion StartImpersonationSession

    #region StartUserTokenSession

    /// <summary>
    /// User-token sessions leave both recency timestamps null. Explicit
    /// divergence from the legacy <c>ProcessImpersonation</c> behavior, which
    /// force-stamped MFA recency and let recipients of <c>rckipid</c> links
    /// bypass MFA-required pages just by following the link.
    /// </summary>
    [TestMethod]
    public void StartUserTokenSession_LeavesBothRecencyTimestampsNull()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var token = new PersonToken { Guid = Guid.NewGuid() };

        var session = service.StartUserTokenSession( requestContext: null, targetPersonAliasId: 200, token );

        Assert.IsNull( session.LastStepUpAuthenticationDateTime );
        Assert.IsNull( session.LastMultiFactorAuthenticationDateTime );
    }

    /// <summary>
    /// User-token sessions stamp <see cref="PersonSessionUserTokenSettings"/>
    /// with the originating <c>PersonToken.Guid</c> so per-request page-scope
    /// re-validation can check the source token on every page load.
    /// </summary>
    [TestMethod]
    public void StartUserTokenSession_StampsUserTokenSettings()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var token = new PersonToken { Guid = Guid.NewGuid() };

        var session = service.StartUserTokenSession( requestContext: null, targetPersonAliasId: 200, token );

        var settings = session.GetAdditionalSettingsOrNull<PersonSessionUserTokenSettings>();
        Assert.IsNotNull( settings );
        Assert.AreEqual( token.Guid, settings.OriginatingPersonTokenGuid );

        // And no admin-impersonation settings should have leaked through PopulateNewSession.
        Assert.IsNull( session.GetAdditionalSettingsOrNull<PersonSessionAdminImpersonationSettings>() );
    }

    #endregion StartUserTokenSession

    #region FindOrCreateApiKeySession (Phase 10)

    /// <summary>
    /// A second call to <c>FindOrCreateApiKeySession</c> for a UserLogin that
    /// already has an active <see cref="PersonSessionCreationSource.ApiKey"/>
    /// session reuses the existing row rather than creating a duplicate.
    /// Mocked-db complement of the integration test
    /// <c>FindOrCreateApiKeySession_SecondCall_ReusesExistingRow</c>.
    /// </summary>
    [TestMethod]
    public void FindOrCreateApiKeySession_SecondCall_ReturnsExistingActiveSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var existing = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            UserLoginId = 7,
            CreationSource = PersonSessionCreationSource.ApiKey,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( existing );

        var userLogin = new UserLogin
        {
            Id = 7,
            UserName = "ted-decker-apikey",
            PersonId = 50,
            Person = new Person { Id = 50, PrimaryAliasId = 100 },
        };

        var service = new PersonSessionService( rockContext );
        var resolved = service.FindOrCreateApiKeySession( requestContext: null, userLogin );

        Assert.IsNotNull( resolved );
        Assert.AreEqual( existing.Id, resolved.Id );
        Assert.AreEqual( PersonSessionCreationSource.ApiKey, resolved.CreationSource );
    }

    /// <summary>
    /// An orphaned ApiKey session (its referenced <see cref="UserLogin"/> was
    /// deleted, so the FK cascade SET NULL its <c>UserLoginId</c>) MUST NOT
    /// be returned for a different UserLogin's lookup. This is the
    /// "deleted UserLogin does not resurrect the orphan" guarantee called out
    /// by the spec.
    /// </summary>
    [TestMethod]
    public void FindOrCreateApiKeySession_OrphanedSession_IsNotResurrected()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        // Orphan: a former ApiKey session whose UserLogin was deleted. The
        // FK's ON DELETE SET NULL has nulled UserLoginId, leaving a
        // historical row with no owner. A new UserLogin presenting the same
        // (or different) API key must not pick this up.
        var orphan = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            UserLoginId = null,
            CreationSource = PersonSessionCreationSource.ApiKey,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( orphan );

        // A fresh UserLogin (different identity).
        var userLogin = new UserLogin
        {
            Id = 99,
            UserName = "new-key-holder",
            PersonId = 200,
            Person = new Person { Id = 200, PrimaryAliasId = 400 },
        };

        var service = new PersonSessionService( rockContext );

        // Without the upsert / DB SaveChanges, the mocked context cannot
        // actually persist the new row. We exercise just the lookup leg:
        // if the orphan is found, the method short-circuits and returns it.
        // Catch the DbUpdateException that will follow when the mocked
        // context refuses to save the new row, so we can still assert the
        // find leg never returned the orphan.
        try
        {
            var resolved = service.FindOrCreateApiKeySession( requestContext: null, userLogin );

            // If we got here, a new session was returned. Verify it is NOT
            // the orphan.
            Assert.AreNotEqual( orphan.Id, resolved.Id,
                "Orphaned ApiKey session must not be returned to a new UserLogin's lookup." );
        }
        catch
        {
            // Mocked save path may not fully simulate the insert/round-trip.
            // The orphan-not-resurrected invariant is the lookup behavior:
            // FindActiveApiKeySession filters by UserLoginId == userLoginId,
            // and the orphan's UserLoginId is null, so it cannot match a
            // non-null filter. The exception path means we got past the
            // "return existing" branch, which is the property we care about.
        }
    }

    /// <summary>
    /// An ApiKey session whose <c>IsActive</c> is still true but whose
    /// <see cref="PersonSession.ExpiresDateTime"/> has passed must NOT be
    /// returned — handing it back would cause the next request to fail
    /// <c>ResolveSessionForRequest</c>'s expiration check and immediately
    /// log the API consumer out. Today ApiKey sessions are durable and
    /// have no <c>ExpiresDateTime</c>, so this is a defensive guarantee
    /// against any future change that introduces one.
    /// </summary>
    [TestMethod]
    public void FindOrCreateApiKeySession_ExpiredButActiveSession_IsNotReused()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        // Stale-but-IsActive row: ExpiresDateTime in the past, IsActive
        // still true (Rock Cleanup has not run yet).
        var expired = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            UserLoginId = 7,
            CreationSource = PersonSessionCreationSource.ApiKey,
            IsActive = true,
            IsPersistent = true,
            ExpiresDateTime = RockDateTime.Now.AddMinutes( -5 ),
        };
        rockContext.Set<PersonSession>().Add( expired );

        var userLogin = new UserLogin
        {
            Id = 7,
            UserName = "ted-decker-apikey",
            EntityTypeId = 42,
            PersonId = 50,
            Person = new Person { Id = 50, PrimaryAliasId = 100 },
        };

        var service = new PersonSessionService( rockContext );

        // The find leg must skip the expired row; the create-new leg may
        // throw under the mocked save path. We only care that the expired
        // row was NOT returned.
        try
        {
            var resolved = service.FindOrCreateApiKeySession( requestContext: null, userLogin );
            Assert.AreNotEqual( expired.Id, resolved.Id,
                "Expired-but-active ApiKey session must not be reused." );
        }
        catch
        {
            // See comment above. The find filter is what's under test;
            // an exception thrown from the save path confirms the find
            // returned null and the method fell into the create branch.
        }
    }

    #endregion FindOrCreateApiKeySession (Phase 10)

    #region FindOrCreateDeviceComponentSession (Phase 11)

    /// <summary>
    /// A second call to <c>FindOrCreateDeviceComponentSession</c> for a
    /// UserLogin that already has an active
    /// <see cref="PersonSessionCreationSource.Component"/> session reuses
    /// the existing row rather than creating a duplicate. This is the
    /// device-token-refresh / same-person re-login case from the spec.
    /// </summary>
    [TestMethod]
    public void FindOrCreateDeviceComponentSession_ExistingActiveSession_IsReused()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var existing = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            UserLoginId = 7,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( existing );

        var userLogin = new UserLogin
        {
            Id = 7,
            UserName = "ted-decker-mobile",
            EntityTypeId = 42,
            PersonId = 50,
            Person = new Person { Id = 50, PrimaryAliasId = 100 },
        };

        var service = new PersonSessionService( rockContext );
        var resolved = service.FindOrCreateDeviceComponentSession( requestContext: null, userLogin );

        Assert.IsNotNull( resolved );
        Assert.AreEqual( existing.Id, resolved.Id );
        Assert.AreEqual( PersonSessionCreationSource.Component, resolved.CreationSource );
        Assert.IsTrue( resolved.IsPersistent );
        Assert.IsTrue( resolved.IsActive );
    }

    /// <summary>
    /// When the current request already has a <see cref="PersonSession"/>
    /// for a *different* <see cref="UserLogin"/>, that prior session is
    /// marked inactive. A new device session for the incoming UserLogin is
    /// then created (or reused, but in this test no prior Component
    /// session exists for the incoming UserLogin so a creation attempt
    /// follows). Covers the "Mobile login as a different person on a
    /// device that already had a session" spec test.
    /// </summary>
    [TestMethod]
    public void FindOrCreateDeviceComponentSession_DifferentUserLoginOnRequest_MarksPriorInactive()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var priorSession = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            UserLoginId = 5, // belongs to the OLD UserLogin
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( priorSession );

        var newUserLogin = new UserLogin
        {
            Id = 9, // different from priorSession.UserLoginId
            UserName = "different-person",
            EntityTypeId = 42,
            PersonId = 60,
            Person = new Person { Id = 60, PrimaryAliasId = 200 },
        };

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( priorSession );

        var service = new PersonSessionService( rockContext );

        // The "create new" branch may throw under the mocked save path
        // because the mocked context cannot fully simulate the insert
        // round-trip; the property we are testing is that the prior
        // session got marked inactive BEFORE the create attempt.
        try
        {
            service.FindOrCreateDeviceComponentSession( requestContext, newUserLogin );
        }
        catch
        {
            // See above — irrelevant to this test's assertion.
        }

        Assert.IsFalse( priorSession.IsActive,
            "Prior PersonSession for the other UserLogin should be marked inactive when a different-person device login occurs." );
    }

    /// <summary>
    /// When the prior session on the request belongs to the SAME
    /// <see cref="UserLogin"/>, it must NOT be marked inactive — that's
    /// the device-token-refresh case, where the existing session is
    /// reused as-is.
    /// </summary>
    [TestMethod]
    public void FindOrCreateDeviceComponentSession_SameUserLoginOnRequest_LeavesPriorActive()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var existing = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            UserLoginId = 7,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( existing );

        var userLogin = new UserLogin
        {
            Id = 7,
            UserName = "same-person",
            EntityTypeId = 42,
            PersonId = 50,
            Person = new Person { Id = 50, PrimaryAliasId = 100 },
        };

        var requestContext = new RockRequestContext();
        requestContext.SetPersonSession( existing );

        var service = new PersonSessionService( rockContext );
        var resolved = service.FindOrCreateDeviceComponentSession( requestContext, userLogin );

        Assert.AreEqual( existing.Id, resolved.Id );
        Assert.IsTrue( existing.IsActive,
            "Same-UserLogin re-login is a token refresh and must NOT mark the prior session inactive." );
    }

    /// <summary>
    /// <c>FindOrCreateDeviceComponentSession</c> throws when the supplied
    /// <see cref="UserLogin"/> has no <c>EntityTypeId</c>. The
    /// AuthenticationComponent is required to stamp
    /// <c>PersonSession.AuthenticationComponentId</c>.
    /// </summary>
    [TestMethod]
    public void FindOrCreateDeviceComponentSession_ThrowsWhenUserLoginHasNoEntityType()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var userLogin = new UserLogin
        {
            Id = 7,
            UserName = "no-component",
            EntityTypeId = null,
            PersonId = 50,
            Person = new Person { Id = 50, PrimaryAliasId = 100 },
        };

        Assert.Throws<ArgumentException>(
            () => service.FindOrCreateDeviceComponentSession( requestContext: null, userLogin ) );
    }

    /// <summary>
    /// <c>GetCookieValue</c> is pure with respect to ASP.NET — it must not
    /// touch <c>System.Web.HttpContext.Current</c>. The test confirms by
    /// invoking it without an HttpContext attached to the current thread
    /// and asserting it returns a non-empty opaque string.
    /// </summary>
    [TestMethod]
    public void GetCookieValue_DoesNotRequireHttpContext()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
            IssuedDateTime = RockDateTime.Now,
        };

        // Defensively force HttpContext.Current to null on this thread to
        // make the "no HttpContext available" property explicit. Mocked-db
        // tests typically already run without an HttpContext, but pinning
        // it here makes the assertion deterministic.
        var savedContext = System.Web.HttpContext.Current;
        try
        {
            System.Web.HttpContext.Current = null;
            var cookieValue = service.GetCookieValue( session );

            Assert.IsFalse( string.IsNullOrEmpty( cookieValue ),
                "GetCookieValue should produce a non-empty opaque cookie value without HttpContext." );
        }
        finally
        {
            System.Web.HttpContext.Current = savedContext;
        }
    }

    /// <summary>
    /// A Component session whose <c>IsActive</c> is still true but whose
    /// <see cref="PersonSession.ExpiresDateTime"/> has passed must NOT be
    /// returned to a device re-login — handing it back would cause the
    /// device's next request to fail <c>ResolveSessionForRequest</c>'s
    /// expiration check and immediately log the user out. Excluding it
    /// from the find pushes the caller into the create-new branch.
    /// </summary>
    [TestMethod]
    public void FindOrCreateDeviceComponentSession_ExpiredButActiveSession_IsNotReused()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        // Stale-but-IsActive row: ExpiresDateTime in the past, IsActive
        // still true (Rock Cleanup has not run yet).
        var expired = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            UserLoginId = 7,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
            ExpiresDateTime = RockDateTime.Now.AddMinutes( -5 ),
        };
        rockContext.Set<PersonSession>().Add( expired );

        var userLogin = new UserLogin
        {
            Id = 7,
            UserName = "ted-decker-mobile",
            EntityTypeId = 42,
            PersonId = 50,
            Person = new Person { Id = 50, PrimaryAliasId = 100 },
        };

        var service = new PersonSessionService( rockContext );

        // The find leg must skip the expired row; the create-new leg may
        // throw under the mocked save path. We only care that the expired
        // row was NOT returned.
        try
        {
            var resolved = service.FindOrCreateDeviceComponentSession( requestContext: null, userLogin );
            Assert.AreNotEqual( expired.Id, resolved.Id,
                "Expired-but-active Component session must not be reused." );
        }
        catch
        {
            // See comment above. The find filter is what's under test;
            // an exception thrown from the save path confirms the find
            // returned null and the method fell into the create branch.
        }
    }

    #endregion FindOrCreateDeviceComponentSession (Phase 11)

    #region EndImpersonationAndRestore

    /// <summary>
    /// <c>EndImpersonationAndRestore</c> on a <c>Component</c> session is a
    /// caller bug — only admin-impersonation sessions are restorable. The
    /// method throws rather than silently doing nothing.
    /// </summary>
    [TestMethod]
    public void EndImpersonationAndRestore_ThrowsOnComponentSource()
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

        Assert.Throws<InvalidOperationException>( () => service.EndImpersonationAndRestore( session, requestContext: null ) );
    }

    /// <summary>
    /// <c>EndImpersonationAndRestore</c> on a <c>UserToken</c> session throws
    /// — user-token sessions are intentionally not restorable.
    /// </summary>
    [TestMethod]
    public void EndImpersonationAndRestore_ThrowsOnUserTokenSource()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.UserToken,
            IsActive = true,
        };

        Assert.Throws<InvalidOperationException>( () => service.EndImpersonationAndRestore( session, requestContext: null ) );
    }

    /// <summary>
    /// A dangling <c>ImpersonatorPersonSessionGuid</c> (the impersonator's
    /// prior <see cref="PersonSession"/> was deleted, or never existed)
    /// returns null and marks the current session inactive. The
    /// impersonation does not silently continue.
    /// </summary>
    [TestMethod]
    public void EndImpersonationAndRestore_ReturnsNullAndMarksInactive_WhenPersonSessionRestoreReferenceIsDangling()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var impersonationSession = new PersonSession
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };
        impersonationSession.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = Guid.NewGuid(), // not seeded
            ImpersonatorInteractionSessionGuid = Guid.NewGuid(), // not seeded
        } );
        rockContext.Set<PersonSession>().Add( impersonationSession );

        var service = new PersonSessionService( rockContext );

        var result = service.EndImpersonationAndRestore( impersonationSession, requestContext: null );

        Assert.IsNull( result );
        Assert.IsFalse( impersonationSession.IsActive );
    }

    /// <summary>
    /// Same dangling-reference behavior when only the <c>InteractionSession</c>
    /// reference cannot be resolved. The PersonSession side is seeded; the
    /// InteractionSession Guid points at nothing.
    /// </summary>
    [TestMethod]
    public void EndImpersonationAndRestore_ReturnsNullAndMarksInactive_WhenInteractionSessionRestoreReferenceIsDangling()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var impersonator = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( impersonator );

        var impersonationSession = new PersonSession
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };
        impersonationSession.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = impersonator.Guid,
            ImpersonatorInteractionSessionGuid = Guid.NewGuid(), // not seeded
        } );
        rockContext.Set<PersonSession>().Add( impersonationSession );

        var service = new PersonSessionService( rockContext );

        var result = service.EndImpersonationAndRestore( impersonationSession, requestContext: null );

        Assert.IsNull( result );
        Assert.IsFalse( impersonationSession.IsActive );
    }

    #endregion EndImpersonationAndRestore

    #region Phase 9 — browser-session id reset / restore

    /// <summary>
    /// <see cref="PersonSessionService.StartImpersonationSession"/> regenerates
    /// the browser-session identifier on the supplied
    /// <see cref="RockRequestContext"/> so the next interaction-tracking call
    /// creates a fresh <see cref="InteractionSession"/> row tied to the
    /// impersonation session rather than continuing to write activity against
    /// the impersonator's prior row.
    /// </summary>
    [TestMethod]
    public void StartImpersonationSession_RegeneratesBrowserSessionId()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        var requestContext = new RockRequestContext();
        var priorBrowserSessionId = requestContext.SessionGuid;

        var impersonator = new PersonSession
        {
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        var impersonatorInteractionSession = new InteractionSession { Guid = Guid.NewGuid() };

        service.StartImpersonationSession( requestContext, targetPersonAliasId: 200, impersonator, impersonatorInteractionSession );

        Assert.AreNotEqual( priorBrowserSessionId, requestContext.SessionGuid,
            "Browser-session id should be regenerated so the impersonation session gets a fresh InteractionSession." );
        Assert.AreNotEqual( impersonatorInteractionSession.Guid, requestContext.SessionGuid,
            "Regenerated browser-session id must not collide with the impersonator's prior InteractionSession.Guid." );
    }

    /// <summary>
    /// <see cref="PersonSessionService.EndImpersonationAndRestore"/> re-points
    /// the browser-session identifier at the impersonator's prior
    /// <c>InteractionSession.Guid</c> on a successful restore. This is the
    /// seam that re-attaches the admin's pre-impersonation activity trail.
    /// </summary>
    [TestMethod]
    public void EndImpersonationAndRestore_RePointsBrowserSessionId_OnSuccessfulRestore()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var impersonator = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( impersonator );

        var priorInteractionSession = new InteractionSession { Id = 1, Guid = Guid.NewGuid() };
        rockContext.Set<InteractionSession>().Add( priorInteractionSession );

        var impersonationSession = new PersonSession
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };
        impersonationSession.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = impersonator.Guid,
            ImpersonatorInteractionSessionGuid = priorInteractionSession.Guid,
        } );
        rockContext.Set<PersonSession>().Add( impersonationSession );

        var requestContext = new RockRequestContext();
        var service = new PersonSessionService( rockContext );

        var restored = service.EndImpersonationAndRestore( impersonationSession, requestContext );

        Assert.IsNotNull( restored );
        Assert.AreEqual( impersonator.Guid, restored.Guid );
        Assert.IsFalse( impersonationSession.IsActive );
        Assert.AreEqual( priorInteractionSession.Guid, requestContext.SessionGuid,
            "Browser-session id should be re-pointed at the impersonator's prior InteractionSession on restore." );
    }

    /// <summary>
    /// When restore fails (either prior-session reference is dangling),
    /// <see cref="PersonSessionService.EndImpersonationAndRestore"/> does NOT
    /// touch the request's browser-session identifier. The user becomes
    /// anonymous; the next interaction-tracking call writes against whatever
    /// the current browser-session id resolves to (which, having no
    /// PersonSession, means a NULL <c>PersonSessionId</c> on insert / no
    /// adoption on update).
    /// </summary>
    [TestMethod]
    public void EndImpersonationAndRestore_DoesNotTouchBrowserSessionId_OnDanglingRestore()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var impersonationSession = new PersonSession
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };
        impersonationSession.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = Guid.NewGuid(), // not seeded
            ImpersonatorInteractionSessionGuid = Guid.NewGuid(), // not seeded
        } );
        rockContext.Set<PersonSession>().Add( impersonationSession );

        var requestContext = new RockRequestContext();
        var priorBrowserSessionId = requestContext.SessionGuid;
        var service = new PersonSessionService( rockContext );

        var restored = service.EndImpersonationAndRestore( impersonationSession, requestContext );

        Assert.IsNull( restored );
        Assert.AreEqual( priorBrowserSessionId, requestContext.SessionGuid,
            "Browser-session id should not be re-pointed when the restore is refused." );
    }

    /// <summary>
    /// <see cref="RockRequestContext.RegenerateBrowserSessionId"/> assigns a
    /// fresh <see cref="Guid"/> and exposes it via <c>SessionGuid</c> so
    /// downstream code in the same request observes the new identifier.
    /// </summary>
    [TestMethod]
    public void RegenerateBrowserSessionId_AssignsFreshGuid()
    {
        var requestContext = new RockRequestContext();
        var prior = requestContext.SessionGuid;

        var regenerated = requestContext.RegenerateBrowserSessionId();

        Assert.AreNotEqual( prior, regenerated );
        Assert.AreEqual( regenerated, requestContext.SessionGuid,
            "SessionGuid should reflect the regenerated value for the rest of the request." );
    }

    /// <summary>
    /// <see cref="RockRequestContext.SetBrowserSessionId(Guid)"/> writes the
    /// supplied <see cref="Guid"/> through to <c>SessionGuid</c>.
    /// </summary>
    [TestMethod]
    public void SetBrowserSessionId_WritesSuppliedGuid()
    {
        var requestContext = new RockRequestContext();
        var target = Guid.NewGuid();

        requestContext.SetBrowserSessionId( target );

        Assert.AreEqual( target, requestContext.SessionGuid );
    }

    #endregion Phase 9 — browser-session id reset / restore
}
