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
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Net;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model;

public partial class PersonSessionService
{
    #region Constants

    /// <summary>
    /// How recently the person must have provided any credential (password,
    /// SMS, TOTP, etc.) for the session to report
    /// <see cref="AuthenticationStrength.Elevated"/>.
    /// </summary>
    private const int ElevatedWindowMinutes = 30;

    /// <summary>
    /// How recently the person must have provided MFA (primary credential plus
    /// a second factor, entered concurrently) for the session to report
    /// <see cref="AuthenticationStrength.MultiFactor"/>.
    /// </summary>
    private const int MultiFactorWindowMinutes = 60;

    /// <summary>
    /// Current schema version stamped into the <c>v</c> field of
    /// <see cref="PersonSessionCookiePayload"/>. Bumps <strong>only</strong>
    /// on breaking changes to existing field meanings. <c>ResolveSessionForRequest</c>
    /// reissues a fresh cookie when a presented cookie's <c>v</c> is older
    /// than this constant.
    /// </summary>
    internal const int CookiePayloadVersion = 1;

    #endregion Constants

    #region Auth Cookie Settings

    /// <summary>
    /// The configured <c>.ROCK</c> authentication cookie name.
    /// </summary>
    /// <remarks>
    /// The plan's "no <c>System.Web</c> in <c>PersonSessionService</c>" rule
    /// allows this single getter to read
    /// <c>System.Web.Security.FormsAuthentication.FormsCookieName</c> directly
    /// (fully qualified) so the rest of the service does not need a config
    /// abstraction yet. The <c>#if WEBFORMS</c> boundary is the seam the
    /// .NET Core port will swap when it lands. Use this property — not raw
    /// <c>System.Web</c> reads — anywhere else in the service that needs the
    /// cookie name.
    /// </remarks>
    internal static string AuthCookieName
    {
        get
        {
#if WEBFORMS
            return System.Web.Security.FormsAuthentication.FormsCookieName;
#else
            return ".ROCK";
#endif
        }
    }

    /// <summary>
    /// The configured forms-authentication timeout, used as the upper bound on
    /// the browser-side <c>Expires</c> attribute under the
    /// <c>MIN( PersonSession.ExpiresDateTime ?? MaxValue, Now + Timeout )</c>
    /// formula. Same <c>#if WEBFORMS</c> seam as <see cref="AuthCookieName"/>.
    /// Default (43200 minutes = 30 days) matches the Rock
    /// <c>web.config.example</c> default.
    /// </summary>
    internal static TimeSpan AuthCookieTimeout
    {
        get
        {
#if WEBFORMS
            return System.Web.Security.FormsAuthentication.Timeout;
#else
            return TimeSpan.FromMinutes( 43200 );
#endif
        }
    }

    #endregion Auth Cookie Settings

    #region Recency Thresholds

    /// <summary>
    /// The cutoff <see cref="DateTime"/> a session's
    /// <c>LastStepUpAuthenticationDateTime</c> must be at or after to report
    /// <see cref="AuthenticationStrength.Elevated"/>.
    /// </summary>
    /// <returns>The threshold <see cref="DateTime"/>.</returns>
    public static DateTime GetElevatedAuthenticationThreshold()
    {
        return RockDateTime.Now.AddMinutes( -ElevatedWindowMinutes );
    }

    /// <summary>
    /// The cutoff <see cref="DateTime"/> a session's
    /// <c>LastMultiFactorAuthenticationDateTime</c> must be at or after to
    /// report <see cref="AuthenticationStrength.MultiFactor"/>.
    /// </summary>
    /// <returns>The threshold <see cref="DateTime"/>.</returns>
    public static DateTime GetMultiFactorAuthenticationThreshold()
    {
        return RockDateTime.Now.AddMinutes( -MultiFactorWindowMinutes );
    }

    #endregion Recency Thresholds

    #region Session Creation

    /// <summary>
    /// Populates and returns a new <see cref="PersonSession"/> entity for the
    /// supplied person, stamped with the shared invariants every creation flow
    /// needs. Does NOT save; callers are expected to <c>Add</c> the result to
    /// the rock context and call <c>SaveChanges</c>.
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>, or <c>null</c> when no request is in scope.</param>
    /// <param name="personAliasId">The <c>PersonAlias.Id</c> that owns the session.</param>
    /// <param name="creationSource">The creation source to stamp on the new row.</param>
    /// <returns>A populated, unsaved <see cref="PersonSession"/>.</returns>
    private PersonSession PopulateNewSession( RockRequestContext requestContext, int personAliasId, PersonSessionCreationSource creationSource )
    {
        var now = RockDateTime.Now;

        return new PersonSession
        {
            PersonAliasId = personAliasId,
            IsActive = true,
            IsPersistent = false,
            IssuedDateTime = now,
            LastActivityDateTime = now,
            CreationSource = creationSource,
            InteractionDeviceTypeId = ResolveInteractionDeviceTypeId( requestContext ),
        };
    }

    /// <summary>
    /// Resolves the supplied <paramref name="requestContext"/>'s User-Agent to
    /// an <c>InteractionDeviceType.Id</c> via the same find-or-create
    /// pattern <see cref="InteractionService"/> uses. Returns null when
    /// <paramref name="requestContext"/> is null (background job, server-side
    /// test, etc.) or when the User-Agent cannot be parsed into the required
    /// browser / OS / client-type triple.
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>, or <c>null</c>.</param>
    /// <returns>The <c>InteractionDeviceType</c> row's <c>Id</c>, or <c>null</c>.</returns>
    private int? ResolveInteractionDeviceTypeId( RockRequestContext requestContext )
    {
        var clientInformation = requestContext?.ClientInformation;
        var browserInfo = clientInformation?.BrowserInfo;
        var deviceOs = browserInfo?.GetOSFamilyVersion();
        var deviceApplication = browserInfo?.GetBrowserFamilyVersion();
        var deviceClientType = browserInfo?.ClientType;

        if ( browserInfo == null
            || deviceOs.IsNullOrWhiteSpace()
            || deviceApplication.IsNullOrWhiteSpace()
            || deviceClientType.IsNullOrWhiteSpace() )
        {
            return null;
        }

        return new InteractionService( Context as RockContext )
            .GetInteractionDeviceTypeId( deviceApplication, deviceOs, deviceClientType, clientInformation.UserAgent );
    }

    /// <summary>
    /// Populates a new <see cref="PersonSession"/> for a regular
    /// authentication via an <c>AuthenticationComponent</c> (web login,
    /// mobile login, TV login, Auth0, or any other
    /// <c>IExternalRedirectAuthentication</c> provider).
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>, or <c>null</c> when no request is in scope.</param>
    /// <param name="personAliasId">The <c>PersonAlias.Id</c> for the authenticating person.</param>
    /// <param name="userLoginId">The <c>UserLogin.Id</c> that performed the authentication.</param>
    /// <param name="authComponentEntityTypeId">The <c>EntityType.Id</c> of the <c>AuthenticationComponent</c> that handled the request.</param>
    /// <param name="isPersistent">Whether the session was created from a "remember me" login.</param>
    /// <param name="mfaRecency">When supplied, the moment the MFA event occurred. Pass <c>null</c> to leave the MFA timestamp null.</param>
    /// <returns>A populated, unsaved <see cref="PersonSession"/>. The caller is responsible for adding it to the context and saving.</returns>
    /// <remarks>
    /// This method is intended to be <c>internal</c>. It is exposed as
    /// <c>public</c> solely so the WebForms <c>AttendanceSelfEntry</c> block in
    /// RockWeb can call it — RockWeb's runtime-generated assembly name cannot be
    /// granted <c>InternalsVisibleTo</c>. It should be reverted to
    /// <c>internal</c> once that block is converted to Obsidian. The
    /// <c>[RockInternal]</c> attribute keeps it out of the documented public API
    /// surface in the meantime.
    /// </remarks>
    [RockInternal( "20.0", keepInternalForever: true )]
    public PersonSession StartComponentSession( RockRequestContext requestContext, int personAliasId, int userLoginId, int authComponentEntityTypeId, bool isPersistent, DateTime? mfaRecency = null )
    {
        var session = PopulateNewSession( requestContext, personAliasId, PersonSessionCreationSource.Component );

        session.UserLoginId = userLoginId;
        session.AuthenticationComponentId = authComponentEntityTypeId;
        session.IsPersistent = isPersistent;
        session.LastStepUpAuthenticationDateTime = RockDateTime.Now;
        session.LastMultiFactorAuthenticationDateTime = mfaRecency;

        return session;
    }

