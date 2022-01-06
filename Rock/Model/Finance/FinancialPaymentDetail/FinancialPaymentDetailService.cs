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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// FinancialPaymentDetail Service class
    /// </summary>
    public partial class FinancialPaymentDetailService
    {
        /// <summary>
        /// Determines whether this instance can delete the specified item, optionally ignoring those Entities that have already been marked for deletion within the <see cref="DbContext"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="ignoreDeleted">if set to <c>true</c> [ignore deleted].</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( FinancialPaymentDetail item, out string errorMessage, bool ignoreDeleted )
        {
            errorMessage = string.Empty;
            var statesToExclude = ignoreDeleted ?
                new EntityState[] { EntityState.Deleted } :
                new EntityState[0];
 
            if ( new Service<FinancialPersonSavedAccount>( Context ).Queryable( statesToExclude ).Any( a => a.FinancialPaymentDetailId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", FinancialPaymentDetail.FriendlyTypeName, FinancialPersonSavedAccount.FriendlyTypeName );
                return false;
            }
 
            if ( new Service<FinancialScheduledTransaction>( Context ).Queryable( statesToExclude ).Any( a => a.FinancialPaymentDetailId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", FinancialPaymentDetail.FriendlyTypeName, FinancialScheduledTransaction.FriendlyTypeName );
                return false;
            }
 
            if ( new Service<FinancialTransaction>( Context ).Queryable( statesToExclude ).Any( a => a.FinancialPaymentDetailId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", FinancialPaymentDetail.FriendlyTypeName, FinancialTransaction.FriendlyTypeName );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes the financial payment detail if it was linked to the referencing Entity and is now orphaned.
        /// </summary>
        /// <param name="referencingEntry">The referencing entry.</param>
        public void DeleteOrphanedFinancialPaymentDetail( DbEntityEntry referencingEntry )
        {
            var financialPaymentDetailId = referencingEntry.OriginalValues["FinancialPaymentDetailId"]?.ToString().AsIntegerOrNull();
            if ( financialPaymentDetailId.HasValue )
            {
                var financialPaymentDetail = this.Get( financialPaymentDetailId.Value );

                if ( financialPaymentDetail != null && this.CanDelete( financialPaymentDetail, out _, true ) )
                {
                    this.Delete( financialPaymentDetail );
                }
            }
        }
    }
}
