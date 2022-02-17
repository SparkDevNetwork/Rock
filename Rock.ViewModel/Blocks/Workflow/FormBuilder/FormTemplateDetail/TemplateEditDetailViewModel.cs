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

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder.FormTemplateDetail
{
    /// <summary>
    /// Representation of the form template that provides the required information
    /// to make edits to an existing form template or to create a new one.
    /// </summary>
    public class TemplateEditDetailViewModel
    {
        /// <summary>
        /// Gets or sets the name of the form template.
        /// </summary>
        /// <value>The name of the form template.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the descriptive purpose for the form template.
        /// </summary>
        /// <value>The descriptive purpose for the form template.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if this form template is active
        /// and should show up as a selection item in lists.
        /// </summary>
        /// <value>
        /// <c>true</c> if this form template is active and should show up as a
        /// selection item in lists; otherwise <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the form will require the
        /// individual to be logged in before they can fill out the form.
        /// </summary>
        /// <value>
        /// <c>true</c> if the individual must be logged in before they can
        /// fill out the form; <c>false</c> if the setting on the form should
        /// be used instead.
        /// </value>
        public bool IsLoginRequired { get; set; }

        /// <summary>
        /// Gets or sets the HTML content that will be displayed before the
        /// form is displayed.
        /// </summary>
        /// <value>The HTML content that will be displayed before the form.</value>
        public string FormHeader { get; set; }

        /// <summary>
        /// Gets or sets the HTML content that will be displayed after the form
        /// is displayed.
        /// </summary>
        /// <value>The HTML content that will be displayed after the form.</value>
        public string FormFooter { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if person entry is enabled for
        /// all forms using this template.
        /// </summary>
        /// <value>
        /// <c>true</c> if all forms using this template will have a person
        /// entry; otherwise <c>false</c>.
        /// </value>
        public bool AllowPersonEntry { get; set; }

        /// <summary>
        /// Gets or sets the configuration options for the person entry.
        /// </summary>
        /// <value>The configuration options for the person entry.</value>
        public FormPersonEntryViewModel PersonEntry { get; set; }

        /// <summary>
        /// Gets or sets the configuration options for sending a confirmation e-mail.
        /// </summary>
        /// <value>The configuration options for sending a confirmation e-mail.</value>
        public FormConfirmationEmailViewModel ConfirmationEmail { get; set; }

        /// <summary>
        /// Gets or sets the configuration options for the completion action.
        /// </summary>
        /// <value>The configuration options for the completion action.</value>
        public FormCompletionActionViewModel CompletionAction { get; set; }
    }
}