    /// <summary>
    /// Populates a new <see cref="PersonSession"/> for an admin-initiated
    /// impersonation handoff. Copies the impersonator's recency timestamps so
    /// MFA-required pages continue to grant access during impersonation, and
    /// stamps the impersonator's prior <c>PersonSession.Guid</c> and
    /// prior <c>InteractionSession.Guid</c> onto the new row via
    /// <see cref="PersonSessionAdminImpersonationSettings"/> so
    /// <see cref="EndImpersonationAndRestore"/> can revert both later.
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>, or <c>null</c> when no request is in scope.</param>
    /// <param name="targetPersonAliasId">The <c>PersonAlias.Id</c> of the person being impersonated.</param>
    /// <param name="impersonatorSession">The admin's prior <see cref="PersonSession"/>.</param>
    /// <param name="impersonatorInteractionSession">The admin's prior <see cref="InteractionSession"/>.</param>
    /// <returns>A populated, unsaved <see cref="PersonSession"/>. The caller is responsible for adding it to the context and saving.</returns>
    internal PersonSession StartImpersonationSession( RockRequestContext requestContext, int targetPersonAliasId, PersonSession impersonatorSession, InteractionSession impersonatorInteractionSession )
    {
        if ( impersonatorSession == null )
        {
            throw new ArgumentNullException( nameof( impersonatorSession ) );
        }

        if ( impersonatorInteractionSession == null )
        {
            throw new ArgumentNullException( nameof( impersonatorInteractionSession ) );
        }

        var session = PopulateNewSession( requestContext, targetPersonAliasId, PersonSessionCreationSource.Impersonation );

        // Copy recency from the impersonator's prior session. Null source
        // values stay null on the new session - the spec intentionally avoids
        // a "stamp to now" fallback so admins who haven't recently
        // authenticated do not get a fresh recency window for free.
        session.LastStepUpAuthenticationDateTime = impersonatorSession.LastStepUpAuthenticationDateTime;
        session.LastMultiFactorAuthenticationDateTime = impersonatorSession.LastMultiFactorAuthenticationDateTime;

        session.SetAdditionalSettings( new PersonSessionAdminImpersonationSettings
        {
            ImpersonatorPersonSessionGuid = impersonatorSession.Guid,
            ImpersonatorInteractionSessionGuid = impersonatorInteractionSession.Guid,
        } );

        // Spec's InteractionSession sync table calls for a fresh
        // InteractionSession on impersonation start. Regenerate the
        // browser-session identifier so the next interaction-tracking call
        // creates a new InteractionSession row tied to the impersonation
        // PersonSession, leaving the impersonator's prior row queryable
        // via PersonSessionAdminImpersonationSettings.ImpersonatorInteractionSessionGuid
        // for EndImpersonationAndRestore to re-point to later.
        requestContext?.RegenerateBrowserSessionId();

        return session;
    }

    /// <summary>
    /// Populates a new <see cref="PersonSession"/> for a user-token
    /// (<c>rckipid</c> email-link) flow. Stamps the originating
    /// <c>PersonToken.Guid</c> via <see cref="PersonSessionUserTokenSettings"/>
    /// so per-request page-scope re-validation can check the source token on
    /// every page load.
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>, or <c>null</c> when no request is in scope.</param>
    /// <param name="targetPersonAliasId">The <c>PersonAlias.Id</c> of the token's target person.</param>
    /// <param name="token">The source <see cref="PersonToken"/>.</param>
    /// <returns>A populated, unsaved <see cref="PersonSession"/>. The caller is responsible for adding it to the context and saving.</returns>
    internal PersonSession StartUserTokenSession( RockRequestContext requestContext, int targetPersonAliasId, PersonToken token )
    {
        if ( token == null )
        {
            throw new ArgumentNullException( nameof( token ) );
        }

        var session = PopulateNewSession( requestContext, targetPersonAliasId, PersonSessionCreationSource.UserToken );

        session.SetAdditionalSettings( new PersonSessionUserTokenSettings
        {
            OriginatingPersonTokenGuid = token.Guid,
        } );

        return session;
    }

    /// <summary>
    /// Finds the active <see cref="PersonSession"/> for the supplied
    /// <see cref="UserLogin"/>'s <c>ApiKey</c>, or creates one if none exists.
    /// Saves the new row internally on a create. <c>SaveChanges</c> is called
    /// when a new session is created.
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>, or <c>null</c> when no request is in scope.</param>
    /// <param name="userLogin">The <see cref="UserLogin"/> whose API key authenticated the request. Must have a non-null <c>PersonId</c> with a primary alias.</param>
    /// <returns>The active <c>ApiKey</c>-source <see cref="PersonSession"/>.</returns>
    internal PersonSession FindOrCreateApiKeySession( RockRequestContext requestContext, UserLogin userLogin )
    {
        if ( userLogin == null )
        {
            throw new ArgumentNullException( nameof( userLogin ) );
        }

        if ( userLogin.PersonId == null )
        {
            throw new ArgumentException( $"UserLogin {userLogin.Id} has no associated Person.", nameof( userLogin ) );
        }

        var rockContext = Context as RockContext;

        var existing = FindActiveApiKeySession( userLogin.Id );
        if ( existing != null )
        {
            return existing;
        }

        var personAliasId = ( userLogin.Person?.PrimaryAliasId )
            ?? throw new InvalidOperationException( $"Person {userLogin.PersonId.Value} has no primary alias; cannot create ApiKey PersonSession." );

        var session = PopulateNewSession( requestContext, personAliasId, PersonSessionCreationSource.ApiKey );
        session.UserLoginId = userLogin.Id;
        session.IsPersistent = true;

        Add( session );

        try
        {
            rockContext.SaveChanges();
            return session;
        }
        catch ( DbUpdateException ex ) when ( IsUniqueConstraintViolation( ex ) )
        {
            rockContext.Entry( session ).State = System.Data.Entity.EntityState.Detached;

            return FindActiveApiKeySession( userLogin.Id );
        }
    }

    /// <summary>
    /// Finds the <see cref="PersonSession"/> created during a previous legacy
    /// <c>FormsAuthenticationTicket</c> upgrade for the supplied
    /// <paramref name="userLoginId"/> and <paramref name="ticketIssueDate"/>,
    /// or creates one if none exists. <c>SaveChanges</c> is called
    /// when a new session is created.
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>, or <c>null</c> when no request is in scope.</param>
    /// <param name="userLoginId">The <c>UserLogin.Id</c> resolved from the legacy ticket's <c>Name</c> field.</param>
    /// <param name="ticketIssueDate">The legacy ticket's <c>IssueDate</c>. Becomes the new row's <see cref="PersonSession.IssuedDateTime"/> so the <c>RejectAuthenticationCookiesIssuedBefore</c> kill switch is correct for upgraded sessions.</param>
    /// <returns>The <see cref="PersonSession"/> matching the composite key, or a newly created one.</returns>
    internal PersonSession FindOrCreateLegacyUpgradeSession( RockRequestContext requestContext, int userLoginId, DateTime ticketIssueDate )
    {
        var rockContext = Context as RockContext;

        var existing = FindLegacyUpgradeSession( userLoginId, ticketIssueDate );
        if ( existing != null )
        {
            return existing;
        }

        var userLogin = new UserLoginService( rockContext ).GetInclude( userLoginId, u => u.Person );
        if ( userLogin?.PersonId == null )
        {
            throw new InvalidOperationException( $"UserLogin {userLoginId} not found or has no associated Person; cannot upgrade legacy session." );
        }

        var personAliasId = userLogin.Person?.PrimaryAliasId
            ?? throw new InvalidOperationException( $"Person {userLogin.PersonId.Value} has no primary alias; cannot upgrade legacy session." );

        var session = PopulateNewSession( requestContext, personAliasId, PersonSessionCreationSource.Legacy );
        session.UserLoginId = userLoginId;
        session.IsPersistent = true;
        session.IssuedDateTime = ticketIssueDate;

        Add( session );

        try
        {
            rockContext.SaveChanges();
            return session;
        }
        catch ( DbUpdateException ex ) when ( IsUniqueConstraintViolation( ex ) )
        {
            rockContext.Entry( session ).State = System.Data.Entity.EntityState.Detached;

            return FindLegacyUpgradeSession( userLoginId, ticketIssueDate );
        }
    }

