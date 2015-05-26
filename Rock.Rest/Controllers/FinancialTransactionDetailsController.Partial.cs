// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;

using Rock.Chart;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FinancialTransactionDetailsController
    {
        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="groupBy">The group by.</param>
        /// <param name="graphBy">The graph by.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="minAmount">The minimum amount.</param>
        /// <param name="maxAmount">The maximum amount.</param>
        /// <param name="currencyTypeIds">The currency type ids.</param>
        /// <param name="sourceTypeIds">The source type ids.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialTransactionDetails/GetChartData" )]
        public IEnumerable<IChartData> GetChartData( ChartGroupBy groupBy = ChartGroupBy.Week, TransactionGraphBy graphBy = TransactionGraphBy.Total, 
            DateTime? startDate = null, DateTime? endDate = null, decimal? minAmount = null, decimal? maxAmount = null, 
            string currencyTypeIds = null, string sourceTypeIds = null, string campusIds = null, int? dataViewId = null )
        {
            var numericCurrencyTypeIds = new List<int>();
            if ( currencyTypeIds != null )
            {
                currencyTypeIds.Split( ',' ).ToList().ForEach( i => numericCurrencyTypeIds.Add( i.AsInteger() ) );
            }

            var numericSourceTypeIds = new List<int>();
            if ( sourceTypeIds != null )
            {
                sourceTypeIds.Split( ',' ).ToList().ForEach( i => numericSourceTypeIds.Add( i.AsInteger() ) );
            }

            var numericCampusIds = new List<int>();
            if ( campusIds != null )
            {
                campusIds.Split( ',' ).ToList().ForEach( i => numericCampusIds.Add( i.AsInteger() ) );
            }

            return new FinancialTransactionDetailService( new RockContext() ).GetChartData(
                groupBy, graphBy, startDate, endDate, minAmount, maxAmount, numericCurrencyTypeIds, 
                numericSourceTypeIds, numericCampusIds, dataViewId );
        }

    }
}
