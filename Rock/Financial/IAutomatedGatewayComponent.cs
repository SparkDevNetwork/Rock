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

using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// Describes a gateway component that is setup to handle payments from a REST endpoint or other automated means. These payments can only be made with saved accounts.
    /// </summary>
    public interface IAutomatedGatewayComponent
    {
        /// <summary>
        /// The most recent exception thrown by the gateway's remote API
        /// </summary>
        Exception MostRecentException { get; }

        /// <summary>
        /// Handle a payment from a REST endpoint or other automated means. This payment can only be made with a saved account.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="metadata">Optional. Additional key value pairs to send to the gateway</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        Payment AutomatedCharge( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage, Dictionary<string, string> metadata = null );
    }
}