    /// <summary>
    /// Finds the active <see cref="PersonSession"/> for the supplied Api Key
    /// <see cref="UserLogin"/>.
    /// </summary>
    /// <remarks>
    /// Filters out sessions whose <see cref="PersonSession.ExpiresDateTime"/>
    /// has passed even when their <c>IsActive</c> flag is still true (Rock
    /// Cleanup is the canonical writer that flips <c>IsActive = false</c>
    /// on expiration; until that job runs there can be expired-but-active
    /// rows). Returning one of those would cause the caller's next request
    /// to hit <c>ResolveSessionForRequest</c>'s expiration check and
    /// immediately log the device out — the exact regression the
    /// find-or-create path is supposed to prevent. Excluding them here
    /// pushes the caller into the "create new" branch instead. Today
    /// ApiKey sessions are documented as durable (no
    /// <c>ExpiresDateTime</c>); the filter is a defensive guarantee
    /// against future changes that introduce one.
    /// </remarks>
    /// <param name="userLoginId">The identifier of the <see cref="UserLogin"/> representing the Api Key.</param>
    /// <returns>The first matching active session for the Api Key.</returns>
    private PersonSession FindActiveApiKeySession( int userLoginId )
    {
        var now = RockDateTime.Now;
        return Queryable()
            .Where( s => s.UserLoginId == userLoginId
                && s.CreationSource == PersonSessionCreationSource.ApiKey
                && s.IsActive
                && ( s.ExpiresDateTime == null || s.ExpiresDateTime > now ) )
            .FirstOrDefault();
    }

    /// <summary>
    /// Finds or creates the active <see cref="PersonSessionCreationSource.Component"/>
    /// <see cref="PersonSession"/> for a Mobile or TV device login. Reuses an
    /// existing active session when the device's <see cref="UserLogin"/>
    /// already has one (the "device token refresh" / "same-person re-login"
    /// case). Marks any prior <see cref="PersonSession"/> on
    /// <paramref name="requestContext"/> for a different <see cref="UserLogin"/>
    /// inactive (the "different person on the same device" case). Otherwise
    /// builds a new persistent session via <see cref="StartComponentSession"/>
    /// using the <see cref="UserLogin.EntityTypeId"/> as the authenticating
    /// component, and saves it. <c>SaveChanges</c> is called when this method
    /// mutates state.
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>, or <c>null</c> when no request is in scope.</param>
    /// <param name="userLogin">The <see cref="UserLogin"/> that authenticated the device. Must have <c>EntityTypeId</c> (the authentication component) populated and a Person with a primary alias.</param>
    /// <returns>The reused or newly created <see cref="PersonSession"/>.</returns>
    internal PersonSession FindOrCreateDeviceComponentSession( RockRequestContext requestContext, UserLogin userLogin )
    {
        if ( userLogin == null )
        {
            throw new ArgumentNullException( nameof( userLogin ) );
        }

        if ( userLogin.PersonId == null )
        {
            throw new ArgumentException( $"UserLogin {userLogin.Id} has no associated Person.", nameof( userLogin ) );
        }

        if ( userLogin.EntityTypeId == null )
        {
            throw new ArgumentException( $"UserLogin {userLogin.Id} has no authentication component (EntityTypeId is null).", nameof( userLogin ) );
        }

        var rockContext = Context as RockContext;

        // "Different person on the same device" handling. If the request
        // came in with an existing PersonSession that belongs to a
        // different UserLogin, mark that prior session inactive. The new
        // session created below takes over the device. This matches the
        // spec's InteractionSession sync table rule
        // "Login, already authenticated, different person | Create new",
        // adapted to mobile / TV where the prior session is the device's
        // previous owner. Re-fetch the prior row into THIS context before
        // mutating because RockRequestContext.PersonSession was tracked by
        // a different context.
        var priorSessionGuid = requestContext?.PersonSession?.Guid;
        var priorSessionUserLoginId = requestContext?.PersonSession?.UserLoginId;
        var changedDevice = priorSessionGuid.HasValue
            && priorSessionGuid.Value != Guid.Empty
            && priorSessionUserLoginId.HasValue
            && priorSessionUserLoginId.Value != userLogin.Id;

        if ( changedDevice )
        {
            var prior = Get( priorSessionGuid.Value );
            if ( prior != null && prior.IsActive )
            {
                prior.IsActive = false;
                rockContext.SaveChanges();
            }
        }

        var existing = FindActiveComponentSession( userLogin.Id );
        if ( existing != null )
        {
            return existing;
        }

        var personAliasId = ( userLogin.Person?.PrimaryAliasId )
            ?? throw new InvalidOperationException( $"Person {userLogin.PersonId.Value} has no primary alias; cannot create device PersonSession." );

        var session = StartComponentSession(
            requestContext,
            personAliasId,
            userLogin.Id,
            userLogin.EntityTypeId.Value,
            isPersistent: true );

        Add( session );

        try
        {
            rockContext.SaveChanges();
            return session;
        }
        catch ( DbUpdateException ex ) when ( IsUniqueConstraintViolation( ex ) )
        {
            rockContext.Entry( session ).State = System.Data.Entity.EntityState.Detached;

            return FindActiveComponentSession( userLogin.Id );
        }
    }

    /// <summary>
    /// Finds the active <see cref="PersonSessionCreationSource.Component"/>
    /// <see cref="PersonSession"/> for the supplied <see cref="UserLogin"/>.
    /// Used by the device flows (Mobile, TV) to detect the "same person
    /// re-login" case before creating a duplicate.
    /// </summary>
    /// <remarks>
    /// Filters out sessions whose <see cref="PersonSession.ExpiresDateTime"/>
    /// has passed even when their <c>IsActive</c> flag is still true (Rock
    /// Cleanup is the canonical writer that flips <c>IsActive = false</c>
    /// on expiration; until that job runs there can be expired-but-active
    /// rows). Returning one of those would cause the device's next request
    /// to hit <c>ResolveSessionForRequest</c>'s expiration check and
    /// immediately log the user out — the exact regression the
    /// find-or-create path is supposed to prevent. Excluding them here
    /// pushes the caller into the "create new" branch instead.
    /// </remarks>
    /// <param name="userLoginId">The identifier of the <see cref="UserLogin"/>.</param>
    /// <returns>The first matching active Component session, or <c>null</c>.</returns>
    private PersonSession FindActiveComponentSession( int userLoginId )
    {
        var now = RockDateTime.Now;
        return Queryable()
            .Where( s => s.UserLoginId == userLoginId
                && s.CreationSource == PersonSessionCreationSource.Component
                && s.IsActive
                && ( s.ExpiresDateTime == null || s.ExpiresDateTime > now ) )
            .FirstOrDefault();
    }

    /// <summary>
    /// Finds an active session created from a legacy <c>FormsAuthenticationTicket</c>
    /// upgrade path.
    /// </summary>
    /// <param name="userLoginId">The identifier of the <see cref="UserLogin"/> that was used in the ticket.</param>
    /// <param name="ticketIssueDate">The date the ticket was issued.</param>
    /// <returns>The first matching active session for the legacy ticket.</returns>
    private PersonSession FindLegacyUpgradeSession( int userLoginId, DateTime ticketIssueDate )
    {
        return Queryable()
            .Where( s => s.UserLoginId == userLoginId
                && s.CreationSource == PersonSessionCreationSource.Legacy
                && s.IssuedDateTime == ticketIssueDate )
            .FirstOrDefault();
    }

    /// <summary>
    /// Returns <c>true</c> when the supplied <see cref="DbUpdateException"/>
    /// wraps a SQL Server unique-constraint violation (error 2601 or 2627).
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns><c>true</c> if the exception is a unique-constraint violation; otherwise, <c>false</c>.</returns>
    private static bool IsUniqueConstraintViolation( DbUpdateException ex )
    {
        if ( ex.GetBaseException() is not SqlException sqlException )
        {
            return false;
        }

        foreach ( SqlError error in sqlException.Errors )
        {
            if ( error.Number == 2601 || error.Number == 2627 )
            {
                return true;
            }
        }

        return false;
    }

    #endregion Session Creation

    #region Impersonation Query Helpers

    /// <summary>
    /// Returns the impersonator's prior <see cref="PersonSession"/> for an
    /// admin-impersonation session, or <c>null</c> if the session is not an
    /// admin-impersonation session or the restore reference is dangling.
    /// </summary>
    /// <param name="session">The <see cref="PersonSession"/> to look up.</param>
    /// <returns>The impersonator's prior session, or <c>null</c>.</returns>
    public PersonSession GetImpersonatorSession( PersonSession session )
    {
        if ( session == null || session.CreationSource != PersonSessionCreationSource.Impersonation )
        {
            return null;
        }

        var settings = session.GetAdditionalSettingsOrNull<PersonSessionAdminImpersonationSettings>();

        if ( settings == null || settings.ImpersonatorPersonSessionGuid == Guid.Empty )
        {
            return null;
        }

        return Get( settings.ImpersonatorPersonSessionGuid );
    }

