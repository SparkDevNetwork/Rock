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

using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// A Financial Gateway that has an Obsidian control
    /// </summary>
    public interface IObsidianFinancialGateway
    {
        /// <summary>
        /// Gets the obsidian control file URL.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        string GetObsidianControlFileUrl( FinancialGateway financialGateway );

        /// <summary>
        /// Gets the obsidian control settings.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        object GetObsidianControlSettings( FinancialGateway financialGateway );

        /// <summary>
        /// Creates the customer account using a token received and returns a customer account token that can be used for future transactions.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        string CreateCustomerAccount( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage );
    }
}
