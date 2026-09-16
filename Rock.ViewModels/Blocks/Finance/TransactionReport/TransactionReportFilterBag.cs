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

namespace Rock.ViewModels.Blocks.Finance.TransactionReport
{
    /// <summary>
    /// The filter selections sent to the server when requesting transaction data.
    /// </summary>
    public class TransactionReportFilterBag
    {
        /// <summary>
        /// Gets or sets the inclusive lower bound of the transaction date range. An empty value
        /// means there is no lower bound.
        /// </summary>
        public string LowerDate { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of the transaction date range. An empty value means there
        /// is no upper bound.
        /// </summary>
        public string UpperDate { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the accounts to filter by. An empty list includes
        /// transactions for all accounts.
        /// </summary>
        public List<string> AccountGuids { get; set; }
    }
}
