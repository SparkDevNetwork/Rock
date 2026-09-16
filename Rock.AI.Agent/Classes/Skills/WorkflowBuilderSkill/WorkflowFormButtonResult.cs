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

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// One button on a workflow form, and the activity it starts when clicked.
/// </summary>
/// <remarks>
/// Buttons are not their own entity. They are stored as a delimited string on the
/// form, which is why this carries no key of its own and why the activity it
/// activates is identified separately.
/// </remarks>
internal class WorkflowFormButtonResult
{
    /// <summary>
    /// The button caption.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The defined value identifying the button's visual style.
    /// </summary>
    public Guid? ButtonStyleGuid { get; set; }

    /// <summary>
    /// The friendly name of the button's visual style.
    /// </summary>
    public string ButtonStyleName { get; set; }

    /// <summary>
    /// The key of the activity this button activates, or <c>null</c> when it
    /// simply submits the form.
    /// </summary>
    public string ActivateActivityIdKey { get; set; }

    /// <summary>
    /// The name of the activity this button activates, so a caller can read the
    /// branch without a second lookup.
    /// </summary>
    public string ActivateActivityName { get; set; }

    /// <summary>
    /// The message shown after the button is clicked.
    /// </summary>
    public string ResponseText { get; set; }
}
