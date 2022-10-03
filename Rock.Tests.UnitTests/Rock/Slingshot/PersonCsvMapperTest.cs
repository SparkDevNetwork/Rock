using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Slingshot;
using Slingshot.Core.Model;

namespace Rock.Tests.UnitTests.Rock.Slingshot
{
    [TestClass]
    public class PersonCsvMapperTest
    {
        [TestMethod]
        public void MaritalStatusShouldBeSetToUnknowIfNotPresent()
        {
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.AreEqual( MaritalStatus.Unknown, person.MaritalStatus );
        }

        [TestMethod]
        public void MaritalStatusShouldBeSetToUnknowAndReturnMessageIfInvalid()
        {
            const string invalidMaritalStatus = "Engaged";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["MaritalStatus"] = invalidMaritalStatus;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper["Marital Status"] = "MaritalStatus";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.AreEqual( MaritalStatus.Unknown, person.MaritalStatus );
            Assert.IsTrue( parserErrors.Count == 1 );
        }

        [TestMethod]
        public void MaritalStatusShouldBeSetIfValid()
        {
            const string validMaritalStatus = "Married";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["MaritalStatus"] = validMaritalStatus;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper["Marital Status"] = "MaritalStatus";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out _ );

            Assert.AreEqual( MaritalStatus.Married, person.MaritalStatus );
        }

        [TestMethod]
        public void GenderShouldBeDefaultedToUnknownAndReturnMessageIfInValid()
        {
            const string invalidGender = "InvalidGender";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["Gender"] = invalidGender;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper["Gender"] = "Gender";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.AreEqual( Gender.Unknown, person.Gender );
            Assert.IsTrue( parserErrors.Count == 1 );
        }

        [TestMethod]
        public void IsDesceasedShouldBeDefaultedToFalseAndReturnMessageIfInValid()
        {
            const string invalidIsDesceasedEntry = "Invalid";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["Is Deceased"] = invalidIsDesceasedEntry;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper["Is Deceased"] = "Is Deceased";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.IsFalse( person.IsDeceased );
            Assert.IsTrue( parserErrors.Count == 1 );
        }

        [TestMethod]
        public void BirthdateShouldBeDefaultedToEmptyAndReturnMessageIfInValid()
        {
            const string invalidBirthday = "Invalid Date String";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["Birthdate"] = invalidBirthday;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper["Birthdate"] = "Birthdate";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.IsNull( person.Birthdate );
            Assert.IsTrue( parserErrors.Count == 1 );
        }

        [TestMethod]
        public void EmailShouldBeDefaultedToEmptyAndReturnMessageIfInValid()
        {
            const string invalidEmailAddress = "Invalid Email";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["Email"] = invalidEmailAddress;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper["Email"] = "Email";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.IsTrue( string.IsNullOrEmpty( person.Email ) );
            Assert.IsTrue( parserErrors.Count == 1 );
        }

        [TestMethod]
        public void RecordStatusShouldBeDefaultedToActiveAndReturnMessageIfInValid()
        {
            const string invalidRecordStatus = "invalidRecordStatus";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["Record Status"] = invalidRecordStatus;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper[CSVHeaders.RecordStatus] = "Record Status";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.AreEqual( RecordStatus.Active, person.RecordStatus );
            Assert.IsTrue( parserErrors.Count == 1 );
        }

        [TestMethod]
        public void RecordStatusActriveShouldBeValid()
        {
            const string activeRecordStatus = "Active";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["Record Status"] = activeRecordStatus;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper[CSVHeaders.RecordStatus] = "Record Status";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.AreEqual( RecordStatus.Active, person.RecordStatus );
            Assert.AreEqual( 0, parserErrors.Count );
        }

        [TestMethod]
        public void EmailPreferenceShouldBeDefaultedToEmailAllowedAndReturnMessageIfInValid()
        {
            const string invalidemailPreference = "invalidemailPreference";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["Email Preference"] = invalidemailPreference;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper[CSVHeaders.EmailPreference] = "Email Preference";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.AreEqual( EmailPreference.EmailAllowed, person.EmailPreference );
            Assert.IsTrue( parserErrors.Count == 1 );
        }

        [TestMethod]
        public void FamilyRoleShouldBeDefaultedToAdultAndReturnMessageIfInValid()
        {
            const string invalidChild = "C";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["Family Role"] = invalidChild;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper[CSVHeaders.FamilyRole] = "Family Role";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.AreEqual( FamilyRole.Adult, person.FamilyRole );
            Assert.IsTrue( parserErrors.Count == 1 );
        }

        [TestMethod]
        public void GivingIndividuallyShouldBeDefaultedToFalseAndReturnMessageIfInValid()
        {
            const string invalidGiving = "Invalid";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["Is Deceased"] = invalidGiving;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper[CSVHeaders.GiveIndividually] = "Is Deceased";

            Person person = PersonCsvMapper.Map( csvEntry, headerMapper, out HashSet<string> parserErrors );

            Assert.IsNull( person.GiveIndividually );
            Assert.IsTrue( parserErrors.Count == 1 );
        }

        private static Dictionary<string, string> RequiredHeaderMapperDictionary()
        {
            Dictionary<string, string> headerMapper = new Dictionary<string, string>();
            headerMapper["Id"] = "Id";
            headerMapper["Family Id"] = "FamilyId";
            headerMapper["Family Role"] = "FamilyRole";
            headerMapper["First Name"] = "FirstName";
            headerMapper["Last Name"] = "LastName";
            return headerMapper;
        }

        private static Dictionary<string, object> BasicCSVEntry()
        {
            Dictionary<string, object> csvEntry = new Dictionary<string, object>();
            csvEntry["Id"] = "42";
            csvEntry["FamilyId"] = "45";
            csvEntry["FamilyRole"] = "Adult";
            csvEntry["FirstName"] = "First Name";
            csvEntry["LastName"] = "Last Name";
            return csvEntry;
        }
    }
}
