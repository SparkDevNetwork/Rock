using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model
{
    [TestClass]
    public class RegistrationTests
    {
        [TestMethod]
        public void RegistrationCreatedDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                var registration = new Rock.Model.Registration();
                registration.CreatedDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, registration.CreatedDateKey );
            }
        }

        [TestMethod]
        public void RegistrationCreatedDateKeyWorksWithNullValue()
        {
            var registration = new Rock.Model.Registration();
            registration.CreatedDateTime = null;
            Assert.IsNull( registration.CreatedDateKey );
        }
    }
}
