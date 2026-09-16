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

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// Contains the adjacent transaction identifiers used to build the Back/Next
    /// navigation buttons when browsing transactions within a batch.
    /// </summary>
    public class BatchNavigationBag
    {
        /// <summary>
        /// Gets or sets the obfuscated IdKey of the previous transaction in the batch,
        /// or <c>null</c> when the current transaction is the first in the batch.
        /// </summary>
        public string PreviousTransactionIdKey { get; set; }

        /// <summary>
        /// Gets or sets the obfuscated IdKey of the next transaction in the batch,
        /// or <c>null</c> when the current transaction is the last in the batch.
        /// </summary>
        public string NextTransactionIdKey { get; set; }
    }
}
