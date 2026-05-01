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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// Summary data for a transaction that shares the same gateway, transaction code,
    /// and authorized person as the current transaction, displayed in the Related
    /// Transactions grid on the view panel.
    /// </summary>
    public class RelatedTransactionBag : ITranslateIdKey
    {
        /// <summary>
        /// Gets or sets the integer Id of the related transaction.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the obfuscated IdKey of the related transaction, used to build detail page links.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the date and time the related transaction was processed.
        /// </summary>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the gateway transaction code shared between the related transactions.
        /// </summary>
        public string TransactionReference { get; set; }

        /// <summary>
        /// Gets or sets the total amount of the related transaction.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
