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

using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// A registration for a person from the check-in kiosk.
    /// </summary>
    public class RegistrationPersonBag
    {
        /// <summary>
        /// The encrypted identifier of the person. When saving, this value
        /// should be an empty string if this is a new person to be created.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The nick name the person is known by.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// The last name of the individual.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// An optional suffix to distinguish between individuals with the
        /// same name in the same family.
        /// </summary>
        public ListItemBag Suffix { get; set; }

        /// <summary>
        /// <c>true</c> if this individual is an adult; otherwise <c>false</c>.
        /// All adults will be placed in the same family group.
        /// </summary>
        public bool IsAdult { get; set; }

        /// <summary>
        /// <c>true</c> if this adult is married. If set to <c>false</c> then
        /// they will be marked as single.
        /// </summary>
        public bool IsMarried { get; set; }

        /// <summary>
        /// The known gender of the individual.
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// The date of birth for the individual.
        /// </summary>
        public DateTimeOffset? BirthDate { get; set; }

        /// <summary>
        /// The main contact e-mail address for the individual.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The grade of the individual. This is encoded as the grade offset
        /// value converted to a string.
        /// </summary>
        public ListItemBag Grade { get; set; }

        /// <summary>
        /// The phone number for this individual. Currently this is the mobile
        /// phone number but in the future this may become configurable.
        /// </summary>
        public PhoneNumberBoxWithSmsControlBag PhoneNumber { get; set; }

        /// <summary>
        /// The primary check-in alternate identifier for this individual.
        /// </summary>
        public string AlternateId { get; set; }

        /// <summary>
        /// The configured race value for the individual.
        /// </summary>
        public ListItemBag Race { get; set; }

        /// <summary>
        /// The configured ethnicity value for the individual.
        /// </summary>
        public ListItemBag Ethnicity { get; set; }

        /// <summary>
        /// The Person record status of the individual.
        /// </summary>
        public ListItemBag RecordStatus { get; set; }

        /// <summary>
        /// The configured connection status for the individual.
        /// </summary>
        public ListItemBag ConnectionStatus { get; set; }

        /// <summary>
        /// The relationship to the adults in the family. A value of <c>null</c>
        /// implies no relationship because they are in the same family. Other
        /// configured relationships may also indicate being in the same family.
        /// </summary>
        public ListItemBag RelationshipToAdult { get; set; }

        /// <summary>
        /// The person attribute values that have been configured to be viewed
        /// or edited on the kiosk.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
