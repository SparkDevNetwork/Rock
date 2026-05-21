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

namespace Rock.Enums.Security;

/// <summary>
/// Describes how strongly the current request is authenticated. Reported by
/// <c>PersonSession.GetAuthenticationStrength()</c> and consumed by
/// <c>RockRequestContext.MeetsRequirement(AuthenticationRequirement)</c>.
/// </summary>
/// <remarks>
/// The reported strength is the strongest one that applies: when both the
/// step-up and MFA recency windows are satisfied, the session reports
/// <see cref="MultiFactor"/>, not <see cref="Elevated"/>.
/// </remarks>
public enum AuthenticationStrength
{
    /// <summary>
    /// No active session, or the underlying session is inactive. Safe
    /// default. Rarely returned by the entity method directly (callers
    /// typically reach this state via a null session).
    /// </summary>
    NotAuthenticated = 0,

    /// <summary>
    /// The session is authenticated, but neither the step-up nor MFA
    /// recency window is satisfied.
    /// </summary>
    Authenticated = 1,

    /// <summary>
    /// The person provided a credential (password, SMS, TOTP, etc.) within
    /// the step-up recency window.
    /// </summary>
    Elevated = 2,

    /// <summary>
    /// The person provided MFA (primary credential plus a second factor,
    /// entered concurrently) within the MFA recency window.
    /// </summary>
    MultiFactor = 3,
}
