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

namespace Rock.Security;

/// <summary>
/// Side-channel metadata returned from <c>PersonSessionService.TryDecodeCookie</c>
/// alongside the decoded <see cref="PersonSessionCookiePayload"/>. Consumers
/// (chiefly <c>ResolveSessionForRequest</c>) read this to decide whether the
/// current response needs to reissue a fresh cookie even when the embedded
/// session is otherwise valid.
/// </summary>
internal class PersonSessionCookieDecodeMetadata
{
    /// <summary>
    /// <c>true</c> when the cookie was decrypted via an
    /// <c>OldDataEncryptionKey{n}</c> rotation fallback rather than the
    /// current <c>DataEncryptionKey</c>. Triggers a reissue with the current
    /// key so rotated keys drain out of circulation.
    /// </summary>
    public bool DecryptedWithOldKey { get; set; }

    /// <summary>
    /// The payload <c>v</c> value the cookie carried. Compared against the
    /// current <c>PersonSessionCookiePayload.Version</c> constant; an older
    /// value triggers a reissue at the current version.
    /// </summary>
    public int PayloadVersion { get; set; }
}
