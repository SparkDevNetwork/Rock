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

using Rock.Enums.CheckIn;
using Rock.Enums.Controls;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// The response that will be sent back from the EditFamily block action.
    /// </summary>
    public class EditFamilyResponseBag
    {
        /// <summary>
        /// The attributes that can be viewed and edited on the family.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> FamilyAttributes { get; set; }

        /// <summary>
        /// The attributes that can be viewed and edited on adults.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> AdultAttributes { get; set; }

        /// <summary>
        /// The attributes that can be viewed and edited on children.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> ChildAttributes { get; set; }

        /// <summary>
        /// The details of the family that is being edited.
        /// </summary>
        public ValidPropertiesBox<RegistrationFamilyBag> Family { get; set; }

        /// <summary>
        /// The details of the people in the family that are being edited.
        /// </summary>
        public List<ValidPropertiesBox<RegistrationPersonBag>> People { get; set; }

        /// <summary>
        /// Determines if the Alternate Id field is visible when editing adults.
        /// </summary>
        public bool IsAlternateIdFieldVisibleForAdults { get; set; }

        /// <summary>
        /// Determines if the Alternate Id field is visible when editing children.
        /// </summary>
        public bool IsAlternateIdFieldVisibleForChildren { get; set; }

        /// <summary>
        /// Determines if the SMS Enabled/Disabled button is visible.
        /// </summary>
        public bool IsSmsButtonVisible { get; set; }

        /// <summary>
        /// Determines if default state of the SMS button.
        /// </summary>
        public bool IsSmsButtonCheckedByDefault { get; set; }

        /// <summary>
        /// Determines if check-in should be allowed after registration has
        /// been completed.
        /// </summary>
        public bool IsCheckInAfterRegistrationAllowed { get; set; }

        /// <summary>
        /// Determines for which people the name suffix should be displayed
        /// when adding a new individual.
        /// </summary>
        public AdultsOrChildrenSelectionMode DisplaySuffix { get; set; }

        /// <summary>
        /// Determines how the birthdate field will be displayed for adults.
        /// </summary>
        public RequirementLevel DisplayBirthdateForAdults { get; set; }

        /// <summary>
        /// Determines how the birthdate field will be displayed for children.
        /// </summary>
        public RequirementLevel DisplayBirthdateForChildren { get; set; }

        /// <summary>
        /// Determines how the ethnicity field will be displayed for adults.
        /// </summary>
        public RequirementLevel DisplayEthnicityForAdults { get; set; }

        /// <summary>
        /// Determines how the ethnicity field will be displayed for children.
        /// </summary>
        public RequirementLevel DisplayEthnicityForChildren { get; set; }

        /// <summary>
        /// Determines how the grade will be displayed for children.
        /// </summary>
        public RequirementLevel DisplayGradeForChildren { get; set; }

        /// <summary>
        /// Determines the requirement level for displaying the mobile phone
        /// input on children. The field can be set to be hidden, be visible
        /// but optional, or be visible and required.
        /// </summary>
        public RequirementLevel DisplayMobilePhoneForChildren { get; set; }

        /// <summary>
        /// Determines how the race will be displayed for adults.
        /// </summary>
        public RequirementLevel DisplayRaceForAdults { get; set; }

        /// <summary>
        /// Determines how the race will be displayed for children.
        /// </summary>
        public RequirementLevel DisplayRaceForChildren { get; set; }

        /// <summary>
        /// Determines which suffixes are available in the UI.
        /// </summary>
        public List<ListItemBag> Suffixes { get; set; }

        /// <summary>
        /// Determines which relationships (like "can check-in") are available
        /// in the UI.
        /// </summary>
        public List<ListItemBag> Relationships { get; set; }

        /// <summary>
        /// Specifies the default relationship that new children will have
        /// towards adults.
        /// </summary>
        public ListItemBag ChildRelationship { get; set; }

        /// <summary>
        /// Forces the selection of known relationship types when adding a new
        /// child to a family in the check-in registration process. Normally the
        /// "Child" option is selected by default, but this setting can be used
        /// to require the user to explicitly select a relationship type.
        /// </summary>
        public bool ForceSelectionOfKnownRelationshipType { get; set; }

        /// <summary>
        /// Defines the minimum age for a child to be considered at an age where
        /// the grade is expected to be filled in. If a child is greater than or
        /// equal to this age and a grade has not been filled in, then a dialog
        /// should pop up to prompt the person to either fill in a grade or to
        /// verify that the child is not yet in school.
        /// </summary>
        public decimal? GradeConfirmationAge { get; set; }
    }
}
