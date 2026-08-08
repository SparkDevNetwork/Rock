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

using System.Collections.Generic;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// The form presented to a person when a workflow reaches a user entry action.
/// </summary>
internal class WorkflowActionFormResult : EntityResultBase
{
    /// <summary>
    /// Markup rendered above the form's fields.
    /// </summary>
    public string Header { get; set; }

    /// <summary>
    /// Indicates that <see cref="Header"/> was clipped because it exceeded the
    /// length a tree read returns. Omitted when the value is complete. The
    /// single-action tool returns it whole.
    /// </summary>
    public bool? IsHeaderTruncated { get; set; }

    /// <summary>
    /// Markup rendered below the form's fields.
    /// </summary>
    public string Footer { get; set; }

    /// <summary>
    /// Indicates that <see cref="Footer"/> was clipped. Omitted when the value is
    /// complete.
    /// </summary>
    public bool? IsFooterTruncated { get; set; }

    /// <summary>
    /// Indicates that the person may add notes when completing the form.
    /// </summary>
    public bool AllowNotes { get; set; }

    /// <summary>
    /// Indicates that the form collects person details as well as attribute
    /// values.
    /// </summary>
    public bool AllowPersonEntry { get; set; }

    /// <summary>
    /// The person entry configuration, returned only when
    /// <see cref="AllowPersonEntry"/> is true.
    /// </summary>
    /// <remarks>
    /// Omitted entirely on a form without person entry, which is the common case, so
    /// carrying it in the tree costs nothing there. Its markup fields clip in a tree
    /// read the same way the form's header and footer do.
    /// </remarks>
    public WorkflowFormPersonEntryResult PersonEntry { get; set; }

    /// <summary>
    /// The template used to notify the assigned person that the form is waiting.
    /// </summary>
    public KeyNameResult NotificationSystemCommunication { get; set; }

    /// <summary>
    /// The buttons shown at the bottom of the form, in order. These are what
    /// branch a workflow.
    /// </summary>
    public List<WorkflowFormButtonResult> Buttons { get; set; }

    /// <summary>
    /// The fields on the form, in order.
    /// </summary>
    /// <remarks>
    /// A flat list rather than a tree of sections. Sections are a Form Builder
    /// concept that the workflow editor cannot show, so this skill does not author
    /// them. A form that does have sections, because Form Builder created it, has
    /// its fields flattened into this list in section then field order.
    /// </remarks>
    public List<WorkflowFormFieldResult> Fields { get; set; }
}
