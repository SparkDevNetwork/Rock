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
using Rock.Data;
using Rock.Logging;
using System;

namespace Rock.Model
{
    /// <summary>
    /// FinancialPersonSavedAccount SaveHook
    /// </summary>
    public partial class FinancialPersonSavedAccount
    {
        /// <inheritdoc/>
        internal class SaveHook : EntitySaveHook<FinancialPersonSavedAccount>
        {
            /// <inheritdoc/>
            protected override void PreSave()
            {
                base.PreSave();
            }

            protected override void PostSave()
            {
                if ( Entry.State == EntityContextState.Deleted || Entry.State == EntityContextState.Modified )
                {
                    // If a FinancialPaymentDetail was linked to this FinancialScheduledTransaction and is now orphaned, delete it
                    var originalFinancialPaymentDetailId = Entry.OriginalValues[nameof( FinancialPersonSavedAccount.FinancialPaymentDetailId )] as int?;
                    if ( originalFinancialPaymentDetailId.HasValue && Entity.FinancialPaymentDetailId != originalFinancialPaymentDetailId.Value )
                    {
                        var rockContext = this.RockContext;
                        var financialPaymentDetailService = new FinancialPaymentDetailService( rockContext );
                        financialPaymentDetailService.DeleteOrphanedFinancialPaymentDetail( Entry );
                        rockContext.SaveChanges();
                    }
                }

                base.PostSave();
            }


            /* 2022-07-22 ED
             * Circular references will result in an error when trying to perform an operation on both items, such as delete.
             * This can result in DependencyOrderingError, Unable to determine a valid ordering for dependent operations. Dependencies may exist dueo to foreign key constratints, model requirements, or store-generated values.
             * Since FinancialPersonSavedAccount has a FinancialPaymentDetailId, we cannot also have a reverse relationship on FinancialPaymentDetail.
             * So the logic below should not be applied.
             * 
             * /// <summary>
             * /// If this FinancialPersonSavedAccount is associated with a FinancialPaymentDetail entity, and that
             * /// FinancialPaymentDetail entity is not already associated with another FinancialPersonSavedAccount,
             * /// then we should create the reverse-association so that the payment detail points back to this
             * /// saved account.  Doing so creates more useful data if the FinancialPaymentDetail entity is cloned
             * /// in the future (i.e., because of tokenized payment methods being reused for new scheduled
             * /// transactions).
             * /// </summary>
             * private void CreateReciprocalPaymentDetailRelationship()
             * {
             *     if ( State != EntityContextState.Added && State != EntityContextState.Modified )
             *     {
             *         return; // Exit if this record is not being inserted or updated.
             *     }

             *     var rockContext = ( RockContext ) DbContext;
             *     var paymentDetailId = Entity.FinancialPaymentDetailId;
             *     FinancialPaymentDetail paymentDetail = null;

             *     if ( paymentDetailId.HasValue )
             *     {
             *         paymentDetail = new FinancialPaymentDetailService( rockContext ).Get( paymentDetailId.Value );
             *     }

             *     if ( paymentDetail != null && paymentDetail.FinancialPersonSavedAccountId == null )
             *     {
             *         paymentDetail.FinancialPersonSavedAccountId = Entity.Id;
             *         rockContext.SaveChanges();
             *     }
             * }
            */
        }
    }
}