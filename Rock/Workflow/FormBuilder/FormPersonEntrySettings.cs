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
using Rock.Model;

namespace Rock.Workflow.FormBuilder
{
    /// <summary>
    /// Identifies all the settings related to configuring the Person Entry
    /// section of a FormBuilder form.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.13.2" )]
    public class FormPersonEntrySettings
    {
        /// <summary>
        /// Indicates if the form should auto-fill values from the <see cref="Rock.Model.Person"/>
        /// that is currently logged in.
        /// </summary>
        public bool AutofillCurrentPerson { get; set; }

        /// <summary>
        /// Indicates if the form should be hidden when a <see cref="Rock.Model.Person"/>
        /// is already logged in an known.
        /// </summary>
        public bool HideIfCurrentPersonKnown { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.DefinedValue"/> identifier that specifies the
        /// value used for <see cref="Rock.Model.Person.RecordStatusValue"/>
        /// when a new <see cref="Rock.Model.Person"/> is created.
        /// </summary>
        public int? RecordStatusValueId { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.DefinedValue"/> identifier that specifies the
        /// value used for <see cref="Rock.Model.Person.ConnectionStatusValue"/>
        /// when a new <see cref="Rock.Model.Person"/> is created.
        /// </summary>
        public int? ConnectionStatusValueId { get; set; }

        /// <summary>
        /// Indicates if the campus picker should be shown on the person entry
        /// form. The campus picker will always be required if it is visible.
        /// </summary>
        public bool ShowCampus { get; set; }

        /// <summary>
        /// Indicates if the campus picker on the person entry form should include Inactive Campuses.
        /// Defaulting to true as it was the existing behavior before this option was introduced.
        /// </summary>
        public bool IncludeInactiveCampus { get; set; } = true;

        /// <summary>
        /// The <see cref="Rock.Model.DefinedValue"/> identifier for the campus
        /// type used to filter <see cref="Rock.Model.Campus">Campuses</see>
        /// when displaying the campus picker.
        /// </summary>
        public int? CampusTypeValueId { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.DefinedValue"/> identifier for the campus
        /// status used to filter <see cref="Rock.Model.Campus">Campuses</see>
        /// when displaying the campus picker.
        /// </summary>
        public int? CampusStatusValueId { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.DefinedValue"/> identifier for the person's race
        /// </summary>
        public int? RaceValueId { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.DefinedValue"/> identifier for the person's ethnicity
        /// </summary>
        public int? EthnicityValueId { get; set; }

        /// <summary>
        /// Determines if the gender control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormPersonEntryOption Gender { get; set; }

        /// <summary>
        /// Determines if the email control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormPersonEntryOption Email { get; set; }

        /// <summary>
        /// Determines if the mobile phone control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormPersonEntryOption MobilePhone { get; set; }

        /// <summary>
        /// Determines if the SMS Opt-In contol shold be hidden, optional or required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormShowHideOption SmsOptIn { get; set; }

        /// <summary>
        /// Determines if the birthdate control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormPersonEntryOption Birthdate { get; set; }

        /// <summary>
        /// Determines if the address control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormPersonEntryOption Address { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.DefinedValue"/> identifier that specifies
        /// which address type will be used on the person entry form.
        /// </summary>
        public int? AddressTypeValueId { get; set; }

        /// <summary>
        /// Determines if the martial status control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormPersonEntryOption MaritalStatus { get; set; }

        /// <summary>
        /// Determines if the spouse controls should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormPersonEntryOption SpouseEntry { get; set; }

        /// <summary>
        /// The text string that is used above the spouse entry controls to
        /// indicate that the following controls are for the spouse.
        /// </summary>
        public string SpouseLabel { get; set; }

        /// <summary>
        /// Determines if the race picker should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormPersonEntryOption RaceEntry { get; set; }

        /// <summary>
        /// Determines if the ethnicity picker should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public WorkflowActionFormPersonEntryOption EthnicityEntry { get; set; }
    }
}
