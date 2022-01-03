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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// The methods that must be implemented for a <see cref="GatewayComponent"/>
    /// to support payment tokens in a URL.
    /// </summary>
    /// <seealso cref="IRedirectionGatewayComponent"/>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public interface IPaymentTokenGateway
    {
        /// <summary>
        /// Tries the get payment token from the page parameters.
        /// </summary>
        /// <param name="financialGateway">The financial gateway associated with the component.</param>
        /// <param name="parameters">The page and query string parameters.</param>
        /// <param name="paymentToken">On return will contain the payment token.</param>
        /// <returns><c>true</c> if a valid payment token was found, <c>false</c> otherwise.</returns>
        bool TryGetPaymentTokenFromParameters( FinancialGateway financialGateway, IDictionary<string, string> parameters, out string paymentToken );

        /// <summary>
        /// Determines whether the <paramref name="paymentToken"/> has already
        /// been charged by the remote gateway.
        /// </summary>
        /// <param name="financialGateway">The financial gateway associated with the component.</param>
        /// <param name="paymentToken">The payment token.</param>
        /// <returns><c>true</c> if the payment token has been charged already; otherwise, <c>false</c>.</returns>
        bool IsPaymentTokenCharged( FinancialGateway financialGateway, string paymentToken );

        /// <summary>
        /// Fetches the transaction from the database if it already exists or
        /// the API otherwise.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="financialGateway">The financial gateway associated with the component.</param>
        /// <param name="fundId">The fund identifier.</param>
        /// <param name="paymentToken">The payment token to be processed.</param>
        /// <returns></returns>
        FinancialTransaction FetchPaymentTokenTransaction( RockContext rockContext, FinancialGateway financialGateway, int? fundId, string paymentToken );

        /// <summary>
        /// Creates the customer account using a token received and returns a
        /// customer account token that can be used for future transactions.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>A string that contains the identifier of the customer on the gateway or <c>null</c> if not supported.</returns>
        string CreateCustomerAccount( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage );
    }
}
