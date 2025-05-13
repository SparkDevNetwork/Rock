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
using Rock.ViewModels.Utility;
using Rock.ViewModels.Controls;
using Rock.Model;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The set of data that represents a Person for the Person Basic Editor control to use.
    /// </summary>
    public class PersonBasicEditorBag : IValidPropertiesBox
    {
        /// <summary>
        /// Person's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Person's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Person's title
        /// </summary>
        public ListItemBag PersonTitle { get; set; }

        /// <summary>
        /// Person's suffix
        /// </summary>
        public ListItemBag PersonSuffix { get; set; }

        /// <summary>
        /// Person's relationship status
        /// </summary>
        public ListItemBag PersonMaritalStatus { get; set; }

        /// <summary>
        /// Person's grade offset
        /// </summary>
        public ListItemBag PersonGradeOffset { get; set; }

        /// <summary>
        /// Person's family group role
        /// </summary>
        public ListItemBag PersonGroupRole { get; set; }

        /// <summary>
        /// Person's connectedness to the organization/church
        /// </summary>
        public ListItemBag PersonConnectionStatus { get; set; }

        /// <summary>
        /// Person's gender
        /// </summary>
        public Gender? PersonGender { get; set; }

        /// <summary>
        /// Person's race
        /// </summary>
        public ListItemBag PersonRace { get; set; }

        /// <summary>
        /// Person's ethnicity
        /// </summary>
        public ListItemBag PersonEthnicity { get; set; }

        /// <summary>
        /// Person's date of birth
        /// </summary>
        public DatePartsPickerValueBag PersonBirthDate { get; set; }

        /// <summary>
        /// Person's email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Person's mobile phone number
        /// </summary>
        public string MobilePhoneNumber { get; set; }

        /// <summary>
        /// Person's mobile phone's country code
        /// </summary>
        public string MobilePhoneCountryCode { get; set; }

        /// <summary>
        /// Whether SMS messaging is permitted to the person's mobile phone
        /// </summary>
        public bool? IsMessagingEnabled { get; set; }

        /// <inheritdocs/>
        public List<string> ValidProperties { get; set; }
    }
}
