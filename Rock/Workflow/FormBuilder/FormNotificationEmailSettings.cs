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
    /// Contains details about a notification e-mail for a Form Builder form.
    /// This specifies if one should be sent, who receives it and the content
    /// it will contain.
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
    public class FormNotificationEmailSettings
    {
        /// <summary>
        /// Specifies if the notification e-mail has been enabled and should be
        /// sent.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Determines the destination recipient type for this notification e-mail.
        /// This also determines which other properties are valid.
        /// </summary>
        public FormNotificationEmailDestination Destination { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.PersonAlias"/> identifier of the person that
        /// will receive the e-mail.
        /// </summary>
        /// <remarks>
        /// This property is only used if <see cref="Destination"/> contains the
        /// value <see cref="FormNotificationEmailDestination.SpecificIndividual"/>.
        /// </remarks>
        public int? RecipientAliasId { get; set; }

        /// <summary>
        /// Contains the e-mail addresses that will receive the notification
        /// e-mail. Multiple addresses may be separated with a comma.
        /// </summary>
        /// <remarks>
        /// This property is only used if <see cref="Destination"/> contains the
        /// value <see cref="FormNotificationEmailDestination.EmailAddress"/>.
        /// </remarks>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Contains the campus topic <see cref="Rock.Model.DefinedValue"/>
        /// identifier that will determine who receives the e-mail. This is
        /// used in conjunction with <see cref="Rock.Model.Workflow.CampusId"/>
        /// to find the specific recipient.
        /// </summary>
        /// <remarks>
        /// This property is only used if <see cref="Destination"/> contains the
        /// value <see cref="FormNotificationEmailDestination.CampusTopic"/>.
        /// </remarks>
        public int? CampusTopicValueId { get; set; }

        /// <summary>
        /// Determines how the content of the e-mail will be generated.
        /// </summary>
        public FormEmailSourceSettings Source { get; set; }
    }
}
