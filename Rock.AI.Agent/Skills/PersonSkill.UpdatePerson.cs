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
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Communication;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PersonSkill
{
    #region Tool(s)

    [Description( "Updates properties on the person." )]
    [AgentToolGuid( "A1198A34-FCF2-4F58-83FA-7D02DD69830E" )]
    [AgentUsage( "Any argument ending with 'ValueIdKey' must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the chosen IdKey." )]
    [AgentUsage( "Include only fields you want to change. Omit or pass null to keep current values. For string fields, pass \"\" (empty) to clear." )]
    [AgentToolExample( "Update Ted Decker's record status to Inactive: 1) call with recordStatusValueIdKey='lookup'; 2) choose the IdKey for 'Inactive'; 3) call again with recordStatusValueIdKey='<IdKey>'" )]
    [AgentToolExample( "Clear middle name: pass middleName=\"\" and leave other fields null." )]
    [AgentToolExample( "Set suffix to Jr.: pass suffixValueIdKey='<IdKey for Jr.>' (or use 'lookup' first to find it)." )]
    public AgentToolResult UpdatePerson(
        string personIdKey,
        string nickName = null,
        string firstName = null,
        string middleName = null,
        string lastName = null,
        Gender? gender = null,
        CommunicationType? communicationPreference = null,
        string email = null,
        bool? isEmailActive = null,
        string emailNote = null,
        AgeClassification? ageClassification = null,
        SetOrClear<int> birthYear = null,
        SetOrClear<int> birthDay = null,
        SetOrClear<int> birthMonth = null,
        SetOrClear<DateTime> anniversaryDate = null,
        [Description( "Setting this will also update the person's record status to inactive with a reason of deceased." )]
        SetOrClear<DateTime> deceasedDate = null,
        string campusIdKey = null,
        string connectionStatusValueIdKey = null, // Not clearable
        string recordStatusValueIdKey = null,     // Not clearable
        [Description( "This is not required to set a person to inactive." )]
        string inactiveReasonValueIdKey = null,   // Clearable
        [Description( "This is an optional note to record with the inactive reason." )]
        string inactiveReasonNote = null,
        string raceValueIdKey = null,             // Clearable
        string ethnicityValueIdKey = null,        // Clearable
        string recordTypeValueIdKey = null,       // Not clearable
        string recordSourceValueIdKey = null,     // Clearable
        string suffixValueIdKey = null,           // Clearable
        string titleValueIdKey = null,            // Clearable
        string preferredLanguageValueIdKey = null,// Clearable
        string maritalStatusValueIdKey = null,    // Clearable
        List<AttributeValueResult> attributeValues = null
    )
    {
        // Quick pre-scan: build a list of parameters explicitly requesting lookups.
        var possibleLookupRequests = new[]
        {
            (Value: connectionStatusValueIdKey,  DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid(),    ResultKey: "connectionStatusValues"),
            (Value: recordStatusValueIdKey,      DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid(),        ResultKey: "recordStatusValues"),
            (Value: inactiveReasonValueIdKey,    DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid(), ResultKey: "inactiveReasonValues"),
            (Value: ethnicityValueIdKey,         DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_ETHNICITY.AsGuid(),            ResultKey: "ethnicityValues"),
            (Value: raceValueIdKey,              DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RACE.AsGuid(),                 ResultKey: "raceValues"),
            (Value: recordTypeValueIdKey,        DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_TYPE.AsGuid(),          ResultKey: "recordTypeValues"),
            (Value: recordSourceValueIdKey,      DefinedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE.AsGuid(),          ResultKey: "recordSourceValues"),
            (Value: suffixValueIdKey,            DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid(),               ResultKey: "suffixValues"),
            (Value: titleValueIdKey,             DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid(),                ResultKey: "titleValues"),
            (Value: preferredLanguageValueIdKey, DefinedTypeGuid: Rock.SystemGuid.DefinedType.LANGUAGES.AsGuid(),                   ResultKey: "languageValues"),
            (Value: maritalStatusValueIdKey,     DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid(),       ResultKey: "maritalStatusValues"),
        }
        .Where( r => r.Value.IsNotNullOrWhiteSpace() )
        .ToList();

        var lookupResults = new Dictionary<string, List<KeyNameResult>>( StringComparer.OrdinalIgnoreCase );

        foreach ( var (value, definedTypeGuid, resultKey) in possibleLookupRequests )
        {

            var isValidIdKey = IdHasher.Instance.GetId( value ).HasValue;
            if ( isValidIdKey )
            {
                continue;
            }

            var definedType = DefinedTypeCache.Get( definedTypeGuid );
            if ( definedType == null )
            {
                return Error( "The system is misconfigured. Please contact your system administrator." );
            }

            var definedValues = definedType.DefinedValues
                .OrderBy( dv => dv.Order )
                .ThenBy( dv => dv.Value )
                .Select( dv => KeyNameResult.FromCache( dv ) )
                .ToList();

            lookupResults[resultKey] = definedValues;
        }

        // If we have any lookup results to return, do so now.
        if ( lookupResults.Count > 0 )
        {
            return Error( "Lookups Required" )
                .WithContent( lookupResults )
                .WithInstructions( "Use the following data to determine the proper IdKey for the tool." );
        }

        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var personService = new PersonService( rockContext );
        var person = personService.Get( IdHasher.Instance.GetId( personIdKey ) ?? 0 );
        var currentPerson = AgentRequestContext.CurrentPerson;
        var inactivePersonRecordStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid();
        var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid(), rockContext );
        var deceasedReason = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid(), rockContext );

        if ( person == null )
        {
            return Error( "No person could be found with the provided personIdKey." );
        }

        // Name properties, gender.
        if ( !TryUpdateBasicPersonProperties( person, nickName, firstName, middleName, lastName, gender, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        // Email & is active.
        if ( !TryUpdatePersonEmailProperties( person, email, isEmailActive, emailNote, out errorMessage ) )
        {
            return Error( errorMessage );
        }

        // Anniversary Date.
        helper.UpdateProperty( person, p => p.AnniversaryDate, anniversaryDate );

        // Birth date related values.
        helper.UpdateProperty( person, p => p.BirthYear, birthYear );

        if ( birthMonth != null && birthMonth.ClearValue )
        {
            person.BirthMonth = null;
        }
        else if ( birthMonth != null )
        {
            if ( birthMonth.Value < 1 || birthMonth.Value > 12 )
            {
                helper.AddError( "If provided, the birth month must be between 1 and 12." );
            }
            else
            {
                person.BirthMonth = birthMonth.Value;
            }
        }

        if ( birthDay != null && birthDay.ClearValue )
        {
            person.BirthDay = null;
        }
        else if ( birthDay != null )
        {
            if ( birthDay.Value < 1 || birthDay.Value > 31 )
            {
                helper.AddError( "If provided, the birth day must be between 1 and 31." );
            }
            else
            {
                person.BirthDay = birthDay.Value;
            }
        }

        helper.UpdateProperty( person, p => p.AgeClassification, ageClassification );
        helper.UpdateProperty( person, p => p.CommunicationPreference, communicationPreference );
        helper.UpdateNavigationProperty( person, p => p.PrimaryCampus, campusIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.ConnectionStatusValue, connectionStatusValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.RecordStatusValue, recordStatusValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.RecordStatusReasonValue, inactiveReasonValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.RecordTypeValue, recordTypeValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.RecordSourceValue, recordSourceValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.EthnicityValue, ethnicityValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.RaceValue, raceValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.SuffixValue, suffixValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.TitleValue, titleValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.PreferredLanguageValue, preferredLanguageValueIdKey );
        helper.UpdateDefinedValueProperty( person, p => p.MaritalStatusValue, maritalStatusValueIdKey );

        // Deceased Date.
        if ( deceasedDate != null && deceasedDate.ClearValue )
        {
            person.DeceasedDate = null;
        }
        else if ( deceasedDate != null )
        {
            person.RecordStatusValueId = inactiveStatus.Id;
            person.RecordStatusReasonValueId = deceasedReason.Id;
            person.DeceasedDate = deceasedDate.Value;
        }

        if ( recordStatusValueIdKey.IsNotNullOrWhiteSpace() && !helper.HasErrors )
        {
            // If the record status was updated to inactive and there was no inactive reason provided, append instructions to follow up.
            if ( person.RecordStatusValue?.Guid == inactivePersonRecordStatusGuid && inactiveReasonValueIdKey.IsNullOrWhiteSpace() )
            {
                helper.AddInstructions( "The person has been marked inactive. Follow up to see if there should be a reason specified." );
            }
        }

        if ( inactiveReasonNote.IsNotNullOrWhiteSpace() )
        {
            if ( person.RecordStatusValue?.Guid != inactivePersonRecordStatusGuid )
            {
                helper.AddError( "The inactiveReasonNote can only be set if the person's record status is set to Inactive." );
                helper.AddInstructions( "Ask the user if they would like you to mark the record inactive." );
            }
            else
            {
                person.InactiveReasonNote = inactiveReasonNote;
            }
        }

        // Person attributes are staged here and persisted by SaveChangesIfNoErrors
        // along with the property changes.
        helper.SetAttributeValues( person, attributeValues );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( new PersonDetailResult
        {
            Id = person.Id,
            FirstName = person.FirstName,
            NickName = person.NickName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            Suffix = person.SuffixValue?.Value,
            Age = person.Age,
            BirthDay = person.BirthDay,
            BirthMonth = person.BirthMonth,
            BirthYear = person.BirthYear,
            AgeClassification = person.AgeClassification,
            Email = person.Email,
            AnniversaryDate = person.AnniversaryDate,
            Campus = person.PrimaryCampus != null ? new KeyNameResult { IdKey = person.PrimaryCampus.IdKey, Name = person.PrimaryCampus.Name } : null,
            Gender = person.Gender,
            RecordStatus = person.RecordStatusValue?.Value,
            ConnectionStatus = person.ConnectionStatusValue?.Value,
        } );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Trims and applies basic person name fields and optional gender, preventing illegal clears.
    /// </summary>
    /// <param name="person">The person to mutate.</param>
    /// <param name="nickName">New nickname; trimmed. Null = no change.</param>
    /// <param name="firstName">New first name; trimmed. Empty string is rejected.</param>
    /// <param name="middleName">New middle name; trimmed. Empty clears middle name.</param>
    /// <param name="lastName">New last name; trimmed. Empty string is rejected.</param>
    /// <param name="gender">Optional gender to set.</param>
    /// <param name="errorMessage">Populated when returning false.</param>
    /// <returns>
    /// True if updated or nothing to change; false if validation failed (with <paramref name="errorMessage"/>).
    /// </returns>
    private bool TryUpdateBasicPersonProperties(
        Rock.Model.Person person,
        string nickName,
        string firstName,
        string middleName,
        string lastName,
        Rock.Model.Gender? gender,
        out string errorMessage )
    {
        errorMessage = null;

        // If nothing needs to be updated, we are done.
        if ( nickName == null && firstName == null && middleName == null && lastName == null && gender == null )
        {
            return true;
        }

        if ( nickName != null )
        {
            nickName = nickName.Trim();
            person.NickName = nickName;
        }

        if ( firstName != null )
        {
            firstName = firstName.Trim();

            if ( firstName == string.Empty )
            {
                errorMessage = "Clearing out a person's first name is not allowed.";
                return false;
            }

            person.FirstName = firstName;
        }

        if ( middleName != null )
        {
            middleName = middleName.Trim();
            person.MiddleName = middleName;
        }

        if ( lastName != null )
        {
            lastName = lastName.Trim();
            if ( lastName == "" )
            {
                errorMessage = "Clearing out a person's last name is not allowed.";
                return false;
            }

            person.LastName = lastName;
        }

        if ( gender.HasValue )
        {
            person.Gender = gender.Value;
        }

        return true;
    }

    /// <summary>
    /// Applies email and active state with API-consistent rules.
    /// </summary>
    /// <param name="person">The person to mutate.</param>
    /// <param name="email">
    /// New email; trimmed. Null = no change; empty string clears email and sets <c>IsEmailActive=false</c>.
    /// </param>
    /// <param name="isEmailActive">
    /// Optional explicit active flag; overrides implicit defaults when supplied.
    /// </param>
    /// <param name="errorMessage">Populated when returning false.</param>
    /// <returns>
    /// True if updated or nothing to change; false if validation failed (with <paramref name="errorMessage"/>).
    /// </returns>
    /// <remarks>
    /// When setting a non-empty email and <paramref name="isEmailActive"/> is not supplied, the address is marked active.
    /// </remarks>
    private bool TryUpdatePersonEmailProperties(
        Rock.Model.Person person,
        string email,
        bool? isEmailActive,
        string emailNote,
        out string errorMessage )
    {
        errorMessage = null;

        if ( email == null && !isEmailActive.HasValue && emailNote == null )
        {
            return true;
        }

        if ( email != null )
        {
            email = email.Trim();

            // If clearing out the email, also mark it inactive.
            if ( email == string.Empty )
            {
                person.Email = string.Empty;
                person.IsEmailActive = false;
                return true;
            }

            if ( !EmailAddressFieldValidator.IsValid( email ) )
            {
                errorMessage = "The provided email address is not valid.";
                return false;
            }

            person.Email = email;

            // Unless explicitly specified, if setting an email address, also mark it active.
            if ( !isEmailActive.HasValue )
            {
                person.IsEmailActive = true;
            }
        }

        if ( emailNote != null )
        {
            person.EmailNote = emailNote.Trim();
        }

        if ( isEmailActive.HasValue )
        {
            person.IsEmailActive = isEmailActive.Value;
        }

        return true;
    }

    #endregion
}