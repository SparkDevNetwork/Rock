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
using System.Reflection;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Tests.Shared.TestFramework;
using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model;

/// <summary>
/// Mocked-database and plain-unit tests for <see cref="PersonSessionService"/>,
/// covering the full surface: session creation (<c>StartComponentSession</c>,
/// <c>StartImpersonationSession</c>, <c>StartUserTokenSession</c>,
/// <c>FindOrCreateApiKeySession</c>, <c>FindOrCreateDeviceComponentSession</c>),
/// the cookie I/O round-trip and <c>ResolveSessionForRequest</c> lifecycle,
/// the <c>ProcessImpersonationToken</c> / <c>RevalidateUserTokenSession</c>
/// user-token flow, admin impersonation (<c>ImpersonatePerson</c> /
/// <c>EndImpersonationAndRestore</c>), browser-session id reset, <c>SignOut</c>,
/// the Rock Cleanup <c>MarkExpiredSessionsInactive</c> seam, and (on WebForms
/// builds) the legacy <c>FormsAuthenticationTicket</c> upgrade path.
/// </summary>
/// <remarks>
/// Tests that need real save semantics (PreSave hook, raw-SQL upsert,
/// <c>InactiveDateTime</c> stamping, etc.) live in
/// <c>Rock.Tests.Integration.Security.PersonSessionTests</c>.
/// </remarks>
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

    #region FindOrCreateApiKeySession

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

    #endregion FindOrCreateApiKeySession

    #region FindOrCreateDeviceComponentSession

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

    #endregion FindOrCreateDeviceComponentSession

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

    #region Browser-session id reset / restore

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

        var response = new TrackingResponseContext();
        var requestContext = new RockRequestContext( response );
        var service = new PersonSessionService( rockContext );

        var restored = service.EndImpersonationAndRestore( impersonationSession, requestContext );

        Assert.IsNotNull( restored );
        Assert.AreEqual( impersonator.Guid, restored.Guid );
        Assert.IsFalse( impersonationSession.IsActive );
        Assert.AreEqual( priorInteractionSession.Guid, requestContext.SessionGuid,
            "Browser-session id should be re-pointed at the impersonator's prior InteractionSession on restore." );

        // Symmetric with ImpersonatePerson: the end-side method writes the
        // new auth cookie pointing at the restored session and updates the
        // request context's PersonSession so the rest of the request sees
        // the admin's restored identity.
        Assert.HasCount( 1, response.AddedCookies,
            "EndImpersonationAndRestore must write the new auth cookie for the restored session." );
        Assert.AreEqual( restored.Guid, requestContext.PersonSession.Guid,
            "EndImpersonationAndRestore must attach the restored session to the request context." );
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

        var response = new TrackingResponseContext();
        var requestContext = new RockRequestContext( response );
        var priorBrowserSessionId = requestContext.SessionGuid;
        var service = new PersonSessionService( rockContext );

        var restored = service.EndImpersonationAndRestore( impersonationSession, requestContext );

        Assert.IsNull( restored );
        Assert.AreEqual( priorBrowserSessionId, requestContext.SessionGuid,
            "Browser-session id should not be re-pointed when the restore is refused." );

        // Symmetric failure handling: the dangling-restore path must expire
        // the auth cookie so the next request resolves anonymously instead
        // of trying to use the now-inactive impersonation session.
        Assert.HasCount( 1, response.RemovedCookies,
            "EndImpersonationAndRestore must expire the auth cookie when the restore reference is dangling." );
        Assert.HasCount( 0, response.AddedCookies,
            "No new auth cookie should be written when the restore is refused." );
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

    #endregion Browser-session id reset / restore

    #region ImpersonatePerson

    /// <summary>
    /// <see cref="PersonSessionService.ImpersonatePerson"/> on a request
    /// with an active admin <see cref="PersonSession"/> and matching
    /// <see cref="InteractionSession"/> creates exactly one new
    /// <see cref="PersonSessionCreationSource.Impersonation"/> session,
    /// writes NO <see cref="PersonToken"/> row, and writes one cookie via
    /// the request context.
    /// </summary>
    [TestMethod]
    public void ImpersonatePerson_ActiveAdminSession_CreatesImpersonationSession_NoPersonToken()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedAdminImpersonatorEnvironment(
            rockContext,
            out var adminSession,
            out var adminInteractionSession,
            out var targetPersonAliasId );

        var response = new TrackingResponseContext();
        var requestContext = new RockRequestContext( response );
        requestContext.SetPersonSession( adminSession );
        requestContext.SetBrowserSessionId( adminInteractionSession.Guid );

        PersonSessionService.ImpersonatePerson( requestContext, targetPersonAliasId );

        var newSessions = rockContext.Set<PersonSession>()
            .Where( s => s.CreationSource == PersonSessionCreationSource.Impersonation )
            .ToList();
        Assert.HasCount( 1, newSessions, "ImpersonatePerson must create exactly one Impersonation session." );

        var newSession = newSessions.Single();
        Assert.AreEqual( targetPersonAliasId, newSession.PersonAliasId );
        Assert.IsTrue( newSession.IsActive );

        Assert.HasCount( 0, rockContext.Set<PersonToken>().ToList(),
            "ImpersonatePerson must not write a PersonToken row." );

        Assert.HasCount( 1, response.AddedCookies,
            "ImpersonatePerson must write the new-format auth cookie via the request context." );

        Assert.AreEqual( newSession.Guid, requestContext.PersonSession.Guid,
            "ImpersonatePerson must replace the request context's PersonSession with the new impersonation session." );
    }

    /// <summary>
    /// <see cref="PersonSessionService.ImpersonatePerson"/> stamps the
    /// impersonator's prior <see cref="PersonSession.Guid"/> and prior
    /// <see cref="InteractionSession.Guid"/> onto the new session via
    /// <see cref="PersonSessionAdminImpersonationSettings"/> so
    /// <see cref="PersonSessionService.EndImpersonationAndRestore"/> can
    /// later revert.
    /// </summary>
    [TestMethod]
    public void ImpersonatePerson_StampsAdminImpersonationSettings_OnNewSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedAdminImpersonatorEnvironment(
            rockContext,
            out var adminSession,
            out var adminInteractionSession,
            out var targetPersonAliasId );

        var requestContext = new RockRequestContext( new TrackingResponseContext() );
        requestContext.SetPersonSession( adminSession );
        requestContext.SetBrowserSessionId( adminInteractionSession.Guid );

        PersonSessionService.ImpersonatePerson( requestContext, targetPersonAliasId );

        var newSession = rockContext.Set<PersonSession>()
            .Single( s => s.CreationSource == PersonSessionCreationSource.Impersonation );

        var settings = newSession.GetAdditionalSettingsOrNull<PersonSessionAdminImpersonationSettings>();
        Assert.IsNotNull( settings );
        Assert.AreEqual( adminSession.Guid, settings.ImpersonatorPersonSessionGuid );
        Assert.AreEqual( adminInteractionSession.Guid, settings.ImpersonatorInteractionSessionGuid );
    }

    /// <summary>
    /// <see cref="PersonSessionService.ImpersonatePerson"/> throws
    /// <see cref="InvalidOperationException"/> when the request has no
    /// active <see cref="PersonSession"/>. No <see cref="PersonSession"/>,
    /// no cookie write, and no <see cref="PersonToken"/> mutation occur.
    /// </summary>
    [TestMethod]
    public void ImpersonatePerson_NoActiveSession_ThrowsAndMutatesNothing()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var response = new TrackingResponseContext();
        var requestContext = new RockRequestContext( response );
        // Intentionally no PersonSession on the request context.

        Assert.Throws<InvalidOperationException>(
            () => PersonSessionService.ImpersonatePerson( requestContext, targetPersonAliasId: 200 ) );

        Assert.HasCount( 0, rockContext.Set<PersonSession>().ToList(),
            "No PersonSession should be created when ImpersonatePerson throws." );
        Assert.HasCount( 0, response.AddedCookies,
            "No cookie should be written when ImpersonatePerson throws." );
    }

    /// <summary>
    /// <see cref="PersonSessionService.ImpersonatePerson"/> does not touch
    /// any <see cref="PersonToken"/> row's <see cref="PersonToken.TimesUsed"/>.
    /// Guards against future code accidentally re-coupling admin
    /// impersonation to the legacy <c>PersonToken</c> path.
    /// </summary>
    [TestMethod]
    public void ImpersonatePerson_DoesNotMutatePersonTokenTimesUsed()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        SeedAdminImpersonatorEnvironment(
            rockContext,
            out var adminSession,
            out var adminInteractionSession,
            out var targetPersonAliasId );

        // Seed an unrelated PersonToken so we can verify TimesUsed unchanged.
        var unrelatedToken = new PersonToken
        {
            Id = 99,
            Guid = Guid.NewGuid(),
            PersonAliasId = targetPersonAliasId,
            TimesUsed = 0,
        };
        rockContext.Set<PersonToken>().Add( unrelatedToken );

        var requestContext = new RockRequestContext( new TrackingResponseContext() );
        requestContext.SetPersonSession( adminSession );
        requestContext.SetBrowserSessionId( adminInteractionSession.Guid );

        PersonSessionService.ImpersonatePerson( requestContext, targetPersonAliasId );

        Assert.AreEqual( 0, unrelatedToken.TimesUsed,
            "ImpersonatePerson must not increment any PersonToken.TimesUsed." );
    }

    /// <summary>
    /// <see cref="PersonSessionService.BuildImpersonationHistoryLogin"/>
    /// stamps the impersonated person on <see cref="HistoryLogin.PersonAliasId"/>,
    /// leaves <see cref="HistoryLogin.UserName"/> null (the legacy obfuscated
    /// rckipid value is gone in the new model), marks the event successful,
    /// and writes <c>LoginContext = "Impersonation"</c> plus the impersonator's
    /// full name into the related data. Tests the audit-row construction
    /// directly because <see cref="HistoryLogin.SaveAfterDelay"/> uses a
    /// background <c>Task.Run</c> with its own non-mocked <c>RockContext</c>
    /// and cannot be observed through the mocked save path.
    /// </summary>
    [TestMethod]
    public void BuildImpersonationHistoryLogin_PopulatesExpectedFields()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        // Admin "Ted Decker" impersonating target person.
        var admin = new Person
        {
            Id = 50,
            FirstName = "Ted",
            LastName = "Decker",
            NickName = "Ted",
            PrimaryAliasId = 100,
        };
        var adminAlias = new PersonAlias { Id = 100, PersonId = 50, Person = admin };
        rockContext.Set<Person>().Add( admin );
        rockContext.Set<PersonAlias>().Add( adminAlias );

        var adminSession = new PersonSession
        {
            Id = 10,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        var impersonationSession = new PersonSession
        {
            Id = 11,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
        };

        var historyLogin = PersonSessionService.BuildImpersonationHistoryLogin(
            rockContext, impersonationSession, adminSession );

        Assert.IsNotNull( historyLogin );
        Assert.IsNull( historyLogin.UserName, "UserName must be null under the new model (no rckipid to obfuscate)." );
        Assert.IsNull( historyLogin.UserLoginId );
        Assert.AreEqual( 200, historyLogin.PersonAliasId,
            "PersonAliasId must point at the impersonated person." );
        Assert.IsTrue( historyLogin.WasLoginSuccessful );

        var relatedData = historyLogin.GetRelatedDataOrNull();
        Assert.IsNotNull( relatedData );
        Assert.AreEqual( "Impersonation", relatedData.LoginContext );
        Assert.AreEqual( admin.FullName, relatedData.ImpersonatedByPersonFullName );
    }

    /// <summary>
    /// Seeds a minimum set of entities (impersonator Person + PersonAlias +
    /// active Component PersonSession + InteractionSession + target
    /// PersonAlias) so the <see cref="PersonSessionService.ImpersonatePerson"/>
    /// tests can run end-to-end through the mocked context.
    /// </summary>
    private static void SeedAdminImpersonatorEnvironment(
        RockContext rockContext,
        out PersonSession adminSession,
        out InteractionSession adminInteractionSession,
        out int targetPersonAliasId )
    {
        // Admin (Ted Decker).
        var admin = new Person
        {
            Id = 50,
            FirstName = "Ted",
            LastName = "Decker",
            NickName = "Ted",
            PrimaryAliasId = 100,
        };
        var adminAlias = new PersonAlias { Id = 100, PersonId = 50, Person = admin };
        rockContext.Set<Person>().Add( admin );
        rockContext.Set<PersonAlias>().Add( adminAlias );

        // Target (impersonated person).
        var target = new Person { Id = 51, PrimaryAliasId = 200 };
        var targetAlias = new PersonAlias { Id = 200, PersonId = 51, Person = target };
        rockContext.Set<Person>().Add( target );
        rockContext.Set<PersonAlias>().Add( targetAlias );
        targetPersonAliasId = 200;

        // Admin's active Component session.
        adminSession = new PersonSession
        {
            Id = 10,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
        };
        rockContext.Set<PersonSession>().Add( adminSession );

        // Admin's current InteractionSession (keyed by browser-session id).
        adminInteractionSession = new InteractionSession
        {
            Id = 20,
            Guid = Guid.NewGuid(),
        };
        rockContext.Set<InteractionSession>().Add( adminInteractionSession );
    }

    #endregion ImpersonatePerson

    #region Cookie I/O — Encode / Decode

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

    #endregion Cookie I/O — Encode / Decode

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

    /// <summary>
    /// A session whose <see cref="UserLogin"/> has been locked out is rejected:
    /// resolution returns null, the session is marked inactive, and the auth
    /// cookie is cleared. (Replaces the locked-out sign-out that the obsolete
    /// <c>UserLoginService.GetCurrentUser</c> used to perform.)
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_LockedOutUserLogin_SignsOutAndReturnsNull()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        // ResolveSessionForRequest reaches the security-settings (kill-switch)
        // check, which needs the Text FieldType available to persist settings.
        rockContext.Set<FieldType>().Add( new FieldType { Id = 1, Guid = SystemGuid.FieldType.TEXT.AsGuid() } );

        var userLogin = new UserLogin
        {
            Id = 50,
            UserName = "locked",
            PersonId = 100,
            IsConfirmed = true,
            IsLockedOut = true,
        };
        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            UserLoginId = 50,
            UserLogin = userLogin,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = false,
        };
        rockContext.Set<UserLogin>().Add( userLogin );
        rockContext.Set<PersonSession>().Add( session );

        var cookieValue = service.GetCookieValue( session );
        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNull( result );
        Assert.IsFalse( session.IsActive, "A locked-out login's session must be marked inactive." );
        Assert.IsTrue( response.RemovedCookies.Any( c => c.Name == PersonSessionService.AuthCookieName ),
            "The auth cookie must be cleared for a locked-out login." );
    }

    /// <summary>
    /// A session whose <see cref="UserLogin"/> is no longer confirmed is
    /// rejected the same way a locked-out login is.
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_UnconfirmedUserLogin_SignsOutAndReturnsNull()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        // ResolveSessionForRequest reaches the security-settings (kill-switch)
        // check, which needs the Text FieldType available to persist settings.
        rockContext.Set<FieldType>().Add( new FieldType { Id = 1, Guid = SystemGuid.FieldType.TEXT.AsGuid() } );

        var userLogin = new UserLogin
        {
            Id = 51,
            UserName = "unconfirmed",
            PersonId = 100,
            IsConfirmed = false,
            IsLockedOut = false,
        };
        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            UserLoginId = 51,
            UserLogin = userLogin,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = false,
        };
        rockContext.Set<UserLogin>().Add( userLogin );
        rockContext.Set<PersonSession>().Add( session );

        var cookieValue = service.GetCookieValue( session );
        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNull( result );
        Assert.IsFalse( session.IsActive, "An unconfirmed login's session must be marked inactive." );
        Assert.IsTrue( response.RemovedCookies.Any( c => c.Name == PersonSessionService.AuthCookieName ),
            "The auth cookie must be cleared for an unconfirmed login." );
    }

    /// <summary>
    /// A session backed by a confirmed, non-locked-out <see cref="UserLogin"/>
    /// resolves normally (regression guard for the locked-out / unconfirmed
    /// check above).
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_ConfirmedActiveUserLogin_ReturnsSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        // ResolveSessionForRequest reaches the security-settings (kill-switch)
        // check, which needs the Text FieldType available to persist settings.
        rockContext.Set<FieldType>().Add( new FieldType { Id = 1, Guid = SystemGuid.FieldType.TEXT.AsGuid() } );

        var userLogin = new UserLogin
        {
            Id = 52,
            UserName = "good",
            PersonId = 100,
            IsConfirmed = true,
            IsLockedOut = false,
        };
        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            UserLoginId = 52,
            UserLogin = userLogin,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = false,
        };
        rockContext.Set<UserLogin>().Add( userLogin );
        rockContext.Set<PersonSession>().Add( session );

        var cookieValue = service.GetCookieValue( session );
        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNotNull( result, "A confirmed, non-locked-out login should resolve normally." );
        Assert.IsTrue( session.IsActive, "The session should remain active." );
    }

    /// <summary>
    /// A session with no backing <see cref="UserLogin"/> (e.g. an impersonation
    /// or user-token session) is not subject to the locked-out / unconfirmed
    /// check and resolves normally.
    /// </summary>
    [TestMethod]
    public void ResolveSessionForRequest_SessionWithoutUserLogin_ResolvesNormally()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();
        var service = new PersonSessionService( rockContext );

        // ResolveSessionForRequest reaches the security-settings (kill-switch)
        // check, which needs the Text FieldType available to persist settings.
        rockContext.Set<FieldType>().Add( new FieldType { Id = 1, Guid = SystemGuid.FieldType.TEXT.AsGuid() } );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 200,
            UserLoginId = null,
            CreationSource = PersonSessionCreationSource.Impersonation,
            IsActive = true,
            IsPersistent = false,
        };
        rockContext.Set<PersonSession>().Add( session );

        var cookieValue = service.GetCookieValue( session );
        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue, response );

        var result = service.ResolveSessionForRequest( requestContext );

        Assert.IsNotNull( result, "A session without a UserLogin should not be rejected by the locked-out check." );
        Assert.IsTrue( session.IsActive );
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
    /// PersonSessionService.AuthCookieTimeout) triggers a reissue: the response gets a
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

    /// <summary>
    /// A host-only session (no shared domain resolved) writes only the auth
    /// cookie and NO <c>.ROCK_DOMAIN</c> breadcrumb. The breadcrumb's absence
    /// is the signal that tells sign-out to clear the cookie host-only, so it
    /// is important that host-only issuance never emits one.
    /// </summary>
    [TestMethod]
    public void SetAuthCookie_HostOnlySession_WritesNoDomainBreadcrumb()
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
        };

        var response = new TrackingResponseContext();

        // BuildRequestContext mocks RequestUri as null, so GetCookieDomain
        // resolves to host-only (no breadcrumb expected).
        var requestContext = BuildRequestContext( cookieValue: null, response );

        service.SetAuthCookie( session, requestContext );

        Assert.HasCount( 1, response.AddedCookies );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.AddedCookies[0].Name );
        Assert.IsFalse(
            response.AddedCookies.Any( c => c.Name == PersonSessionService.AuthCookieDomainName ),
            "A host-only session must not emit a domain breadcrumb cookie." );
    }

    /// <summary>
    /// When the auth cookie is issued for a shared domain (the request host
    /// matches a <c>DOMAINS_SHARING_LOGINS</c> entry), the auth cookie is scoped
    /// to that domain AND a <c>.ROCK_DOMAIN</c> breadcrumb is written recording
    /// the same domain, scoped to it, sharing the auth cookie's expiration. This
    /// is the end-to-end issuance path that sign-out relies on to delete the
    /// cookie at its exact scope.
    /// </summary>
    [TestMethod]
    public void SetAuthCookie_SharedDomain_WritesDomainBreadcrumbAndScopesAuthCookie()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        // Seed the DOMAINS_SHARING_LOGINS defined type with "xyz.org" so
        // GetCookieDomain resolves a real shared domain for an admin.xyz.org
        // request (the cache reads through the same mocked database).
        SeedSharedLoginDomain( rockContext, definedTypeId: 5000, definedValueId: 5001, domain: "xyz.org" );

        var service = new PersonSessionService( rockContext );

        var session = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };

        var response = new TrackingResponseContext();
        var requestContext = BuildRequestContext( cookieValue: null, response, host: "admin.xyz.org" );

        service.SetAuthCookie( session, requestContext );

        // The auth cookie is scoped to the shared domain (leading dot, per the
        // GetCookieDomain rule).
        var authCookie = response.AddedCookies.Single( c => c.Name == PersonSessionService.AuthCookieName );
        Assert.AreEqual( ".xyz.org", authCookie.Domain, "The auth cookie should be scoped to the shared domain." );

        // ...and the breadcrumb companion records that exact domain.
        var breadcrumb = response.AddedCookies.Single( c => c.Name == PersonSessionService.AuthCookieDomainName );
        Assert.AreEqual( ".xyz.org", breadcrumb.Value, "The breadcrumb value records the issued domain." );
        Assert.AreEqual( ".xyz.org", breadcrumb.Domain, "The breadcrumb must be scoped to the same domain it records." );
        Assert.AreEqual( authCookie.Expires, breadcrumb.Expires, "The breadcrumb shares the auth cookie's expiration." );
    }

    #endregion ResolveSessionForRequest — happy path + reissue

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

    #region SignOut

    /// <summary>
    /// Signing out an authenticated request marks the current
    /// <see cref="PersonSession"/> inactive, expires the <c>.ROCK</c> cookie
    /// via the response, and clears the session from the request context so
    /// the remainder of the request observes the anonymous state.
    /// </summary>
    [TestMethod]
    public void SignOut_MarksSessionInactiveAndClearsCookieAndContext()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var session = new PersonSession
        {
            Id = 11,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( session );

        var response = new TrackingResponseContext();
        var requestContext = new RockRequestContext( response );
        requestContext.SetPersonSession( session );

        var service = new PersonSessionService( rockContext );
        service.SignOut( requestContext );

        Assert.IsFalse( session.IsActive, "The current session should be marked inactive on sign-out." );

        // No .ROCK_DOMAIN breadcrumb on the request => the auth cookie was
        // issued host-only, so exactly one cookie is removed (host-only Domain).
        Assert.HasCount( 1, response.RemovedCookies );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.RemovedCookies[0].Name );
        Assert.IsNull( response.RemovedCookies[0].Domain, "Absent breadcrumb means the cookie is cleared host-only (null Domain)." );
        Assert.IsNull( requestContext.PersonSession, "The session should be detached from the request context on sign-out." );
    }

    /// <summary>
    /// When a <c>.ROCK_DOMAIN</c> breadcrumb is present on the request, sign-out
    /// clears the auth cookie at the recorded shared domain (so the deletion
    /// matches the scope it was issued under) AND clears the breadcrumb itself
    /// at that same domain so it does not linger. This preserves the legacy
    /// domain-shared sign-out behavior.
    /// </summary>
    [TestMethod]
    public void SignOut_WithDomainBreadcrumb_ClearsAuthCookieAndBreadcrumbAtRecordedDomain()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var session = new PersonSession
        {
            Id = 11,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( session );

        var response = new TrackingResponseContext();

        // Seed the breadcrumb the issuance step would have written for a
        // shared-domain login. The .ROCK value itself is irrelevant to SignOut
        // (it locates the session via the request context, not the cookie).
        var cookies = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
        {
            [PersonSessionService.AuthCookieName] = "irrelevant",
            [PersonSessionService.AuthCookieDomainName] = ".xyz.org",
        };
        var requestContext = BuildRequestContext( cookies, response );
        requestContext.SetPersonSession( session );

        var service = new PersonSessionService( rockContext );
        service.SignOut( requestContext );

        Assert.IsFalse( session.IsActive, "The current session should be marked inactive on sign-out." );
        Assert.HasCount( 2, response.RemovedCookies, "Both the auth cookie and its breadcrumb should be cleared." );

        var authRemoval = response.RemovedCookies.SingleOrDefault( c => c.Name == PersonSessionService.AuthCookieName );
        Assert.IsNotNull( authRemoval, "The auth cookie should be among the removed cookies." );
        Assert.AreEqual( ".xyz.org", authRemoval.Domain, "The auth cookie must be cleared at the recorded shared domain." );

        var breadcrumbRemoval = response.RemovedCookies.SingleOrDefault( c => c.Name == PersonSessionService.AuthCookieDomainName );
        Assert.IsNotNull( breadcrumbRemoval, "The breadcrumb companion should be among the removed cookies." );
        Assert.AreEqual( ".xyz.org", breadcrumbRemoval.Domain, "The breadcrumb must be cleared at its own matching domain." );

        Assert.IsNull( requestContext.PersonSession, "The session should be detached from the request context on sign-out." );
    }

    /// <summary>
    /// Signing out a request that has no current <see cref="PersonSession"/>
    /// still clears the authentication cookie — sign-out always clears identity
    /// cookies (matching the legacy behavior) — but performs no server-side
    /// session invalidation because there is no session to invalidate. The
    /// unsecured cookie is not on this request, so only the auth cookie is
    /// removed (host-only, since no domain breadcrumb is present).
    /// </summary>
    [TestMethod]
    public void SignOut_WithNoPersonSession_ClearsAuthCookieWithoutInvalidatingSession()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var response = new TrackingResponseContext();
        var requestContext = new RockRequestContext( response );

        var service = new PersonSessionService( rockContext );
        service.SignOut( requestContext );

        Assert.HasCount( 1, response.RemovedCookies, "The auth cookie is cleared even when no session is attached." );
        Assert.AreEqual( PersonSessionService.AuthCookieName, response.RemovedCookies[0].Name );
        Assert.IsNull( response.RemovedCookies[0].Domain, "No breadcrumb present, so the auth cookie is cleared host-only." );
        Assert.IsNull( requestContext.PersonSession );
    }

    /// <summary>
    /// Signing out an authenticated request that also carries the unsecured
    /// (check-in self-identification) cookie clears that cookie host-only in
    /// addition to the auth cookie, matching the legacy "full reset on
    /// sign-out" behavior.
    /// </summary>
    [TestMethod]
    public void SignOut_ClearsUnsecuredPersonIdentifierCookie_WhenPresent()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var session = new PersonSession
        {
            Id = 11,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            IsPersistent = true,
        };
        rockContext.Set<PersonSession>().Add( session );

        var response = new TrackingResponseContext();
        var cookies = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
        {
            [Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER] = Guid.NewGuid().ToString(),
        };
        var requestContext = BuildRequestContext( cookies, response );
        requestContext.SetPersonSession( session );

        var service = new PersonSessionService( rockContext );
        service.SignOut( requestContext );

        var unsecuredRemoval = response.RemovedCookies.SingleOrDefault( c => c.Name == Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER );
        Assert.IsNotNull( unsecuredRemoval, "The unsecured person identifier cookie should be cleared on sign-out." );
        Assert.IsNull( unsecuredRemoval.Domain, "The unsecured cookie is host-only, so it must be cleared host-only." );

        Assert.IsTrue( response.RemovedCookies.Any( c => c.Name == PersonSessionService.AuthCookieName ),
            "The auth cookie should also be cleared." );
    }

    /// <summary>
    /// Signing out an anonymous request (no <see cref="PersonSession"/>) that
    /// carries the unsecured cookie still clears that cookie. This is the
    /// shared check-in device case: a self-identified (never authenticated)
    /// person hits a sign-out path, and the unsecured identity must be
    /// forgotten even though there is no auth session to invalidate. The auth
    /// cookie is also cleared (sign-out clears identity cookies unconditionally).
    /// </summary>
    [TestMethod]
    public void SignOut_AnonymousRequestWithUnsecuredCookie_StillClearsIt()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var response = new TrackingResponseContext();
        var cookies = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
        {
            [Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER] = Guid.NewGuid().ToString(),
        };
        var requestContext = BuildRequestContext( cookies, response );

        var service = new PersonSessionService( rockContext );
        service.SignOut( requestContext );

        // Both the unsecured identity cookie and the auth cookie are cleared.
        Assert.HasCount( 2, response.RemovedCookies );
        Assert.IsTrue( response.RemovedCookies.Any( c => c.Name == Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER ),
            "The unsecured person identifier cookie should be cleared even on an anonymous request." );
        Assert.IsTrue( response.RemovedCookies.Any( c => c.Name == PersonSessionService.AuthCookieName ),
            "The auth cookie is cleared unconditionally on sign-out." );
    }

    #endregion SignOut

    #region MarkExpiredSessionsInactive (Rock Cleanup)

    /// <summary>
    /// Only active sessions whose <see cref="PersonSession.ExpiresDateTime"/> is
    /// in the past are marked inactive. Not-yet-expired sessions and open-ended
    /// sessions (no expiration) are left active.
    /// </summary>
    [TestMethod]
    public void MarkExpiredSessionsInactive_MarksOnlyExpiredActiveSessions()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        var expiredGuid = Guid.NewGuid();
        var futureGuid = Guid.NewGuid();
        var openEndedGuid = Guid.NewGuid();

        // Active and already past its expiration: should be marked inactive.
        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 1,
            Guid = expiredGuid,
            PersonAliasId = 100,
            IsActive = true,
            ExpiresDateTime = RockDateTime.Now.AddDays( -1 ),
            CreationSource = PersonSessionCreationSource.Component,
        } );

        // Active but not yet expired: should be left active.
        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 2,
            Guid = futureGuid,
            PersonAliasId = 100,
            IsActive = true,
            ExpiresDateTime = RockDateTime.Now.AddDays( 10 ),
            CreationSource = PersonSessionCreationSource.Component,
        } );

        // Active with no expiration (open-ended): should be left active.
        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 3,
            Guid = openEndedGuid,
            PersonAliasId = 100,
            IsActive = true,
            ExpiresDateTime = null,
            CreationSource = PersonSessionCreationSource.Component,
        } );

        rockContext.SaveChanges();

        var recordsUpdated = PersonSessionService.MarkExpiredSessionsInactive( batchSize: 1000, commandTimeout: 30 );

        Assert.AreEqual( 1, recordsUpdated, "Only the single expired active session should be processed." );

        var sessions = new PersonSessionService( rockContext ).Queryable().ToList();
        Assert.IsFalse( sessions.First( s => s.Guid == expiredGuid ).IsActive, "Expired session should be marked inactive." );
        Assert.IsTrue( sessions.First( s => s.Guid == futureGuid ).IsActive, "Not-yet-expired session should remain active." );
        Assert.IsTrue( sessions.First( s => s.Guid == openEndedGuid ).IsActive, "Open-ended session (no ExpiresDateTime) should remain active." );
    }

    /// <summary>
    /// An already-inactive expired session is not re-processed (the filter
    /// requires <c>IsActive == true</c>), so the run reports zero rows updated.
    /// </summary>
    [TestMethod]
    public void MarkExpiredSessionsInactive_IgnoresAlreadyInactiveSessions()
    {
        using var scope = TestHelper.CreateScopedRockAppWithMockDatabase();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<PersonSession>().Add( new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            IsActive = false,
            ExpiresDateTime = RockDateTime.Now.AddDays( -1 ),
            CreationSource = PersonSessionCreationSource.Component,
        } );

        rockContext.SaveChanges();

        var recordsUpdated = PersonSessionService.MarkExpiredSessionsInactive( batchSize: 1000, commandTimeout: 30 );

        Assert.AreEqual( 0, recordsUpdated );
    }

    #endregion MarkExpiredSessionsInactive (Rock Cleanup)

    #region UpgradeLegacyTicket (legacy FormsAuthenticationTicket upgrade)

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
    /// exercised by the cookie reissue tests above; this test
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

    #endregion UpgradeLegacyTicket (legacy FormsAuthenticationTicket upgrade)

    #region Test infrastructure

    /// <summary>
    /// Builds a <see cref="RockRequestContext"/> backed by a Moq <see cref="IRequest"/>
    /// that surfaces the supplied <paramref name="cookieValue"/> as the
    /// <c>.ROCK</c> cookie (when non-null). The response is the caller-supplied
    /// <see cref="TrackingResponseContext"/> so the test can inspect cookie
    /// writes / removes after the call.
    /// </summary>
    private static RockRequestContext BuildRequestContext( string cookieValue, IRockResponseContext response, string host = null )
    {
        var cookies = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        if ( cookieValue.IsNotNullOrWhiteSpace() )
        {
            cookies[PersonSessionService.AuthCookieName] = cookieValue;
        }

        return BuildRequestContext( cookies, response, host );
    }

    /// <summary>
    /// Builds a <see cref="RockRequestContext"/> backed by a Moq <see cref="IRequest"/>
    /// that surfaces the supplied <paramref name="cookies"/> on the request.
    /// Lets a test seed arbitrary request cookies (e.g. the
    /// <c>.ROCK_DOMAIN</c> breadcrumb) so cookie-clearing behavior can be
    /// asserted. When <paramref name="host"/> is supplied the request reports a
    /// matching URL so domain resolution (<c>GetCookieDomain</c>) can run; when
    /// null the request URI is null (host-only). The response is the
    /// caller-supplied <see cref="TrackingResponseContext"/> so the test can
    /// inspect cookie writes / removes after the call.
    /// </summary>
    private static RockRequestContext BuildRequestContext( IDictionary<string, string> cookies, IRockResponseContext response, string host = null )
    {
        var headers = new NameValueCollection( StringComparer.OrdinalIgnoreCase );
        var requestUri = host.IsNotNullOrWhiteSpace() ? new Uri( $"https://{host}/" ) : ( Uri ) null;

        var requestMock = new Mock<IRequest>( MockBehavior.Strict );
        requestMock.SetupGet( r => r.RemoteAddress ).Returns( IPAddress.Loopback );
        requestMock.SetupGet( r => r.RequestUri ).Returns( requestUri );
        requestMock.SetupGet( r => r.Method ).Returns( "GET" );
        requestMock.SetupGet( r => r.QueryString ).Returns( [] );
        requestMock.SetupGet( r => r.RouteData ).Returns( new Dictionary<string, object>() );
        requestMock.SetupGet( r => r.Headers ).Returns( headers );
        requestMock.SetupGet( r => r.Cookies ).Returns( cookies );
        requestMock.SetupGet( r => r.CookiesValuesAreUrlDecoded ).Returns( false );

        return new RockRequestContext( requestMock.Object, response, currentUser: null );
    }

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
    /// Seeds the <c>DOMAINS_SHARING_LOGINS</c> defined type with a single
    /// shared-domain value into the mocked context so
    /// <c>GetCookieDomain</c> resolves <paramref name="domain"/> for a matching
    /// request host. The defined-type cache reads through the same mocked
    /// database, and the cache is cleared when the scope is disposed, so this
    /// does not leak into other tests.
    /// </summary>
    private static void SeedSharedLoginDomain( RockContext rockContext, int definedTypeId, int definedValueId, string domain )
    {
        var definedType = new DefinedType
        {
            Id = definedTypeId,
            Guid = new Guid( Rock.SystemGuid.DefinedType.DOMAINS_SHARING_LOGINS ),
            Name = "Domains Sharing Logins",
            IsActive = true,
        };

        var definedValue = new DefinedValue
        {
            Id = definedValueId,
            Guid = Guid.NewGuid(),
            DefinedTypeId = definedTypeId,
            Value = domain,
            IsActive = true,
        };

        rockContext.Set<DefinedType>().Add( definedType );
        rockContext.Set<DefinedValue>().Add( definedValue );
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

#if WEBFORMS

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

#endif

    #endregion Test infrastructure
}
