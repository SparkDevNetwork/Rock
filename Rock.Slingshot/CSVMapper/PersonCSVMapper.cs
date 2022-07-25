using System;
using System.Collections.Generic;
using Rock;

public class PersonCSVMapper
{
    public static Slingshot.Core.Model.Person Map( IDictionary<string, object> csvEntryLookup, Dictionary<string, string> csvHeaderMapper )
    {
        var person = new Slingshot.Core.Model.Person();

        #region Required Fields

        string csvColumnId = csvHeaderMapper["Id"];
        person.Id = csvEntryLookup[csvColumnId].ToIntSafe();

        string csvColumnFamilyId = csvHeaderMapper["Family Id"];
        person.FamilyId = csvEntryLookup[csvColumnFamilyId].ToIntSafe();

        string csvColumnFamilyRole = csvHeaderMapper["Family Role"];
        string familyRoleString = csvEntryLookup[csvColumnFamilyRole].ToStringSafe();
        person.FamilyRole = ( Slingshot.Core.Model.FamilyRole ) Enum.Parse( typeof( Slingshot.Core.Model.FamilyRole ), familyRoleString );

        string csvColumnFirstName = csvHeaderMapper["First Name"];
        person.FirstName = csvEntryLookup[csvColumnFirstName].ToStringSafe();

        string csvColumnLastName = csvHeaderMapper["Last Name"];
        person.LastName = csvEntryLookup[csvColumnLastName].ToStringSafe();

        #endregion Required Fields

        #region Optionals Fields
        // These may not be present in the csvHeaderMapper thus needs to be checked.

        {
            if ( csvHeaderMapper.TryGetValue( "Nick Name", out string csvColumnNickName ) )
            {
                person.NickName = csvEntryLookup[csvColumnNickName].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Middle Name", out string csvColumnMiddleName ) )
            {
                person.MiddleName = csvEntryLookup[csvColumnMiddleName].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Suffix", out string csvColumnSuffix ) )
            {
                person.Suffix = csvEntryLookup[csvColumnSuffix].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Email", out string csvColumnEmail ) )
            {
                person.Email = csvEntryLookup[csvColumnEmail].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Gender", out string csvColumnGender ) )
            {
                string genderString = csvEntryLookup[csvColumnGender].ToStringSafe();
                if ( Enum.TryParse( genderString, out Slingshot.Core.Model.Gender GenderEnum ) )
                {
                    person.Gender = GenderEnum;
                }
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Email Preference", out string csvColumnEmailPreference ) )
            {
                string emailPreferenceString = csvEntryLookup[csvColumnEmailPreference].ToStringSafe();
                if ( Enum.TryParse( emailPreferenceString, out Slingshot.Core.Model.EmailPreference EmailPreferenceEnum ) )
                {
                    person.EmailPreference = EmailPreferenceEnum;
                }
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Salutation", out string csvColumnSalutation ) )
            {
                person.Salutation = csvEntryLookup[csvColumnSalutation].ToStringSafe();
            }
        }

        {
            person.MaritalStatus = Slingshot.Core.Model.MaritalStatus.Unknown;
            if ( csvHeaderMapper.TryGetValue( "Marital Status", out string csvColumnMaritalStatus ) )
            {
                string martialStatusString = csvEntryLookup[csvColumnMaritalStatus].ToStringSafe();
                if ( Enum.TryParse( martialStatusString, out Slingshot.Core.Model.MaritalStatus maritalStatusEnum ) )
                {
                    person.MaritalStatus = maritalStatusEnum;
                }
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Birthdate", out string csvColumnBirthdate ) )
            {
                string birthdateString = csvEntryLookup[csvColumnBirthdate].ToStringSafe();
                if ( DateTime.TryParse( birthdateString, out DateTime birthdateDateTime ) )
                {
                    person.Birthdate = birthdateDateTime;
                }
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Anniversary Date", out string csvColumnAnniversaryDate ) )
            {
                string anniversaryDateString = csvEntryLookup[csvColumnAnniversaryDate].ToStringSafe();
                if ( DateTime.TryParse( anniversaryDateString, out DateTime AnniversaryDateTime ) )
                {
                    person.AnniversaryDate = AnniversaryDateTime;
                }
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Record Status", out string csvColumnRecordStatus ) )
            {
                string recordStatusString = csvEntryLookup[csvColumnRecordStatus].ToStringSafe();
                if ( Enum.TryParse( recordStatusString, out Slingshot.Core.Model.RecordStatus RecordStatusEnum ) )
                {
                    person.RecordStatus = RecordStatusEnum;
                }
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Inactive Reason", out string csvColumnInactiveReason ) )
            {
                person.InactiveReason = csvEntryLookup[csvColumnInactiveReason].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Is Deceased", out string csvColumnIsDeceased ) )
            {
                if ( Boolean.TryParse( csvColumnIsDeceased, out bool isDeceasedBoolean ) )
                {
                    person.IsDeceased = isDeceasedBoolean;
                }
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Connection Status", out string csvColumnConnectionStatus ) )
            {
                person.ConnectionStatus = csvEntryLookup[csvColumnConnectionStatus].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Grade", out string csvColumnGrade ) )
            {
                person.Grade = csvEntryLookup[csvColumnGrade].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Note", out string csvColumnNote ) )
            {
                person.Note = csvEntryLookup[csvColumnNote].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Campus Id", out string csvColumnCampusId ) )
            {
                person.Campus.CampusId = csvEntryLookup[csvColumnCampusId].ToIntSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Campus Name", out string csvColumnCampusName ) )
            {
                person.Campus.CampusName = csvEntryLookup[csvColumnCampusName].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Give Individually", out string csvColumnGivingIndividually ) )
            {
                if ( Boolean.TryParse( csvColumnGivingIndividually, out bool givingIndividuallyBoolean ) )
                {
                    person.GiveIndividually = givingIndividuallyBoolean;
                }
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Created Date Time", out string csvColumnCreatedDateTime ) )
            {
                string createdDateTimeString = csvEntryLookup[csvColumnCreatedDateTime].ToStringSafe();
                if ( DateTime.TryParse( createdDateTimeString, out DateTime createdDateTime ) )
                {
                    person.CreatedDateTime = createdDateTime;
                }
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Modified Date Time", out string csvColumnModifiedDateTime ) )
            {
                string modifiedDateTimeString = csvEntryLookup[csvColumnModifiedDateTime].ToStringSafe();
                if ( DateTime.TryParse( modifiedDateTimeString, out DateTime modifiedDateTime ) )
                {
                    person.ModifiedDateTime = modifiedDateTime;
                }
            }
        }

        #endregion Optionals Fields

        return person;
    }
}