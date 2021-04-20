using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Model
{
    [TestClass]
    public class FinancialTransactionExtensionMethodTests
    {
        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 1.188 )]
        [DataRow( .849 )]
        [DataRow( .0125 )]
        [DataRow( 100.2564 )]
        [DataRow( .5 )]
        public void SetApportionedDetailAmounts_CalculatesTotalsCorrectly( double exchangeRate )
        {
            var financialTransaction = new FinancialTransaction();
            financialTransaction.TransactionDetails.Add( new FinancialTransactionDetail
            {
                Amount = .5M
            } );
            financialTransaction.TransactionDetails.Add( new FinancialTransactionDetail
            {
                Amount = .51M
            } );
            financialTransaction.TransactionDetails.Add( new FinancialTransactionDetail
            {
                Amount = 10
            } );

            var decimalExchangeRate = Convert.ToDecimal( exchangeRate );
            var organizationCurrencyTotal = financialTransaction.TotalAmount * decimalExchangeRate;

            var expectedfinancialTransaction = new FinancialTransaction();
            var itemCount = financialTransaction.TransactionDetails.Count;
            var remainingTotal = organizationCurrencyTotal;

            foreach ( var detail in financialTransaction.TransactionDetails )
            {
                itemCount -= 1;

                var isLastItem = itemCount == 0;
                if ( isLastItem )
                {
                    expectedfinancialTransaction.TransactionDetails.Add( new FinancialTransactionDetail
                    {
                        Amount = remainingTotal,
                        ForeignCurrencyAmount = detail.Amount
                    } );
                }
                else
                {
                    var organizationCurrencyAmount = Math.Round( detail.Amount * decimalExchangeRate, 2 );
                    remainingTotal -= organizationCurrencyAmount;

                    expectedfinancialTransaction.TransactionDetails.Add( new FinancialTransactionDetail
                    {
                        Amount = organizationCurrencyAmount,
                        ForeignCurrencyAmount = detail.Amount
                    } );
                }
            }

            financialTransaction.SetApportionedDetailAmounts( organizationCurrencyTotal );

            Assert.That.AreEqual( expectedfinancialTransaction.TotalAmount, financialTransaction.TotalAmount );

            var expectedTransactionDetails = expectedfinancialTransaction.TransactionDetails.Cast<FinancialTransactionDetail>().ToList();
            var actualTransactionDetails = financialTransaction.TransactionDetails.Cast<FinancialTransactionDetail>().ToList();

            for ( var i = 0; i < expectedTransactionDetails.Count; i++ )
            {
                Assert.That.AreEqual( expectedTransactionDetails[i].Amount, actualTransactionDetails[i].Amount );
            }
        }
    }
}
