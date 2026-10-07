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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// A single row of the cost summary of a registration.
    /// </summary>
    public class RegistrationFeeRowBag
    {
        /// <summary>
        /// Gets or sets the type of the row (cost, fee, discount or total).
        /// </summary>
        public string RowType { get; set; }

        /// <summary>
        /// Gets or sets the description of the row.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the formatted discounted amount.
        /// </summary>
        public string DiscountedCostText { get; set; }

        /// <summary>
        /// Gets or sets the formatted amount.
        /// </summary>
        public string CostText { get; set; }
    }
}
