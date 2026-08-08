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

using System.ComponentModel;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// One button to place on a workflow form.
/// </summary>
/// <remarks>
/// Buttons are how a workflow branches: each can activate a different activity,
/// so the set of buttons on a form is the set of paths out of it.
/// </remarks>
internal class WorkflowFormButtonInput
{
    /// <summary>
    /// The button caption. Required.
    /// </summary>
    [Description( "The button caption, such as Approve or Deny." )]
    public string Name { get; set; }

    /// <summary>
    /// The key of the defined value for the button's visual style. Defaults to
    /// the primary style when omitted.
    /// </summary>
    [Description( "The key of the defined value for the button's visual style." )]
    public string ButtonStyleDefinedValueIdKey { get; set; }

    /// <summary>
    /// The key of the activity this button activates. Omit for a button that
    /// only submits the form.
    /// </summary>
    [Description( "The key of the activity this button activates. Omit for a button that only submits the form." )]
    public string ActivateActivityTypeIdKey { get; set; }

    /// <summary>
    /// The message shown after the button is clicked.
    /// </summary>
    [Description( "The message shown to the person after the button is clicked." )]
    public string ResponseText { get; set; }
}
