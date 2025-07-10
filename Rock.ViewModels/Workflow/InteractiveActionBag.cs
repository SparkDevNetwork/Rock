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

using System;
using System.Collections.Generic;

using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Workflow
{
    /// <summary>
    /// The details about a single Workflow action with an interactive user
    /// experience that should be displayed.
    /// </summary>
    public class InteractiveActionBag
    {
        /// <summary>
        /// The title to display in the panel for this workflow.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The CSS icon class to display in the panel for this workflow.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// The workflow unique identifier. May be <c>null</c> if the workflow
        /// has not been persisted yet.
        /// </summary>
        public Guid? WorkflowGuid { get; set; }

        /// <summary>
        /// The URL to use for updating the URL in the browser so that the URL
        /// can be copied by the individual if required.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The prefixed identifier of this workflow. Will be <c>null</c> or
        /// empty if it has not been persisted yet.
        /// </summary>
        public string PrefixedId { get; set; }

        /// <summary>
        /// The message to display if the UI for the action is not available.
        /// </summary>
        public string NoActionMessage { get; set; }

        /// <summary>
        /// The date this workflow was created. Will be <c>null</c> if it
        /// has not been persisted yet.
        /// </summary>
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Determines if the note panel should be visible and allow note entry.
        /// </summary>
        public bool IsNotesVisible { get; set; }

        /// <summary>
        /// The list of current notes to be displayed.
        /// </summary>
        public List<NoteBag> Notes { get; set; }

        /// <summary>
        /// The list of note types that are valid for this workflow.
        /// </summary>
        public List<NoteTypeBag> NoteTypes { get; set; }

        /// <summary>
        /// The date and time this action was displayed. This should be passed
        /// back when submitting component data to update interaction duration.
        /// </summary>
        public DateTimeOffset? ActionStartDateTime { get; set; }

        /// <summary>
        /// The unique identifier of the action type that will provide the
        /// user interface to display.
        /// </summary>
        public Guid? ActionTypeGuid { get; set; }

        /// <summary>
        /// The unique identifier of the component that is handling processing
        /// for the current action.
        /// </summary>
        public Guid? ActionComponentGuid { get; set; }

        /// <summary>
        /// The data that will be used to display the interactive action or
        /// the result of the action's execution.
        /// </summary>
        public InteractiveActionDataBag ActionData { get; set; }
    }
}