    /// <summary>
    /// Ends an admin-impersonation session by marking the supplied
    /// <paramref name="session"/> inactive and resolving the impersonator's
    /// prior <see cref="PersonSession"/>. On a successful restore, the
    /// browser-session identifier (<c>RockSessionId</c>) on
    /// <paramref name="requestContext"/> is re-pointed at the impersonator's
    /// prior <c>InteractionSession.Guid</c> so subsequent interaction
    /// tracking resumes against the admin's pre-impersonation row. The
    /// caller is responsible for reissuing the <c>.ROCK</c> cookie to point
    /// at the returned session. <c>SaveChanges</c> is called to mark the
    /// current session as inactive.
    /// </summary>
    /// <param name="session">The impersonation <see cref="PersonSession"/> to end.</param>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>, or <c>null</c> when no request is in scope (in which case browser-session restoration is skipped).</param>
    /// <returns>The impersonator's prior session, or <c>null</c> if either restore reference was dangling.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="session"/>'s <see cref="PersonSession.CreationSource"/> is anything other than <see cref="PersonSessionCreationSource.Impersonation"/>.</exception>
    internal PersonSession EndImpersonationAndRestore( PersonSession session, RockRequestContext requestContext )
    {
        if ( session == null )
        {
            throw new ArgumentNullException( nameof( session ) );
        }

        if ( session.CreationSource != PersonSessionCreationSource.Impersonation )
        {
            throw new InvalidOperationException( $"EndImpersonationAndRestore requires CreationSource = Impersonation; got {session.CreationSource}." );
        }

        var rockContext = Context as RockContext;
        var settings = session.GetAdditionalSettingsOrNull<PersonSessionAdminImpersonationSettings>();

        PersonSession priorSession = null;
        InteractionSession priorInteractionSession = null;

        if ( settings != null )
        {
            if ( settings.ImpersonatorPersonSessionGuid != Guid.Empty )
            {
                priorSession = Get( settings.ImpersonatorPersonSessionGuid );
            }

            if ( settings.ImpersonatorInteractionSessionGuid != Guid.Empty )
            {
                priorInteractionSession = new InteractionSessionService( rockContext ).Get( settings.ImpersonatorInteractionSessionGuid );
            }
        }

        // Mark the current impersonation session inactive regardless of the
        // restore outcome. The SaveHook stamps InactiveDateTime.
        session.IsActive = false;
        rockContext.SaveChanges();

        if ( priorSession == null || priorInteractionSession == null )
        {
            // Either restore reference is dangling. Fail closed: the user
            // becomes anonymous rather than silently continuing as the
            // impersonated person OR silently dropping back to the admin.
            // Clear the `.ROCK` cookie so the next request resolves
            // anonymously instead of trying to use the now-inactive session.
            if ( requestContext != null )
            {
                ExpireAuthCookie( requestContext );
            }

            return null;
        }

        // Re-point the browser-session identifier so subsequent
        // interaction-tracking calls on this browser update the admin's
        // pre-impersonation InteractionSession row rather than continuing
        // to write activity against the impersonation-period row (which
        // remains in the database as a historical record).
        requestContext?.SetBrowserSessionId( priorInteractionSession.Guid );

        // Write the new auth cookie pointing at the restored session and
        // attach it to the request context so the remainder of this
        // request observes the admin's restored identity. Mirrors the
        // start-side `ImpersonatePerson` shape so every caller of either
        // method gets the cookie + context write for free without
        // duplicating the follow-ups at each site.
        if ( requestContext != null )
        {
            SetAuthCookie( priorSession, requestContext );
            requestContext.SetPersonSession( priorSession );
        }

        return priorSession;
    }

    /// <summary>
    /// Server-side orchestration of an admin-initiated impersonation handoff.
    /// Reads the admin's current <see cref="PersonSession"/> and
    /// <see cref="InteractionSession"/> from <paramref name="context"/>,
    /// builds a new <see cref="PersonSessionCreationSource.Impersonation"/>
    /// session targeting <paramref name="targetPersonAliasId"/>, writes the
    /// new <c>.ROCK</c> cookie, attaches the new session to the request
    /// context, and records the impersonation start in
    /// <see cref="HistoryLogin"/>. No <see cref="PersonToken"/> row is
    /// written; the entire handoff is cookie-based.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lives on the service rather than on the Bio block's code-behind so
    /// the orchestration is mocked-database testable; Bio.ascx.cs is
    /// compiled at runtime and cannot be exercised from Rock.Tests. The
    /// caller (block) is reduced to a thin shim: call this method, then
    /// redirect to the configured target URL.
    /// </para>
    /// <para>
    /// Owns its own <see cref="RockContext"/> via
    /// <c>RockApp.Current.CreateRockContext()</c> so the test harness's
    /// mocked context factory can intercept; that pattern is the same one
    /// <c>UserLoginService.UpdateLastLogin</c> uses. The redirect to the
    /// configured target URL is intentionally NOT in this method, since
    /// the target URL is a block setting and the service should not know
    /// about it; with no <c>rckipid</c>-appending in this flow, the
    /// "no token in URL" property holds by construction.
    /// </para>
    /// </remarks>
    /// <param name="context">The current <see cref="RockRequestContext"/>.</param>
    /// <param name="targetPersonAliasId">The <c>PersonAlias.Id</c> of the person to impersonate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the request has no active <see cref="PersonSession"/> (e.g., the admin's session expired between rendering the Impersonate button and clicking it).</exception>
    [RockInternal( "20.0", keepInternalForever: true )]
    public static void ImpersonatePerson( RockRequestContext context, int targetPersonAliasId )
    {
        if ( context == null )
        {
            throw new ArgumentNullException( nameof( context ) );
        }

        var impersonatorSession = context.PersonSession;
        if ( impersonatorSession == null || !impersonatorSession.IsActive )
        {
            // Defensive backstop: the Bio block's rendering check prevents
            // this from happening in normal flow, but the admin's session
            // could have expired between render and click. The caller is
            // responsible for surfacing this through the existing
            // session-expired error path rather than crashing.
            throw new InvalidOperationException( "Cannot impersonate: the current request has no active PersonSession." );
        }

        using ( var rockContext = RockApp.Current.CreateRockContext() )
        {
            // Resolve the admin's current InteractionSession via the
            // browser-session identifier (RockSessionId). StartImpersonationSession
            // needs this so the impersonation session can later be reversed via
            // EndImpersonationAndRestore (which re-points the browser back at
            // the admin's pre-impersonation InteractionSession row).
            var browserSessionId = context.SessionGuid;
            var impersonatorInteractionSession = new InteractionSessionService( rockContext )
                .Queryable()
                .FirstOrDefault( s => s.Guid == browserSessionId );

            if ( impersonatorInteractionSession == null )
            {
                // Same shape as the no-PersonSession case: the admin's request
                // is missing the state we need to support a clean restore.
                throw new InvalidOperationException( "Cannot impersonate: the current request has no InteractionSession for the admin's browser." );
            }

            var service = new PersonSessionService( rockContext );

            var newSession = service.StartImpersonationSession(
                context,
                targetPersonAliasId,
                impersonatorSession,
                impersonatorInteractionSession );

            service.Add( newSession );
            rockContext.SaveChanges();

            // Write the new-format auth cookie via the request context.
            // SetAuthCookie reads the new session's Guid for the cookie's
            // sid field, and the cookie's Expires attribute is computed
            // from the new session's IsPersistent + ExpiresDateTime.
            service.SetAuthCookie( newSession, context );

            // Replace the cached PersonSession on the request context so the
            // remainder of this request observes the impersonated identity
            // (the cookie alone takes effect on the NEXT request; this is
            // the in-request bridge).
            context.SetPersonSession( newSession );

            // HistoryLogin audit trail. Mirrors the relevant fields from
            // the legacy UpdateLastLogin impersonation branch: PersonAliasId
            // points at the impersonated person, LoginContext = "Impersonation",
            // and ImpersonatedByPersonFullName carries the admin's name.
            // The legacy UserName field stored an obfuscated rckipid; under
            // the new model there is no rckipid, so the field is left null
            // (the admin / target identity is recoverable from PersonAliasId
            // + RelatedData).
            BuildImpersonationHistoryLogin( rockContext, newSession, impersonatorSession )
                .SaveAfterDelay();
        }
    }

