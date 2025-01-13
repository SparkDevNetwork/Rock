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

using System.ComponentModel.DataAnnotations.Schema;

using Rock.Lava;
using Rock.Security;

namespace Rock.Model
{
    public partial class RegistrationRegistrantFee
    {
        #region Navigation Properties

        /// <summary>
        /// Gets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual decimal TotalCost
        {
            get
            {
                return Quantity * Cost;
            }
        }

        #endregion Navigation Properties
        
        #region Methods

        /// <summary>
        /// Discounts the cost.
        /// </summary>
        /// <param name="discountPercent">The discount percent.</param>
        /// <returns></returns>
        public decimal DiscountedCost ( decimal discountPercent )
        {
            var discountedCost = TotalCost;
            
            if ( RegistrationTemplateFee != null && RegistrationTemplateFee.DiscountApplies && ( RegistrationRegistrant == null || RegistrationRegistrant.DiscountApplies ) )
            {
                discountedCost -= discountedCost * discountPercent;
            }

            return discountedCost;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return RegistrationTemplateFee != null ? RegistrationTemplateFee.Name : "Fee";
        }

        #endregion Methods

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => RegistrationRegistrant ?? base.ParentAuthority;

        #endregion
    }
}
