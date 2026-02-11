using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

using AngleSharp.Dom;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)

        [Description( "Updates a person's phone number." )]
        [AgentUsage( "The phoneTypeValueIdKey must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the appropriate IdKey." )]
        [AgentToolGuid( "89A9F9C5-87F2-9197-46DA-5C96D0BDA628" )]
        public RockToolResult UpdatePhoneNumber(
            string personIdKey,
            string phoneNumber,
            string phoneTypeValueIdKey = null,
            bool isMessagingEnabled = false,
            bool isUnlisted = false
        )
        {
            var phoneTypeValueId = IdHasher.Instance.GetId( phoneTypeValueIdKey );
            var phoneTypeValue = DefinedValueCache.Get( phoneTypeValueId ?? 0 );

            // Check for valid phone type
            if ( !phoneTypeValueId.HasValue || phoneTypeValue == null )
            {
                var phoneTypes = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_PHONE_TYPE ).DefinedValues
                    .Select( dv => new KeyNameResult { Id = dv.Id, Name = dv.Value } )
                    .ToList();

                return RockToolResult.Error( "Lookups Required" )
                    .WithContent( phoneTypes )
                    .WithHistoryContent( phoneTypes )
                    .WithInstructions( "Use the following phone types to determine the proper IdKey for the tool." );
            }

            
            using var rockContext = _rockContextFactory.CreateRockContext();

            // Load the person to ensure they exist
            var personService = new PersonService( rockContext );
            var person = personService.Get( IdHasher.Instance.GetId( personIdKey ) ?? 0 );

            if ( person == null )
            {
                return RockToolResult.Error( "No person could be found with the provided personIdKey." );
            }

            // Save the phone number
            var personPhoneService = new PhoneNumberService( rockContext );
            var personPhone = personPhoneService.Queryable().Where( ph => ph.PersonId == person.Id && ph.NumberTypeValueId == phoneTypeValueId ).FirstOrDefault();

            if ( personPhone == null )
            {
                personPhone.PersonId = person.Id;
                personPhone.NumberTypeValueId = phoneTypeValueId;
                personPhoneService.Add( personPhone );
            }

            personPhone.Number = phoneNumber;
            personPhone.IsUnlisted = isUnlisted;
            personPhone.IsMessagingEnabled = isMessagingEnabled;

            rockContext.SaveChanges();

            return RockToolResult.Success( $"The phone number for {person.FullName} has been updated to {personPhone.NumberFormatted} with messaging set to {isMessagingEnabled} and unlisted set to {isUnlisted}." );
        }

        #endregion
    }
}