    /// <summary>
    /// Builds (but does not save) the <see cref="HistoryLogin"/> audit row
    /// for an admin impersonation start. Resolves the impersonator's
    /// display name from the impersonator's <see cref="PersonSession.PersonAlias"/>.
    /// </summary>
    /// <remarks>
    /// Split from the call to <see cref="HistoryLogin.SaveAfterDelay"/> so
    /// mocked-database tests can assert on the built entity without
    /// running the real <c>Task.Run</c> background save path (which uses a
    /// non-mocked <c>RockContext</c>).
    /// </remarks>
    /// <param name="rockContext">The rock context to read the impersonator's <see cref="Person"/> through.</param>
    /// <param name="impersonationSession">The newly created <see cref="PersonSessionCreationSource.Impersonation"/> session.</param>
    /// <param name="impersonatorSession">The admin's prior <see cref="PersonSession"/>.</param>
    /// <returns>A populated <see cref="HistoryLogin"/> ready for <c>SaveAfterDelay</c>.</returns>
    internal static HistoryLogin BuildImpersonationHistoryLogin( RockContext rockContext, PersonSession impersonationSession, PersonSession impersonatorSession )
    {
        // Resolve the impersonator's full name through PersonAliasService so
        // we do not depend on a nav property being lazily loaded.
        var personAliasService = new PersonAliasService( rockContext );
        var impersonatorAlias = personAliasService.Get( impersonatorSession.PersonAliasId );
        var impersonatorFullName = impersonatorAlias?.Person?.FullName;

        var historyLogin = new HistoryLogin
        {
            UserName = null,
            UserLoginId = null,
            PersonAliasId = impersonationSession.PersonAliasId,
            WasLoginSuccessful = true,
        };

        // Always stamp LoginContext = "Impersonation" so audit consumers can
        // filter by that field even when the impersonator's name cannot be
        // resolved (PersonAlias deleted between impersonation start and the
        // service's read, etc.).
        historyLogin.SetRelatedDataJson( new HistoryLoginRelatedData
        {
            ImpersonatedByPersonFullName = impersonatorFullName,
            LoginContext = "Impersonation",
        } );

        return historyLogin;
    }

    #endregion Impersonation Query Helpers

    #region Impersonation Token Processing

    /// <summary>
    /// <para>
    /// Single Pattern B seam for every code path that inspects an <c>rckipid</c>
    /// query parameter on an incoming request. Applies the user-token
    /// impersonation matrix from the spec ("Test Plan / Impersonation:
    /// <c>ProcessImpersonationToken</c> matrix"):
    /// </para>
    /// <list type="number">
    ///     <item>Invalid, expired, revoked, over-<c>UsageLimit</c>, or out-of-page-scope token: mark any current session inactive, expire the cookie, return redirect-required.</item>
    ///     <item>Current session is <see cref="PersonSessionCreationSource.UserToken"/> and references this same <c>PersonToken</c>: no session change, no <c>TimesUsed</c> increment, return redirect-required.</item>
    ///     <item>Current session is <see cref="PersonSessionCreationSource.Component"/> for the token's target person: no session change, return redirect-required. <c>TimesUsed</c> still increments per the strict literal spec wording: Component sessions reference no token, so the incoming token "differs."</item>
    ///     <item>All other cases (Anonymous, Impersonation, ApiKey, Legacy, mismatched Component, mismatched UserToken): mark any current session inactive, create a new <see cref="PersonSessionCreationSource.UserToken"/> session for the token's target, write the new cookie, return redirect-required.</item>
    /// </list>
    /// <para>
    /// Redirect-required is always <c>true</c> on return: anywhere this helper
    /// is called, the <c>rckipid</c> MUST come out of the URL so it does not
    /// persist in browser history, get re-processed on the next page load, or
    /// leak via referer.
    /// </para>
    /// </summary>
    /// <param name="rckipidToken">The <c>rckipid</c> token value lifted from the request's query string.</param>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>.</param>
    /// <param name="currentPageId">The <c>Page.Id</c> the request is targeting, used to enforce a page-scoped token. Pass <c>null</c> when called from a non-page context (e.g. an API controller).</param>
    /// <returns>An <see cref="ImpersonationProcessResult"/> capturing the resulting session and the redirect signal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestContext"/> is null.</exception>
    internal ImpersonationProcessResult ProcessImpersonationToken( string rckipidToken, RockRequestContext requestContext, int? currentPageId = null )
    {
        if ( requestContext == null )
        {
            throw new ArgumentNullException( nameof( requestContext ) );
        }

        var rockContext = Context as RockContext;
        var personToken = new PersonTokenService( rockContext ).GetByImpersonationToken( rckipidToken );

        // Token validity. The rules below collapse into matrix rule 1 ("token
        // not usable"): mark any current session inactive, expire the cookie,
        // and return the redirect signal so the caller can strip rckipid from
        // the URL. We compare TimesUsed >= UsageLimit (not >) because TimesUsed
        // is the post-use count; a UsageLimit=1 token has TimesUsed=1 after
        // one valid use, and a re-click then hits this branch. A page-scoped
        // token presented to a non-page context (currentPageId == null, e.g.
        // an API request) is also rejected: the scope's intent is to confine
        // the token to one page, so absence of a page identifier fails the
        // check rather than passes it.
        var now = RockDateTime.Now;
        var isTokenInvalid =
            personToken == null
            || personToken.PersonAlias == null
            || ( personToken.ExpireDateTime.HasValue && personToken.ExpireDateTime.Value < now )
            || ( personToken.UsageLimit.HasValue && personToken.TimesUsed >= personToken.UsageLimit.Value )
            || ( personToken.PageId.HasValue && personToken.PageId.Value != currentPageId );

        if ( isTokenInvalid )
        {
            DeactivateCurrentSessionIfActive( requestContext );
            ExpireAuthCookie( requestContext );

            return new ImpersonationProcessResult
            {
                IsRedirectRequired = true,
                Session = null,
            };
        }

        var currentSession = requestContext.PersonSession;

        // Rule 2: current session is a UserToken session AND references this
        // same source PersonToken. No new session row, no TimesUsed
        // increment, since the token has already done its work for this browser.
        // Returning the unchanged session lets the caller redirect to the
        // clean URL without disturbing the existing identity.
        if ( currentSession != null
            && currentSession.CreationSource == PersonSessionCreationSource.UserToken )
        {
            var currentSettings = currentSession.GetAdditionalSettingsOrNull<PersonSessionUserTokenSettings>();
            if ( currentSettings != null && currentSettings.OriginatingPersonTokenGuid == personToken.Guid )
            {
                return new ImpersonationProcessResult
                {
                    IsRedirectRequired = true,
                    Session = currentSession,
                };
            }
        }

        // Past this point the rckipid is treated as "different from the token
        // referenced by the current session," so the spec calls for a
        // TimesUsed increment. The persistence happens below alongside any
        // session writes so a single SaveChanges covers both.
        personToken.TimesUsed++;
        personToken.LastUsedDateTime = now;

        var personAliasService = new PersonAliasService( rockContext );
        var tokenTargetPersonId = personToken.PersonAlias.PersonId;
        var targetPersonAliasId = personToken.PersonAliasId;

        // Rule 3 (matrix row "Component-for-X | Token for X"): the user is
        // already logged in as the token's target person. Don't create a
        // second session for the same identity, but DO increment TimesUsed
        // (the current session references no PersonToken, so the incoming
        // token "differs from the token referenced by the current session").
        // PersonId comparison is on the underlying Person, not the alias,
        // because the same person can have multiple aliases. The token
        // could legitimately target a different alias than the one the
        // current session was created against.
        var currentSessionPersonId = ( currentSession != null )
            ? personAliasService.Get( currentSession.PersonAliasId )?.PersonId
            : null;

        if ( currentSession != null
            && currentSession.CreationSource == PersonSessionCreationSource.Component
            && currentSessionPersonId == tokenTargetPersonId )
        {
            rockContext.SaveChanges();

            return new ImpersonationProcessResult
            {
                IsRedirectRequired = true,
                Session = currentSession,
            };
        }

        // Rule 4 (every other current-session shape): abandon the current
        // session (Impersonation, mismatched Component, mismatched UserToken,
        // ApiKey, Legacy) and start a new UserToken session for the token's
        // target person.
        DeactivateCurrentSessionIfActive( requestContext );

        var newSession = StartUserTokenSession( requestContext, targetPersonAliasId, personToken );
        Add( newSession );
        rockContext.SaveChanges();

        SetAuthCookie( newSession, requestContext );
        requestContext.SetPersonSession( newSession );

        // Spec's InteractionSession sync table calls for a fresh
        // InteractionSession on the auth transition. Regenerating the
        // browser-session identifier here means the next interaction-tracking
        // call creates a new InteractionSession row tied to the new
        // UserToken PersonSession rather than continuing to write activity
        // against the prior session's row.
        requestContext.RegenerateBrowserSessionId();

        return new ImpersonationProcessResult
        {
            IsRedirectRequired = true,
            Session = newSession,
        };
    }

