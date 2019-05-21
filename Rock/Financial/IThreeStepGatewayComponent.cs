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
using System.Collections.Generic;

using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// Describes a financial provider component that is setup to handle a three step payment process (typically involving an iFrame)
    /// </summary>
    public interface IThreeStepGatewayComponent
    {
        /// <summary>
        /// Gets the step2 form URL.
        /// </summary>
        /// <value>
        /// The step2 form URL.
        /// </value>
        string Step2FormUrl { get; }

        /// <summary>
        /// Gets the financial transaction parameters that are passed to step 1
        /// </summary>
        /// <param name="redirectUrl">The redirect URL.</param>
        /// <returns></returns>
        Dictionary<string, string> GetStep1Parameters( string redirectUrl );

        /// <summary>
        /// Performs the first step of a three-step charge
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        string ChargeStep1( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Performs the final step of a three-step charge.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="resultQueryString">The result query string from step 2.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        FinancialTransaction ChargeStep3( FinancialGateway financialGateway, string resultQueryString, out string errorMessage );

        /// <summary>
        /// Performs the first step of adding a new payment schedule
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        string AddScheduledPaymentStep1( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Performs the third step of adding a new payment schedule
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="resultQueryString">The result query string from step 2.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        FinancialScheduledTransaction AddScheduledPaymentStep3( FinancialGateway financialGateway, string resultQueryString, out string errorMessage );
    }
}
