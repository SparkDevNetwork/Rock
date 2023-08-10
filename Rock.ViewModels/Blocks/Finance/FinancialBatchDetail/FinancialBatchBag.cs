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

using Rock.ViewModels.Utility;
using Rock.Model;

namespace Rock.ViewModels.Blocks.Finance.FinancialBatchDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class FinancialBatchBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets an optional transaction code from an accounting system that batch is associated with
        /// </summary>
        public string AccountingSystemCode { get; set; }

        /// <summary>
        /// Gets or sets end of the posting date and time range for FinancialTransactions that are included in this batch.
        /// Transactions that post before or on this date and time and after the Rock.Model.FinancialBatch.BatchStartDateTime can be included in this batch.
        /// </summary>
        public DateTime? BatchEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the start posting date and time range of FinancialTransactions that are included in this batch.  
        /// Transactions that post on or after this date and time and before the Rock.Model.FinancialBatch.BatchEndDateTime can be included in this batch.
        /// </summary>
        public DateTime? BatchStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Campus that this batch is associated with.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the control amount. This should match the total value of all
        /// FinancialTransactions that are included in the batch.
        /// Use Rock.Model.FinancialBatchService.IncrementControlAmount(System.Int32,System.Decimal,Rock.Model.History.HistoryChangeList) if you are incrementing the control amount
        /// based on a transaction amount.
        /// </summary>
        public decimal ControlAmount { get; set; }

        /// <summary>
        /// Gets or sets the control item count.
        /// </summary>
        public int? ControlItemCount { get; set; }

        /// <summary>
        /// Gets or sets the name of the batch.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// The flag which is set to true if the user is forbidden from reopening the batch
        /// </summary>
        public bool IsReopenDisabled { get; set; }

        /// <summary>
        /// Gets or sets the status of the batch.
        /// </summary>
        public BatchStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the value of the flag which shows if the batch is automated or not.
        /// </summary>
        public bool IsAutomated { get; set; }

        /// <summary>
        /// The Batch ID
        /// Motive: Although the IdKey is passed to the user, there is no way to get the id from it in the frontend
        /// Id is required to be displayed in the frontend and so is included.
        /// </summary>
        public int Id { get; set; }
    }
}
