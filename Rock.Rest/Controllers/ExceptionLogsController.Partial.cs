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
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

using Rock.Chart;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// ExceptionLogs REST API
    /// </summary>
    public partial class ExceptionLogsController
    {
        /// <summary>
        /// Gets the exceptions grouped by date.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ExceptionLogs/GetChartData" )]
        public IEnumerable<IChartData> GetChartData()
        {
            // Load data into a List so we can so all the aggregate calculations in C# instead making the Database do it
            var exceptionList = this.Get()
                .Where( x => x.HasInnerException == false && x.CreatedDateTime != null ).Select( s => new
                {
                    s.CreatedDateTime,
                    s.ExceptionType
                } ).ToList();

            var exceptionSummaryList = exceptionList.GroupBy( x => x.CreatedDateTime.Value.Date )
            .Select( eg => new
            {
                DateValue = eg.Key,
                ExceptionCount = eg.Count(),
                UniqueExceptionCount = eg.Select( y => y.ExceptionType ).Distinct().Count()
            } )
            .OrderBy( eg => eg.DateValue ).ToList();

            var allCountsQry = exceptionSummaryList.Select( c => new ExceptionChartData
            {
                DateTimeStamp = c.DateValue.ToJavascriptMilliseconds(),
                YValue = c.ExceptionCount,
                SeriesName = "Total Exceptions"
            } );

            var uniqueCountsQry = exceptionSummaryList.Select( c => new ExceptionChartData
            {
                DateTimeStamp = c.DateValue.ToJavascriptMilliseconds(),
                YValue = c.UniqueExceptionCount,
                SeriesName = "Unique Exceptions"
            } );

            var result = allCountsQry.Union( uniqueCountsQry );
            return result;
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ExceptionLogs/LogException" )]
        [HttpPost]
        public void LogException( Exception ex )
        {
            var personAlias = this.GetPersonAlias();
            Rock.Model.ExceptionLogService.LogException( ex, System.Web.HttpContext.Current, null, null, personAlias );
        }

        /// <summary>
        /// 
        /// </summary>
        public class ExceptionChartData : IChartData
        {
            /// <summary>
            /// Gets the date time stamp.
            /// </summary>
            /// <value>
            /// The date time stamp.
            /// </value>
            public long DateTimeStamp { get; set; }

            /// <summary>
            /// Gets the y value.
            /// </summary>
            /// <value>
            /// The y value.
            /// </value>
            public decimal? YValue { get; set; }

            /// <summary>
            /// Gets or sets the name of the series. This will be the default name of the series if MetricValuePartitionEntityIds can't be resolved
            /// </summary>
            /// <value>
            /// The name of the series.
            /// </value>
            public string SeriesName { get; set; }

            /// <summary>
            /// Gets the metric value partitions as a comma-delimited list of EntityTypeId|EntityId
            /// </summary>
            /// <value>
            /// The metric value entityTypeId,EntityId partitions
            /// </value>
            public string MetricValuePartitionEntityIds { get; set; }
        }
    }
}
