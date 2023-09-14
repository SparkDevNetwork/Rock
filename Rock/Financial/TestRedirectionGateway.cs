// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Test Payment Gateway
    /// </summary>
    [Description( "Test Redirection Payment Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "TestRedirectionGateway" )]

    [TextField( "Declined Card Numbers",
        Key = AttributeKey.DeclinedCardNumbers,
        Description = "Enter partial card numbers that you wish to be declined separated by commas. Any card number that ends with a number matching a value entered here will be declined.",
        IsRequired = false,
        Order = 0 )]

    [TextField( "Declined CVV",
        Key = AttributeKey.DeclinedCVV,
        Description = "Enter a CVV that should be declined",
        DefaultValue = "911",
        IsRequired = false,
        Order = 1 )]

    [IntegerField( "Years until Max Expiration",
        Key = AttributeKey.MaxExpirationYears,
        Description = "The number of years before an 'Invalid Card Expiration' validation error occurs.",
        DefaultIntegerValue = 10,
        IsRequired = false,
        Order = 2 )]

    [BooleanField(
        "Generate Fake Payments",
        Description = "Enable this to return some sample payments for the Download Payments job and block. Note that this requires that some scheduled transactions are created for this gateway.",
        Key = AttributeKey.GenerateFakeGetPayments,
        DefaultBooleanValue = false,
        Order = 3 )]
    [Rock.SystemGuid.EntityTypeGuid( "AB3C5BF6-4D99-4289-84AF-3EF798849705")]
    public class TestRedirectionGateway : GatewayComponent, IAutomatedGatewayComponent, IRedirectionGatewayComponent
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Component Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DeclinedCardNumbers = "DeclinedCardNumbers";
            public const string GenerateFakeGetPayments = "GenerateFakeGetPayments";
            public const string MaxExpirationYears = "MaxExpirationYears";
            public const string DeclinedCVV = "DeclinedCVV";
        }

        #endregion

        #region Automated Gateway Component

        /// <summary>
        /// The most recent exception thrown by the gateway's remote API
        /// </summary>
        public Exception MostRecentException { get; private set; }

        /// <summary>
        /// Handle a payment from a REST endpoint or other automated means. This payment can only be made with a saved account.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="metadata">Optional. Metadata key value pairs to send to the gateway</param>
        /// <returns></returns>
        public Payment AutomatedCharge( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage, Dictionary<string, string> metadata = null )
        {
            MostRecentException = null;
            errorMessage = string.Empty;
            var transaction = Charge( financialGateway, paymentInfo, out errorMessage );

            if ( !string.IsNullOrEmpty( errorMessage ) )
            {
                MostRecentException = new Exception( errorMessage );
                return null;
            }

            if ( transaction == null )
            {
                errorMessage = "No error was indicated but the transaction was null";
                MostRecentException = new Exception( errorMessage );
                return null;
            }

            return new Payment
            {
                TransactionCode = transaction.TransactionCode,
                Amount = paymentInfo.Amount
            };
        }

        #endregion

        #region Gateway Component Implementation

        /// <summary>
        /// Gets the supported payment schedules.
        /// </summary>
        /// <value>
        /// The supported payment schedules.
        /// </value>
        public override List<DefinedValueCache> SupportedPaymentSchedules
        {
            get
            {
                var values = new List<DefinedValueCache>();
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY ) );
                return values;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the gateway requires the name on card for CC processing
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        /// <value>
        ///   <c>true</c> if [name on card required]; otherwise, <c>false</c>.
        /// </value>
        public override bool PromptForNameOnCard( FinancialGateway financialGateway )
        {
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether [address required].
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        /// <value>
        ///   <c>true</c> if [address required]; otherwise, <c>false</c>.
        /// </value>
        public override bool PromptForBillingAddress( FinancialGateway financialGateway )
        {
            return true;
        }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( ValidateCard( financialGateway, paymentInfo, out errorMessage ) )
            {
                var transaction = new FinancialTransaction();
                transaction.TransactionCode = "T" + RockDateTime.Now.ToString( "yyyyMMddHHmmssFFF" );
                return transaction;
            }

            return null;
        }

        /// <summary>
        /// Authorizes the specified payment information.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Authorize( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( ValidateCard( financialGateway, paymentInfo, out errorMessage ) )
            {
                var transaction = new FinancialTransaction();
                transaction.TransactionCode = "T" + RockDateTime.Now.ToString( "yyyyMMddHHmmssFFF" );
                return transaction;
            }

            return null;
        }
        /// <summary>
        /// Credits the specified transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Credit( FinancialTransaction transaction, decimal amount, string comment, out string errorMessage )
        {
            errorMessage = string.Empty;

            var refundTransaction = new FinancialTransaction();
            refundTransaction.TransactionCode = "T" + RockDateTime.Now.ToString( "yyyyMMddHHmmssFFF" );
            return refundTransaction;
        }

        /// <summary>
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( ValidateCard( financialGateway, paymentInfo, out errorMessage ) )
            {
                var scheduledTransaction = new FinancialScheduledTransaction();
                scheduledTransaction.IsActive = true;
                scheduledTransaction.StartDate = schedule.StartDate;
                scheduledTransaction.NextPaymentDate = schedule.StartDate;
                scheduledTransaction.TransactionCode = "T" + RockDateTime.Now.ToString( "yyyyMMddHHmmssFFF" );
                scheduledTransaction.GatewayScheduleId = "Subscription_" + RockDateTime.Now.ToString( "yyyyMMddHHmmssFFF" );
                scheduledTransaction.LastStatusUpdateDateTime = RockDateTime.Now;
                scheduledTransaction.Status = FinancialScheduledTransactionStatus.Active;
                scheduledTransaction.StatusMessage = "active";
                return scheduledTransaction;
            }

            return null;
        }

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            transaction.IsActive = true;
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            var referencePaymentInfo = paymentInfo as ReferencePaymentInfo;

            if ( referencePaymentInfo != null )
            {
                transaction.TransactionCode = referencePaymentInfo.TransactionCode;
            }

            return true;
        }

        /// <summary>
        /// Cancels the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            transaction.IsActive = false;
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets the scheduled payment status.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            transaction.LastStatusUpdateDateTime = RockDateTime.Now;
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage )
        {
            errorMessage = string.Empty;
            if ( !this.GetAttributeValue( financialGateway, AttributeKey.GenerateFakeGetPayments ).AsBoolean() )
            {
                return new List<Payment>();
            }

            var fakePayments = new List<Payment>();
            var randomNumberOfPayments = new Random().Next( 1, 1000 );
            var rockContext = new Rock.Data.RockContext();
            var scheduledTransactionList = new FinancialScheduledTransactionService( rockContext ).Queryable().Where( a => a.FinancialGatewayId == financialGateway.Id ).ToList();
            if ( !scheduledTransactionList.Any() )
            {
                return fakePayments;
            }

            var transactionDateTime = startDate;
            for ( int paymentNumber = 0; paymentNumber < randomNumberOfPayments; paymentNumber++ )
            {
                // get a random scheduled Transaction (if any)
                var scheduledTransaction = scheduledTransactionList.OrderBy( a => a.Guid ).FirstOrDefault();
                if ( scheduledTransaction == null )
                {
                    return new List<Payment>();
                }

                var fakePayment = new Payment
                {
                    Amount = scheduledTransaction.TotalAmount,
                    TransactionDateTime = startDate,
                    CreditCardTypeValue = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE.AsGuid() ).DefinedValues.OrderBy( a => Guid.NewGuid() ).First(),
                    CurrencyTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() ),
                    TransactionCode = Guid.NewGuid().ToString( "N" ),
                    GatewayScheduleId = scheduledTransaction.GatewayScheduleId,
                    GatewayPersonIdentifier = scheduledTransaction?.FinancialPaymentDetail?.GatewayPersonIdentifier
                };

                fakePayments.Add( fakePayment );
            }


            return fakePayments;
        }

        /// <summary>
        /// Gets an optional reference identifier needed to process future transaction from saved account.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            return transaction.TransactionCode;
        }

        /// <summary>
        /// Gets an optional reference identifier needed to process future transaction from saved account.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            return string.Empty;
        }

        /// <summary>
        /// Gets the next payment date.
        /// </summary>
        /// <param name="scheduledTransaction">The transaction.</param>
        /// <param name="lastTransactionDate">The last transaction date.</param>
        /// <returns></returns>
        public override DateTime? GetNextPaymentDate( FinancialScheduledTransaction scheduledTransaction, DateTime? lastTransactionDate )
        {
            if ( scheduledTransaction.IsActive )
            {
                return CalculateNextPaymentDate( scheduledTransaction, lastTransactionDate );
            }
            else
            {
                return scheduledTransaction.NextPaymentDate;
            }
        }

        #endregion

        #region Private Methods

        private bool ValidateCard( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            string cardNumber = string.Empty;
            var declinedCVV = this.GetAttributeValue( financialGateway, AttributeKey.DeclinedCVV );
            int maxExpirationYears = this.GetAttributeValue( financialGateway, AttributeKey.MaxExpirationYears ).AsIntegerOrNull() ?? 10;

            CreditCardPaymentInfo ccPayment = paymentInfo as CreditCardPaymentInfo;
            if ( ccPayment != null )
            {
                if ( declinedCVV.IsNotNullOrWhiteSpace() && ccPayment.Code == declinedCVV )
                {
                    errorMessage = "Declined CVV";
                    return false;
                }

                cardNumber = ccPayment.Number;

                if ( ccPayment.ExpirationDate < RockDateTime.Now.Date )
                {
                    errorMessage = "Card Expired";
                    return false;
                }

                if ( ccPayment.ExpirationDate > RockDateTime.Now.AddYears( maxExpirationYears ) )
                {
                    errorMessage = "Invalid Card Expiration";
                    return false;
                }

                if ( ccPayment.Number.IsNullOrWhiteSpace() )
                {
                    errorMessage = "Card number is required.";
                    return false;
                }

                if ( ccPayment.Code.IsNullOrWhiteSpace() )
                {
                    errorMessage = "CVV is required.";
                    return false;
                }
            }

            SwipePaymentInfo swipePayment = paymentInfo as SwipePaymentInfo;
            if ( swipePayment != null )
            {
                cardNumber = swipePayment.Number;
            }

            if ( !string.IsNullOrWhiteSpace( cardNumber ) )
            {
                var declinedNumbers = GetAttributeValue( financialGateway, AttributeKey.DeclinedCardNumbers );
                if ( !string.IsNullOrWhiteSpace( declinedNumbers ) )
                {
                    if ( declinedNumbers.SplitDelimitedValues().Any( n => cardNumber.EndsWith( n ) ) )
                    {
                        errorMessage = "Declined Card";
                        return false;
                    }
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        #endregion

        #region IRedirectionGateway Implementation

        /// <summary>
        /// Gets the merchant field label.
        /// </summary>
        /// <value>
        /// The merchant field label.
        /// </value>
        public string MerchantFieldLabel => "Test Gateway Merchant Field";

        /// <summary>
        /// Gets the fund field label.
        /// </summary>
        /// <value>
        /// The fund field label.
        /// </value>
        public string FundFieldLabel => "Test Gateway Fund Field";

        /// <summary>
        /// Gets the merchants.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<KeyValuePair<string, string>> GetMerchants()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("1", "Test 1"),
                new KeyValuePair<string, string>("2", "Test 2"),
                new KeyValuePair<string, string>("3", "Test 3"),
            };
        }

        /// <summary>
        /// Gets the merchant funds.
        /// </summary>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<KeyValuePair<string, string>> GetMerchantFunds( string merchantId )
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("1", $"Test {merchantId}-1"),
                new KeyValuePair<string, string>("2", $"Test {merchantId}-2"),
                new KeyValuePair<string, string>("3", $"Test {merchantId}-3"),
            };
        }

        /// <inheritdoc/>
        public string GetPaymentRedirectUrl( int? fundId, decimal amount, string returnUrl, Dictionary<string, string> metadata )
        {
            if ( returnUrl.Contains( "?" ) )
            {
                returnUrl = $"{returnUrl}&paymentToken={Guid.NewGuid()}:{amount}";
            }
            else
            {
                returnUrl = $"{returnUrl}?paymentToken={Guid.NewGuid()}:{amount}";
            }

            /*
             * Okay, there are about 18 levels of madness here.
             *
             * Level 1: We can't actually redirect anywhere since we don't have
             * a test give page setup anywhere.
             * 
             * Level 2: Instead, we construct some fake HTML that we can display
             * in the browser.
             * 
             * Level 3: data: URLs aren't allowed to be set from Javascript anymore.
             * 
             * Level 4: Use a javascript: URL to replace the body contents with
             * our custom HTML.
             * 
             * Level 5: When the person clicks the link in the fake body page it
             * redirects them back to the return url with the new query parameters.
             *
             * -Daniel Hazelbaker 9/13/2021
             */
            var javascript = $@"javascript:window.document.body.innerHTML = '<h1>Test Redirection Gateway</h1>
<p>You will pay a simulated amount of {amount.FormatAsCurrency()}</p>
<p><a href=""{returnUrl}"">Pay and Return</a></p>'";

            return javascript;
        }

        /// <inheritdoc/>
        public bool TryGetPaymentTokenFromParameters( FinancialGateway financialGateway, IDictionary<string, string> parameters, out string paymentToken )
        {
            return parameters.TryGetValue( "paymentToken", out paymentToken );
        }

        /// <inheritdoc/>
        public bool IsPaymentTokenCharged( FinancialGateway financialGateway, string paymentToken )
        {
            return true;
        }

        /// <inheritdoc/>
        public FinancialTransaction FetchPaymentTokenTransaction( Data.RockContext rockContext, FinancialGateway financialGateway, int? fundId, string paymentToken )
        {
            var tokenComponents = paymentToken.ToStringSafe().Split( ':' );

            decimal amount = tokenComponents.Length >= 2 ? tokenComponents[1].AsDecimalOrNull() ?? 10 : 10;

            return new FinancialTransaction
            {
                TransactionCode = paymentToken,
                TransactionDetails = new List<FinancialTransactionDetail> {
                    new FinancialTransactionDetail {
                        Amount = amount
                    }
                }
            };
        }

        /// <inheritdoc/>
        public string CreateCustomerAccount( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            return Guid.NewGuid().ToString( "N" );
        }

        #endregion IRedirectionGateway Implementation
    }
}