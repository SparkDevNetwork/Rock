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

using Rock.Data;
using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// Interface to implement if your gateway requires redirection.
    /// </summary>
    public interface IRedirectionGatewayComponent : IPaymentTokenGateway
    {
        /// <summary>
        /// Gets the merchant field label.
        /// </summary>
        /// <value>
        /// The merchant field label.
        /// </value>
        string MerchantFieldLabel { get; }

        /// <summary>
        /// Gets the fund field label.
        /// </summary>
        /// <value>
        /// The fund field label.
        /// </value>
        string FundFieldLabel { get; }

        /// <summary>
        /// Gets the merchants.
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, string>> GetMerchants();

        /// <summary>
        /// Gets the merchant funds.
        /// </summary>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, string>> GetMerchantFunds( string merchantId );

        /// <summary>
        /// Gets the redirect URL.
        /// </summary>
        /// <param name="fundId">The fund identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="returnUrl">The URL the individual should be returned to after payment has completed.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>The URL the individual will be sent to in order to complete payment.</returns>
        string GetPaymentRedirectUrl( int? fundId, decimal amount, string returnUrl, Dictionary<string, string> metadata );
    }
}
