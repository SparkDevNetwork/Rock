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
/// A single entity type.
/// </summary>
/// <remarks>
/// This is the only list result in the skill that carries a Guid, and only
/// because it has no matching detail tool. There is no GetEntityType, so
/// dropping the Guid here would put it out of reach entirely rather than one
/// call away. If a detail tool is ever added, the Guid moves to it.
/// </remarks>
internal class EntityTypeResult : EntityResultBase
{
    /// <summary>
    /// The full class name of the entity type, for example
    /// <c>Rock.Model.Workflow</c>. Output only; no tool accepts a class name as
    /// a parameter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The human readable name of the entity type.
    /// </summary>
    public string FriendlyName { get; set; }

    /// <summary>
    /// Indicates that this is a Rock model rather than a component such as a
    /// field type or workflow action. This is a plain column, so it tells a
    /// caller the two apart without any classification machinery.
    /// </summary>
    public bool IsEntity { get; set; }

    /// <summary>
    /// Indicates that instances of this entity type carry their own security.
    /// </summary>
    public bool IsSecured { get; set; }
}
