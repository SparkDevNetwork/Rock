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

namespace Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;

/// <summary>
/// One security role an audience can name, as listed by ResolveAudience.
/// The name is the exact value to pass in the audiences parameter of
/// AddOrUpdateLavaApplication.
/// </summary>
internal class SecurityRoleResult
{
    /// <summary>
    /// The name of the security role, exactly as AddOrUpdateLavaApplication
    /// expects it.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the security role, when one has been entered.
    /// </summary>
    public string Description { get; set; }
}
