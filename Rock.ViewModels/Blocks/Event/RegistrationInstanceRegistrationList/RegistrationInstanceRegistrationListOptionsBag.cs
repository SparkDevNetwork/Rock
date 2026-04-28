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
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceRegistrationList
{
    /// <summary>
    /// The additional configuration options for the Registration Instance Registration List block.
    /// </summary>
    public class RegistrationInstanceRegistrationListOptionsBag
    {
        /// <summary>
        /// Gets or sets the title for the exported excel or csv file.
        /// </summary>
        /// <value>
        /// The export title.
        /// </value>
        public string ExportTitle { get; set; }

        /// <summary>
        /// Gets or sets the registration template unique identifier. The value
        /// is used on the client to scope grid filter preferences to each
        /// template so that switching templates does not leak filter state.
        /// </summary>
        /// <value>
        /// The registration template unique identifier.
        /// </value>
        public Guid? RegistrationTemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets the campus items shown in the grid settings campus filter.
        /// </summary>
        /// <value>
        /// The campus items.
        /// </value>
        public List<ListItemBag> CampusItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Discount Code" column
        /// should be shown on the grid. Mirrors the block's
        /// <c>DisplayDiscountCodes</c> attribute.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the discount code column should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayDiscountCodes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the registration instance
        /// (or its template) has a non-zero cost. Drives visibility of the
        /// "Total Cost" and "Balance Due" columns.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the instance has a cost; otherwise, <c>false</c>.
        /// </value>
        public bool InstanceHasCost { get; set; }

        /// <summary>
        /// Gets or sets the currency information used to format costs and
        /// balances in the grid.
        /// </summary>
        /// <value>
        /// The currency information.
        /// </value>
        public CurrencyInfoBag CurrencyInfo { get; set; }
    }
}
