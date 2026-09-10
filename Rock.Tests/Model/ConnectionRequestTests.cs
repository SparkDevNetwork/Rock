using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model
{
    [TestClass]
    public class ConnectionRequestTests
    {
        [TestMethod]
        public void ConnectionRequestCreatedDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                ConnectionRequest connectionRequest = new ConnectionRequest();
                connectionRequest.CreatedDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, connectionRequest.CreatedDateKey );
            }
        }

        [TestMethod]
        public void ConnectionRequestCreatedDateKeyWorksWithNullValue()
        {
            ConnectionRequest connectionRequest = new ConnectionRequest();
            connectionRequest.CreatedDateTime = null;
            Assert.IsNull( connectionRequest.CreatedDateKey );
        }
    }
}
