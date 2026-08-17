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

namespace Rock.ViewModels.Blocks.Finance.ScheduledTransactionEditV2
{
    /// <summary>
    /// A single account/amount allocation for a scheduled transaction.
    /// </summary>
    public class ScheduledTransactionAccountAmountBag
    {
        /// <summary>
        /// Gets or sets the identifier of the financial account.
        /// </summary>
        public string AccountGuid { get; set; }

        /// <summary>
        /// Gets or sets the public name of the financial account.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the amount allocated to the account.
        /// </summary>
        public decimal? Amount { get; set; }
    }
}
