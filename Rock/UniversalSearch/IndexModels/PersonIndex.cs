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
using System.Linq;

using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Person Index
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexModels.IndexModelBase" />
    public class PersonIndex : IndexModelBase
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [RockIndexField( Boost = 3 )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        [RockIndexField( Boost = 3 )]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the last name.  
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [RockIndexField( Boost = 3.5 )] // gives slight nudge to last name over first name
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        /// <value>
        /// The suffix.
        /// </value>
        [RockIndexField]
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the previous last names.
        /// </summary>
        /// <value>
        /// The previous last names.
        /// </value>
        [RockIndexField]
        public string PreviousLastNames { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the spouse.
        /// </summary>
        /// <value>
        /// The spouse.
        /// </value>
        [RockIndexField]
        public string Spouse { get; set; }

        /// <summary>
        /// Gets or sets the connection status value identifier.
        /// </summary>
        /// <value>
        /// The connection status value identifier.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public int? ConnectionStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the record status value identifier.
        /// </summary>
        /// <value>
        /// The record status value identifier.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public int? RecordStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        /// <value>
        /// The street address.
        /// </value>
        [RockIndexField]
        public string StreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [RockIndexField]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [RockIndexField]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [RockIndexField]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [RockIndexField]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        [RockIndexField]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the family role.
        /// </summary>
        /// <value>
        /// The family role.
        /// </value>
        [RockIndexField]
        public string FamilyRole { get; set; }

        /// <summary>
        /// Gets or sets the photo URL.
        /// </summary>
        /// <value>
        /// The photo URL.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [RockIndexField ( Index = IndexType.NotAnalyzed, Analyzer = "whitespace")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone numbers.
        /// </summary>
        /// <value>
        /// The phone numbers.
        /// </value>
        [RockIndexField]
        public string PhoneNumbers { get; set; }

        /// <summary>
        /// Gets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public override string IconCssClass
        {
            get
            {
                return "fa fa-user";
            }
        }

        /// <summary>
        /// Loads the by model.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static PersonIndex LoadByModel(Person person )
        {
            var personIndex = new PersonIndex();
            try
            {
                personIndex.SourceIndexModel = "Rock.Model.Person";
                personIndex.ModelConfiguration = "nofilters";

                personIndex.Id = person.Id;
                personIndex.FirstName = person.FirstName;
                personIndex.NickName = person.NickName;
                personIndex.LastName = person.LastName;

                personIndex.ModelOrder = 10;

                if (person.SuffixValue != null)
                {
                    personIndex.Suffix = person.SuffixValue.Value;
                }

                var campuses = person.GetCampusIds();

                if (campuses != null && campuses.Count > 0)
                {
                    personIndex.CampusId = campuses.FirstOrDefault();
                }

                personIndex.ConnectionStatusValueId = person.ConnectionStatusValueId;
                personIndex.RecordStatusValueId = person.RecordStatusValueId;
                personIndex.PreviousLastNames = string.Join(",", person.GetPreviousNames().Select(n => n.LastName));
                personIndex.Age = person.Age;
                personIndex.Gender = person.Gender.ToString();
                personIndex.PhotoUrl = person.PhotoUrl;
                personIndex.Email = person.Email;
                personIndex.DocumentName = person.FullName;

                if (person.PhoneNumbers != null)
                {
                    personIndex.PhoneNumbers = string.Join("|", person.PhoneNumbers.Select(p => p.NumberTypeValue.Value + "^" + p.Number));
                }

                // get family role
                var familyRole = person.GetFamilyRole();

                if (familyRole != null)
                {
                    personIndex.FamilyRole = familyRole.Name;
                }

                // get home address
                var address = person.GetHomeLocation();

                if (address != null)
                {
                    personIndex.StreetAddress = address.Street1 + " " + address.Street2;
                    personIndex.City = address.City;
                    personIndex.State = address.State;
                    personIndex.PostalCode = address.PostalCode;
                    personIndex.Country = address.Country;
                }

                // get spouse
                var spouse = person.GetSpouse();

                if (spouse != null)
                {
                    personIndex.Spouse = person.GetSpouse().FullName;
                }

                AddIndexableAttributes(personIndex, person);
            }
            catch (Exception) { }

            return personIndex;
        }
    }
}
