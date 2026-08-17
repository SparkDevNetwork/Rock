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

namespace Rock.AI.Agent.Classes.Skills.WorkflowSkill;

/// <summary>
/// Represents the result of launching a workflow, containing relevant
/// information about the workflow's execution.
/// </summary>
internal class LaunchWorkflowResult : EntityResultBase
{
    /// <summary>
    /// The name of the workflow.
    /// </summary>
    public string Name { get; set; }
}
