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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Finance.ReportSetting
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportSetting"/> class.
    /// </summary>
    [Serializable]
    public class ReportSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportSetting"/> class.
        /// </summary>
        public ReportSetting()
        {
            this.TransactionSetting = new TransactionSetting();
            this.PledgeSetting = new PledgeSetting();
            this.PDFObjectSettings = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the transaction settings.
        /// </summary>
        /// <value>
        /// The transaction settings.
        /// </value>
        public TransactionSetting TransactionSetting { get; set; }

        /// <summary>
        /// Gets or sets the pledge settings.
        /// </summary>
        /// <value>
        /// The pledge settings.
        /// </value>
        public PledgeSetting PledgeSetting { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of Key,Value for PDF Object Settings.
        /// </summary>
        /// <value>
        /// The Dictionary of Key,Value for PDF Object Settings.
        /// </value>
        public Dictionary<string,string> PDFObjectSettings { get; set; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionSetting"/> class.
    /// </summary>
    public class TransactionSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionSetting"/> class.
        /// </summary>
        public TransactionSetting()
        {
            AccountIds = new List<int>();
            CurrencyTypesForCashGiftIds = new List<int>();
            CurrencyTypesForNonCashIds = new List<int>();
            TransactionTypeIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets the account identifiers.
        /// </summary>
        /// <value>
        /// The account identifiers.
        /// </value>
        public List<int> AccountIds { get; set; }

        /// <summary>
        /// Gets or sets the currency types for cash gifts.
        /// </summary>
        /// <value>
        /// The currency types for cash gifts.
        /// </value>
        public List<int> CurrencyTypesForCashGiftIds { get; set; }

        /// <summary>
        /// Gets or sets the currency types for non-cash gifts.
        /// </summary>
        /// <value>
        /// The currency types for non-cash gifts.
        /// </value>
        public List<int> CurrencyTypesForNonCashIds { get; set; }

        /// <summary>
        /// Gets or sets the transaction types.
        /// </summary>
        /// <value>
        /// The transaction types.
        /// </value>
        public List<int> TransactionTypeIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether refunded transaction should be hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if refunded transaction should be hidden; otherwise, <c>false</c>.
        /// </value>
        public bool HideRefundedTransaction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether corrected transaction on same date should be hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if corrected transaction on same date should be hidden; otherwise, <c>false</c>.
        /// </value>
        public bool HideCorrectedTransactionOnSameData { get; set; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PledgeSetting"/> class.
    /// </summary>
    public class PledgeSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PledgeSetting"/> class.
        /// </summary>
        public PledgeSetting()
        {
            AccountIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets the account identifiers.
        /// </summary>
        /// <value>
        /// The account identifiers.
        /// </value>
        public List<int> AccountIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gifts to child accounts should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include gifts to child accounts]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeGiftsToChildAccounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether non-cash gifts should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include non-cash gifts]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeNonCashGifts { get; set; }
    }
}
