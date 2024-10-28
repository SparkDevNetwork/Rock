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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Identifies all the settings related to configuring the Person Entry
    /// section of a FormBuilder form.
    /// </summary>
    public class FormPersonEntryViewModel
    {
        /// <summary>
        /// Indicates if the form should auto-fill values from the Person
        /// that is currently logged in.
        /// </summary>
        public bool AutofillCurrentPerson { get; set; }

        /// <summary>
        /// Indicates if the form should be hidden when a Person
        /// is already logged in and known.
        /// </summary>
        public bool HideIfCurrentPersonKnown { get; set; }

        /// <summary>
        /// The DefinedValue unique identifier that specifies the value used for
        /// Person.RecordStatusValue when a new Person is created.
        /// </summary>
        public Guid? RecordStatus { get; set; }

        /// <summary>
        /// The DefinedValue unique identifier that specifies the value used for
        /// Person.ConnectionStatusValue when a new Person is created.
        /// </summary>
        public Guid? ConnectionStatus { get; set; }

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
        /// The DefinedValue unique identifier for the campus type used to
        /// filter Campuses when displaying the campus picker.
        /// </summary>
        public Guid? CampusType { get; set; }

        /// <summary>
        /// The DefinedValue unique identifier for the campus status used to filter
        /// Campuses when displaying the campus picker.
        /// </summary>
        public Guid? CampusStatus { get; set; }

        /// <summary>
        /// Determines if the gender control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public FormFieldVisibility Gender { get; set; }

        /// <summary>
        /// Determines if the e-mail control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public FormFieldVisibility Email { get; set; }

        /// <summary>
        /// Determines if the mobile phone control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public FormFieldVisibility MobilePhone { get; set; }

        /// <summary>
        /// Determines if the SmsOptIn control should be hidden, optional, or required when displaying on the person entry form.
        /// </summary>
        public FormFieldShowHide SmsOptIn { get; set; }

        /// <summary>
        /// Determines if the birthdate control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public FormFieldVisibility Birthdate { get; set; }

        /// <summary>
        /// Determines if the address control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public FormFieldVisibility Address { get; set; }

        /// <summary>
        /// The DefinedValue unique identifier that specifies which address
        /// type will be used on the person entry form.
        /// </summary>
        public Guid? AddressType { get; set; }

        /// <summary>
        /// Determines if the martial status control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public FormFieldVisibility MaritalStatus { get; set; }

        /// <summary>
        /// Determines if the spouse controls should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public FormFieldVisibility SpouseEntry { get; set; }

        /// <summary>
        /// The text string that is used above the spouse entry controls to
        /// indicate that the following controls are for the spouse.
        /// </summary>
        public string SpouseLabel { get; set; }

        /// <summary>
        /// Determines if the race control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public FormFieldVisibility RaceEntry { get; set; }

        /// <summary>
        /// Determines if the ethnicity control should be hidden, optional or
        /// required when displaying the person entry form.
        /// </summary>
        public FormFieldVisibility EthnicityEntry { get; set; }
    }
}
