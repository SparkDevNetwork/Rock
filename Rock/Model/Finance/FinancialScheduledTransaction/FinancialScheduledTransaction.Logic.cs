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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// FinancialScheduledTransaction Logic
    /// </summary>
    public partial class FinancialScheduledTransaction : Model<FinancialScheduledTransaction>, IHasActiveFlag
    {
        #region Virtual Properties

        /// <summary>
        /// Gets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        [LavaVisible]
        public decimal TotalAmount 
        {
            get { return ScheduledTransactionDetails.Sum( d => d.Amount ); }
        }

        /// <inheritdoc cref="FinancialScheduledTransactionExtensionMethods.GetPaymentPlan(FinancialScheduledTransaction)" />
        [NotMapped]
        [RockInternal( "1.16.6" )]
        public virtual PaymentPlan PaymentPlan
        {
            get
            {
                return this.GetPaymentPlan();
            }
        }

        #endregion Virtual Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this transaction.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this transaction.
        /// </returns>
        public override string ToString()
        {
            return this.TotalAmount.ToStringSafe();
        }

        #endregion Public Methods
    }
}