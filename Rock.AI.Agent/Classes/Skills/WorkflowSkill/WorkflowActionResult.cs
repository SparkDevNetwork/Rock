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

using System;

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.WorkflowSkill;

/// <summary>
/// A single action within a workflow activity. Actions do not carry attribute
/// values, so this reports only their processing state.
/// </summary>
internal class WorkflowActionResult : EntityResultBase
{
    /// <summary>
    /// The name of the action type this action was created from.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Indicates whether the action has completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// The date and time the action was last processed, or <c>null</c> when it
    /// has never been processed.
    /// </summary>
    public DateTime? LastProcessedDateTime { get; set; }

    /// <summary>
    /// The date and time the action completed, or <c>null</c> when it has not
    /// completed.
    /// </summary>
    public DateTime? CompletedDateTime { get; set; }

    /// <summary>
    /// The form action (button) that was taken to complete this action, when the
    /// action is a form.
    /// </summary>
    public string FormAction { get; set; }
}
