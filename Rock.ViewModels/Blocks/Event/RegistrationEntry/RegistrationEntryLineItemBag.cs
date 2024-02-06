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

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// RegistrationEntryBlockLineItemViewModel
    /// </summary>
    public sealed class RegistrationEntryLineItemBag
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is fee.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is fee; otherwise, <c>false</c>.
        /// </value>
        public bool IsFee { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the discounted amount.
        /// </summary>
        /// <value>
        /// The discounted amount.
        /// </value>
        public decimal DiscountedAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount help.
        /// </summary>
        /// <value>
        /// The discount help.
        /// </value>
        public string DiscountHelp { get; set; }
    }
}
