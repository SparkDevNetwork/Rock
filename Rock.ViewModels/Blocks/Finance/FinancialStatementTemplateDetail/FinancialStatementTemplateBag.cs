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

using Rock.ViewModels.Utility;
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Finance.FinancialStatementTemplateDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialStatementTemplateBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active financial statement template. This value is required.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the logo binary file.
        /// </summary>
        public ListItemBag LogoBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the Financial Statement Template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the report template.
        /// </summary>
        public string ReportTemplate { get; set; }

        /// <summary>
        /// Gets or sets the currency types for cash gift guids.
        /// </summary>
        /// <value>
        /// The currency types for cash gift guids.
        /// </value>
        public List<Guid> CurrencyTypesForCashGifts { get; set; }
        /// <summary>
        /// Gets or sets the currency types for non cash guids.
        /// </summary>
        /// <value>
        /// The currency types for non cash guids.
        /// </value>
        public List<Guid> CurrencyTypesForNonCashGifts { get; set; }
        /// <summary>
        /// Gets or sets the transaction type guids.
        /// </summary>
        /// <value>
        /// The transaction type guids.
        /// </value>
        public List<Guid> TransactionTypes { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [hide refunded transactions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide refunded transactions]; otherwise, <c>false</c>.
        /// </value>
        public bool HideRefundedTransactions { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [hide corrected transaction on same data].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide corrected transaction on same data]; otherwise, <c>false</c>.
        /// </value>
        public bool HideCorrectedTransactionOnSameData { get; set; }
        /// <summary>
        /// Gets or sets the account selection option.
        /// </summary>
        /// <value>
        /// The account selection option.
        /// </value>
        public string AccountSelectionOption { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [include child accounts custom].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include child accounts custom]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeChildAccountsCustom { get; set; }
        /// <summary>
        /// Gets or sets the selected accounts.
        /// </summary>
        /// <value>
        /// The selected accounts.
        /// </value>
        public List<ListItemBag> SelectedAccounts { get; set; }

        /// <summary>
        /// Gets or sets the margin top for pdf design.
        /// </summary>
        /// <value>
        /// The margin top millimeters.
        /// </value>
        public int? MarginTopMillimeters { get; set; }

        /// <summary>
        /// Gets or sets the margin bottom for pdf design.
        /// </summary>
        /// <value>
        /// The margin bottom millimeters.
        /// </value>
        public int? MarginBottomMillimeters { get; set; }

        /// <summary>
        /// Gets or sets the margin right for pdf design.
        /// </summary>
        /// <value>
        /// The margin right millimeters.
        /// </value>
        public int? MarginRightMillimeters { get; set; }

        /// <summary>
        /// Gets or sets the margin left for pdf design.
        /// </summary>
        /// <value>
        /// The margin left millimeters.
        /// </value>
        public int? MarginLeftMillimeters { get; set; }

        /// <summary>
        /// Gets or sets the size of the paper when generating pdfs.
        /// </summary>
        /// <value>
        /// The size of the paper.
        /// </value>
        public string PaperSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include child accounts pledges].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include child accounts pledges]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeChildAccountsPledges{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include non cash gifts pledge].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include non cash gifts pledge]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeNonCashGiftsPledge { get; set; }

        /// <summary>
        /// Gets or sets the pledge accounts.
        /// </summary>
        /// <value>
        /// The pledge accounts.
        /// </value>
        public List<ListItemBag> PledgeAccounts { get; set; }

        /// <summary>
        /// Gets or sets the footer template HTML fragment.
        /// </summary>
        /// <value>
        /// The footer template HTML fragment.
        /// </value>
        public string FooterTemplateHtmlFragment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use custom account ids].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use custom account ids]; otherwise, <c>false</c>.
        /// </value>
        public bool UseCustomAccountIds { get; set; }

        /// <summary>
        /// Gets or sets the transaction settings for view mode.
        /// </summary>
        /// <value>
        /// The transaction settings.
        /// </value>
        public string AccountsForTransactions { get; set; }

        /// <summary>
        /// Gets or sets the selected transaction types.
        /// </summary>
        /// <value>
        /// The selected transaction types.
        /// </value>
        public string SelectedTransactionTypes { get; set; }
    }
}
