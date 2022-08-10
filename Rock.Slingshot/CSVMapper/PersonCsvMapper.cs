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
using Rock.Communication;
// Alias Slingshot.Core namespace to avoid conflict with Rock.Slingshot.*
using SlingshotCore = global::Slingshot.Core;

namespace Rock.Slingshot
{

    /// <summary>
    /// Maps the dictionary input to a Slingshot Person class.
    /// </summary>
    public class PersonCsvMapper
    {
        /// <summary>
        /// A static class to map a dictionary data to a Slingshot Person object.
        /// </summary>
        /// <param name="csvEntryLookup">Dictionary having the header to value mapping of the input data</param>
        /// <param name="csvHeaderMapper">Dictionary mapping every field in the Slingshot Person class to the header name of the input data</param>
        /// <param name="parserErrors">Set of all the error that occurred during the mapping</param>
        /// <returns></returns>
        public static SlingshotCore.Model.Person Map( IDictionary<string, object> csvEntryLookup, Dictionary<string, string> csvHeaderMapper, out HashSet<string> parserErrors )
        {
            var person = new SlingshotCore.Model.Person();
            parserErrors = new HashSet<string>();

            #region Required Fields

            person.Id = csvEntryLookup[csvHeaderMapper[CSVHeaders.Id]].ToIntSafe();

            string csvColumnFamilyId = csvHeaderMapper[CSVHeaders.FamilyId];
            person.FamilyId = csvEntryLookup[csvColumnFamilyId].ToIntSafe();

            string csvColumnFamilyRole = csvHeaderMapper[CSVHeaders.FamilyRole];
            string familyRoleString = csvEntryLookup[csvColumnFamilyRole].ToStringSafe();
            person.FamilyRole = ( SlingshotCore.Model.FamilyRole ) Enum.Parse( typeof( SlingshotCore.Model.FamilyRole ), familyRoleString );

            string csvColumnFirstName = csvHeaderMapper[CSVHeaders.FirstName];
            person.FirstName = csvEntryLookup[csvColumnFirstName].ToStringSafe();

            string csvColumnLastName = csvHeaderMapper[CSVHeaders.LastName];
            person.LastName = csvEntryLookup[csvColumnLastName].ToStringSafe();

            #endregion Required Fields

            #region Optionals Fields
            // These may not be present in the csvHeaderMapper thus needs to be checked.
            var csvColumnNickName = csvHeaderMapper.GetValueOrNull( CSVHeaders.NickName );
            if ( csvColumnNickName != null )
            {
                person.NickName = csvEntryLookup[csvColumnNickName].ToStringSafe();
            }

            var csvColumnMiddleName = csvHeaderMapper.GetValueOrNull( CSVHeaders.MiddleName );
            if ( csvColumnMiddleName != null )
            {
                person.MiddleName = csvEntryLookup[csvColumnMiddleName].ToStringSafe();
            }

            var csvColumnSuffix = csvHeaderMapper.GetValueOrNull( CSVHeaders.Suffix );
            if ( csvColumnSuffix != null )
            {
                person.Suffix = csvEntryLookup[csvColumnSuffix].ToStringSafe();
            }

            var csvColumnEmail = csvHeaderMapper.GetValueOrNull( CSVHeaders.Email );
            if ( csvColumnEmail != null )
            {
                person.Email = csvEntryLookup[csvColumnEmail].ToStringSafe();
                bool isEmailValid = EmailAddressFieldValidator.Validate( person.Email, allowMultipleAddresses: false, allowLava: false ) == EmailFieldValidationResultSpecifier.Valid;
                if ( !isEmailValid )
                {
                    parserErrors.Add( $"Email Address {person.Email} could not be read" );
                    person.Email = string.Empty;
                }
            }

            var csvColumnGender = csvHeaderMapper.GetValueOrNull( CSVHeaders.Gender );
            if ( csvColumnGender != null )
            {
                string genderString = csvEntryLookup[csvColumnGender].ToStringSafe();
                var genderEnum = genderString.ConvertToEnumOrNull<SlingshotCore.Model.Gender>();
                if ( genderEnum != null )
                {
                    person.Gender = genderEnum.Value;
                }
                else
                {
                    parserErrors.Add( $"Gender {genderString} is invalid defaulting to {person.Gender}" );
                }
            }

            var csvColumnEmailPreference = csvHeaderMapper.GetValueOrNull( CSVHeaders.EmailPreference );
            if ( csvColumnEmailPreference != null )
            {
                string emailPreferenceString = csvEntryLookup[csvColumnEmailPreference].ToStringSafe();
                var emailPreferenceEnum = emailPreferenceString.ConvertToEnumOrNull<SlingshotCore.Model.EmailPreference>();
                if ( emailPreferenceEnum != null )
                {
                    person.EmailPreference = emailPreferenceEnum.Value;
                }
                else
                {
                    parserErrors.Add( $"Email Preference {emailPreferenceString} is invalid defaulting to {person.EmailPreference}" );
                }
            }

            var csvColumnSalutation = csvHeaderMapper.GetValueOrNull( CSVHeaders.TitleValueId );
            if ( csvColumnSalutation != null )
            {
                person.Salutation = csvEntryLookup[csvColumnSalutation].ToStringSafe();
            }

            person.MaritalStatus = SlingshotCore.Model.MaritalStatus.Unknown;
            var csvColumnMaritalStatus = csvHeaderMapper.GetValueOrNull( CSVHeaders.MaritalStatus );
            if ( csvColumnMaritalStatus != null )
            {
                string martialStatusString = csvEntryLookup[csvColumnMaritalStatus].ToStringSafe();
                var maritalStatusEnum = martialStatusString.ConvertToEnumOrNull<SlingshotCore.Model.MaritalStatus>();
                if ( maritalStatusEnum != null )
                {
                    person.MaritalStatus = maritalStatusEnum.Value;
                }
                else
                {
                    parserErrors.Add( $"Marital Status {martialStatusString} is invalid defaulting to {person.MaritalStatus}" );
                }
            }

            var csvColumnBirthdate = csvHeaderMapper.GetValueOrNull( CSVHeaders.Birthdate );
            if ( csvColumnBirthdate != null )
            {
                string birthdateString = csvEntryLookup[csvColumnBirthdate].ToStringSafe();
                if ( DateTime.TryParse( birthdateString, out DateTime birthdateDateTime ) )
                {
                    person.Birthdate = birthdateDateTime;
                }
                else
                {
                    parserErrors.Add( $"Birthdate {birthdateString} could not be read" );
                }
            }

            var csvColumnAnniversaryDate = csvHeaderMapper.GetValueOrNull( CSVHeaders.AnniversaryDate );
            if ( csvColumnAnniversaryDate != null )
            {
                string anniversaryDateString = csvEntryLookup[csvColumnAnniversaryDate].ToStringSafe();
                if ( DateTime.TryParse( anniversaryDateString, out DateTime AnniversaryDateTime ) )
                {
                    person.AnniversaryDate = AnniversaryDateTime;
                }
                else
                {
                    parserErrors.Add( $"Anniversary Date {anniversaryDateString} could not be read" );
                }
            }

            var csvColumnRecordStatus = csvHeaderMapper.GetValueOrNull( CSVHeaders.RecordStatus );
            if ( csvColumnRecordStatus != null )
            {
                string recordStatusString = csvEntryLookup[csvColumnRecordStatus].ToStringSafe();
                if ( Enum.TryParse( recordStatusString, out SlingshotCore.Model.RecordStatus RecordStatusEnum ) )
                {
                    person.RecordStatus = RecordStatusEnum;
                }
                else
                {
                    parserErrors.Add( $"Record Status {recordStatusString} is invalid defaulting to {person.RecordStatus}" );
                }
            }

            var csvColumnInactiveReason = csvHeaderMapper.GetValueOrNull( CSVHeaders.InactiveReason );
            if ( csvColumnInactiveReason != null )
            {
                person.InactiveReason = csvEntryLookup[csvColumnInactiveReason].ToStringSafe();
            }

            var csvColumnIsDeceased = csvHeaderMapper.GetValueOrNull( CSVHeaders.IsDeceased );
            if ( csvColumnIsDeceased != null )
            {
                string isDeceasedString = csvEntryLookup[csvColumnIsDeceased].ToStringSafe();
                if ( Boolean.TryParse( isDeceasedString, out bool isDeceasedBoolean ) )
                {
                    person.IsDeceased = isDeceasedBoolean;
                }
                else
                {
                    parserErrors.Add( $"Could not set Is Deceased to {isDeceasedString} defaulting to \'{person.IsDeceased}\'" );
                }
            }

            var csvColumnConnectionStatus = csvHeaderMapper.GetValueOrNull( CSVHeaders.ConnectionStatus );
            if ( csvColumnConnectionStatus != null )
            {
                person.ConnectionStatus = csvEntryLookup[csvColumnConnectionStatus].ToStringSafe();
            }

            var csvColumnGrade = csvHeaderMapper.GetValueOrNull( CSVHeaders.Grade );
            if ( csvColumnGrade != null )
            {
                person.Grade = csvEntryLookup[csvColumnGrade].ToStringSafe();
            }

            var csvColumnCampusId = csvHeaderMapper.GetValueOrNull( CSVHeaders.CampusId );
            if ( csvColumnCampusId != null )
            {
                person.Campus.CampusId = csvEntryLookup[csvColumnCampusId].ToIntSafe();
            }

            var csvColumnCampusName = csvHeaderMapper.GetValueOrNull( CSVHeaders.CampusName );
            if ( csvColumnCampusName != null )
            {
                person.Campus.CampusName = csvEntryLookup[csvColumnCampusName].ToStringSafe();
            }

            var csvColumnGivingIndividually = csvHeaderMapper.GetValueOrNull( CSVHeaders.GiveIndividually );
            if ( csvColumnGivingIndividually != null )
            {
                string givingIndividuallyString = csvEntryLookup[csvColumnGivingIndividually].ToStringSafe();
                if ( Boolean.TryParse( givingIndividuallyString, out bool givingIndividuallyBoolean ) )
                {
                    person.GiveIndividually = givingIndividuallyBoolean;
                }
                else
                {
                    parserErrors.Add( $"Could not set Give Individually to {givingIndividuallyString} defaulting to \'{person.GiveIndividually}\'" );
                }
            }

            var csvColumnCreatedDateTime = csvHeaderMapper.GetValueOrNull( CSVHeaders.CreatedDateTime );
            if ( csvColumnCreatedDateTime != null )
            {
                string createdDateTimeString = csvEntryLookup[csvColumnCreatedDateTime].ToStringSafe();
                if ( DateTime.TryParse( createdDateTimeString, out DateTime createdDateTime ) )
                {
                    person.CreatedDateTime = createdDateTime;
                }
                else
                {
                    parserErrors.Add( $"Created Date Time {createdDateTimeString} could not be read" );
                }
            }

            var csvColumnModifiedDateTime = csvHeaderMapper.GetValueOrNull( CSVHeaders.ModifiedDateTime );
            if ( csvColumnModifiedDateTime != null )
            {
                string modifiedDateTimeString = csvEntryLookup[csvColumnModifiedDateTime].ToStringSafe();
                if ( DateTime.TryParse( modifiedDateTimeString, out DateTime modifiedDateTime ) )
                {
                    person.ModifiedDateTime = modifiedDateTime;
                }
                else
                {
                    parserErrors.Add( $"Modified Date Time {modifiedDateTimeString} could not be read" );
                }
            }

            #endregion Optionals Fields

            return person;
        }
    }
}