using System;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Linq;
using Rock;

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
            var mobilePhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
            var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

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
        Incoming,
        Outgoing,
        Unknown
    }
}
