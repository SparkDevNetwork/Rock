using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model
{
    [TestClass]
    public class CommunicationTests
    {
        [TestMethod]
        public void CommunicationSendDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                var communication = new Rock.Model.Communication();
                communication.SendDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, communication.SendDateKey );
            }
        }

        [TestMethod]
        public void ConnectionRequestSendDateKeyWorksWithNullValue()
        {
            var communication = new Rock.Model.Communication();
            communication.SendDateTime = null;
            Assert.IsNull( communication.SendDateKey );
        }
    }
}
