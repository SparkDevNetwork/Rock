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
using System.Text.Json.Serialization;

using Rock.Utility;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Describes a link between two financial transactions where one is a refund
    /// of the other.
    /// </summary>
    internal class FinancialTransactionRefundLinkResult
    {
        /// <summary>
        /// The original transaction identifier. This is not serialized.
        /// </summary>
        [JsonIgnore]
        public int? OriginalTransactionId { get; set; }

        /// <summary>
        /// The refund transaction identifier. This is not serialized.
        /// </summary>
        [JsonIgnore]
        public int? RefundTransactionId { get; set; }

        /// <summary>
        /// The encoded identifier key for the original transaction.
        /// </summary>
        public string OriginalTransactionIdKey => OriginalTransactionId.HasValue ? IdHasher.Instance.GetHash( OriginalTransactionId.Value ) : null;

        /// <summary>
        /// The encoded identifier key for the refund transaction.
        /// </summary>
        public string RefundTransactionIdKey => RefundTransactionId.HasValue ? IdHasher.Instance.GetHash( RefundTransactionId.Value ) : null;

        /// <summary>
        /// The total amount of the refund.
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// The financial account associated with this transaction.
        /// </summary>
        public List<FinancialAccountTransactionResult> Accounts { get; set; }
    }
}
