using System.Dynamic;
using Slingshot.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Rock.Tests.UnitTests.Rock.Slingshot
{
    [TestClass]
    public class PersonCSVMapperTest
    {
        [TestMethod]
        public void MaritalStatusShouldBeSetToUnknowIfNotPresent()
        {
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();

            Person person = PersonCSVMapper.Map( csvEntry, headerMapper );

            Assert.AreEqual( MaritalStatus.Unknown, person.MaritalStatus );
        }

        [TestMethod]
        public void MaritalStatusShouldBeSetToUnknowIfInvalid()
        {
            const string invalidMaritalStatus = "Engaged";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["MaritalStatus"] = invalidMaritalStatus;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper["Marital Status"] = "MaritalStatus";

            Person person = PersonCSVMapper.Map( csvEntry, headerMapper );
            
            Assert.AreEqual( MaritalStatus.Unknown, person.MaritalStatus );
        }

        [TestMethod]
        public void MaritalStatusShouldBeSetIfValid()
        {
            const string validMaritalStatus = "Married";
            Dictionary<string, object> csvEntry = BasicCSVEntry();
            csvEntry["MaritalStatus"] = validMaritalStatus;
            Dictionary<string, string> headerMapper = RequiredHeaderMapperDictionary();
            headerMapper["Marital Status"] = "MaritalStatus";

            Person person = PersonCSVMapper.Map( csvEntry, headerMapper );
            
            Assert.AreEqual( MaritalStatus.Married, person.MaritalStatus );
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
