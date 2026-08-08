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

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// One installed workflow action component, meaning a kind of action that can be
/// added to an activity.
/// </summary>
/// <remarks>
/// This deliberately does not derive from EntityResultBase. An action component
/// has no identity of its own; its EntityType is the identity, which is why the
/// key is named for the entity type rather than being a bare IdKey.
/// </remarks>
internal class WorkflowActionComponentResult
{
    /// <summary>
    /// The key of the component's entity type. This is what identifies the
    /// component when adding an action or asking for its settings.
    /// </summary>
    public string EntityTypeIdKey { get; set; }

    /// <summary>
    /// The full class name of the component. Returned so a caller can recognize
    /// an action it reads elsewhere and match it to knowledge pack articles. It
    /// is never accepted as a parameter.
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    /// The display name of the action, taken from the component's export
    /// metadata.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The category the action is grouped under. This comes from component
    /// metadata rather than the Category entity, so it is a plain string.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// The description of what the action does.
    /// </summary>
    public string Description { get; set; }
}
