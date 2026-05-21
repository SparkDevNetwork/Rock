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
using System.Text.Json.Serialization;

namespace Rock.Security;

/// <summary>
/// Plaintext payload encoded inside the encrypted <c>.ROCK</c> cookie. Keys are
/// minified (<c>v</c> / <c>sid</c> / <c>iat</c>) to keep cumulative
/// request-header weight down.
/// </summary>
/// <remarks>
/// This must be encoded and decoded with <c>System.Text.Json</c> instead of
/// <see cref="Newtonsoft.Json" />. This was chosen for performance reasons as
/// this sits on the hot path.
/// </remarks>
internal class PersonSessionCookiePayload
{
    /// <summary>
    /// Payload schema version. Bumps <strong>only</strong> on breaking changes
    /// to existing field meanings. Additive fields land alongside without a
    /// version bump (JSON's natural forward-compatibility carries them).
    /// </summary>
    [JsonPropertyName( "v" )]
    public int Version { get; set; }

    /// <summary>
    /// The <c>PersonSession.Guid</c> that this cookie represents.
    /// </summary>
    [JsonPropertyName( "sid" )]
    public Guid SessionGuid { get; set; }

    /// <summary>
    /// Issued-at timestamp for <strong>this cookie</strong>, distinct from
    /// <c>PersonSession.IssuedDateTime</c>. Drives the sliding-expiration
    /// reissue cadence: cookies whose <c>iat</c> is older than
    /// <c>PersonSessionService.AuthCookieTimeout / 2</c> are reissued on the
    /// response.
    /// </summary>
    [JsonPropertyName( "iat" )]
    public DateTime IssuedAt { get; set; }
}
