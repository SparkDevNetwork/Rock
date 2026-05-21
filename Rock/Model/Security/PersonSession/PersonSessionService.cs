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
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

using Microsoft.Azure.Amqp.Framing;

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Net;
using Rock.Security;

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

    #endregion Constants

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
}
