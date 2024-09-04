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
using System.ComponentModel.DataAnnotations;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// FinancialPledge Logic
    /// </summary>
    public partial class FinancialPledge : Model<FinancialPledge>
    {
        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this pledge.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this pledge.
        /// </returns>
        public override string ToString()
        {
            return this.TotalAmount.ToStringSafe();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result && TotalAmount < 0 )
                {
                    this.ValidationResults.Add( new ValidationResult( "Total Amount can't be negative." ) );
                    return false;
                }

                if ( result && StartDate.Date > EndDate.Date )
                {
                    this.ValidationResults.Add( new ValidationResult( "Start date cannot be later than End date." ) );
                    return false;
                }

                return result;
            }
        }

        #endregion Public Methods
    }
}