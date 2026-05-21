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

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// Lightweight reference to the scheduled transaction that generated this
    /// financial transaction, used to render the link in the view panel.
    /// </summary>
    public class ScheduledTransactionBag
    {
        /// <summary>
        /// Gets or sets the integer Id of the scheduled transaction.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the text shown for the scheduled transaction link.
        /// Prefers the gateway schedule Id; falls back to the integer Id as a string.
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// Gets or sets the URL to the scheduled transaction detail page.
        /// </summary>
        public string Url { get; set; }
    }
}
