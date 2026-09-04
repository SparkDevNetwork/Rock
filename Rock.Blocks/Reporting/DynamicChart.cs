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
using System.ComponentModel;
using System.Data;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.ViewModels.Blocks.Reporting.DynamicChart;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Block to display a chart using SQL as the chart datasource.
    /// </summary>

    [DisplayName( "Dynamic Chart" )]
    [Category( "Reporting" )]
    [Description( "Block to display a chart using SQL as the chart datasource" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField( "Chart Height",
        Key = AttributeKey.ChartHeight,
        DefaultIntegerValue = 200,
        IsRequired = false )]

    [TextField( "Query Params",
        Key = AttributeKey.QueryParams,
        Description = "The parameters that the stored procedure expects in the format of 'param1=value;param2=value'. Any parameter with the same name as a page parameter (i.e. querystring, form, or page route) will have its value replaced with the page's current value. A parameter with the name of 'CurrentPersonId' will have its value replaced with the currently logged in person's id.",
        IsRequired = false )]

    [CodeEditorField( "SQL",
        Key = AttributeKey.Sql,
        Description = "See the code example in the default text of the block.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Sql,
        DefaultValue = @"/*The SQL for the datasource. Output columns must be as follows:
Bar or Line Chart
    [SeriesName] : string or numeric
    [DateTime] : DateTime
    [YValue] : numeric

Pie Chart
[MetricTitle] : string
[YValueTotal] : numeric
*/

-- Get Exception count per day for the last 10 days.
WITH [Last10Days]
AS
(
    SELECT CONVERT(date, GETDATE()) [Date]
    UNION ALL
    SELECT DATEADD(day, -1, [Date])
    FROM [Last10Days]
    WHERE ([Date] > GETDATE() - 9)
)
SELECT 'Exception Count' [SeriesName]
    , d.[Date] [DateTime]
    , CASE WHEN exceptions.[ExceptionCount] IS NOT NULL THEN exceptions.[ExceptionCount] ELSE 0 END [YValue]
FROM [Last10Days] d
LEFT OUTER JOIN
(
    SELECT CONVERT(date, [CreatedDateTime]) [Date]
        , COUNT(*) [ExceptionCount]
    FROM [ExceptionLog]
    GROUP BY CONVERT(date, [CreatedDateTime])
) exceptions
    ON d.[Date] = exceptions.[Date]
ORDER BY d.[Date];",
        IsRequired = false )]

    [TextField( "Title",
        Key = AttributeKey.Title,
        Description = "The title of the widget",
        IsRequired = false,
        Order = 0 )]

    [TextField( "Subtitle",
        Key = AttributeKey.Subtitle,
        Description = "The subtitle of the widget",
        IsRequired = false,
        Order = 1 )]

    [CustomDropdownListField( "Column Width",
        Key = AttributeKey.ColumnWidth,
        Description = "The width of the widget.",
        ListSource = ",1,2,3,4,5,6,7,8,9,10,11,12",
        IsRequired = false,
        DefaultValue = "4",
        Order = 2 )]

    [BooleanField( "Show Legend",
        Key = AttributeKey.ShowLegend,
        DefaultBooleanValue = true,
        Order = 7 )]

    [CustomDropdownListField( "Legend Position",
        Key = AttributeKey.LegendPosition,
        Description = "Select the position of the Legend (corner)",
        ListSource = "n,ne,e,se,s,sw,w,nw",
        IsRequired = false,
        DefaultValue = "ne",
        Order = 8 )]

    [CustomDropdownListField( "Chart Type",
        Key = AttributeKey.ChartType,
        ListSource = "Line,Bar,Pie",
        IsRequired = false,
        DefaultValue = "Line",
        Order = 9 )]

    [DecimalField( "Pie Inner Radius",
        Key = AttributeKey.PieInnerRadius,
        Description = "If this is a pie chart, specific the inner radius to have a donut hole. For example, specify: 0.75 to have the inner radius as 75% of the outer radius.",
        IsRequired = false,
        DefaultDecimalValue = 0,
        Order = 10 )]

    [BooleanField( "Pie Show Labels",
        Key = AttributeKey.PieShowLabels,
        Description = "If this is a pie chart, specify if labels should be shown.",
        DefaultBooleanValue = true,
        Order = 11 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B866DC03-23AD-4581-BD83-15938F0D0574" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "BF334BF8-7E57-4BEF-94F2-B9B7D9394E8A" )]
    [Rock.SystemGuid.BlockTypeGuid( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723" )]
    public class DynamicChart : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ChartHeight = "ChartHeight";
            public const string QueryParams = "QueryParams";
            public const string Sql = "SQL";
            public const string Title = "Title";
            public const string Subtitle = "Subtitle";
            public const string ColumnWidth = "ColumnWidth";
            public const string ShowLegend = "ShowLegend";
            public const string LegendPosition = "LegendPosition";
            public const string ChartType = "ChartType";
            public const string PieInnerRadius = "PieInnerRadius";
            public const string PieShowLabels = "PieShowLabels";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The name used for a chart's single data series when the query does
        /// not include a series column.
        /// </summary>
        private const string DefaultSeriesName = "Series 1";

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var options = new DynamicChartOptionsBag
            {
                Title = GetAttributeValue( AttributeKey.Title ),
                Subtitle = GetAttributeValue( AttributeKey.Subtitle ),
                ChartType = GetAttributeValue( AttributeKey.ChartType ).ToStringOrDefault( "line" ).Trim().ToLower(),
                ChartHeight = GetAttributeValue( AttributeKey.ChartHeight ).AsIntegerOrNull() ?? 200,
                IsLegendShown = GetAttributeValue( AttributeKey.ShowLegend ).AsBooleanOrNull() ?? true,
                LegendPosition = GetAttributeValue( AttributeKey.LegendPosition ),
                PieInnerRadius = GetAttributeValue( AttributeKey.PieInnerRadius ).AsDoubleOrNull() ?? 0,
                ArePieLabelsShown = GetAttributeValue( AttributeKey.PieShowLabels ).AsBooleanOrNull() ?? true,
                ColumnWidth = GetAttributeValue( AttributeKey.ColumnWidth ).AsIntegerOrNull(),
                Labels = new List<string>(),
                Series = new List<DynamicChartSeriesBag>()
            };

            var sql = GetAttributeValue( AttributeKey.Sql );

            if ( sql.IsNullOrWhiteSpace() )
            {
                options.ErrorMessage = "[Dynamic Chart]: SQL needs to be configured in block settings.";
                return options;
            }

            DataTable dataTable;
            try
            {
                sql = sql.ResolveMergeFields( GetMergeFields() );

                var parameters = GetSqlParameters( GetAttributeValue( AttributeKey.QueryParams ).SplitDelimitedValues() );

                dataTable = DbService.GetDataSet( sql, CommandType.Text, parameters ).Tables[0];
            }
            catch ( Exception ex )
            {
                // Log the underlying error so an administrator can diagnose the
                // configured SQL; the viewer only sees the generic message.
                ExceptionLogService.LogException( ex );

                options.ErrorMessage = "[Dynamic Chart]: The data could not be retrieved.";
                return options;
            }

            var rows = dataTable.Rows.OfType<DataRow>().ToList();

            if ( !rows.Any() )
            {
                // An empty result set renders an empty widget rather than an error.
                return options;
            }

            /* Identify the fields available in the dataset. */

            // A series represents a set of related data points that are displayed
            // on the chart. A line or bar chart can display multiple series at
            // the same time.
            var seriesFieldName = GetFirstMatchedFieldName( dataTable, new List<string> { "SeriesName", "SeriesID" } );
            var categoryFieldName = GetFirstMatchedFieldName( dataTable, new List<string> { "Category", "MetricTitle" } );
            var yValueFieldName = GetFirstMatchedFieldName( dataTable, new List<string> { "Value", "YValue", "YValueTotal" } );
            var xValueFieldName = GetFirstMatchedFieldName( dataTable, new List<string> { "XValue", "XValueTotal" } );
            var dateTimeFieldName = GetFirstMatchedFieldName( dataTable, new List<string> { "DateTimeValue", "DateTime" } );

            if ( options.ChartType == "pie" )
            {
                // The Pie Chart data set requires the following columns:
                // MetricTitle (string), YValueTotal (numeric).
                // It can only be used to plot category data, not a time series.
                if ( categoryFieldName == null )
                {
                    if ( seriesFieldName.IsNotNullOrWhiteSpace() )
                    {
                        // Assume that each series is intended to be plotted as a pie category.
                        categoryFieldName = seriesFieldName;
                        seriesFieldName = null;
                    }
                    else if ( xValueFieldName.IsNotNullOrWhiteSpace() )
                    {
                        // Assume that the XValue is intended to be plotted as a pie category.
                        categoryFieldName = xValueFieldName;
                        xValueFieldName = null;
                    }
                    else
                    {
                        options.ErrorMessage = "[Dynamic Chart]: Pie Chart dataset must contain a category field: [Category] or [MetricTitle]";
                    }
                }

                if ( yValueFieldName == null )
                {
                    options.ErrorMessage = "[Dynamic Chart]: Pie Chart dataset must contain a value field: [YValue] or [YValueTotal]";
                }
            }
            else if ( options.ChartType == "bar" )
            {
                // The Bar Chart data set requires the following columns:
                // SeriesName (string), YValue (numeric).
                // It can only be used to plot category data, not a time series.
                if ( categoryFieldName == null )
                {
                    if ( xValueFieldName.IsNotNullOrWhiteSpace() )
                    {
                        // Assume that the X-axis values represent the bar categories.
                        categoryFieldName = xValueFieldName;
                        xValueFieldName = null;
                    }
                    else if ( seriesFieldName.IsNotNullOrWhiteSpace() )
                    {
                        // Assume that each series is intended to be plotted as a bar category.
                        categoryFieldName = seriesFieldName;
                        seriesFieldName = null;
                    }
                    else
                    {
                        options.ErrorMessage = "[Dynamic Chart]: Bar Chart dataset must contain a category field: [Category] or [MetricTitle]";
                    }
                }

                if ( yValueFieldName == null )
                {
                    options.ErrorMessage = "[Dynamic Chart]: Bar Chart dataset must contain a value field: [Value], [YValue] or [DateTime]";
                }
            }
            else
            {
                // The Line Chart can represent a time series or an X vs Y graph.
                // The data set may contain the following columns:
                // SeriesName (string,optional), XValue (numeric) or DateTime (datetime), YValue (numeric).
                // If DateTime exists, the chart will be plotted as a time series.
                // If XValue exists, the chart will be plotted as an X vs Y graph.
                if ( xValueFieldName == null && dateTimeFieldName == null )
                {
                    options.ErrorMessage = "[Dynamic Chart]: Line Chart dataset must contain an X-value or datetime field: [XValue] or [DateTime]";
                }

                if ( yValueFieldName == null )
                {
                    options.ErrorMessage = "[Dynamic Chart]: Line Chart dataset must contain a Y-value field: [Value] or [YValue]";
                }
            }

            if ( options.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                return options;
            }

            var isTimeSeries = options.ChartType != "pie"
                && options.ChartType != "bar"
                && dateTimeFieldName != null;

            if ( isTimeSeries )
            {
                BuildTimeSeriesData( options, rows, seriesFieldName, dateTimeFieldName, yValueFieldName );
            }
            else
            {
                // If a category field name is not specified, use the XValue.
                if ( categoryFieldName.IsNullOrWhiteSpace() )
                {
                    categoryFieldName = xValueFieldName;
                }

                BuildCategorySeriesData( options, rows, seriesFieldName, categoryFieldName, yValueFieldName );
            }

            return options;
        }

        /// <summary>
        /// Populates the options bag with time series data by pivoting the query
        /// rows into a sorted set of date/time labels and one value per label
        /// for each series.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="rows">The rows returned by the query.</param>
        /// <param name="seriesFieldName">The name of the series column, or null if the data has a single series.</param>
        /// <param name="dateTimeFieldName">The name of the date/time column.</param>
        /// <param name="valueFieldName">The name of the value column.</param>
        private void BuildTimeSeriesData( DynamicChartOptionsBag options, List<DataRow> rows, string seriesFieldName, string dateTimeFieldName, string valueFieldName )
        {
            /*
                7/1/26 - MSE

                The chart controls take one shared set of labels with each series
                supplying a value (or null) per label, so rows are pivoted here
                rather than passed through as individual data points. If a query
                returns more than one row for the same series and date/time, the
                values are summed.

                Reason: Chart data must be pivoted into label-aligned series, so
                duplicate data points are summed.
            */
            var seriesNames = new List<string>();
            var valuesBySeries = new Dictionary<string, Dictionary<DateTime, decimal>>( StringComparer.OrdinalIgnoreCase );

            foreach ( var row in rows )
            {
                var seriesName = seriesFieldName.IsNotNullOrWhiteSpace()
                    ? row[seriesFieldName].ToStringOrDefault( string.Empty ).Trim()
                    : DefaultSeriesName;

                if ( !valuesBySeries.TryGetValue( seriesName, out var seriesValues ) )
                {
                    seriesValues = new Dictionary<DateTime, decimal>();
                    valuesBySeries.Add( seriesName, seriesValues );
                    seriesNames.Add( seriesName );
                }

                var dateTimeValue = row[dateTimeFieldName].ToStringOrDefault( string.Empty ).AsDateTime();

                if ( dateTimeValue == null )
                {
                    // A data point without a valid date/time cannot be plotted.
                    continue;
                }

                var value = row[valueFieldName].ToStringOrDefault( "0" ).AsDecimal();

                seriesValues.TryGetValue( dateTimeValue.Value, out var existingValue );
                seriesValues[dateTimeValue.Value] = existingValue + value;
            }

            var labelDateTimes = valuesBySeries.Values
                .SelectMany( v => v.Keys )
                .Distinct()
                .OrderBy( d => d )
                .ToList();

            options.IsTimeSeries = true;
            options.Labels = labelDateTimes.Select( d => d.ToString( "s" ) ).ToList();
            options.Series = seriesNames
                .Select( name => new DynamicChartSeriesBag
                {
                    Name = name,
                    Values = labelDateTimes
                        .Select( d => valuesBySeries[name].TryGetValue( d, out var value ) ? value : ( decimal? ) null )
                        .ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Populates the options bag with category data by pivoting the query
        /// rows into a set of category labels, ordered by first appearance, and
        /// one value per label for each series.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="rows">The rows returned by the query.</param>
        /// <param name="seriesFieldName">The name of the series column, or null if the data has a single series.</param>
        /// <param name="categoryFieldName">The name of the category column.</param>
        /// <param name="valueFieldName">The name of the value column.</param>
        private void BuildCategorySeriesData( DynamicChartOptionsBag options, List<DataRow> rows, string seriesFieldName, string categoryFieldName, string valueFieldName )
        {
            // Rows are pivoted into label-aligned series values, with values for
            // duplicate (series, category) pairs summed together.
            var labels = new List<string>();
            var seriesNames = new List<string>();
            var valuesBySeries = new Dictionary<string, Dictionary<string, decimal>>( StringComparer.OrdinalIgnoreCase );

            foreach ( var row in rows )
            {
                var seriesName = seriesFieldName.IsNotNullOrWhiteSpace()
                    ? row[seriesFieldName].ToStringOrDefault( string.Empty ).Trim()
                    : DefaultSeriesName;

                if ( !valuesBySeries.TryGetValue( seriesName, out var seriesValues ) )
                {
                    seriesValues = new Dictionary<string, decimal>();
                    valuesBySeries.Add( seriesName, seriesValues );
                    seriesNames.Add( seriesName );
                }

                var category = row[categoryFieldName].ToStringOrDefault( string.Empty ).Trim();

                if ( !labels.Contains( category ) )
                {
                    labels.Add( category );
                }

                var value = row[valueFieldName].ToStringOrDefault( "0" ).AsDecimal();

                seriesValues.TryGetValue( category, out var existingValue );
                seriesValues[category] = existingValue + value;
            }

            options.Labels = labels;
            options.Series = seriesNames
                .Select( name => new DynamicChartSeriesBag
                {
                    Name = name,
                    Values = labels
                        .Select( category => valuesBySeries[name].TryGetValue( category, out var value ) ? value : ( decimal? ) null )
                        .ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the first of the candidate field names that exists as a column
        /// in the data table.
        /// </summary>
        /// <param name="dataTable">The data table to inspect.</param>
        /// <param name="fieldNames">The candidate field names, in order of preference.</param>
        /// <returns>The first matching field name, or null if none match.</returns>
        private string GetFirstMatchedFieldName( DataTable dataTable, List<string> fieldNames )
        {
            return fieldNames.FirstOrDefault( fieldName => dataTable.Columns.Contains( fieldName ) );
        }

        /// <summary>
        /// Gets the merge fields available to the SQL query.
        /// </summary>
        /// <returns>The merge fields.</returns>
        private Dictionary<string, object> GetMergeFields()
        {
            var mergeFields = this.RequestContext.GetCommonMergeFields();

            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
            mergeFields.AddOrReplace( "CurrentPage", this.PageCache );

            return mergeFields;
        }

        /// <summary>
        /// Gets the SQL parameters from the configured query parameters, with
        /// page parameter values and the current person's ID substituted where
        /// applicable.
        /// </summary>
        /// <param name="queryParams">The configured query parameters in "name=value" format.</param>
        /// <returns>The SQL parameters, or null if there are none.</returns>
        private Dictionary<string, object> GetSqlParameters( string[] queryParams )
        {
            if ( queryParams.Length == 0 )
            {
                return null;
            }

            var parameters = new Dictionary<string, object>();

            foreach ( var queryParam in queryParams )
            {
                var paramParts = queryParam.Split( '=' );

                if ( paramParts.Length != 2 )
                {
                    continue;
                }

                var queryParamName = paramParts[0];
                var queryParamValue = paramParts[1];

                // Remove the leading '@' character if it was included.
                if ( queryParamName.StartsWith( "@" ) )
                {
                    queryParamName = queryParamName.Substring( 1 );
                }

                // If a page parameter (query or form) value matches, use its value instead.
                var pageValue = PageParameter( queryParamName );
                if ( pageValue.IsNotNullOrWhiteSpace() )
                {
                    queryParamValue = pageValue;
                }
                else if ( queryParamName.ToLower() == "currentpersonid" && this.RequestContext.CurrentPerson != null )
                {
                    queryParamValue = this.RequestContext.CurrentPerson.Id.ToString();
                }

                parameters.Add( queryParamName, queryParamValue );
            }

            return parameters;
        }

        #endregion Methods
    }
}
