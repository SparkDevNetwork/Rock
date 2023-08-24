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
    /// Marks a gateway as supporting automatic settlement. This happens by
    /// storing transactions in a dedicated batch and then once the gateway
    /// knows the final settlement information it moves those transactions
    /// into a new batch. This way all the deposits amounts match perfectly
    /// and can be easily reconciled by the fincance team.
    /// </summary>
    public interface ISettlementGateway
    {
        /// <summary>
        /// Gets the batch identifier to use as the temporary location
        /// for the transaction while waiting for settlement.
        /// </summary>
        /// <remarks>
        /// Usually this would always return the same batch identifier for any
        /// transaction. But gateways could internally use multiple batches
        /// if they needed to.
        /// </remarks>
        /// <param name="financialGateway">The financial gateway instance that will be handling the transaction.</param>
        /// <param name="financialTransaction">The new transaction that will be placed into the batch.</param>
        /// <returns>The batch identifier or <c>null</c> if one is not available.</returns>
        int? GetSettlementBatchId( FinancialGateway financialGateway, FinancialTransaction financialTransaction );
    }
}
