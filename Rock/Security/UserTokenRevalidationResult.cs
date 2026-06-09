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

namespace Rock.Security;

/// <summary>
/// Result of <c>PersonSessionService.RevalidateUserTokenSession</c>: the
/// per-request page-scope re-validation hook for active
/// <see cref="PersonSessionCreationSource.UserToken"/> sessions.
/// </summary>
/// <remarks>
/// A small sentinel enum rather than a bool so a future "user must
/// re-authenticate" outcome can be added without changing call sites.
/// Internal so the shape can evolve without a breaking-change cost on
/// plugins; the revalidation hook itself is internal too.
/// </remarks>
internal enum UserTokenRevalidationResult
{
    /// <summary>
    /// Either there is no active <see cref="PersonSessionCreationSource.UserToken"/>
    /// session on the request (the common case) or the source
    /// <c>PersonToken</c> is still valid and in scope. The caller should
    /// continue with the request as normal.
    /// </summary>
    Ok,

    /// <summary>
    /// The source <c>PersonToken</c> has been revoked, expired, or has
    /// exceeded its <c>UsageLimit</c> since the session was created. The
    /// session has been marked inactive and the cookie has been expired;
    /// the caller should treat the request as anonymous from here on.
    /// </summary>
    SessionRevoked,

    /// <summary>
    /// The source <c>PersonToken</c> is page-scoped and the current
    /// request targets a different page. The session is left active (the
    /// recipient can still return to the in-scope page), but the caller
    /// MUST refuse access to the current page with a not-authorized
    /// response. Matches the spec's
    /// "Per-request page-scope re-validation fails; not-authorized
    /// error" outcome.
    /// </summary>
    PageScopeMiss,
}
