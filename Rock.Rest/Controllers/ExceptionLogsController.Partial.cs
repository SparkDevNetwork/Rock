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
using System.Web;
using System.Web.Http;

using Rock.Chart;
using Rock.Model;
using Rock.Rest.Filters;
using SharpRaven;
using SharpRaven.Data;

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
            var exceptionList = this.Get().Where( x => x.HasInnerException == false && x.CreatedDateTime != null )
            .GroupBy( x => DbFunctions.TruncateTime( x.CreatedDateTime.Value ) )
            .Select( eg => new
            {
                DateValue = eg.Key.Value,
                ExceptionCount = eg.Count(),
                UniqueExceptionCount = eg.Select( y => y.ExceptionType ).Distinct().Count()
            } )
            .OrderBy( eg => eg.DateValue ).ToList();

            var allCountsQry = exceptionList.Select( c => new ExceptionChartData
            {
                DateTimeStamp = c.DateValue.ToJavascriptMilliseconds(),
                YValue = c.ExceptionCount,
                SeriesName = "Total Exceptions"
            } );

            var uniqueCountsQry = exceptionList.Select( c => new ExceptionChartData
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
            Model.ExceptionLogService.LogException( ex, HttpContext.Current, null, null, personAlias );

            // send the event to Sentry if configured
            var sentryDSN = Web.Cache.GlobalAttributesCache.Read().GetValue( "SentryDSN" ) ?? string.Empty;
            var sentryClient = new RavenClient( sentryDSN );
            if ( !string.IsNullOrEmpty( sentryDSN ) && sentryClient != null )
            {
                //var exceptionLog = new ExceptionLog
                //{
                //    HasInnerException = ex.InnerException != null,
                //    ExceptionType = ex.GetType().ToString(),
                //    Description = ex.Message,
                //    Source = ex.Source,
                //    StackTrace = ex.StackTrace,
                //    CreatedByPersonAliasId = personAlias.Id,
                //    ModifiedByPersonAliasId = personAlias.Id,
                //    CreatedDateTime = RockDateTime.Now,
                //    ModifiedDateTime = RockDateTime.Now,
                //};

                //var context = HttpContext.Current;
                //if ( context != null && context.Request != null && context.Response != null )
                //{
                //    exceptionLog.StatusCode = context.Response.StatusCode.ToString();
                //    exceptionLog.PageUrl = context.Request.Url.ToString();
                //    exceptionLog.QueryString = context.Request.Url.Query;

                //    var formItems = context.Request.Form;
                //    if ( formItems.Keys.Count > 0 )
                //    {
                //        exceptionLog.Form = formItems.AllKeys.ToDictionary( k => k, k => formItems[k] ).ToString();
                //    }

                //    var serverVars = context.Request.ServerVariables;
                //    if ( serverVars.Keys.Count > 0 )
                //    {
                //        exceptionLog.ServerVariables = serverVars.AllKeys.ToDictionary( k => k, k => serverVars[k] ).ToString();
                //    }
                //}

                //ex.Data.Add( "context", exceptionLog );
                sentryClient.Capture( new SentryEvent( ex ) );
            }
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