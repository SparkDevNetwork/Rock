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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Registration Helper Class
    /// </summary>
    [Serializable]
    public class RegistrationInfo
    {
        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        public int? RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the slots available.
        /// </summary>
        /// <value>
        /// The slots available.
        /// </value>
        public int? SlotsAvailable { get; set; }

        /// <summary>
        /// Gets or sets your first name.
        /// </summary>
        /// <value>
        /// Your first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets your last name.
        /// </summary>
        /// <value>
        /// Your last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        /// <value>
        /// The family unique identifier.
        /// </value>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the confirmation email.
        /// </summary>
        /// <value>
        /// The confirmation email.
        /// </value>
        public string ConfirmationEmail { get; set; }

        /// <summary>
        /// Gets or sets the discount code.
        /// </summary>
        /// <value>
        /// The discount code.
        /// </value>
        public string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage.
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount.
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        public decimal DiscountedCost { get; set; }

        /// <summary>
        /// Gets or sets the previous payment total.
        /// </summary>
        /// <value>
        /// The previous payment total.
        /// </value>
        public decimal PreviousPaymentTotal { get; set; }

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        /// <value>
        /// The payment amount.
        /// </value>
        public decimal? PaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the registrants.
        /// </summary>
        /// <value>
        /// The registrants.
        /// </value>
        public List<RegistrantInfo> Registrants { get; set; }

        /// <summary>
        /// Gets the registrant count.
        /// </summary>
        /// <value>
        /// The registrant count.
        /// </value>
        public int RegistrantCount
        {
            get { return Registrants.Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
        /// </summary>
        public RegistrationInfo()
        {
            FamilyGuid = Guid.Empty;
            Registrants = new List<RegistrantInfo>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        public RegistrationInfo( Person person )
            : this()
        {
            if ( person != null )
            {
                FirstName = person.NickName;
                LastName = person.LastName;
                ConfirmationEmail = person.Email;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo" /> class.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        public RegistrationInfo( Registration registration, RockContext rockContext )
            : this()
        {
            if ( registration != null )
            {
                RegistrationId = registration.Id;
                if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                {
                    FirstName = registration.PersonAlias.Person.NickName;
                    LastName = registration.PersonAlias.Person.LastName;
                    ConfirmationEmail = registration.ConfirmationEmail;
                }

                DiscountCode = registration.DiscountCode != null ? registration.DiscountCode.Trim() : string.Empty;
                DiscountPercentage = registration.DiscountPercentage;
                DiscountAmount = registration.DiscountAmount;
                TotalCost = registration.TotalCost;
                DiscountedCost = registration.DiscountedCost;
                SlotsAvailable = registration.Registrants.Where( r => !r.OnWaitList ).Count();

                if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                {
                    var family = registration.PersonAlias.Person.GetFamily( rockContext );
                    if ( family != null )
                    {
                        FamilyGuid = family.Guid;
                    }
                }

                foreach ( var registrant in registration.Registrants )
                {
                    Registrants.Add( new RegistrantInfo( registrant, rockContext ) );
                }
            }
        }

        /// <summary>
        /// Gets the options that should be available for additional registrants to specify the family they belong to
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="currentRegistrantIndex">Index of the current registrant.</param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetFamilyOptions( RegistrationTemplate template, int currentRegistrantIndex )
        {
            // Return a dictionary of family group guid, and the formated name (i.e. "Ted & Cindy Decker" )
            var result = new Dictionary<Guid, string>();

            // Get all the registrants prior to the current registrant
            var familyRegistrants = new Dictionary<Guid, List<RegistrantInfo>>();
            for ( int i = 0; i < currentRegistrantIndex; i++ )
            {
                if ( Registrants != null && Registrants.Count > i )
                {
                    var registrant = Registrants[i];
                    familyRegistrants.TryAdd( registrant.FamilyGuid, new List<RegistrantInfo>() );
                    familyRegistrants[registrant.FamilyGuid].Add( registrant );
                }
                else
                {
                    break;
                }
            }

            // Loop through those registrants
            foreach ( var keyVal in familyRegistrants )
            {
                // Find all the people and group them by same last name
                var lastNames = new Dictionary<string, List<string>>();
                foreach ( var registrant in keyVal.Value )
                {
                    string firstName = registrant.GetFirstName( template );
                    string lastName = registrant.GetLastName( template );
                    lastNames.TryAdd( lastName, new List<string>() );
                    lastNames[lastName].Add( firstName );
                }

                // Build a formated output for each unique last name
                var familyNames = new List<string>();
                foreach ( var lastName in lastNames )
                {
                    familyNames.Add( string.Format( "{0} {1}", lastName.Value.AsDelimited( " & " ), lastName.Key ) );
                }

                // Join each of the formated values for each unique last name for the current family
                result.Add( keyVal.Key, familyNames.AsDelimited( " and " ) );
            }

            return result;
        }
    }
}