    /// <summary>
    /// Per-request page-scope re-validation hook for active
    /// <see cref="PersonSessionCreationSource.UserToken"/> sessions. Re-reads
    /// the source <c>PersonToken</c> on every request and applies the spec's
    /// "fail closed" rules: revocation, expiration, and over-<c>UsageLimit</c>
    /// mark the session inactive; a page-scoped token used outside its
    /// configured page returns a not-authorized signal without deactivating
    /// the session.
    /// </summary>
    /// <remarks>
    /// Called from <see cref="Rock.Web.UI.RockPage"/> on every page load so
    /// the page-scope check the legacy <c>ProcessImpersonation</c> performed
    /// for <c>rckipid=</c>-in-identity requests continues to run under the
    /// new model. The cookie no longer carries <c>rckipid</c>, so this hook
    /// is how the check stays effective. No-op for non-UserToken sessions and
    /// for anonymous requests.
    /// </remarks>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>.</param>
    /// <param name="currentPageId">The <c>Page.Id</c> the request is targeting, or <c>null</c> when called from a non-page context.</param>
    /// <returns>The revalidation outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestContext"/> is null.</exception>
    internal UserTokenRevalidationResult RevalidateUserTokenSession( RockRequestContext requestContext, int? currentPageId )
    {
        if ( requestContext == null )
        {
            throw new ArgumentNullException( nameof( requestContext ) );
        }

        var currentSession = requestContext.PersonSession;
        if ( currentSession == null
            || !currentSession.IsActive
            || currentSession.CreationSource != PersonSessionCreationSource.UserToken )
        {
            return UserTokenRevalidationResult.Ok;
        }

        var settings = currentSession.GetAdditionalSettingsOrNull<PersonSessionUserTokenSettings>();
        if ( settings == null || settings.OriginatingPersonTokenGuid == Guid.Empty )
        {
            // UserToken session with no token reference is structurally broken;
            // treat as revoked rather than silently continuing.
            DeactivateCurrentSessionIfActive( requestContext );
            ExpireAuthCookie( requestContext );
            return UserTokenRevalidationResult.SessionRevoked;
        }

        var rockContext = Context as RockContext;
        var personToken = new PersonTokenService( rockContext ).Get( settings.OriginatingPersonTokenGuid );

        // UsageLimit is intentionally NOT checked here: it governs whether
        // a token can establish a NEW session (enforced in
        // ProcessImpersonationToken), not whether an already-established
        // session can continue. Per the spec deliverable, the per-request
        // hook checks page-scope, expiration, and revocation. Token
        // deletion is the "revocation" signal.
        var now = RockDateTime.Now;
        var isRevoked =
            personToken == null
            || ( personToken.ExpireDateTime.HasValue && personToken.ExpireDateTime.Value < now );

        if ( isRevoked )
        {
            DeactivateCurrentSessionIfActive( requestContext );
            ExpireAuthCookie( requestContext );
            return UserTokenRevalidationResult.SessionRevoked;
        }

        // A null currentPageId means the caller is not in a page context
        // (e.g., an API call made from JavaScript or an Obsidian block
        // running inside the in-scope page). Those calls are legitimate
        // and must NOT be blocked by the page-scope check: an in-scope
        // page can need to fetch campuses, accounts, etc. via the REST
        // API to render itself. The page-scope check exists to prevent
        // a user from NAVIGATING away from the in-scope page, which is
        // distinct: navigation goes through RockPage with a concrete
        // PageId. Sister check in ProcessImpersonationToken treats null
        // differently because that path decides whether to START a new
        // session, not whether to permit a request under an existing one.
        if ( personToken.PageId.HasValue
            && currentPageId.HasValue
            && personToken.PageId.Value != currentPageId.Value )
        {
            return UserTokenRevalidationResult.PageScopeMiss;
        }

        return UserTokenRevalidationResult.Ok;
    }

    /// <summary>
    /// Marks any active <see cref="PersonSession"/> on
    /// <paramref name="requestContext"/> inactive (refetched through this
    /// service's context so the <c>SaveHook</c> can stamp
    /// <see cref="PersonSession.InactiveDateTime"/> correctly). Used by
    /// <see cref="ProcessImpersonationToken"/> and
    /// <see cref="RevalidateUserTokenSession"/> when an auth event abandons
    /// the existing session.
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>.</param>
    private void DeactivateCurrentSessionIfActive( RockRequestContext requestContext )
    {
        var existingGuid = requestContext?.PersonSession?.Guid;
        if ( !existingGuid.HasValue || existingGuid.Value == Guid.Empty )
        {
            return;
        }

        var attached = Get( existingGuid.Value );
        if ( attached == null || !attached.IsActive )
        {
            return;
        }

        attached.IsActive = false;
        ( Context as RockContext ).SaveChanges();
    }

    #endregion Impersonation Token Processing

    #region Cookie I/O

    /// <summary>
    /// Encodes the supplied <paramref name="session"/> into the opaque,
    /// encrypted cookie value the auth pipeline transmits.
    /// </summary>
    /// <param name="session">The <see cref="PersonSession"/> to encode.</param>
    /// <returns>The opaque encrypted cookie value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is null.</exception>
    public string GetCookieValue( PersonSession session )
    {
        if ( session == null )
        {
            throw new ArgumentNullException( nameof( session ) );
        }

        var payload = new PersonSessionCookiePayload
        {
            Version = CookiePayloadVersion,
            SessionGuid = session.Guid,
            IssuedAt = RockDateTime.Now,
        };

        // System.Text.Json (not Rock's .ToJson() / Newtonsoft) on purpose:
        // the cookie payload is a black box owned end-to-end by this service,
        // nothing external touches it, and STJ is faster than Newtonsoft.
        var plaintext = JsonSerializer.Serialize( payload );

        return Encryption.EncryptString( plaintext );
    }

    /// <summary>
    /// Decodes a previously-encoded cookie value back into its payload and
    /// metadata. Returns <c>false</c> for non-new-format cookies (legacy
    /// <c>FormsAuthenticationTicket</c>, tampered, or otherwise undecodable);
    /// legacy-format cookies are intentionally left alone for the cookie
    /// upgrade path to pickup and deal with.
    /// </summary>
    /// <param name="cookieValue">The opaque cookie value to decode.</param>
    /// <param name="payload">The parsed payload on success; <c>null</c> on failure.</param>
    /// <param name="metadata">The decode metadata on success; <c>null</c> on failure.</param>
    /// <returns><c>true</c> when the cookie is a valid new-format payload; otherwise <c>false</c>.</returns>
    internal static bool TryDecodeCookie( string cookieValue, out PersonSessionCookiePayload payload, out PersonSessionCookieDecodeMetadata metadata )
    {
        payload = null;
        metadata = null;

        if ( cookieValue.IsNullOrWhiteSpace() )
        {
            return false;
        }

        // Encryption.DecryptString with isLegacyAllowed: false short-circuits
        // when the V2 footer is absent, so a legacy FormsAuthenticationTicket
        // value returns null here without expensive crypto work.
        string plaintext;
        bool decryptedWithCurrentKey;
        try
        {
            plaintext = Encryption.DecryptString( cookieValue, isLegacyAllowed: false, out decryptedWithCurrentKey );
        }
        catch
        {
            return false;
        }

        if ( plaintext.IsNullOrWhiteSpace() )
        {
            return false;
        }

        // Try to decode the JSON payload. Failure here most likely means that
        // the cookie was tampered with.
        PersonSessionCookiePayload decoded;
        try
        {
            decoded = JsonSerializer.Deserialize<PersonSessionCookiePayload>( plaintext );
        }
        catch ( JsonException )
        {
            return false;
        }

        if ( decoded == null || decoded.SessionGuid == Guid.Empty )
        {
            return false;
        }

        payload = decoded;
        metadata = new PersonSessionCookieDecodeMetadata
        {
            DecryptedWithOldKey = !decryptedWithCurrentKey,
            PayloadVersion = decoded.Version,
        };

        return true;
    }

