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

namespace Rock.ViewModels.Blocks.Finance.FundraisingDonationEntry
{
    /// <summary>
    /// The configuration options for the Fundraising Donation Entry block.
    /// </summary>
    public class FundraisingDonationEntryOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Transaction Entry Page block
        /// setting has been configured. When false, the block cannot navigate the donor
        /// to the payment page and a configuration warning is shown.
        /// </summary>
        public bool IsTransactionEntryPageConfigured { get; set; }
    }
}
