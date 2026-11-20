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
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class FinancialBatchesController
    {
        /// <summary>
        /// Deletes the specified financial batch after checking that the person
        /// holds the Delete permission on the batch.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [Authenticate, Secured]
        public override void Delete( int id )
        {
            SetProxyCreation( true );

            FinancialBatch batch;
            if ( !Service.TryGet( id, out batch ) )
            {
                throw new System.Web.Http.HttpResponseException( System.Net.HttpStatusCode.NotFound );
            }

            /*
                11/16/2026 - MSE

                The base DELETE only requires EDIT on the batch, but the Batch List screens
                require the batch Delete permission before they delete a batch. Check the same
                permission here. This keeps the base signature so the ApiId and any custom
                REST security do not change.

                Reason: Deleting a batch over REST must require the batch Delete permission.
            */
            if ( !batch.IsAuthorized( Rock.Security.Authorization.DELETE, GetPerson() ) )
            {
                throw new System.Web.Http.HttpResponseException( System.Net.HttpStatusCode.Unauthorized );
            }

            /*
                11/16/2026 - MSE

                Automated batches are created and managed by Rock's giving automation. The
                Batch List block (FinancialBatchList.cs) refuses to delete them, so REST
                refuses them the same way.

                Reason: Automated batches cannot be deleted over REST, matching the Batch List block.
            */
            if ( batch.IsAutomated )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( System.Net.HttpStatusCode.BadRequest, "Automated batches cannot be deleted." );
                throw new System.Web.Http.HttpResponseException( errorResponse );
            }

            base.Delete( id );
        }

        /// <summary>
        /// Gets the control totals.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialBatches/GetControlTotals" )]
        [Rock.SystemGuid.RestActionGuid( "76DA430F-1470-4BDC-8466-28BDD47894BE" )]
        public IEnumerable<ControlTotalResult> GetControlTotals( System.Web.Http.OData.Query.ODataQueryOptions<FinancialBatch> queryOptions = null )
        {
            var financialBatchQuery = new FinancialBatchService( this.Service.Context as Rock.Data.RockContext ).Queryable();
            financialBatchQuery = queryOptions.ApplyTo( financialBatchQuery ) as IOrderedQueryable<FinancialBatch>;

            /*
                11/20/2026 - MSE

                The REST action grant only says the person may use this endpoint. Each batch has
                its own VIEW rule, so only total the batches the person can view, the same rule
                the batch list and batch detail blocks apply.

                Reason: Anyone with the REST action could read the count and total of any batch.
            */
            var person = GetPerson();
            var viewableBatchIds = financialBatchQuery
                .ToList()
                .Where( b => b.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                .Select( b => b.Id )
                .ToList();

            var batchControlTotalsQuery = financialBatchQuery.Where( b => viewableBatchIds.Contains( b.Id ) ).SelectMany( a => a.Transactions ).Where(a => a.BatchId.HasValue).GroupBy( a => a.BatchId.Value ).Select( a => new
            {
                BatchId = a.Key,
                TransactionTotalAmounts = a.Select( x => x.TransactionDetails.Sum( d => (decimal?)d.Amount ) )
            } );

            var batchControlTotalsList = batchControlTotalsQuery.ToList();

            var controlTotalsList = batchControlTotalsList.Select( a => new ControlTotalResult
            {
                FinancialBatchId = a.BatchId,
                ControlTotalCount = a.TransactionTotalAmounts.Count(),
                ControlTotalAmount = a.TransactionTotalAmounts.Sum( x => (decimal?)x) ?? 0
            } ).ToList();

            return controlTotalsList;
        }
    }
}