    /// <summary>
    /// Writes the supplied <paramref name="session"/>'s opaque cookie value to
    /// the response via <paramref name="requestContext"/>.
    /// </summary>
    /// <param name="session">The <see cref="PersonSession"/> whose cookie should be written.</param>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when either argument is null.</exception>
    public void SetAuthCookie( PersonSession session, RockRequestContext requestContext )
    {
        if ( session == null )
        {
            throw new ArgumentNullException( nameof( session ) );
        }

        if ( requestContext == null )
        {
            throw new ArgumentNullException( nameof( requestContext ) );
        }

        var cookieValue = GetCookieValue( session );

        var cookie = new BrowserCookie
        {
            Name = AuthCookieName,
            Value = cookieValue,
            Path = "/",
            HttpOnly = true,
            IsEssential = true,
            Domain = GetCookieDomain( requestContext ),
        };

        if ( session.IsPersistent )
        {
            var cap = RockDateTime.Now.Add( AuthCookieTimeout );

            cookie.Expires = ( session.ExpiresDateTime.HasValue && session.ExpiresDateTime.Value < cap )
                ? session.ExpiresDateTime.Value
                : cap;
        }

        requestContext.Response.AddCookie( cookie );
    }

    /// <summary>
    /// <para>
    /// Owns the full read-side cookie lifecycle: read the cookie value off
    /// the request, decode it, validate the session, enforce the
    /// <c>RejectAuthenticationCookiesIssuedBefore</c> kill switch, apply
    /// reissue triggers, and return the resolved session. Legacy-format
    /// cookies are intentionally left alone here; the legacy upgrade path
    /// picks them up and creates new-format sessions for them.
    /// </para>
    /// <para>
    /// If the session is valid but expired, then the cookie is expired via the
    /// response and the <c>PersonSession</c> is updated to be inactive.
    /// <c>SaveChanges</c> is called in this case.
    /// </para>
    /// </summary>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>.</param>
    /// <returns>The resolved <see cref="PersonSession"/>, or <c>null</c> when no valid session is present.</returns>
    [RockInternal( "20.0", keepInternalForever: true )]
    public PersonSession ResolveSessionForRequest( RockRequestContext requestContext )
    {
        if ( requestContext == null )
        {
            throw new ArgumentNullException( nameof( requestContext ) );
        }

        var cookieValue = requestContext.GetCookieValue( AuthCookieName );
        if ( cookieValue.IsNullOrWhiteSpace() )
        {
            return null;
        }

        if ( !TryDecodeCookie( cookieValue, out var payload, out var metadata ) )
        {
            // Legacy-format cookie, tampered, or otherwise undecodable.
            return null;
        }

        var session = Get( payload.SessionGuid );

        if ( session == null
            || !session.IsActive
            || ( session.ExpiresDateTime.HasValue && session.ExpiresDateTime.Value <= RockDateTime.Now ) )
        {
            ExpireAuthCookie( requestContext );
            return null;
        }

        // Kill-switch check. Always compares against PersonSession.IssuedDateTime,
        // never the cookie's iat, which closes the prior bypass-via-reissue
        // weakness.
        var killSwitchThreshold = new SecuritySettingsService()
            .SecuritySettings?
            .RejectAuthenticationCookiesIssuedBefore;

        if ( killSwitchThreshold.HasValue
            && killSwitchThreshold.Value <= RockDateTime.Now
            && session.IssuedDateTime < killSwitchThreshold.Value )
        {
            // Mark inactive; SaveHook stamps InactiveDateTime.
            session.IsActive = false;
            ( Context as RockContext ).SaveChanges();
            ExpireAuthCookie( requestContext );
            return null;
        }

        // Reissue triggers (any one fires reissue). Reissue MUST NOT touch
        // PersonSession.IssuedDateTime — only the cookie's iat changes.
        var halfLife = TimeSpan.FromTicks( AuthCookieTimeout.Ticks / 2 );
        var halfLifeReached = ( RockDateTime.Now - payload.IssuedAt ) >= halfLife;
        var olderPayloadVersion = payload.Version < CookiePayloadVersion;

        if ( halfLifeReached || metadata.DecryptedWithOldKey || olderPayloadVersion )
        {
            SetAuthCookie( session, requestContext );
        }

        return session;
    }

    /// <summary>
    /// Signs the current request out of its <see cref="PersonSession"/>. Marks
    /// the session inactive, clears the <c>.ROCK</c> cookie, and detaches the
    /// session from <paramref name="requestContext"/> so the remainder of this
    /// request observes the anonymous state without re-resolving from the
    /// cookie. The post-condition "this request is anonymous" holds whether or
    /// not a session was attached.
    /// </summary>
    /// <remarks>
    /// This is the single seam every logout path migrates onto. It deliberately
    /// does NOT regenerate the browser-session identifier — that stays the
    /// caller's responsibility (mirroring <c>Logout.cs</c>, which calls
    /// <c>RequestContext.RegenerateBrowserSessionId()</c> after sign-out), so a
    /// "re-login expected, keep the trail" caller (e.g. an MFA bounce-out) can
    /// opt out of regeneration while an explicit logout opts in.
    /// </remarks>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>.</param>
    [RockInternal( "20.0", keepInternalForever: true )]
    public void SignOut( RockRequestContext requestContext )
    {
        if ( requestContext == null )
        {
            throw new ArgumentNullException( nameof( requestContext ) );
        }

        var currentSession = requestContext.PersonSession;
        if ( currentSession == null )
        {
            // Already anonymous; nothing to sign out.
            return;
        }

        // Reload the session on this service's RockContext so the IsActive
        // flip + SaveHook run against a tracked entity (the context-attached
        // instance may have been resolved on a different, possibly disposed,
        // context). Mirrors the reload pattern in EndImpersonationAndRestore.
        var trackedSession = Get( currentSession.Guid );
        if ( trackedSession != null )
        {
            // SaveHook stamps InactiveDateTime when IsActive flips false.
            trackedSession.IsActive = false;
            ( Context as RockContext ).SaveChanges();
        }

        ExpireAuthCookie( requestContext );

        // Detach so downstream code in the same request observes the anonymous
        // state without re-resolving from the (now-expired) cookie.
        requestContext.SetPersonSession( null );
    }

