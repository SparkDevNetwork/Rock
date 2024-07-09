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
using Rock.Model;

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Contains the cost summary information for the Registration Entry block.
    /// </summary>
    public class RegistrationEntryCostSummaryBag
    {
        /// <summary>
        /// Gets or sets the cost summary type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public RegistrationCostSummaryType Type { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        public decimal DiscountedCost { get; set; }

        /// <summary>
        /// Gets or sets the minimum payment amount.
        /// </summary>
        /// <value>
        /// The minimum payment.
        /// </value>
        public decimal MinimumPaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the default payment amount.
        /// </summary>
        /// <value>
        /// The default payment.
        /// </value>
        public decimal? DefaultPaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the registrant unique identifier if this cost is associated with a registrant.
        /// </summary>
        public Guid? RegistrationRegistrantGuid { get; set; }
    }
}
