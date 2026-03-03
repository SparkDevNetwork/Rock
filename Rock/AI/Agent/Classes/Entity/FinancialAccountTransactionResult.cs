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

using System.Text.Json.Serialization;

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Represents an account and amount detail for a financial transaction.
    /// </summary>
    internal class FinancialAccountTransactionResult : FinancialAccountResult
    {
        /// <summary>
        /// The amount of the transaction for this account.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The entity type identifier. This is not sent to the language model,
        /// but can be used internally to populate the <see cref="RelatedEntity"/>
        /// property.
        /// </summary>
        [JsonIgnore]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// The entity identifier. This is not sent to the language model,
        /// but can be used internally to populate the <see cref="RelatedEntity"/>
        /// property.
        /// </summary>
        [JsonIgnore]
        public int? EntityId { get; set; }

        /// <summary>
        /// Information about the related entity for this transaction.
        /// </summary>
        public KeyNameResult RelatedEntity { get; set; }
    }
}
