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

using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Base class for financial provider components
    /// </summary>
    public abstract class GatewayComponent : Component, IGatewayComponent
    {

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "Active", "True" );
                defaults.Add( "Order", "0" );
                return defaults;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayComponent" /> class.
        /// </summary>
        public GatewayComponent() : base( false )
        {
            // Override default constructor of Component that loads attributes (not needed for gateway components, needs to be done by each financial gateway)
        }

        /// <summary>
        /// Loads the attributes for the financial gateway.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        public void LoadAttributes( FinancialGateway financialGateway )
        {
            financialGateway.LoadAttributes();
        }

        /// <summary>
        /// Use GetAttributeValue( FinancialGateway financialGateway, string key) instead.  gateway component attribute values are 
        /// specific to the financial gateway instance (rather than global).  This method will throw an exception
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Gateway Component attributes are saved specific to the financial gateway, which requires that the current financial gateway is included in order to load or retrieve values. Use the GetAttributeValue( FinancialGateway financialGateway, string key ) method instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Gateway Component attributes are saved specific to the financial gateway, which requires that the current financial gateway is included in order to load or retrieve values. Use the GetAttributeValue( FinancialGateway financialGateway, string key ) method instead." );
        }

        /// <summary>
        /// Always returns 0.  
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Always returns true. 
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive
        {
            get
            {
                return true;
                ;
            }
        }

        /// <summary>
        /// Gets the attribute value for the gateway 
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough]
        public string GetAttributeValue( FinancialGateway financialGateway, string key )
        {
            if ( financialGateway.AttributeValues == null )
            {
                financialGateway.LoadAttributes();
            }

            var values = financialGateway.AttributeValues;
            if ( values != null && values.ContainsKey( key ) )
            {
                var keyValues = values[key];
                if ( keyValues != null )
                {
                    return keyValues.Value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether gateway provider needs first and last name on credit card as two distinct fields.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [split name on card]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SplitNameOnCard
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the supported payment schedules.
        /// </summary>
        /// <value>
        /// The supported payment schedules.
        /// </value>
        public virtual List<DefinedValueCache> SupportedPaymentSchedules
        {
            get { return new List<DefinedValueCache>(); }
        }

        /// <summary>
        /// Gets a value indicating whether this gateway can be used by Rock to create new transactions (vs. just used to download externally created transactions)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports charge]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SupportsRockInitiatedTransactions
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the gateway requires the name on card for CC processing
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        /// <value>
        ///   <c>true</c> if [name on card required]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool PromptForNameOnCard( FinancialGateway financialGateway )
        {
            return true;
        }

        /// <summary>
        /// Prompts for the person name associated with a bank account.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        public virtual bool PromptForBankAccountName( FinancialGateway financialGateway )
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
        public virtual bool PromptForBillingAddress( FinancialGateway financialGateway )
        {
            return true;
        }

        /// <summary>
        /// Returns a boolean value indicating if 'Saved Account' functionality is supported for the given currency type. 
        /// </summary>
        /// <param name="currencyType">Type of the currency.</param>
        /// <returns></returns>
        public virtual bool SupportsSavedAccount( DefinedValueCache currencyType )
        {
            return true;
        }

        /// <summary>
        /// Returns a boolean value indicating if 'Saved Account' functionality is supported for frequency (i.e. one-time vs repeating )
        /// </summary>
        /// <param name="isRepeating">if set to <c>true</c> [is repeating].</param>
        /// <returns></returns>
        public virtual bool SupportsSavedAccount( bool isRepeating )
        {
            return true;
        }

        /// <summary>
        /// Authorizes the specified payment information.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public virtual FinancialTransaction Authorize( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = "Gateway does not support Authorizations";
            return null;
        }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Credits (Refunds) the specified transaction.
        /// </summary>
        /// <param name="origTransaction">The original transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract FinancialTransaction Credit( FinancialTransaction origTransaction, decimal amount, string comment, out string errorMessage );

        /// <summary>
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Flag indicating if gateway supports updating a scheduled payment.
        /// </summary>
        /// <returns></returns>
        public virtual bool UpdateScheduledPaymentSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Flag indicating if the gateway supports modifying an existing schedule's payment method.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsUpdatingSchedulePaymentMethodSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Cancels the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage );

        /// <summary>
        /// Flag indicating if gateway supports reactivating a scheduled payment.
        /// </summary>
        /// <returns></returns>
        public virtual bool ReactivateScheduledPaymentSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage );

        /// <summary>
        /// Flag indicating if gateway supports getting status of a scheduled payment.
        /// </summary>
        /// <returns></returns>
        public virtual bool GetScheduledPaymentStatusSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the scheduled payment status.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool GetScheduledPaymentStatus( FinancialScheduledTransaction transaction, out string errorMessage );

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage );

        /// <summary>
        /// Gets an optional reference number needed to process future transaction from saved account.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage );

        /// <summary>
        /// Gets an optional reference number needed to process future transaction from saved account.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage );

        /// <summary>
        /// Gets the next payment date.
        /// </summary>
        /// <param name="scheduledTransaction">The transaction.</param>
        /// <param name="lastTransactionDate">The last transaction date.</param>
        /// <returns></returns>
        public virtual DateTime? GetNextPaymentDate( FinancialScheduledTransaction scheduledTransaction, DateTime? lastTransactionDate )
        {
            return scheduledTransaction.NextPaymentDate;
        }

        /// <summary>
        /// Calculates the next payment date based off of frequency and last transaction date.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="lastTransactionDate">The last transaction date.</param>
        /// <returns></returns>
        protected DateTime? CalculateNextPaymentDate( FinancialScheduledTransaction scheduledTransaction, DateTime? lastTransactionDate )
        {
            // If scheduled transaction is null, just return null
            if ( scheduledTransaction == null )
            {
                return null;
            }

            // If start date is today or in future, return that value
            if ( scheduledTransaction.StartDate >= RockDateTime.Today.Date )
            {
                return scheduledTransaction.StartDate;
            }

            // If scheduled transaction does not have a frequency, just return null
            if ( scheduledTransaction.TransactionFrequencyValue == null )
            {
                return null;
            }

            // Calculate the later of start date or last transaction date, and use that to calculate next payment
            var startDate = scheduledTransaction.StartDate;
            if ( lastTransactionDate.HasValue && lastTransactionDate > startDate )
            {
                startDate = lastTransactionDate.Value;
            }

            // Calculate the next payment date based on the frequency
            DateTime? nextPayment = null;
            switch ( scheduledTransaction.TransactionFrequencyValue.Guid.ToString().ToUpper() )
            {
                case SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY:
                    nextPayment = startDate.AddDays( 7 );
                    break;
                case SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY:
                    nextPayment = startDate.AddDays( 14 );
                    break;
                case SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY:
                    nextPayment = startDate.AddDays( 15 );
                    break;
                case SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY:
                    nextPayment = startDate.AddMonths( 1 );
                    break;
                case SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_QUARTERLY:
                    nextPayment = startDate.AddMonths( 3 );
                    break;
                case SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEYEARLY:
                    nextPayment = startDate.AddMonths( 6 );
                    break;
                case SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_YEARLY:
                    nextPayment = startDate.AddYears( 1 );
                    break;
            }

            // If a date was calculated and it is not in the past, return that value
            if ( nextPayment.HasValue && nextPayment.Value >= RockDateTime.Now.Date )
            {
                return nextPayment;
            }

            return null;
        }
    }
}
