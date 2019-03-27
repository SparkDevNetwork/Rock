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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Pbx
{
    /// <summary>
    /// Class to hold the details of a Call Detail Record
    /// </summary>
    public class CdrRecord
    {
        /// <summary>
        /// Gets or sets the record key.
        /// </summary>
        /// <value>
        /// The record key.
        /// </value>
        public string RecordKey { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        /// <value>
        /// The destination.
        /// </value>
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>
        /// The end date time.
        /// </value>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the duration of the call in seconds.
        /// </summary>
        /// <value>
        /// The duration in seconds.
        /// </value>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the caller identifier.
        /// </summary>
        /// <value>
        /// The caller identifier.
        /// </value>
        public string CallerId { get; set; }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>
        /// The direction.
        /// </value>
        public CdrDirection Direction { get; set; }

        /// <summary>
        /// Utility method to look up a person by phone
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        public static int CdrPersonLookupFromPhone(string phoneNumber)
        {
            var rockContext = new RockContext();

            var personService = new PersonService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            // give preference to people with the phone in the mobile phone type
            // first look for a person with the phone number as a mobile phone order by family role then age
            var mobilePhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

            // Get all people phone number
            var peopleWithMobileNumber = personService.Queryable()
                .Where( p =>
                    p.PhoneNumbers.Any( n =>
                        ( n.CountryCode + n.Number ) == phoneNumber.Replace( "+", "" ) &&
                        n.NumberTypeValueId == mobilePhoneType.Id )
                    )
                .Select( p => p.Id );

            // Find first person ordered by role (adult first), then by birthdate (oldest first)
            var fromPerson = groupMemberService.Queryable()
                .Where( m =>
                    m.Group.GroupTypeId == familyGroupType.Id &&
                    peopleWithMobileNumber.Contains( m.PersonId ) )
                .OrderBy( m => m.GroupRole.Order )
                .ThenBy( m => m.Person.BirthDate ?? DateTime.MinValue )
                .Select( m => m.Person )
                .FirstOrDefault();

            // if no match then look for the phone in any phone type ordered by family role then age
            if ( fromPerson == null )
            {
                var peopleWithAnyNumber = personService.Queryable()
                    .Where( p =>
                        p.PhoneNumbers.Any( n =>
                            ( n.CountryCode + n.Number ) == phoneNumber.Replace( "+", "" ) &&
                            n.NumberTypeValueId == mobilePhoneType.Id )
                        )
                    .Select( p => p.Id );

                fromPerson = groupMemberService.Queryable()
                    .Where( m =>
                        m.Group.GroupTypeId == familyGroupType.Id &&
                        peopleWithMobileNumber.Contains( m.PersonId ) )
                    .OrderBy( m => m.GroupRole.Order )
                    .ThenBy( m => m.Person.BirthDate ?? DateTime.MinValue )
                    .Select( m => m.Person ).FirstOrDefault();
            }

            if (fromPerson != null && fromPerson.PrimaryAliasId.HasValue)
            {
                return fromPerson.PrimaryAliasId.Value;
            }

            return -1;
        }
    }

    /// <summary>
    /// Enumeration to describe the call direction
    /// </summary>
    public enum CdrDirection
    {
        /// <summary>
        /// Incoming
        /// </summary>
        Incoming,

        /// <summary>
        /// Outgoing
        /// </summary>
        Outgoing,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }
}
