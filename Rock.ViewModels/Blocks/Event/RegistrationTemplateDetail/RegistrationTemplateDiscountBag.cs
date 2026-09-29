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

namespace Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail
{
    /// <summary>
    /// A discount code that can be applied to a registration.
    /// </summary>
    public class RegistrationTemplateDiscountBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the discount.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the display order of the discount.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the discount code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage as a fraction, for example 0.10 for ten percent.
        /// Zero when the discount is an amount.
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount. Zero when the discount is a percentage.
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of registrations the discount code can be used on.
        /// </summary>
        public int? MaxUsage { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of registrants in a single registration the discount code can be used on.
        /// </summary>
        public int? MaxRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of registrants required to use the discount code.
        /// </summary>
        public int? MinRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the first date the discount code can be used.
        /// </summary>
        public DateTimeOffset? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the last date the discount code can be used.
        /// </summary>
        public DateTimeOffset? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the discount is applied automatically
        /// when the registration meets its criteria.
        /// </summary>
        public bool AutoApplyDiscount { get; set; }
    }
}
