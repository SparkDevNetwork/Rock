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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PersonSkill
{
    #region Tool(s)

    [Description( "Updates a person's phone number." )]
    [AgentUsage( "The phoneTypeValueIdKey must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the appropriate IdKey." )]
    [AgentToolGuid( "89A9F9C5-87F2-9197-46DA-5C96D0BDA628" )]
    public IAgentToolResult UpdatePhoneNumber(
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

            return Error( "Lookups Required" )
                .WithContent( phoneTypes )
                .WithHistoryContent( phoneTypes )
                .WithInstructions( "Use the following phone types to determine the proper IdKey for the tool." );
        }

        using var rockContext = RockApp.Current.CreateRockContext();

        // Load the person to ensure they exist
        var personService = new PersonService( rockContext );
        var person = personService.Get( IdHasher.Instance.GetId( personIdKey ) ?? 0 );

        if ( person == null )
        {
            return Error( "No person could be found with the provided personIdKey." );
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

        return Success( $"The phone number for {person.FullName} has been updated to {personPhone.NumberFormatted} with messaging set to {isMessagingEnabled} and unlisted set to {isUnlisted}." );
    }

    #endregion
}