    /// <summary>
    /// Marks every active <see cref="PersonSession"/> whose
    /// <see cref="PersonSession.ExpiresDateTime"/> has passed as inactive. Rows
    /// are flipped through the save pipeline (not a bulk SQL update) so the
    /// <c>PersonSession</c> save hook stamps <c>InactiveDateTime</c> in lockstep
    /// with <c>IsActive</c>. Rows are never deleted, preserving session history.
    /// Intended to be driven by the Rock Cleanup job; the work runs in batches
    /// against fresh <see cref="RockContext"/> instances so a large backlog does
    /// not accumulate in a single change tracker or exceed the command timeout.
    /// </summary>
    /// <param name="batchSize">The maximum number of rows to process per batch.</param>
    /// <param name="commandTimeout">The command timeout, in seconds, applied to each batch's <see cref="RockContext"/>.</param>
    /// <returns>The number of <see cref="PersonSession"/> rows marked inactive.</returns>
    internal static int MarkExpiredSessionsInactive( int batchSize, int commandTimeout )
    {
        var recordsUpdated = 0;

        // Capture "now" once so a row that expires while this run is in progress
        // is picked up by the next run rather than extending this one.
        var asOfDateTime = RockDateTime.Now;

        var keepGoing = true;
        while ( keepGoing )
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                rockContext.Database.SetCommandTimeout( commandTimeout );

                var expiredSessions = new PersonSessionService( rockContext )
                    .Queryable()
                    .Where( s => s.IsActive
                        && s.ExpiresDateTime.HasValue
                        && s.ExpiresDateTime.Value < asOfDateTime )
                    .OrderBy( s => s.Id )
                    .Take( batchSize )
                    .ToList();

                if ( !expiredSessions.Any() )
                {
                    break;
                }

                foreach ( var personSession in expiredSessions )
                {
                    // The save hook stamps InactiveDateTime when IsActive flips
                    // false. The row is intentionally not deleted.
                    personSession.IsActive = false;
                }

                rockContext.SaveChanges();
                recordsUpdated += expiredSessions.Count;

                // If we filled the batch there may be more expired rows.
                keepGoing = expiredSessions.Count == batchSize;
            }
        }

        return recordsUpdated;
    }

    /// <summary>
    /// Removes the <c>.ROCK</c> cookie from the client via
    /// <paramref name="requestContext"/>. Used when the resolved session is
    /// rejected (inactive, expired, kill-switch fire).
    /// </summary>
    private static void ExpireAuthCookie( RockRequestContext requestContext )
    {
        requestContext.Response.RemoveCookie( new BrowserCookie
        {
            Name = AuthCookieName,
            Path = "/",
            Domain = GetCookieDomain( requestContext ),
        } );
    }

    /// <summary>
    /// Computes the cookie <c>Domain</c> attribute from the current request's
    /// host and the <c>DOMAINS_SHARING_LOGINS</c> defined type. Returns
    /// <c>null</c> when no configured cross-subdomain entry matches (the
    /// cookie is then host-only).
    /// </summary>
    /// <remarks>
    /// Equivalent to <c>Authorization.GetCookieDomain()</c> in behavior but
    /// reads the host from <see cref="RockRequestContext"/> rather than
    /// <c>HttpContext.Current</c>. Drops the <c>FormsAuthentication.CookieDomain</c>
    /// fallback the legacy helper used (a host-only cookie is the safer modern
    /// default when no explicit domain is configured).
    /// </remarks>
    private static string GetCookieDomain( RockRequestContext requestContext )
    {
        var host = requestContext?.RequestUri?.Host;
        if ( host.IsNullOrWhiteSpace() )
        {
            return null;
        }

        var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.DOMAINS_SHARING_LOGINS.AsGuid() );
        var sharedDomains = definedType?.DefinedValues.Select( v => v.Value ).ToList() ?? [];

        // Get the first domain in the list that the current request's host
        // name ends with.
        var matchingDomain = sharedDomains.FirstOrDefault( d => host.ToLower().EndsWith( d.ToLower() ) );
        if ( matchingDomain.IsNullOrWhiteSpace() )
        {
            return null;
        }

        // Ensure the domain is prefixed with a '.' (required for cross-subdomain cookies).
        if ( !matchingDomain.StartsWith( "." ) )
        {
            matchingDomain = $".{matchingDomain}";
        }

        // Browsers require at least two '.' characters in a cookie Domain.
        return matchingDomain.Count( c => c == '.' ) >= 2 ? matchingDomain : null;
    }

    #endregion Cookie I/O

    #region Legacy Cookie Upgrade

    /// <summary>
    /// Upgrades a legacy <c>FormsAuthenticationTicket</c> on the current
    /// request to a new-format <see cref="PersonSession"/>. Called from
    /// <c>Application_PostAuthenticateRequest</c> after
    /// <c>FormsAuthenticationModule</c> has validated the legacy cookie and
    /// populated <c>HttpContext.Current.User</c> as a <c>FormsIdentity</c>;
    /// returns <c>null</c> when no legacy ticket is present (the common case
    /// once the dual-reader window has been open for a while).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bridge code introduced specifically to ferry legacy cookies forward
    /// into the <see cref="PersonSession"/> model during the dual-reader
    /// window. The method is <c>public</c> because <c>RockWeb</c>'s
    /// <c>Global.asax.cs</c> calls it across the assembly boundary; it is
    /// <c>[RockInternal]</c> (so plugins do not take a dependency on it)
    /// and <c>[Obsolete]</c> from day one. Removal is targeted around
    /// Rock v23 once the default forms-authentication cookie lifetime
    /// (30 days) has elapsed since the last release that issued legacy
    /// cookies.
    /// </para>
    /// <para>
    /// The body is fully wrapped in <c>#if WEBFORMS</c>: .NET Core has no
    /// <c>FormsAuthenticationTicket</c> to upgrade from, so the .NET Core
    /// branch returns <c>null</c> unconditionally. System.Web types are
    /// fully qualified inline to keep the rest of this file free of a
    /// <c>using System.Web;</c> directive (per the spec's "no System.Web
    /// in PersonSessionService" rule, of which this method is the one
    /// accepted deviation).
    /// </para>
    /// </remarks>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>.</param>
    /// <returns>The upgraded <see cref="PersonSession"/>, or <c>null</c> when no legacy ticket was present or the ticket could not be upgraded.</returns>
    [Obsolete( "Bridge code for the legacy FormsAuthenticationTicket cookie format. Will be removed once legacy cookie support is sunset (targeted around Rock v23)." )]
    [RockObsolete( "20.0" )]
    [RockInternal( "20.0", keepInternalForever: true )]
    public PersonSession UpgradeLegacyCookieForRequest( RockRequestContext requestContext )
    {
        if ( requestContext == null )
        {
            throw new ArgumentNullException( nameof( requestContext ) );
        }

#if WEBFORMS
        var formsIdentity = System.Web.HttpContext.Current?.User?.Identity as System.Web.Security.FormsIdentity;
        if ( formsIdentity?.Ticket == null )
        {
            return null;
        }

        return UpgradeLegacyTicket( formsIdentity.Ticket, requestContext );
#else
        return null;
#endif
    }

#if WEBFORMS

    /// <summary>
    /// Performs the legacy-cookie upgrade against a supplied
    /// <see cref="System.Web.Security.FormsAuthenticationTicket"/>. Carries
    /// the real upgrade logic so tests can synthesize a ticket directly
    /// without booting <c>HttpContext</c>; the public
    /// <see cref="UpgradeLegacyCookieForRequest"/> shim is a trivial
    /// wrapper around this helper.
    /// </summary>
    /// <remarks>
    /// Bridge code; removed alongside <see cref="UpgradeLegacyCookieForRequest"/>
    /// when legacy cookie support is sunset (targeted around Rock v23).
    /// The whole declaration is wrapped in <c>#if WEBFORMS</c> because the
    /// <c>FormsAuthenticationTicket</c> parameter type can only exist on
    /// the WebForms build.
    /// </remarks>
    /// <param name="ticket">The legacy <c>FormsAuthenticationTicket</c> to upgrade.</param>
    /// <param name="requestContext">The current <see cref="RockRequestContext"/>.</param>
    /// <returns>The upgraded <see cref="PersonSession"/>, or <c>null</c> when the ticket is impersonated, its <c>Name</c> does not resolve to a <see cref="UserLogin"/>, or upgrade is otherwise refused.</returns>
    [Obsolete( "Bridge code for the legacy FormsAuthenticationTicket cookie format. Will be removed once legacy cookie support is sunset (targeted around Rock v23)." )]
    [RockObsolete( "20.0" )]
    internal PersonSession UpgradeLegacyTicket( System.Web.Security.FormsAuthenticationTicket ticket, RockRequestContext requestContext )
    {
        if ( ticket == null )
        {
            throw new ArgumentNullException( nameof( ticket ) );
        }

        if ( requestContext == null )
        {
            throw new ArgumentNullException( nameof( requestContext ) );
        }

        // Step 1: Refuse impersonation tickets. Silently upgrading them
        // into long-lived PersonSession rows would extend impersonation
        // past its intended ("let me impersonate Ted real quick") lifetime.
        // The impersonator can simply re-impersonate after the rollout.
        var userData = Authorization.GetUserData( ticket );
        if ( userData?.IsImpersonated == true )
        {
            ExpireAuthCookie( requestContext );
            return null;
        }

        // Step 2: Refuse non-persistent tickets. The user unchecked
        // "remember me" at login, so the legacy cookie was a transient
        // session cookie that would have died with the browser. Every
        // PersonSession created by FindOrCreateLegacyUpgradeSession is
        // stamped IsPersistent = true, so silently upgrading would
        // promote a transient session to a long-lived one and contradict
        // the user's original choice. Drop the cookie and let the user
        // re-authenticate on the new format with whatever persistence
        // they prefer at that point.
        if ( !ticket.IsPersistent )
        {
            ExpireAuthCookie( requestContext );
            return null;
        }

        // Step 3: Resolve UserLoginId from the ticket's Name. The Name
        // field is the only identity carried on the wire by the legacy
        // ticket. If the user has been deleted since the cookie was
        // issued, refuse the upgrade and clear the stale cookie.
        var rockContext = Context as RockContext;
        var userLogin = new UserLoginService( rockContext ).GetByUserName( ticket.Name );
        if ( userLogin == null )
        {
            ExpireAuthCookie( requestContext );
            return null;
        }

        // Step 4: Find or create the Legacy PersonSession via the
        // composite key (UserLoginId, IssuedDateTime = ticket.IssueDate,
        // CreationSource = Legacy). Repeated presentations of the same
        // legacy cookie (notably from clients that do not honor Set-Cookie)
        // resolve to the same row across requests rather than spamming
        // new rows. Using the ticket's own IssueDate as the session's
        // IssuedDateTime also makes the RejectAuthenticationCookiesIssuedBefore
        // kill switch correct for upgraded sessions for free.
        var session = FindOrCreateLegacyUpgradeSession( requestContext, userLogin.Id, ticket.IssueDate );

        // Step 5: Emit the new-format cookie unconditionally. The standard
        // reissue-trigger logic (half-life, key rotation, payload version)
        // does not apply here because the source cookie is not new-format
        // — there is nothing to compare against.
        SetAuthCookie( session, requestContext );

        // Step 6: Return the upgraded session so the caller can replace
        // the FormsIdentity principal with one backed by the upgraded
        // session.
        return session;
    }

#endif

    #endregion Legacy Cookie Upgrade
}
