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
/// One action an entity supports securing, such as View, Edit, or Administrate.
/// </summary>
internal class AuthorizationActionResult
{
    /// <summary>
    /// The action name, used wherever an authorization tool takes an action.
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// A human-readable description of what the action governs.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Indicates that a person with no matching rule is allowed the action by
    /// default. When false, a person with no matching rule is denied.
    /// </summary>
    public bool IsAllowedByDefault { get; set; }
}
