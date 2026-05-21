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

using Rock.Enums.Security;
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

    #endregion Impersonation Query Helpers
}
