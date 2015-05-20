// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data;
using Rock.Attribute;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Base class for financial provider components
    /// </summary>
    public abstract class GatewayComponent : Component
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
        public GatewayComponent()
        {
            // Override default constructor of Component that loads attributes (not needed for gateway components, needs to be done by each financial gateway)
        }

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <exception cref="System.Exception">Gateway Component attributes are saved specific to the financial gateway, which requires that the current financial gateway is included in order to load or retrieve values. Use the LoadAttributes( FinancialGateway financialGateway ) method instead.</exception>
        [Obsolete( "Use LoadAttributes( FinancialGateway financialGateway ) instead", true )]
        public void LoadAttributes()
        {
            // Compiler should generate error if referencing this method, so exception should never be thrown
            // but method is needed to "override" the extension method for IHasAttributes objects
            throw new Exception( "Gateway Component attributes are saved specific to the financial gateway, which requires that the current financial gateway is included in order to load or retrieve values. Use the LoadAttributes( FinancialGateway financialGateway ) method instead." );
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
                return true; ;
            }
        }

        /// <summary>
        /// Gets the attribute value for the gateway 
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetAttributeValue( FinancialGateway financialGateway, string key )
        {
            var values = financialGateway.AttributeValues;
            if ( values.ContainsKey( key ) )
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
        /// Returnes a boolean value indicating if 'Saved Account' functionality is supported for the given currency type. 
        /// </summary>
        /// <param name="currencyType">Type of the currency.</param>
        /// <returns></returns>
        public virtual bool SupportsSavedAccount( DefinedValueCache currencyType )
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
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Cancels the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage );

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage );

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
    }
}
