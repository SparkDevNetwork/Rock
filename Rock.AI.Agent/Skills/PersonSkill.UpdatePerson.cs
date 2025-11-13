using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Communication;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
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
        public RockToolResult UpdatePerson(
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
            SetOrClear<int?> birthYear = null,
            SetOrClear<int?> birthDay = null,
            SetOrClear<int?> birthMonth = null,
            SetOrClear<DateTime?> anniversaryDate = null,
            [Description( "Setting this will also update the person's record status to inactive with a reason of deceased." )]
            SetOrClear<DateTime?> deceasedDate = null,
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
            string maritalStatusValueIdKey = null     // Clearable
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
                    return RockToolResult.Error( "The system is misconfigured. Please contact your system administrator." );
                }

                var definedValues = definedType.DefinedValues
                    .OrderBy( dv => dv.Order )
                    .ThenBy( dv => dv.Value )
                    .Select( dv => new KeyNameResult { Id = dv.Id, Name = dv.Value } )
                    .ToList();

                lookupResults[resultKey] = definedValues;
            }

            // If we have any lookup results to return, do so now.
            if ( lookupResults.Count > 0 )
            {
                return RockToolResult.Error( "Lookups Required" )
                    .WithContent( lookupResults )
                    .WithInstructions( "Use the following data to determine the proper IdKey for the tool." );
            }

            using var rockContext = _rockContextFactory.CreateRockContext();
            var personService = new PersonService( rockContext );
            var person = personService.Get( IdHasher.Instance.GetId( personIdKey ) ?? 0 );
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            var instructions = "";

            if ( person == null )
            {
                return RockToolResult.Error( "No person could be found with the provided personIdKey." );
            }

            // Name properties, gender.
            if ( !TryUpdateBasicPersonProperties( person, nickName, firstName, middleName, lastName, gender, out var errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Email & is active.
            if ( !TryUpdatePersonEmailProperties( person, email, isEmailActive, emailNote, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Anniversary Date.
            SetOrClearUtilities.SetOrClearValue<DateTime?>( anniversaryDate, v => person.AnniversaryDate = v, () => person.AnniversaryDate = null );

            // Birth date related values.
            SetOrClearUtilities.SetOrClearValue<int?>( birthYear, v => person.BirthYear = v, () => person.BirthYear = null );
            SetOrClearUtilities.SetOrClearValue<int?>( birthMonth, v =>
            {
                if ( v.HasValue && ( v < 1 || v > 12 ) )
                {
                    throw new ArgumentException( "If provided, the birth month must be between 1 and 12." );
                }
                person.BirthMonth = v;
            }, () => person.BirthMonth = null );

            SetOrClearUtilities.SetOrClearValue<int?>( birthDay, v =>
            {
                if ( v.HasValue && ( v < 1 || v > 31 ) )
                {
                    throw new ArgumentException( "If provided, the birth day must be between 1 and 31." );
                }
                person.BirthDay = v;
            }, () => person.BirthDay = null );

            // Age Classification.
            if ( ageClassification.HasValue )
            {
                person.AgeClassification = ageClassification.Value;
            }

            // Communication Preference.
            if ( communicationPreference.HasValue )
            {
                person.CommunicationPreference = communicationPreference.Value;
            }

            // Campus.
            if ( campusIdKey.IsNotNullOrWhiteSpace() )
            {
                var campusId = IdHasher.Instance.GetId( campusIdKey );
                if ( !campusId.HasValue || campusId <= 0 )
                {
                    return RockToolResult.Error( "The provided campusIdKey is not valid." );
                }

                var campus = CampusCache.Get( campusId.Value );
                if ( campus == null )
                {
                    return RockToolResult.Error( "No campus could be found with the provided campusIdKey." );
                }

                person.PrimaryCampusId = campus.Id;
            }

            // Connection Status Defined Value.
            if ( !TryUpdateDefinedValueProperty( connectionStatusValueIdKey, Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, id => person.ConnectionStatusValueId = id, null, false, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Record Status Defined Value.
            var recordStatusUpdated = false;
            if ( !TryUpdateDefinedValueProperty( recordStatusValueIdKey, Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, id => { person.RecordStatusValueId = id; recordStatusUpdated = true; }, null, false, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            var inactivePersonRecordStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid();
            if ( recordStatusUpdated )
            {
                // If the record status was updated to inactive and there was no inactive reason provided, append instructions to follow up.
                if ( person.RecordStatusValue?.Guid == inactivePersonRecordStatusGuid && inactiveReasonValueIdKey.IsNullOrWhiteSpace() )
                {
                    instructions += "The person has been marked inactive. Follow up to see if there should be a reason specified.";
                }
            }

            // Inactive Reason Defined Value.
            if ( !TryUpdateDefinedValueProperty( inactiveReasonValueIdKey, Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON, id => person.RecordStatusReasonValueId = id, () => person.RecordStatusReasonValueId = null, true, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Record type.
            if ( !TryUpdateDefinedValueProperty( recordTypeValueIdKey, Rock.SystemGuid.DefinedType.PERSON_RECORD_TYPE, id => person.RecordTypeValueId = id, null, false, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Record Source Reason Defined Value.
            if ( !TryUpdateDefinedValueProperty( recordSourceValueIdKey, Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE, id => person.RecordSourceValueId = id, () => person.RecordSourceValueId = null, true, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            if ( inactiveReasonNote.IsNotNullOrWhiteSpace() )
            {
                if ( person.RecordStatusValue?.Guid != inactivePersonRecordStatusGuid )
                {
                    return RockToolResult.Error( "The inactiveReasonNote can only be set if the person's record status is set to Inactive." )
                        .WithInstructions( "Ask the user if they would like you to mark the record inactive." );
                }
                person.InactiveReasonNote = inactiveReasonNote;
            }

            // Ethnicity.
            if ( !TryUpdateDefinedValueProperty( ethnicityValueIdKey, Rock.SystemGuid.DefinedType.PERSON_ETHNICITY, id => person.EthnicityValueId = id, () => person.EthnicityValueId = null, true, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Race.
            if ( !TryUpdateDefinedValueProperty( raceValueIdKey, Rock.SystemGuid.DefinedType.PERSON_RACE, id => person.RaceValueId = id, () => person.RaceValueId = null, true, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Suffix.
            if ( !TryUpdateDefinedValueProperty( suffixValueIdKey, Rock.SystemGuid.DefinedType.PERSON_SUFFIX, id => person.SuffixValueId = id, () => person.SuffixValueId = null, true, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Title.
            if ( !TryUpdateDefinedValueProperty( titleValueIdKey, Rock.SystemGuid.DefinedType.PERSON_TITLE, id => person.TitleValueId = id, () => person.TitleValueId = null, true, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Preferred Language.
            if ( !TryUpdateDefinedValueProperty( preferredLanguageValueIdKey, Rock.SystemGuid.DefinedType.LANGUAGES, id => person.PreferredLanguageValueId = id, () => person.PreferredLanguageValueId = null, true, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Marital Status.
            if ( !TryUpdateDefinedValueProperty( maritalStatusValueIdKey, Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, id => person.MaritalStatusValueId = id, () => person.MaritalStatusValueId = null, true, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            var deceasedReason = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid() );

            // Deceased Date.
            SetOrClearUtilities.SetOrClearValue<DateTime?>( deceasedDate, v => SetDeceased( person, v ), () => person.DeceasedDate = null );
            void SetDeceased( Rock.Model.Person p, DateTime? dt )
            {
                p.RecordStatusValueId = inactiveStatus.Id;
                p.RecordStatusReasonValueId = deceasedReason.Id;
                p.DeceasedDate = dt;
            }

            // Save changes.
            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "UpdatePerson failed for PersonIdKey={PersonIdKey}", personIdKey );
                return RockToolResult.Error( "Failed to update person. " + ex.Message );
            }

            return RockToolResult.Success( new PersonResult
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

        /// <summary>
        /// Resolves and applies a DefinedValue by IdKey, or clears it (if allowed).
        /// </summary>
        /// <param name="idKey">
        /// Null = no change; empty string = clear (when <paramref name="allowClear"/> is true); otherwise must be a valid IdKey.
        /// </param>
        /// <param name="definedTypeGuid">The defined type GUID constant (e.g., <c>Rock.SystemGuid.DefinedType.PERSON_RACE</c>).</param>
        /// <param name="setAction">Called with the resolved DefinedValue Id.</param>
        /// <param name="clearAction">Called when clearing (only if <paramref name="allowClear"/> is true and <paramref name="idKey"/> is empty).</param>
        /// <param name="allowClear">Whether clearing via empty string is permitted.</param>
        /// <param name="errorMessage">Populated when returning false.</param>
        /// <returns>
        /// True if applied/no-op; false if the IdKey is invalid or a prohibited clear was requested.
        /// </returns>
        private bool TryUpdateDefinedValueProperty(
            string idKey,
            string definedTypeGuid,
            Action<int> setAction,
            Action clearAction,
            bool allowClear,
            out string errorMessage )
        {
            errorMessage = null;

            // no matter what null means no change.
            if ( idKey == null )
            {
                return true;
            }

            // clear the value if requested and allowed.
            if ( idKey == string.Empty )
            {
                if ( !allowClear )
                {
                    errorMessage = "Clearing out this value is not allowed";
                    return false;
                }

                clearAction();
                return true;
            }

            if ( !TryGetDefinedValueOfType( idKey, definedTypeGuid, out var dvc, out errorMessage ) )
            {
                return false;
            }

            setAction( dvc.Id );
            return true;
        }

        private static bool TryGetDefinedValueOfType( string definedValueIdKey, string definedTypeGuid, out DefinedValueCache dvc, out string errorMessage )
        {
            dvc = null;
            errorMessage = string.Empty;

            if ( definedValueIdKey.IsNullOrWhiteSpace() )
            {
                errorMessage = "The definedValueIdKey is required.";
                return false;
            }

            var id = IdHasher.Instance.GetId( definedValueIdKey );
            if ( !id.HasValue || id <= 0 )
            {
                errorMessage = "The definedValueIdKey is not valid.";
                return false;
            }

            var definedValue = DefinedValueCache.Get( id.Value );
            if ( definedValue == null )
            {
                errorMessage = "No defined value could be found with the provided definedValueIdKey.";
                return false;
            }

            if ( !definedValue.DefinedType.Guid.Equals( definedTypeGuid.AsGuid() ) )
            {
                errorMessage = "The provided definedValueIdKey is not of the expected type.";
                return false;
            }

            dvc = definedValue;

            return true;
        }

        #endregion
    }
}