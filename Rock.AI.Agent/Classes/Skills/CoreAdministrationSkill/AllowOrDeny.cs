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

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// Whether an authorization rule grants or denies access. This is the explicit
/// form of the single-character value stored on an <c>Auth</c> record
/// (<c>"A"</c> or <c>"D"</c>). It exists only for the authorization tools, so it
/// lives with the skill rather than in the core enum catalog.
/// </summary>
internal enum AllowOrDeny
{
    /// <summary>
    /// The rule grants access.
    /// </summary>
    Allow = 0,

    /// <summary>
    /// The rule denies access.
    /// </summary>
    Deny = 1,
}
