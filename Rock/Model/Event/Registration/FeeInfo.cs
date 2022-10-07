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

using System;

namespace Rock.Model
{
    /// <summary>
    /// Registrant  Fee Helper Class
    /// </summary>
    [Serializable]
    public class FeeInfo
    {
        /// <summary>
        /// Gets or sets the fee label.
        /// </summary>
        /// <value>
        /// The fee label.
        /// </value>
        public string FeeLabel { get; set; }

        /// <summary>
        /// Gets or sets the registration template fee item identifier.
        /// </summary>
        /// <value>
        /// The registration template fee item identifier.
        /// </value>
        public int? RegistrationTemplateFeeItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [discount applies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [discount applies]; otherwise, <c>false</c>.
        /// </value>
        public bool DiscountApplies { get; set; }

        /// <summary>
        /// Gets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        public decimal TotalCost
        {
            get { return Cost * Quantity; }
        }

        /// <summary>
        /// Gets or sets the previous cost.
        /// </summary>
        /// <value>
        /// The previous cost.
        /// </value>
        public decimal PreviousCost { get; set; }

        /// <summary>
        /// Discounts the cost.
        /// </summary>
        /// <param name="discountPercent">The discount percent.</param>
        /// <returns></returns>
        public decimal DiscountedCost( decimal discountPercent )
        {
            var discountedCost = TotalCost;

            if ( DiscountApplies )
            {
                discountedCost = discountedCost - ( discountedCost * discountPercent );
            }

            return discountedCost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo"/> class.
        /// </summary>
        public FeeInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo" /> class.
        /// </summary>
        /// <param name="feeItem">The fee item.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="cost">The cost.</param>
        public FeeInfo( RegistrationTemplateFeeItem feeItem, int quantity, decimal cost )
            : this()
        {
            FeeLabel = feeItem.Name;
            RegistrationTemplateFeeItemId = feeItem.Id;
            Quantity = quantity;
            Cost = cost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo"/> class.
        /// </summary>
        /// <param name="fee">The fee.</param>
        public FeeInfo( RegistrationRegistrantFee fee )
            : this()
        {
            FeeLabel = fee.RegistrationTemplateFeeItem?.Name;
            RegistrationTemplateFeeItemId = fee.RegistrationTemplateFeeItemId;
            Quantity = fee.Quantity;
            Cost = fee.Cost;
            PreviousCost = fee.Cost;
            DiscountApplies = fee.RegistrationTemplateFee != null && fee.RegistrationTemplateFee.DiscountApplies;
        }
    }
}
