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
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    public partial class FinancialTransactionRefundsController
    {
        /// <summary>
        /// POST endpoint. Use this to INSERT a new FinancialTransactionRefund, optionally with
        /// its new refund <see cref="FinancialTransactionRefund.FinancialTransaction"/> and that
        /// transaction's TransactionDetails.
        /// </summary>
        /// <remarks>
        /// A refund's Id is its transaction's Id, so the refund transaction can only be saved
        /// with the refund. The nested transaction is checked the same way a POST to the
        /// FinancialTransactions controller checks it.
        /// </remarks>
        /// <param name="value">The FinancialTransactionRefund to add.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public override HttpResponseMessage Post( [FromBody] FinancialTransactionRefund value )
        {
            var transaction = value?.FinancialTransaction;

            if ( transaction != null )
            {
                /*
                    11/10/2026 - MSE

                    The refund does not take its permissions from its transaction (it has no
                    ParentAuthority), so the refund's own EDIT check does not cover the new
                    transaction saved with it. The person must be able to EDIT the transaction
                    as a new transaction, the same check POST /api/FinancialTransactions uses.
                    Like the posted refund, the transaction is always inserted, so its Id is
                    cleared. Its TransactionDetails take their permissions from the transaction.
                    Related records are checked first so a disallowed record returns 400.

                    Reason: Keep creating a refund with its transaction in one request working (#7148).
                */
                CheckNoUntrackedRelatedEntities( value );

                transaction.Id = 0;

                if ( !IsAuthorizedForNewModel( transaction, GetPerson(), rockContext => new FinancialTransactionService( rockContext ) ) )
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }
            }

            return base.Post( value );
        }

        /// <inheritdoc/>
        protected override bool IsRelatedEntityAllowed( string path, bool isCollection )
        {
            /*
                11/10/2026 - MSE

                Giving integrations post a new refund with its refund transaction and that
                transaction's TransactionDetails in one request, because the refund's Id is
                the transaction's Id. Post checks the transaction itself, and a detail takes
                its permissions from its transaction. Everything else, including the
                OriginalTransaction and the details' own related records, is still rejected.

                Reason: Keep creating a refund with its transaction in one request working (#7148).
            */
            if ( isCollection )
            {
                return path == $"{nameof( FinancialTransactionRefund.FinancialTransaction )}.{nameof( FinancialTransaction.TransactionDetails )}";
            }

            return path == nameof( FinancialTransactionRefund.FinancialTransaction );
        }
    }
}
