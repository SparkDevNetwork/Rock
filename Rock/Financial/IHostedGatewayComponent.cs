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
using System.Web.UI;
using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// A Financial gateway provider that supports collecting Payment Info (Credit Card Number fields or ACH fields) in the browser.
    /// An IHostedGatewayComponent will return a token in the browser client instead of sending payment info to the Rock Server.
    /// </summary>
    public interface IHostedGatewayComponent: IGatewayComponent
    {
        /// <summary>
        /// Gets the hosted payment information control which will be used to collect CreditCard, ACH fields
        /// Note: A HostedPaymentInfoControl can optionally implement <seealso cref="IHostedGatewayPaymentControlTokenEvent" />
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="controlId">The control identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Control GetHostedPaymentInfoControl( FinancialGateway financialGateway, string controlId, HostedPaymentInfoControlOptions options );

        /// <summary>
        /// Gets the JavaScript needed to tell the hostedPaymentInfoControl to get send the paymentInfo and get a token.
        /// Have your 'Next' or 'Submit' call this so that the hostedPaymentInfoControl will fetch the token/response
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <returns></returns>
        string GetHostPaymentInfoSubmitScript( FinancialGateway financialGateway, Control hostedPaymentInfoControl );

        /// <summary>
        /// Gets the paymentInfoToken that the hostedPaymentInfoControl returned (see also <seealso cref="M:Rock.Financial.IHostedGatewayComponent.GetHostedPaymentInfoControl(Rock.Model.FinancialGateway,System.String)" />)
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        void UpdatePaymentInfoFromPaymentControl( FinancialGateway financialGateway, Control hostedPaymentInfoControl, ReferencePaymentInfo referencePaymentInfo, out string errorMessage );

        /// <summary>
        /// Gets the URL that the Gateway Information UI will navigate to when they click the 'Configure' link
        /// </summary>
        /// <value>
        /// The configure URL.
        /// </value>
        string ConfigureURL { get; }

        /// <summary>
        /// Gets the URL that the Gateway Information UI will navigate to when they click the 'Learn More' link
        /// </summary>
        /// <value>
        /// The learn more URL.
        /// </value>
        string LearnMoreURL { get; }

        /// <summary>
        /// Creates the customer account using a token received from the HostedPaymentInfoControl <seealso cref="GetHostedPaymentInfoControl" />
        /// and returns a customer account token that can be used for future transactions.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        string CreateCustomerAccount( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Gets the earliest scheduled start date that the gateway will accept for the start date, based on the current local time.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        DateTime GetEarliestScheduledStartDate( FinancialGateway financialGateway );
        
    }
}