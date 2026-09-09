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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// A single global attribute as it appears in a list. Identity only; the field
/// type, description, categories, and current value come from the detail tool.
/// </summary>
internal class GlobalAttributeResult : EntityResultBase
{
    /// <summary>
    /// The programmatic key of the global attribute. This is what Lava and code
    /// reference, such as <c>OrganizationName</c>.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The friendly name of the global attribute.
    /// </summary>
    public string Name { get; set; }
}
