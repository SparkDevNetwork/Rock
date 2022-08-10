using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Slingshot;
using Slingshot.Core.Model;

namespace Rock.Tests.UnitTests.Rock.Slingshot
{
    [TestClass]
    public class PersonPhoneCsvMapperTest
    {
        Dictionary<string, object> csvEntry = new Dictionary<string, object>();
        Dictionary<string, string> headerMapper = new Dictionary<string, string>();

        [TestMethod]
        public void MobilePhoneNumberShouldBeAddedWithoutSMSEnabled()
        {
            headerMapper["Id"] = "Id";
            headerMapper["Mobile Phone"] = "Mobile Phone";
            csvEntry["Id"] = "45";
            csvEntry["Mobile Phone"] = "6233452378";
            HashSet<string> parserErrors = new HashSet<string>();

            List<PersonPhone> personPhones = PersonPhoneCsvMapper.Map( csvEntry, headerMapper, ref parserErrors );

            Assert.AreEqual( 45, personPhones[0].PersonId );
            Assert.AreEqual( "6233452378", personPhones[0].PhoneNumber );
            Assert.AreEqual( "Mobile", personPhones[0].PhoneType );
            Assert.IsFalse( personPhones[0].IsMessagingEnabled ?? false );
        }

        [TestMethod]
        public void MobilePhoneNumberShouldBeAddedWithSMSEnabled()
        {
            headerMapper["Id"] = "Id";
            headerMapper["Mobile Phone"] = "Mobile Phone";
            headerMapper["Is SMS Enabled"] = "Is SMS Enabled";
            csvEntry["Id"] = "45";
            csvEntry["Mobile Phone"] = "6233452378";
            csvEntry["Is SMS Enabled"] = "TRUE";
            HashSet<string> parserErrors = new HashSet<string>();

            List<PersonPhone> personPhones = PersonPhoneCsvMapper.Map( csvEntry, headerMapper, ref parserErrors );

            Assert.AreEqual( 45, personPhones[0].PersonId );
            Assert.AreEqual( "6233452378", personPhones[0].PhoneNumber );
            Assert.AreEqual( "Mobile", personPhones[0].PhoneType );
            Assert.IsTrue( personPhones[0].IsMessagingEnabled ?? false );
        }

        [TestMethod]
        public void MobilePhoneNumberShouldBeAddedWithSMSDisabled()
        {
            headerMapper["Id"] = "Id";
            headerMapper["Mobile Phone"] = "Mobile Phone";
            headerMapper["Is SMS Enabled"] = "Is SMS Enabled";
            csvEntry["Id"] = "45";
            csvEntry["Mobile Phone"] = "6233452378";
            csvEntry["Is SMS Enabled"] = "False";
            HashSet<string> parserErrors = new HashSet<string>();

            List<PersonPhone> personPhones = PersonPhoneCsvMapper.Map( csvEntry, headerMapper, ref parserErrors );

            Assert.AreEqual( 45, personPhones[0].PersonId );
            Assert.AreEqual( "6233452378", personPhones[0].PhoneNumber );
            Assert.AreEqual( "Mobile", personPhones[0].PhoneType );
            Assert.IsFalse( personPhones[0].IsMessagingEnabled ?? false );
        }

        [TestMethod]
        public void MobilePhoneNumberShouldBeSetToFlaseIfInvalidData()
        {
            headerMapper["Id"] = "Id";
            headerMapper["Mobile Phone"] = "Mobile Phone";
            headerMapper["Is SMS Enabled"] = "Is SMS Enabled";
            csvEntry["Id"] = "45";
            csvEntry["Mobile Phone"] = "6233452378";
            csvEntry["Is SMS Enabled"] = "Some String";
            HashSet<string> parserErrors = new HashSet<string>();

            List<PersonPhone> personPhones = PersonPhoneCsvMapper.Map( csvEntry, headerMapper, ref parserErrors );

            Assert.AreEqual( 45, personPhones[0].PersonId );
            Assert.AreEqual( "6233452378", personPhones[0].PhoneNumber );
            Assert.AreEqual( "Mobile", personPhones[0].PhoneType );
            Assert.IsFalse( personPhones[0].IsMessagingEnabled ?? false );
        }
    }
}
