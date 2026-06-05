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

using Rock.Attribute;
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
    internal PersonSession StartComponentSession( RockRequestContext requestContext, int personAliasId, int userLoginId, int authComponentEntityTypeId, bool isPersistent, DateTime? mfaRecency = null )
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
    /// <param name="userLoginId">The identifier of the <see cref="UserLogin"/> representing the Api Key.</param>
    /// <returns>The first matching active session for the Api Key.</returns>
    private PersonSession FindActiveApiKeySession( int userLoginId )
    {
        return Queryable()
            .Where( s => s.UserLoginId == userLoginId
                && s.CreationSource == PersonSessionCreationSource.ApiKey
                && s.IsActive )
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
    /// prior <see cref="PersonSession"/>. The caller is responsible for
    /// reissuing the <c>.ROCK</c> cookie to point at the returned session.
    /// <c>SaveChanges</c> is called to mark the current session as inactive.
    /// </summary>
    /// <param name="session">The impersonation <see cref="PersonSession"/> to end.</param>
    /// <returns>The impersonator's prior session, or <c>null</c> if either restore reference was dangling.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="session"/>'s <see cref="PersonSession.CreationSource"/> is anything other than <see cref="PersonSessionCreationSource.Impersonation"/>.</exception>
    internal PersonSession EndImpersonationAndRestore( PersonSession session )
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
            return null;
        }

        return priorSession;
    }

    #endregion Impersonation Query Helpers

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

        // Step 5: Emit the new-format cookie unconditionally. The Phase 4
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
