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
/// How far a workflow variable reaches.
/// </summary>
/// <remarks>
/// The two are stored as Attribute rows against different entities with different
/// qualifiers, so the same key can legitimately exist in both scopes at once. That
/// is why the scope is reported rather than inferred: without it the two read as
/// duplicates of each other.
/// </remarks>
internal enum WorkflowAttributeScope
{
    /// <summary>
    /// Available everywhere in the workflow. Stored against the Workflow entity,
    /// qualified by the workflow type.
    /// </summary>
    Workflow = 0,

    /// <summary>
    /// Available only inside one activity. Stored against the WorkflowActivity
    /// entity, qualified by the activity type.
    /// </summary>
    Activity = 1
}
