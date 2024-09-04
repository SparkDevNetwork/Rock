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

using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.CheckIn.Config.CheckinTypeDetail
{
    /// <summary>
    /// The item details for the Check-In Type Detail block Registration Settings.
    /// </summary>
    public class CheckInRegistrationSettingsBag
    {
        /// <summary>
        /// Gets or sets the registration default person connection status.
        /// </summary>
        /// <value>
        /// The registration default person connection status.
        /// </value>
        public ListItemBag RegistrationDefaultPersonConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [registration display alternate identifier field for adults].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [registration display alternate identifier field for adults]; otherwise, <c>false</c>.
        /// </value>
        public bool RegistrationDisplayAlternateIdFieldForAdults { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [registration display alternate identifier field for children].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [registration display alternate identifier field for children]; otherwise, <c>false</c>.
        /// </value>
        public bool RegistrationDisplayAlternateIdFieldForChildren { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [registration display SMS enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [registration display SMS enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool RegistrationDisplaySmsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [registration SMS enabled by default].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [registration SMS enabled by default]; otherwise, <c>false</c>.
        /// </value>
        public bool RegistrationSmsEnabledByDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable check in after registration].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable check in after registration]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableCheckInAfterRegistration { get; set; }

        /// <summary>
        /// Gets or sets the known relationship types.
        /// </summary>
        /// <value>
        /// The known relationship types.
        /// </value>
        public List<string> KnownRelationshipTypes { get; set; }

        /// <summary>
        /// Gets or sets the same family known relationship types.
        /// </summary>
        /// <value>
        /// The same family known relationship types.
        /// </value>
        public List<string> SameFamilyKnownRelationshipTypes { get; set; }

        /// <summary>
        /// Gets or sets the can check in known relationship types.
        /// </summary>
        /// <value>
        /// The can check in known relationship types.
        /// </value>
        public List<string> CanCheckInKnownRelationshipTypes { get; set; }

        /// <summary>
        /// Gets or sets the registration add family workflow types.
        /// </summary>
        /// <value>
        /// The registration add family workflow types.
        /// </value>
        public List<ListItemBag> RegistrationAddFamilyWorkflowTypes { get; set; }

        /// <summary>
        /// Gets or sets the registration add person workflow types.
        /// </summary>
        /// <value>
        /// The registration add person workflow types.
        /// </value>
        public List<ListItemBag> RegistrationAddPersonWorkflowTypes { get; set; }

        /// <summary>
        /// Gets or sets the registration required attributes for adults.
        /// </summary>
        /// <value>
        /// The registration required attributes for adults.
        /// </value>
        public List<string> RegistrationRequiredAttributesForAdults { get; set; }

        /// <summary>
        /// Gets or sets the registration optional attributes for adults.
        /// </summary>
        /// <value>
        /// The registration optional attributes for adults.
        /// </value>
        public List<string> RegistrationOptionalAttributesForAdults { get; set; }

        /// <summary>
        /// Gets or sets the registration required attributes for children.
        /// </summary>
        /// <value>
        /// The registration required attributes for children.
        /// </value>
        public List<string> RegistrationRequiredAttributesForChildren { get; set; }

        /// <summary>
        /// Gets or sets the registration optional attributes for children.
        /// </summary>
        /// <value>
        /// The registration optional attributes for children.
        /// </value>
        public List<string> RegistrationOptionalAttributesForChildren { get; set; }

        /// <summary>
        /// Gets or sets the registration required attributes for families.
        /// </summary>
        /// <value>
        /// The registration required attributes for families.
        /// </value>
        public List<string> RegistrationRequiredAttributesForFamilies { get; set; }

        /// <summary>
        /// Gets or sets the registration optional attributes for families.
        /// </summary>
        /// <value>
        /// The registration optional attributes for families.
        /// </value>
        public List<string> RegistrationOptionalAttributesForFamilies { get; set; }

        /// <summary>
        /// Gets or sets the registration display birthdate on adults.
        /// </summary>
        /// <value>
        /// The registration display birthdate on adults.
        /// </value>
        public string RegistrationDisplayBirthdateOnAdults { get; set; }

        /// <summary>
        /// Gets or sets the registration display birthdate on children.
        /// </summary>
        /// <value>
        /// The registration display birthdate on children.
        /// </value>
        public string RegistrationDisplayBirthdateOnChildren { get; set; }

        /// <summary>
        /// Gets or sets the registration display grade on children.
        /// </summary>
        /// <value>
        /// The registration display grade on children.
        /// </value>
        public string RegistrationDisplayGradeOnChildren { get; set; }

        /// <summary>
        /// Gets or sets the registration display race on adults.
        /// </summary>
        /// <value>
        /// The registration display race on adults.
        /// </value>
        public string RegistrationDisplayRaceOnAdults { get; set; }

        /// <summary>
        /// Gets or sets the registration display ethnicity on adults.
        /// </summary>
        /// <value>
        /// The registration display ethnicity on adults.
        /// </value>
        public string RegistrationDisplayEthnicityOnAdults { get; set; }

        /// <summary>
        /// Gets or sets the registration display race on children.
        /// </summary>
        /// <value>
        /// The registration display race on children.
        /// </value>
        public string RegistrationDisplayRaceOnChildren { get; set; }

        /// <summary>
        /// Gets or sets the registration display ethnicity on children.
        /// </summary>
        /// <value>
        /// The registration display ethnicity on children.
        /// </value>
        public string RegistrationDisplayEthnicityOnChildren { get; set; }
    }
}
