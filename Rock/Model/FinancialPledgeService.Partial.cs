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
using System.Data;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.FinancialPledge"/> entity objects.
    /// </summary>
    public partial class FinancialPledgeService 
    {
        /// <summary>
        /// Gets the pledge analytics.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="minAmountPledged">The minimum amount pledged.</param>
        /// <param name="maxAmountPledged">The maximum amount pledged.</param>
        /// <param name="minComplete">The minimum complete.</param>
        /// <param name="maxComplete">The maximum complete.</param>
        /// <param name="minAmountGiven">The minimum amount given.</param>
        /// <param name="maxAmountGiven">The maximum amount given.</param>
        /// <param name="includePledges">if set to <c>true</c> [include pledges].</param>
        /// <param name="includeGifts">if set to <c>true</c> [include gifts].</param>
        /// <returns></returns>
        public static DataSet GetPledgeAnalytics( int accountId, DateTime? start, DateTime? end,
            decimal? minAmountPledged, decimal? maxAmountPledged, decimal? minComplete, decimal? maxComplete, decimal? minAmountGiven, decimal? maxAmountGiven, 
            bool includePledges, bool includeGifts )
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add( "AccountId", accountId );

            if ( start.HasValue )
            {
                parameters.Add( "StartDate", start.Value );
            }

            if ( end.HasValue )
            {
                parameters.Add( "EndDate", end.Value );
            }

            if ( minAmountPledged.HasValue )
            {
                parameters.Add( "MinAmountPledged", minAmountPledged.Value );
            }

            if ( maxAmountPledged.HasValue )
            {
                parameters.Add( "MaxAmountPledged", maxAmountPledged.Value );
            }

            if ( minComplete.HasValue )
            {
                parameters.Add( "MinComplete", minComplete.Value / 100 );
            }

            if ( maxComplete.HasValue )
            {
                parameters.Add( "MaxComplete", maxComplete.Value / 100 );
            }

            if ( minAmountGiven.HasValue )
            {
                parameters.Add( "MinAmountGiven", minAmountGiven.Value );
            }

            if ( maxAmountGiven.HasValue )
            {
                parameters.Add( "MaxAmountGiven", maxAmountGiven.Value );
            }

            parameters.Add( "IncludePledges", includePledges );
            parameters.Add( "IncludeGifts", includeGifts );

            var result = DbService.GetDataSet( "spFinance_PledgeAnalyticsQuery", System.Data.CommandType.StoredProcedure, parameters, 180 );

            return result;
        }

    }
}