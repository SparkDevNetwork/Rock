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

using Rock.ViewModel.NonEntities;

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// Custom <see cref="ListItemViewModel"/> that extends the item to include
    /// details about what forced settings the template has.
    /// </summary>
    public class FormTemplateListItemViewModel : ListItemViewModel
    {
        /// <summary>
        /// Gets or sets the form header content that will be displayed above
        /// the form.
        /// </summary>
        /// <value>The form header content that will be displayed above the form.</value>
        public string FormHeader { get; set; }

        /// <summary>
        /// Gets or sets the form footer content that will be displayed below
        /// the form.
        /// </summary>
        /// <value>The form footer content that will be displayed below the form.</value>
        public string FormFooter { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the template has forced the
        /// configuration of the login required setting.
        /// </summary>
        /// <value><c>true</c> if the template forces the login required setting; otherwise <c>false</c>.</value>
        public bool IsLoginRequiredConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the template has forced the
        /// configuration of the person entry settings.
        /// </summary>
        /// <value><c>true</c> if the template forces the person entry settings; otherwise <c>false</c>.</value>
        public bool IsPersonEntryConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the template has forced the
        /// configuration of the confirmation email.
        /// </summary>
        /// <value><c>true</c> if the template forces the confirmation email settings; otherwise <c>false</c>.</value>
        public bool IsConfirmationEmailConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the template has forced the
        /// configuration of the completion action.
        /// </summary>
        /// <value><c>true</c> if the template forces the completion action settings; otherwise <c>false</c>.</value>
        public bool IsCompletionActionConfigured { get; set; }
    }
}
