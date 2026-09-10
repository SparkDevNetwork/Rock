using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model
{
    [TestClass]
    public class FinancialTransactionTests
    {
        [TestMethod]
        public void FinancialTransactionDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                FinancialTransaction financialTransaction = new FinancialTransaction();
                financialTransaction.TransactionDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, financialTransaction.TransactionDateKey );
            }
        }

        [TestMethod]
        public void FinancialTransactionDateKeyWorksWithNullValue()
        {
            var financialTransaction = new Rock.Model.FinancialTransaction();
            financialTransaction.TransactionDateTime = null;
            Assert.IsNull( financialTransaction.TransactionDateKey );
        }

        [TestMethod]
        public void SettledDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                FinancialTransaction financialTransaction = new FinancialTransaction();
                financialTransaction.SettledDate = keyValue.Value;
                Assert.AreEqual( keyValue.Key, financialTransaction.SettledDateKey );
            }
        }

        [TestMethod]
        public void SettledDateKeyWorksWithNullValue()
        {
            var financialTransaction = new Rock.Model.FinancialTransaction();
            financialTransaction.SettledDate = null;
            Assert.IsNull( financialTransaction.SettledDateKey );
        }
    }
}
