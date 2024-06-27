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
using Rock.Enums.Blocks.Crm.FamilyPreRegistration;
using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.FamilyPreRegistration
{
    /// <summary>
    /// The bag that contains all the person request information for the Family Pre-Registration block.
    /// </summary>
    public class FamilyPreRegistrationPersonBag
    {
        /// <summary>
        /// Gets or sets the person unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the suffix defined value unique identifier.
        /// </summary>
        public Guid? SuffixDefinedValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        public BirthdayPickerBag BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the mobile phone.
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets a value to set PhoneNumber.IsMessagingEnabled for the specified mobie number
        /// </summary>
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the mobile phone country code.
        /// </summary>
        public string MobilePhoneCountryCode { get; set; }

        /// <summary>
        /// Gets or sets the communication preference.
        /// </summary>
        public CommunicationPreference CommunicationPreference { get; set; }

        /// <summary>
        /// Gets or sets the race defined value unique identifier.
        /// </summary>
        public Guid? RaceDefinedValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the ethnicity defined value unique identifier.
        /// </summary>
        public Guid? EthnicityDefinedValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the profile photo unique identifier.
        /// </summary>
        public Guid? ProfilePhotoGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is first name read only.
        /// </summary>
        public bool IsFirstNameReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is last name read only.
        /// </summary>
        public bool IsLastNameReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the marital status defined value unique identifier.
        /// </summary>
        public Guid? MaritalStatusDefinedValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the grade defined value unique identifier.
        /// </summary>
        public Guid? GradeDefinedValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the family role unique identifier.
        /// </summary>
        public Guid? FamilyRoleGuid { get; set; }

        // LPC CODE
        /// <summary>
        /// Gets or sets the allergy attribute value.
        /// </summary>
        public string Allergy {  get; set; }

        /// <summary>
        /// Gets or sets the Self Release attribute value.
        /// </summary>
        public bool IsSelfRelease { get; set; }
        // END LPC CODE
    }
}
