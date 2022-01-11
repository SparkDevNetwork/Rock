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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// FinancialTransaction Logic
    /// </summary>
    public partial class FinancialTransaction
    {
        #region Virtual Properties

        /// <summary>
        /// Gets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        [LavaVisible]
        [BoundFieldTypeAttribute( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public virtual decimal TotalAmount
        {
            get { return TransactionDetails.Sum( d => d.Amount ); }
        }

        /// <summary>
        /// Gets the total fee amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        [LavaVisible]
        [BoundFieldType( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public virtual decimal? TotalFeeAmount
        {
            get
            {
                var hasFeeInfo = false;
                var totalFee = 0m;

                foreach ( var detail in TransactionDetails )
                {
                    hasFeeInfo |= detail.FeeAmount.HasValue;
                    totalFee += detail.FeeAmount ?? 0m;
                }

                return hasFeeInfo ? totalFee : ( decimal? ) null;
            }
        }

        /// <summary>
        /// Gets the total fee coverage amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        [LavaVisible]
        [BoundFieldType( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public virtual decimal? TotalFeeCoverageAmount
        {
            get
            {
                var hasFeeConverageInfo = false;
                var totalFeeCoverage = 0m;

                foreach ( var detail in TransactionDetails )
                {
                    hasFeeConverageInfo |= detail.FeeCoverageAmount.HasValue;
                    totalFeeCoverage += detail.FeeCoverageAmount ?? 0m;
                }

                return hasFeeConverageInfo ? totalFeeCoverage : ( decimal? ) null;
            }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( "Refund", "The roles and/or users that have access to refund a transaction." );
                return supportedActions;
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

        /// <summary>
        /// Processes the refund.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public FinancialTransaction ProcessRefund( out string errorMessage )
        {
            return this.ProcessRefund( null, null, string.Empty, true, string.Empty, out errorMessage );
        }

        #endregion Public Methods
    }
}
