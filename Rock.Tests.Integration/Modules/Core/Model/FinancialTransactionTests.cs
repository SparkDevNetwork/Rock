using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class FinancialTransactionTests : DatabaseTestsBase
    {
        private string financialTransactionForeignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            financialTransactionForeignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( financialTransactionForeignKey );
        }

        [TestMethod]
        public void FinancialTransactionDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

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
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

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

        [TestMethod]
        public void FinancialTransactionDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var financialTransactionService = new FinancialTransactionService( rockContext );

            var financialTransaction = BuildFinancialTransaction( rockContext, Convert.ToDateTime( "2010-3-15" ), Convert.ToDateTime( "2010-3-16" ) );

            financialTransactionService.Add( financialTransaction );
            rockContext.SaveChanges();

            var financialTransactionId = financialTransaction.Id;

            // We're bypassing the model because the model doesn't user the FinancialTransactionDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT TransactionDateKey FROM FinancialTransaction WHERE Id = {financialTransactionId}" ).First();

            Assert.AreEqual( 20100315, result );

            result = rockContext.Database.
                            SqlQuery<int>( $"SELECT SettledDateKey FROM FinancialTransaction WHERE Id = {financialTransactionId}" ).First();

            Assert.AreEqual( 20100316, result );
        }

        [TestMethod]
        public void FinancialTransactionDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var transactionYear = 2015;
            var settledYear = 2016;
            using ( var rockContext = new RockContext() )
            {
                var financialTransactionService = new FinancialTransactionService( rockContext );

                var minTransactionDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, transactionYear );
                var maxTransactionDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, transactionYear );

                var minSettledDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, settledYear );
                var maxSettledDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, settledYear );



                for ( var i = 0; i < 15; i++ )
                {

                    var financialTransaction = BuildFinancialTransaction( rockContext,
                        TestDataHelper.GetRandomDateInRange( minTransactionDateValue, maxTransactionDateValue ),
                        TestDataHelper.GetRandomDateInRange( minSettledDateValue, maxSettledDateValue ) );

                    financialTransactionService.Add( financialTransaction );
                }

                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var financialTransactionService = new FinancialTransactionService( rockContext );

                var financialTransactions = financialTransactionService.
                                Queryable().
                                Where( i => i.ForeignKey == financialTransactionForeignKey ).
                                Where( i => i.TransactionSourceDate.CalendarYear == transactionYear );

                Assert.AreEqual( expectedRecordCount, financialTransactions.Count() );
                Assert.IsNotNull( financialTransactions.First().TransactionSourceDate );

                financialTransactions = financialTransactionService.
                                    Queryable().
                                    Where( i => i.ForeignKey == financialTransactionForeignKey ).
                                    Where( i => i.SettledSourceDate.CalendarYear == settledYear );

                Assert.AreEqual( expectedRecordCount, financialTransactions.Count() );
                Assert.IsNotNull( financialTransactions.First().SettledSourceDate );
            }
        }

        private Rock.Model.FinancialTransaction BuildFinancialTransaction( RockContext rockContext, DateTime transactionDateTime, DateTime settledDate )
        {
            var transactionTypeContributionValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

            var financialTransaction = new FinancialTransaction();
            financialTransaction.TransactionTypeValueId = transactionTypeContributionValueId;
            financialTransaction.TransactionCode = "Test";
            financialTransaction.ForeignKey = financialTransactionForeignKey;
            financialTransaction.TransactionDateTime = transactionDateTime;
            financialTransaction.SettledDate = settledDate;
            return financialTransaction;
        }

        private void CleanUpData( string financialTransactionForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE [FinancialTransaction] WHERE [ForeignKey] = '{financialTransactionForeignKey}'" );
        }
    }
}
