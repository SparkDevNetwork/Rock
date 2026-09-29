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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail
{
    /// <summary>
    /// A fee that can be added to a registration.
    /// </summary>
    public class RegistrationTemplateFeeBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the fee.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the display order of the fee.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the name of the fee.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether the fee has a single option or multiple options.
        /// </summary>
        public RegistrationFeeType FeeType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether registrants can select more than one of this item.
        /// </summary>
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether discounts apply to this fee.
        /// </summary>
        public bool DiscountApplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the fee is offered to new registrations.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the fee is required for new registrations.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether fee items with none remaining are hidden.
        /// </summary>
        public bool HideWhenNoneRemaining { get; set; }

        /// <summary>
        /// Gets or sets the options of the fee, in display order. A single option fee has exactly one item.
        /// </summary>
        public List<RegistrationTemplateFeeItemBag> FeeItems { get; set; }
    }
}
