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

using Rock.Enums.Security;

namespace Rock.Model;

public partial class PersonSession
{
    /// <summary>
    /// Whether the session represents an impersonated identity — either
    /// admin-initiated impersonation (<see cref="PersonSessionCreationSource.Impersonation"/>)
    /// or a user-token email-link flow (<see cref="PersonSessionCreationSource.UserToken"/>).
    /// </summary>
    /// <returns><c>true</c> when session represents some form of impersonated identity; otherwise <c>false</c>.</returns>
    public bool IsImpersonated()
    {
        return CreationSource == PersonSessionCreationSource.Impersonation
            || CreationSource == PersonSessionCreationSource.UserToken;
    }

    /// <summary>
    /// Computes the <see cref="AuthenticationStrength"/> of this session from
    /// its recency timestamps. Strongest applicable value wins: when both the
    /// step-up and MFA windows are satisfied, this returns
    /// <see cref="AuthenticationStrength.MultiFactor"/>.
    /// </summary>
    /// <returns>The strongest <see cref="AuthenticationStrength"/> the session can attest to right now.</returns>
    public AuthenticationStrength GetAuthenticationStrength()
    {
        if ( !IsActive )
        {
            return AuthenticationStrength.NotAuthenticated;
        }

        if ( LastMultiFactorAuthenticationDateTime.HasValue
            && LastMultiFactorAuthenticationDateTime.Value >= PersonSessionService.GetMultiFactorAuthenticationThreshold() )
        {
            return AuthenticationStrength.MultiFactor;
        }

        if ( LastStepUpAuthenticationDateTime.HasValue
            && LastStepUpAuthenticationDateTime.Value >= PersonSessionService.GetElevatedAuthenticationThreshold() )
        {
            return AuthenticationStrength.Elevated;
        }

        return AuthenticationStrength.Authenticated;
    }
}
