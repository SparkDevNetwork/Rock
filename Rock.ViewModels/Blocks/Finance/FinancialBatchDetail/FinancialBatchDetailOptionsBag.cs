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

namespace Rock.ViewModels.Blocks.Finance.FinancialBatchDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialBatchDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the currency types.
        /// </summary>
        /// <value>
        /// The currency types.
        /// </value>
        public List<FinancialBatchCurrencyTotalsBag> CurrencyTypes { get; set; }

        /// <summary>
        /// Gets or sets the accounts.
        /// </summary>
        /// <value>
        /// The accounts.
        /// </value>
        public List<FinancialBatchAccountTotalsBag> Accounts { get; set; }

        /// <summary>
        /// Gets or sets the transaction item count.
        /// </summary>
        /// <value>
        /// The transaction item count.
        /// </value>
        public int TransactionItemCount { get; set; }

        /// <summary>
        /// Gets or sets the transaction amount.
        /// </summary>
        /// <value>
        /// The transaction amount.
        /// </value>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// The flag which the frontend should use to determine if the status can be edited
        /// </summary>
        public bool IsStatusChangeDisabled { get; set; }

        /// <summary>
        /// Motive: a user who does not have the ReopenBatch permission should not be able to open a batch
        /// once closed even if they themselves were the ones who closed it.
        /// The front end is required to check this flag and not show the edit block button if the flag is set to true and the batch is closed.
        /// </summary>
        public bool IsReopenAuthorized { get; set; }

        /// <summary>
        /// The message to be displayed if the batch is not editable.
        /// </summary>
        public string EditModeMessage { get; } = "Batch is closed and requires authorization to re-open before editing.";

        /// <summary>
        /// The message to be shown on the frontend if the batch is automated.
        /// </summary>
        public string AutomatedToolTip { get; } = "This is an automated batch. The system will automatically set this batch to OPEN when all transactions have been downloaded.";

        /// <summary>
        /// The Defined Type Guid for the Batch Name Defined Type Picker if the Defined Type is specified.
        /// </summary>
        public string BatchNameDefinedTypeGuid { get; set; }
    }
}
