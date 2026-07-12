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

namespace Rock.ViewModels.Blocks.Event.RegistrantDetail
{
    /// <summary>
    /// Represents a single fee selection for a registrant.
    /// </summary>
    public class RegistrantFeeBag
    {
        /// <summary>
        /// Gets or sets the registration template fee identifier.
        /// </summary>
        public int RegistrationTemplateFeeId { get; set; }

        /// <summary>
        /// Gets or sets the registration template fee item identifier.
        /// Null for single-option fees.
        /// </summary>
        public int? RegistrationTemplateFeeItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity selected.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the cost per unit for this fee selection.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the display label for the fee option (e.g. the item name for multi-option fees).
        /// </summary>
        public string Option { get; set; }
    }
}