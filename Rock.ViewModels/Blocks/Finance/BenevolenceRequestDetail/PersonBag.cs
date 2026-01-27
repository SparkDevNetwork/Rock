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

using System;

namespace Rock.ViewModels.Blocks.Finance.BenevolenceRequestDetail
{
    /// <summary>
    /// Represents a collection of personal information for an individual, including identifiers, contact details, and
    /// names <u>that are relevevant in the context of a benevolence request.</u>
    /// </summary>
    /// <remarks>The <c>PersonBag</c> class is used to store and manage various attributes related to a
    /// person, such as their unique identifier, connection status, and contact information.</remarks>
    public class PersonBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for a person.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the alias identifier for a person.
        /// </summary>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the person's alias.
        /// </summary>
        public Guid PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the Id representing the Person's connection status
        /// </summary>
        /// <value>
        /// An Id representing the Person's connection status.
        /// </value>
        public ConnectionStatusBag ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the URL of the photo associated with the entity.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the nick name of the Person.  If a nickname was not entered, the first name is used.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the nick name of the Person.
        /// </value>
        /// <remarks>
        /// The name that the person goes by.
        /// </remarks>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the first name of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the first name of the Person.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name (Sir Name) of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Last Name of the Person.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the location information.
        /// </summary>
        public LocationBag Location { get; set; }

        /// <summary>
        /// Gets or sets the Home Phone Number of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="PhoneNumberBag"/> representing the Home Phone Number of the person who requested benevolence.
        /// </value>
        public PhoneNumberBag HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Cell Phone Number of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="PhoneNumberBag"/> representing the Cell Phone Number of the person who requested benevolence.
        /// </value>
        public PhoneNumberBag CellPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Work Phone Number of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="PhoneNumberBag"/> representing the Work Phone Number of the person who requested benevolence.
        /// </value>
        public PhoneNumberBag WorkPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person requesting benevolence.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the email address of the person requesting benevolence.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the GovernmentId of the person who requested benevolence.
        /// </summary>
        /// <value>
        /// A string representing the GovernmentId of the person who requested benevolence.
        /// </value>
        public string GovernmentId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the race.
        /// </summary>
        public Guid RaceGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the ethnicity.
        /// </summary>
        public Guid EthnicityGuid { get; set; }
    }
}
