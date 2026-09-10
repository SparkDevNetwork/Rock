using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model
{
    [TestClass]
    public class BenevolenceRequestTests
    {
        [TestMethod]
        public void BenevolenceRequestDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                BenevolenceRequest benevolenceRequest = new BenevolenceRequest();
                benevolenceRequest.RequestDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, benevolenceRequest.RequestDateKey );
            }
        }
    }
}
