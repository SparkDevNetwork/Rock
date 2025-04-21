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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.BenevolenceRequestList
{
    /// <summary>
    /// Additional Configuration for the Benevolence Request List Block.
    /// </summary>
    public class BenevolenceRequestListOptionsBag
    {
        /// <summary>
        /// Gets or sets the currency information.
        /// </summary>
        /// <value>
        /// The currency information.
        /// </value>
        public CurrencyInfoBag CurrencyInfo { get; set; }

        /// <summary>
        /// Gets or sets the case workers for the case worker filter dropdown.
        /// </summary>
        /// <value>
        /// The case workers.
        /// </value>
        public List<ListItemBag> CaseWorkers { get; set; }

        /// <summary>
        /// Gets or sets the benevolence types for the benevolence type filter dropdown.
        /// </summary>
        /// <value>
        /// The benevolence types.
        /// </value>
        public List<ListItemBag> BenevolenceTypes { get; set; }

        /// <summary>
        /// Gets or sets the columns to hide.
        /// </summary>
        /// <value>
        /// The columns to hide.
        /// </value>
        public List<string> ColumnsToHide { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has the administrate permission.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the current user can administrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }
    }
}
