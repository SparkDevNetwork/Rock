using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model
{
    [TestClass]
    public class FinancialPledgeTests
    {
        [TestMethod]
        public void FinancialPledgeDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                var financialPledge = new Rock.Model.FinancialPledge();
                financialPledge.StartDate = keyValue.Value;
                Assert.AreEqual( keyValue.Key, financialPledge.StartDateKey );
            }

            testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                var financialPledge = new Rock.Model.FinancialPledge();
                financialPledge.EndDate = keyValue.Value;
                Assert.AreEqual( keyValue.Key, financialPledge.EndDateKey );
            }
        }
    }
}
