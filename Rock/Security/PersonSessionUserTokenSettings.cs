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

namespace Rock.Security;

/// <summary>
/// Source-token reference persisted on a <c>PersonSession</c> with
/// <c>CreationSource = UserToken</c>.
/// </summary>
internal class PersonSessionUserTokenSettings
{
    /// <summary>
    /// The source <c>PersonToken.Guid</c>. The value the per-request
    /// page-scope re-validation reads against the source <c>PersonToken</c>
    /// row to enforce page scope, expiration, and revocation.
    /// </summary>
    public Guid OriginatingPersonTokenGuid { get; set; }
}
