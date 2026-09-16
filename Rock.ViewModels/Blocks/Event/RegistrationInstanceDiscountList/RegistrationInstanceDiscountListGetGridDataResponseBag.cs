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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceDiscountList
{
    /// <summary>
    /// The response bag for the GetGridData block action, containing both
    /// grid data and summary totals that reflect the currently filtered dataset.
    /// </summary>
    public class RegistrationInstanceDiscountListGetGridDataResponseBag
    {
        /// <summary>
        /// Gets or sets the grid data.
        /// </summary>
        public GridDataBag GridData { get; set; }

        /// <summary>
        /// Gets or sets the sum of total cost across all filtered rows.
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the sum of discount qualified cost across all filtered rows.
        /// </summary>
        public decimal DiscountQualifiedCost { get; set; }

        /// <summary>
        /// Gets or sets the sum of total discount across all filtered rows.
        /// </summary>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Gets or sets the sum of registration cost across all filtered rows.
        /// </summary>
        public decimal RegistrationCost { get; set; }

        /// <summary>
        /// Gets or sets the count of filtered registrations.
        /// </summary>
        public int TotalRegistrations { get; set; }

        /// <summary>
        /// Gets or sets the sum of registrant count across all filtered rows.
        /// </summary>
        public int TotalRegistrants { get; set; }
    }
}
