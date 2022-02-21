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

using Rock.Attribute;

namespace Rock.Workflow.FormBuilder
{
    /// <summary>
    /// Provides additional settings used by forms built by the Form Builder that
    /// do not have database properties. This data is stored in JSON format into
    /// the <see cref="Rock.Model.WorkflowType.FormBuilderSettingsJson"/> property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public class FormSettings
    {
        /// <summary>
        /// The details about the confirmation e-mail that should be sent once
        /// the person has completed the form.
        /// </summary>
        public FormConfirmationEmailSettings ConfirmationEmail { get; set; }

        /// <summary>
        /// The details about the notification e-mail that should be sent once
        /// the person has completed the form.
        /// </summary>
        public FormNotificationEmailSettings NotificationEmail { get; set; }

        /// <summary>
        /// The action to take once the person has completed the form.
        /// </summary>
        public FormCompletionActionSettings CompletionAction { get; set; }

        /// <summary>
        /// Determines how <see cref="Rock.Model.Workflow.CampusId"/> will be
        /// automatically set when the form executes.
        /// </summary>
        public int? CampusSetFrom { get; set; }
    }
}
