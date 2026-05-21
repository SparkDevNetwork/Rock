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

        Assert.Throws<InvalidOperationException>( () => service.EndImpersonationAndRestore( session ) );
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

        Assert.Throws<InvalidOperationException>( () => service.EndImpersonationAndRestore( session ) );
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

        var result = service.EndImpersonationAndRestore( impersonationSession );

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

        var result = service.EndImpersonationAndRestore( impersonationSession );

        Assert.IsNull( result );
        Assert.IsFalse( impersonationSession.IsActive );
    }

    #endregion EndImpersonationAndRestore
}
