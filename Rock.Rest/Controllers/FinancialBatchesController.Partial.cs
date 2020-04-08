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
using System.Linq;
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
        /// Gets the control totals.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialBatches/GetControlTotals" )]
        public IEnumerable<ControlTotalResult> GetControlTotals( System.Web.Http.OData.Query.ODataQueryOptions<FinancialBatch> queryOptions = null )
        {
            var financialBatchQuery = new FinancialBatchService( this.Service.Context as Rock.Data.RockContext ).Queryable();
            financialBatchQuery = queryOptions.ApplyTo( financialBatchQuery ) as IOrderedQueryable<FinancialBatch>;

            var batchControlTotalsQuery = financialBatchQuery.SelectMany( a => a.Transactions ).Where(a => a.BatchId.HasValue).GroupBy( a => a.BatchId.Value ).Select( a => new
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
