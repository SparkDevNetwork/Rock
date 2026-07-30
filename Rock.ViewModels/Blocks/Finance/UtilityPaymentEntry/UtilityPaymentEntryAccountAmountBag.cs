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

namespace Rock.ViewModels.Blocks.Finance.UtilityPaymentEntry
{
    /// <summary>
    /// A single account and the amount the giver chose to contribute to it.
    /// </summary>
    public class UtilityPaymentEntryAccountAmountBag
    {
        /// <summary>
        /// Gets or sets the Guid of the account the amount is contributed to.
        /// </summary>
        public Guid? AccountGuid { get; set; }

        /// <summary>
        /// Gets or sets the amount contributed to the account.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
