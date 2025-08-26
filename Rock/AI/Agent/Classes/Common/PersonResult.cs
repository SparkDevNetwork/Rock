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
using System.Text.Json.Serialization;

using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Model;

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// A common POCO for storing information about a person. Apply only the properties that are needed for the specific use case.
    /// Null properties will not be serialized.
    /// </summary>
    public class PersonResult
    {
        #region Ignored Properties
        // These properties exist to help with internal logic but they should not be serialized to JSON.

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        [JsonIgnore]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the primary family identifier.
        /// </summary>
        [JsonIgnore]
        public int? PrimaryFamilyId { get; set; }

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        [JsonIgnore]
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the record type value identifier.
        /// </summary>
        [JsonIgnore]
        public int? RecordTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        [JsonIgnore]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the marital status unique identifier.
        /// </summary>
        [JsonIgnore]
        public Guid? MaritalStatusGuid { get; set; }
        #endregion

        #region Common Properties
        /// <summary>
        /// Gets or sets the stable identifier for the person (used by functions; avoid showing to end users).
        /// </summary>
        public string PersonIdKey {
            get
            {
                return this.Id.AsIdKey();
            }
        }

        /// <summary>
        /// Gets or sets the stable identifier for the person's primary family (used by functions; avoid showing to end users).
        /// </summary>
        public string PrimaryFamilyIdKey { get; set; }

        /// <summary>
        /// The URL to the person's internal profile.
        /// </summary>
        public string ProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets the person's first/given name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's nickname.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the person's middle name.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the person's last/family name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the list of previous last names.
        /// </summary>
        public List<string> PreviousLastNames { get; set; }

        /// <summary>
        /// Gets or sets the person's name suffix.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the person's e-mail.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the list of phone numbers.
        /// </summary>
        public List<PhoneNumberResult> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets the URL for the person's avatar image.
        /// </summary>
        public string AvatarUrl
        {
            get
            {
                var initials = this.FirstName.Left( 1 ) + this.LastName.Left( 1 );
                return Rock.Model.Person.GetPersonPhotoUrl(
                    initials,
                    this.PhotoId,
                    this.Age,
                    this.Gender,
                    this.RecordTypeValueId,
                    this.AgeClassification );
            }
        }

        /// <summary>
        /// Gets or sets the list of addresses.
        /// </summary>
        public List<LocationResult> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the age classification.
        /// </summary>
        public AgeClassification AgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the spouse person result.
        /// </summary>
        public PersonResult Spouse { get; set; }

        /// <summary>
        /// Gets or sets the list of children in the family.
        /// </summary>
        public List<PersonResult> ChildrenInFamily { get; set; }

        /// <summary>
        /// Gets or sets the list of adults in the family.
        /// </summary>
        public List<PersonResult> AdultsInFamily { get; set; }

        /// <summary>
        /// Gets or sets the campus name.
        /// </summary>
        public string Campus { get; set; }

        /// <summary>
        /// Gets the campus key (hashed identifier).
        /// </summary>
        public string CampusKey
        {
            get
            {
                if ( !this.CampusId.HasValue )
                {
                    return null;
                }

                return this.CampusId.Value.AsIdKey();
            }
        }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the record status.
        /// </summary>
        public string RecordStatus { get; set; }

        /// <summary>
        /// Gets or sets the marital status.
        /// </summary>
        public string MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the birth month (1-12).
        /// </summary>
        public int? BirthMonth { get; set; }

        /// <summary>
        /// Gets or sets the birth day of month (1-31).
        /// </summary>
        public int? BirthDay { get; set; }

        /// <summary>
        /// Gets or sets the birth year.
        /// </summary>
        public int? BirthYear { get; set; }

        /// <summary>
        /// Gets or sets the anniversary date.
        /// </summary>
        public DateTime? AnniversaryDate { get; set; }

        /// <summary>
        /// Gets or sets the graduation year.
        /// </summary>
        public int? GraduationYear { get; set; }

        /// <summary>
        /// Attributes of the defined value.
        /// </summary>
        public List<AttributeResult> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the known relationships (e.g. Aunt, Uncle, Grandparent, etc.) where the key is the relationship name and the value is the related person.
        /// </summary>
        public Dictionary<string,PersonResult> KnownRelationships { get; set; }

        #endregion
    }
}
