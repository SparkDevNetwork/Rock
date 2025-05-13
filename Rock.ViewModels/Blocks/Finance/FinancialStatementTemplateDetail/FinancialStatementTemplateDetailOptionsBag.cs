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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Finance.FinancialStatementTemplateDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialStatementTemplateDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the currency types for cash gifts options.
        /// </summary>
        /// <value>
        /// The currency types for cash gifts options.
        /// </value>
        public List<ListItemBag> CurrencyTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the transaction types options.
        /// </summary>
        /// <value>
        /// The transaction types options.
        /// </value>
        public List<ListItemBag> TransactionTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the page sizes.
        /// </summary>
        /// <value>
        /// The page sizes.
        /// </value>
        public List<ListItemBag> PaperSizeOptions { get; set; }
    }
}
