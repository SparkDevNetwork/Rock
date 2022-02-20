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

namespace Rock.Model
{
    /// <summary>
    /// A POCO that represents a single month of giving history for a single financial account.
    /// </summary>
    public sealed class MonthlyAccountGivingHistory
    {
        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the month. 1 = January (not zero based)
        /// </summary>
        /// <value>
        /// The month.
        /// </value>
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        public int AccountId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// Note that this amount factors in refunds, so no additional adjustment is needed.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }
    }
}