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

using System.Collections.Generic;

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// The settings that describe a single form.
    /// </summary>
    public class FormSettingsViewModel
    {
        /// <summary>
        /// The HTML content that will be displayed before all the sections of
        /// the form.
        /// </summary>
        public string HeaderContent { get; set; }

        /// <summary>
        /// The HTML content that will be displayed after all sections of the
        /// form.
        /// </summary>
        public string FooterContent { get; set; }

        /// <summary>
        /// The list of sections that exist in this form, including all of the
        /// fields.
        /// </summary>
        public List<FormSectionViewModel> Sections { get; set; }

        /// <summary>
        /// The general settings about this form.
        /// </summary>
        public FormGeneralViewModel General { get; set; }

        /// <summary>
        /// The settings that describe the confirmation e-mail to be sent when
        /// this form is submitted.
        /// </summary>
        public FormConfirmationEmailViewModel ConfirmationEmail { get; set; }

        /// <summary>
        /// The settings that describe the notification e-mail to be sent when
        /// this form is submitted.
        /// </summary>
        public FormNotificationEmailViewModel NotificationEmail { get; set; }

        /// <summary>
        /// The action to perform after this form is submitted.
        /// </summary>
        public FormCompletionActionViewModel Completion { get; set; }

        /// <summary>
        /// Determines how the form's campus context will be set when it first
        /// runs.
        /// </summary>
        public int? CampusSetFrom { get; set; }

        /// <summary>
        /// Determines if the person entry section should be displayed at the
        /// top of the form.
        /// </summary>
        public bool AllowPersonEntry { get; set; }

        /// <summary>
        /// The settings that describe how the person entry section will be
        /// displayed.
        /// </summary>
        public FormPersonEntryViewModel PersonEntry { get; set; }
    }
}
