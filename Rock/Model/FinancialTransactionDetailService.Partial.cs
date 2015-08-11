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
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;

using Rock.Chart;
using Rock.Data;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.FinancialTransactionDetail"/> entity objects.
    /// </summary>
    public partial class FinancialTransactionDetailService 
    {

        /// <summary>
        /// Gets the gifts.
        /// </summary>
        /// <returns></returns>
        public IQueryable<FinancialTransactionDetail> GetGifts()
        {
            return Queryable().AsNoTracking()
                .Where( t =>
                    t.Account != null &&
                    t.Account.IsTaxDeductible &&
                    t.Transaction != null &&
                    t.Transaction.TransactionDateTime.HasValue &&
                    t.Transaction.AuthorizedPersonAlias != null &&
                    t.Transaction.AuthorizedPersonAlias.Person != null );
        }

        /// <summary>
        /// Gets financial transaction details based on selected filter values.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="minAmount">The minimum amount.</param>
        /// <param name="maxAmount">The maximum amount.</param>
        /// <param name="currencyTypeIds">The currency type ids.</param>
        /// <param name="sourceTypeIds">The source type ids.</param>
        /// <param name="accountIds">The account ids.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns></returns>
        public IQueryable<FinancialTransactionDetail> GetGifts(
            DateTime? start, DateTime? end, decimal? minAmount, decimal? maxAmount,
            List<int> currencyTypeIds, List<int> sourceTypeIds, List<int> accountIds, int? dataViewId )
        {
            // Base Transaction Detail query
            var qry = GetGifts();

            // Start Date Filter
            if ( start.HasValue )
            {
                qry = qry.Where( t => t.Transaction.TransactionDateTime >= start.Value );
            }

            // End Date Filter
            if ( end.HasValue )
            {
                qry = qry.Where( t => t.Transaction.TransactionDateTime < end.Value );
            }

            // Account Id Filter
            var distictAccountIds = accountIds.Where( i => i != 0 ).Distinct().ToList();
            if ( distictAccountIds.Any() )
            {
                qry = qry
                    .Where( t =>
                        distictAccountIds.Contains( t.AccountId ) );
            }


            // Currency Type Filter
            var distictCurrencyTypeIds = currencyTypeIds.Where( i => i != 0 ).Distinct().ToList();
            if ( distictCurrencyTypeIds.Any() )
            {
                qry = qry
                    .Where( t =>
                        t.Transaction.FinancialPaymentDetail != null &&
                        t.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue &&
                        distictCurrencyTypeIds.Contains( t.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );
            }

            // Source Type Filter
            var distictSourceTypeIds = sourceTypeIds.Where( i => i != 0 ).Distinct().ToList();
            if ( distictSourceTypeIds.Any() )
            {
                qry = qry
                    .Where( t =>
                        t.Transaction.SourceTypeValueId.HasValue &&
                        distictSourceTypeIds.Contains( t.Transaction.SourceTypeValueId.Value ) );
            }

            // Amount Range Filter
            if ( minAmount.HasValue || maxAmount.HasValue )
            {
                var givingIdQry = qry
                    .GroupBy( d => d.Transaction.AuthorizedPersonAlias.Person.GivingId )
                    .Select( d => new { d.Key, Total = d.Sum( t => t.Amount ) } )
                    .Where( s =>
                        ( !minAmount.HasValue || s.Total >= minAmount.Value ) &&
                        ( !maxAmount.HasValue || s.Total <= maxAmount.Value ) )
                    .Select( s => s.Key );

                qry = qry
                    .Where( d =>
                        givingIdQry.Contains( d.Transaction.AuthorizedPersonAlias.Person.GivingId ) );
            }
            
            // Data View Filter
            if ( dataViewId.HasValue )
            {
                var rockContext = (RockContext)this.Context;
                if ( rockContext != null )
                {
                    var personService = new PersonService( rockContext );
                    var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                    if ( dataView != null )
                    {
                        var errorMessages = new List<string>();
                        ParameterExpression paramExpression = personService.ParameterExpression;
                        Expression whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );

                        SortProperty sortProperty = null;
                        var dataViewGivingIdQry = personService
                            .Queryable().AsNoTracking()
                            .Where( paramExpression, whereExpression, sortProperty )
                            .Select( p => p.GivingId );

                        qry = qry
                            .Where( t =>
                                dataViewGivingIdQry.Contains( t.Transaction.AuthorizedPersonAlias.Person.GivingId ) );
                    }
                }
            }

            return qry;
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="groupBy">The group by.</param>
        /// <param name="graphBy">The graph by.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="minAmount">The minimum amount.</param>
        /// <param name="maxAmount">The maximum amount.</param>
        /// <param name="currencyTypeIds">The currency type ids.</param>
        /// <param name="sourceTypeIds">The source type ids.</param>
        /// <param name="accountIds">The account ids.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns></returns>
        public IEnumerable<IChartData> GetChartData(
            ChartGroupBy groupBy, TransactionGraphBy graphBy, DateTime? start, DateTime? end, decimal? minAmount, decimal? maxAmount,
            List<int> currencyTypeIds, List<int> sourceTypeIds, List<int> accountIds, int? dataViewId )
        {
            var qry = GetGifts( start, end, minAmount, maxAmount, currencyTypeIds, sourceTypeIds, accountIds, dataViewId );

            var qryWithSummaryDateTime = qry.GetFinancialTransactionDetailWithSummaryDateTime( groupBy );

            var summaryQry = qryWithSummaryDateTime.Select( d => new
            {
                d.SummaryDateTime,
                Campus = new
                {
                    Id = d.FinancialTransactionDetail.Account.CampusId ?? 0,
                    Name = d.FinancialTransactionDetail.Account.Campus != null ?
                        d.FinancialTransactionDetail.Account.Campus.Name : "None"
                },
                Account = new
                {
                    Id = d.FinancialTransactionDetail.AccountId,
                    Name = d.FinancialTransactionDetail.Account.Name
                },
                Amount = d.FinancialTransactionDetail.Amount
            } );

            List<SummaryData> result = null;

            if ( graphBy == TransactionGraphBy.Total )
            {
                var groupByQry = summaryQry.GroupBy( d => new { d.SummaryDateTime } ).Select( s => new { s.Key, Amount = s.Sum( a => a.Amount ) } ).OrderBy( o => o.Key );
                result = groupByQry.ToList().Select( d => new SummaryData
                    {
                        DateTimeStamp = d.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                        DateTime = d.Key.SummaryDateTime,
                        SeriesId = "Total",
                        YValue = d.Amount
                    } ).ToList();
            } 
            else if ( graphBy == TransactionGraphBy.Campus )
            {
                var groupByQry = summaryQry.GroupBy( d => new { d.SummaryDateTime, Series = d.Campus } ).Select( s => new { s.Key, Amount = s.Sum( a => a.Amount ) } ).OrderBy( o => o.Key );
                result = groupByQry.ToList().Select( d => new SummaryData
                {
                    DateTimeStamp = d.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = d.Key.SummaryDateTime,
                    SeriesId = d.Key.Series.Name,
                    YValue = d.Amount
                } ).ToList();
            }
            else if ( graphBy == TransactionGraphBy.FinancialAccount )
            {
                var groupByQry = summaryQry.GroupBy( d => new { d.SummaryDateTime, Series = d.Account } ).Select( s => new { s.Key, Amount = s.Sum( a => a.Amount ) } ).OrderBy( o => o.Key );
                result = groupByQry.ToList().Select( d => new SummaryData
                {
                    DateTimeStamp = d.Key.SummaryDateTime.ToJavascriptMilliseconds(),
                    DateTime = d.Key.SummaryDateTime,
                    SeriesId = d.Key.Series.Name,
                    YValue = d.Amount
                } ).ToList();
            }

            if ( result.Count == 1 )
            {
                var dummyZeroDate = start ?? DateTime.MinValue;
                result.Insert( 0, new SummaryData { DateTime = dummyZeroDate, DateTimeStamp = dummyZeroDate.ToJavascriptMilliseconds(), SeriesId = result[0].SeriesId, YValue = 0 } );
            }

            return result;
        }

        /// <summary>
        /// Gets the gifts.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="minAmount">The minimum amount.</param>
        /// <param name="maxAmount">The maximum amount.</param>
        /// <param name="accountIds">The account ids.</param>
        /// <param name="currencyTypeIds">The currency type ids.</param>
        /// <param name="sourceTypeIds">The source type ids.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="giversViewBy">The givers view by.</param>
        /// <returns></returns>
        public static DataSet GetGivingAnalytics(
            DateTime? start, DateTime? end, decimal? minAmount, decimal? maxAmount,
            List<int> accountIds, List<int> currencyTypeIds, List<int> sourceTypeIds, int? dataViewId, GiversViewBy giversViewBy )
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if ( start.HasValue )
            {
                parameters.Add( "StartDate", start.Value );
            }

            if ( end.HasValue )
            {
                parameters.Add( "EndDate", end.Value );
            }

            if ( minAmount.HasValue )
            {
                parameters.Add( "MinAmount", minAmount.Value );
            }

            if ( maxAmount.HasValue )
            {
                parameters.Add( "MaxAmount", maxAmount.Value );
            }

            if ( accountIds != null && accountIds.Any() )
            {
                parameters.Add( "AccountIds", accountIds.AsDelimited(",") );
            }

            if ( currencyTypeIds != null && currencyTypeIds.Any() )
            {
                parameters.Add( "CurrencyTypeIds", currencyTypeIds.AsDelimited(",") );
            }

            if ( sourceTypeIds != null && sourceTypeIds.Any() )
            {
                parameters.Add( "SourceTypeIds", sourceTypeIds.AsDelimited(",") );
            }

            string viewBy = "G";
            switch ( giversViewBy )
            {
                case GiversViewBy.Giver: viewBy = "G"; break;
                case GiversViewBy.Adults: viewBy = "A"; break;
                case GiversViewBy.Children: viewBy = "C"; break;
                case GiversViewBy.Family: viewBy = "F"; break;
            }
            parameters.Add( "ViewBy", viewBy );

            var result = DbService.GetDataSet( "spFinance_GivingAnalyticsQuery", System.Data.CommandType.StoredProcedure, parameters, 180 );

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public class FinancialTransactionDetailWithSummaryDateTime
        {
            /// <summary>
            /// Gets or sets the summary date time.
            /// </summary>
            /// <value>
            /// The summary date time.
            /// </value>
            public DateTime SummaryDateTime {get; set;}

            /// <summary>
            /// Gets or sets the financial transaction detail.
            /// </summary>
            /// <value>
            /// The financial transaction detail.
            /// </value>
            public FinancialTransactionDetail FinancialTransactionDetail { get; set;}
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public enum GiversViewBy
    {
        /// <summary>
        /// The giver
        /// </summary>
        Giver = 0,

        /// <summary>
        /// The adults
        /// </summary>
        Adults = 1,

        /// <summary>
        /// The children
        /// </summary>
        Children = 2,

        /// <summary>
        /// The family
        /// </summary>
        Family = 3,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class FinancialTransactionDetailQryExtensions
    {
        /// <summary>
        /// Gets the financial transaction detail with summary date time.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="summarizeBy">The summarize by.</param>
        /// <returns></returns>
        public static IQueryable<FinancialTransactionDetailService.FinancialTransactionDetailWithSummaryDateTime> 
            GetFinancialTransactionDetailWithSummaryDateTime( this IQueryable<FinancialTransactionDetail> qry, ChartGroupBy summarizeBy )
        {
            //// for Date SQL functions, borrowed some ideas from http://stackoverflow.com/a/1177529/1755417 and http://stackoverflow.com/a/133101/1755417 and http://stackoverflow.com/a/607837/1755417

            if ( summarizeBy == ChartGroupBy.Week )
            {
                var knownSunday = new DateTime( 1966, 1, 30 );    // Because we can't use the @@DATEFIRST option in Linq to query how DATEPART("weekday",) will work, use a known Sunday date instead.
                var qryWithSundayDate = qry.Select( d => new
                {
                    FinancialTransactionDetail = d,
                    SundayDate = SqlFunctions.DateAdd(
                            "day",
                            SqlFunctions.DateDiff( "day",
                                "1900-01-01",
                                SqlFunctions.DateAdd( "day",
                                    ( ( ( SqlFunctions.DatePart( "weekday", knownSunday ) + 7 ) - SqlFunctions.DatePart( "weekday", d.Transaction.TransactionDateTime ) ) % 7 ),
                                    d.Transaction.TransactionDateTime
                                )
                            ),
                            "1900-01-01"
                        )
                } );

                var qryGroupedBy = qryWithSundayDate.Select( d => new FinancialTransactionDetailService.FinancialTransactionDetailWithSummaryDateTime
                {
                    SummaryDateTime = (DateTime)d.SundayDate,
                    FinancialTransactionDetail = d.FinancialTransactionDetail
                } );

                return qryGroupedBy;
            }
            else
            {
                var qryGroupedBy = qry.Select( d => new FinancialTransactionDetailService.FinancialTransactionDetailWithSummaryDateTime
                {
                    // Build a CASE statement to group by week, or month, or year
                    SummaryDateTime = (DateTime)(

                        // GroupBy Month 
                        summarizeBy == ChartGroupBy.Month ? SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "day", d.Transaction.TransactionDateTime ) + 1, DbFunctions.TruncateTime( d.Transaction.TransactionDateTime.Value ) ) :

                        // GroupBy Year
                        summarizeBy == ChartGroupBy.Year ? SqlFunctions.DateAdd( "day", -SqlFunctions.DatePart( "dayofyear", d.Transaction.TransactionDateTime ) + 1, DbFunctions.TruncateTime( d.Transaction.TransactionDateTime.Value ) ) :

                        // shouldn't happen
                        null
                    ),
                    FinancialTransactionDetail = d
                } );

                return qryGroupedBy;
            }
        }

    }
}