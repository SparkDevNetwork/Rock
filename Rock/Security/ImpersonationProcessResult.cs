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

using Rock.Model;

namespace Rock.Security;

/// <summary>
/// Return type for <c>PersonSessionService.ProcessImpersonationToken</c>.
/// Carries both the resulting session reference and the redirect signal so
/// callers do not have to fetch one and infer the other.
/// </summary>
/// <remarks>
/// Internal so the shape can evolve without a breaking-change cost on plugins.
/// Starting impersonation is a core-only operation; only core code drives
/// those state transitions.
/// </remarks>
internal class ImpersonationProcessResult
{
    /// <summary>
    /// <c>true</c> if the caller MUST redirect to a URL without the
    /// <c>rckipid</c> query parameter. Set for every rule defined in the
    /// <c>ProcessImpersonationToken</c> matrix, including the failure case
    /// where the token did not resolve to a valid session. Explicit (rather
    /// than implicit "always true") so future code paths that do not require
    /// redirect (AJAX, server-side fixtures) can produce a result with
    /// <see cref="IsRedirectRequired"/> = <c>false</c> without breaking the
    /// contract.
    /// </summary>
    public bool IsRedirectRequired { get; set; }

    /// <summary>
    /// The <see cref="PersonSession"/> the request is associated with after
    /// processing. <c>null</c> if the request is now anonymous (the token was
    /// invalid and there was no other auth context).
    /// </summary>
    public PersonSession Session { get; set; }
}
