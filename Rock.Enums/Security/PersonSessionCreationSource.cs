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
/// How a <c>PersonSession</c> row was created.
/// </summary>
public enum PersonSessionCreationSource
{
    /// <summary>
    /// It is unknown how the session was created. This value should not
    /// normally be used; it is reserved for situation where we truly just
    /// don't know how this session came to be.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Regular authentication via an <c>AuthenticationComponent</c> (web
    /// login, mobile login, TV login, Auth0, or any other
    /// <c>IExternalRedirectAuthentication</c> provider).
    /// </summary>
    Component = 1,

    /// <summary>
    /// Admin-initiated impersonation, restorable to the impersonator's
    /// prior session via <c>EndImpersonationAndRestore()</c>.
    /// </summary>
    Impersonation = 2,

    /// <summary>
    /// User-facing token (for example, an <c>rckipid</c> email link). Not
    /// restorable; there is no prior impersonator session to revert to.
    /// </summary>
    UserToken = 3,

    /// <summary>
    /// Long-lived session tied to a <c>UserLogin</c> whose <c>ApiKey</c>
    /// property is set. Reused across all requests for that key.
    /// </summary>
    ApiKey = 4,

    /// <summary>
    /// Created during the legacy <c>FormsAuthenticationTicket</c> cookie
    /// upgrade. Isolates the upgrade row from real <see cref="Component"/>
    /// sessions so the composite-key lookup cannot collide with a live
    /// session for the same user.
    /// </summary>
    Legacy = 5,
}
