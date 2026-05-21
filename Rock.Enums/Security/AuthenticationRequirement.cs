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
/// Authentication requirement that a block or API can enforce on the
/// current request via <c>RockRequestContext.MeetsRequirement(...)</c>.
/// </summary>
/// <remarks>
/// Kept separate from <see cref="AuthenticationStrength"/> so the requirement
/// set can grow independently. Future entries (for example, a trusted-network
/// or device-bound requirement) describe properties of the request that
/// have no analog on the strength side.
/// </remarks>
public enum AuthenticationRequirement
{
    /// <summary>
    /// Caller requires a recent (re-)authentication. Satisfied by an
    /// <see cref="AuthenticationStrength.Elevated"/> or
    /// <see cref="AuthenticationStrength.MultiFactor"/> session.
    /// </summary>
    Elevated = 0,

    /// <summary>
    /// Caller requires a recent MFA event. Satisfied only by an
    /// <see cref="AuthenticationStrength.MultiFactor"/> session.
    /// </summary>
    MultiFactor = 1,
}
