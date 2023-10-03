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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.FamilyPreRegistration
{
    /// <summary>
    /// The box that contains all the initialization information for the Family Pre-Registration block.
    /// </summary>
    public class FamilyPreRegistrationInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the adult 1 information.
        /// </summary>
        public FamilyPreRegistrationPersonBag Adult1 { get; set; }

        /// <summary>
        /// Gets or sets the adult 2 information.
        /// </summary>
        public FamilyPreRegistrationPersonBag Adult2 { get; set; }

        /// <summary>
        /// Filters the campus field by campus types.
        /// </summary>
        public List<Guid> CampusTypesFilter { get; set; }

        /// <summary>
        /// Filters the campus field by campus statuses.
        /// </summary>
        public List<Guid> CampusStatusesFilter { get; set; }

        /// <summary>
        /// Gets or sets the attribute unique identifier used to select campus schedules.
        /// </summary>
        public Guid? CampusSchedulesAttributeGuid { get; set; }

        /// <summary>
        /// The campus unique identifier.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the visit information title.
        /// </summary>
        public string VisitInfoTitle { get; set; }

        /// <summary>
        /// Gets or sets the create account title.
        /// </summary>
        public string CreateAccountTitle { get; set; }

        /// <summary>
        /// Gets or sets the create account description.
        /// </summary>
        public string CreateAccountDescription { get; set; }

        /// <summary>
        /// Gets or sets the family attributes.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> FamilyAttributes { get; set; }

        /// <summary>
        /// Gets or sets the family attribute values.
        /// </summary>
        public Dictionary<string, string> FamilyAttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the child relationship types.
        /// </summary>
        public List<ListItemBag> ChildRelationshipTypes { get; set; }

        /// <summary>
        /// Gets or sets the child attributes.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> ChildAttributes { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<FamilyPreRegistrationPersonBag> Children { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets the display SMS opt in.
        /// </summary>
        public FamilyPreRegistrationSmsOptInFieldBag DisplaySmsOptIn { get; set; }

        /// <summary>
        /// Gets or sets the campus field.
        /// </summary>
        public FamilyPreRegistrationFieldBag CampusField { get; set; }

        /// <summary>
        /// Gets or sets the adult mobile phone field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AdultMobilePhoneField { get; set; }

        /// <summary>
        /// Gets or sets the adult profile photo field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AdultProfilePhotoField { get; set; }

        /// <summary>
        /// Gets or sets the create account field.
        /// </summary>
        public FamilyPreRegistrationFieldBag CreateAccountField { get; set; }

        /// <summary>
        /// Gets or sets the address field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AddressField { get; set; }

        /// <summary>
        /// Gets or sets the adult gender field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AdultGenderField { get; set; }

        /// <summary>
        /// Gets or sets the adult suffix field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AdultSuffixField { get; set; }

        /// <summary>
        /// Gets or sets the adult birthday field.
        /// </summary>
        public FamilyPreRegistrationDatePickerFieldBag AdultBirthdayField { get; set; }

        /// <summary>
        /// Gets or sets the adult email field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AdultEmailField { get; set; }

        /// <summary>
        /// Gets or sets the visit date field.
        /// </summary>
        public FamilyPreRegistrationDateAndTimeFieldBag VisitDateField { get; set; }

        /// <summary>
        /// Gets or sets the adult marital status field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AdultMaritalStatusField { get; set; }

        /// <summary>
        /// Gets or sets the adult communication preference field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AdultCommunicationPreferenceField { get; set; }

        /// <summary>
        /// Gets or sets the adult race field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AdultRaceField { get; set; }

        /// <summary>
        /// Gets or sets the adult ethnicity field.
        /// </summary>
        public FamilyPreRegistrationFieldBag AdultEthnicityField { get; set; }

        /// <summary>
        /// Gets or sets the child gender field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildGenderField { get; set; }

        /// <summary>
        /// Gets or sets the child birth date field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildBirthDateField { get; set; }

        /// <summary>
        /// Gets or sets the child grade field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildGradeField { get; set; }

        /// <summary>
        /// Gets or sets the child mobile phone field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildMobilePhoneField { get; set; }

        /// <summary>
        /// Gets or sets the child email field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildEmailField { get; set; }

        /// <summary>
        /// Gets or sets the child profile photo field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildProfilePhotoField { get; set; }

        /// <summary>
        /// Gets or sets the child race field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildRaceField { get; set; }

        /// <summary>
        /// Gets or sets the child ethnicity field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildEthnicityField { get; set; }

        /// <summary>
        /// Gets or sets the child suffix field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildSuffixField { get; set; }

        /// <summary>
        /// Gets or sets the child communication preference field.
        /// </summary>
        public FamilyPreRegistrationFieldBag ChildCommunicationPreferenceField { get; set; }
    }
}
