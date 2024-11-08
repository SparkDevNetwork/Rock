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

namespace Rock.ViewModels.Blocks.Fundraising.FundraisingDonationList
{
    /// <summary>
    /// The additional configuration options for the Financial Transaction Detail List block.
    /// </summary>
    public class FundraisingDonationListOptionsBag
    {
        /// <summary>
        /// Gets or sets the columns to hide.
        /// </summary>
        /// <value>
        /// The columns to hide.
        /// </value>
        public List<string> ColumnsToHide { get; set; }

        /// <summary>
        /// Gets or sets the actions to hide.
        /// </summary>
        /// <value>
        /// The actions to hide.
        /// </value>
        public List<string> ActionsToHide { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the context entity is a group member.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the context entity is a group member; otherwise, <c>false</c>.
        /// </value>
        public bool IsContextEntityGroupMember { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the is block visible.
        /// If no group is found as a context entity, then the block is hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the is block visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets the currency information.
        /// </summary>
        /// <value>
        /// The currency information.
        /// </value>
        public CurrencyInfoBag CurrencyInfo { get; set; }
    }
}
