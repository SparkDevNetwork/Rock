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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// FinancialBatch Logic
    /// </summary>
    public partial class FinancialBatch : Model<FinancialBatch>
    {
        #region Public Methods

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
                if ( result )
                {
                    if ( this.Status == BatchStatus.Closed )
                    {
                        var rockContext = new RockContext();
                        if ( this.ControlAmount != this.GetTotalTransactionAmount( rockContext ) )
                        {
                            ValidationResults.Add( new ValidationResult( "Control variance must be 0 before closing a batch." ) );
                            result = false;
                        }

                        if ( this.HasUnmatchedTransactions( rockContext ) )
                        {
                            ValidationResults.Add( new ValidationResult( "All transactions must be matched before closing a batch." ) );
                            result = false;
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Determines whether [is valid batch status change] [the specified original status].
        /// </summary>
        /// <param name="origStatus">The original status.</param>
        /// <param name="newStatus">The new status.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if [is valid batch status change] [the specified original status]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidBatchStatusChange( BatchStatus origStatus, BatchStatus newStatus, Person currentPerson, out string errorMessage )
        {
            errorMessage = string.Empty;
            if ( origStatus == BatchStatus.Closed && newStatus != BatchStatus.Closed )
            {
                if ( !this.IsAuthorized( "ReopenBatch", currentPerson ) )
                {
                    errorMessage = "User is not authorized to reopen a closed batch";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Total transaction amount of all the transactions in the batch
        /// </summary>
        /// <returns></returns>
        public virtual decimal GetTotalTransactionAmount( RockContext rockContext )
        {
            return new FinancialTransactionService( rockContext ).Queryable()
                .Where( a => a.BatchId == this.Id )
                .Sum( t => ( decimal? ) ( t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M ) ) ?? 0.0M;
        }

        /// <summary>
        /// Determines whether any of the transactions in the batch haven't been matched to a person yet
        /// </summary>
        /// <returns></returns>
        public virtual bool HasUnmatchedTransactions( RockContext rockContext )
        {
            return new FinancialTransactionService( rockContext ).Queryable()
                .Where( a => a.BatchId == this.Id )
                .Any( t => !t.AuthorizedPersonAliasId.HasValue );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this FinancialBatch.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this FinancialBatch.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }
}