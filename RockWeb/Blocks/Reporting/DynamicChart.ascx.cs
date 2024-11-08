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

using Rock;
using Rock.Attribute;
using Rock.Chart;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Dynamic Chart" )]
    [Category( "Reporting" )]
    [Description( "Block to display a chart using SQL as the chart datasource" )]
    [IntegerField( "Chart Height",
        IsRequired = false,
        DefaultIntegerValue = 200 )]
    [TextField( "Query Params",
        Description = "The parameters that the stored procedure expects in the format of 'param1=value;param2=value'. Any parameter with the same name as a page parameter (i.e. querystring, form, or page route) will have its value replaced with the page's current value. A parameter with the name of 'CurrentPersonId' will have its value replaced with the currently logged in person's id.",
        IsRequired = false )]
    [CodeEditorField( "SQL",
        Description = "See the code example in the default text of the block.",
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
        EditorMode = CodeEditorMode.Sql )]
    [TextField( "Title",
        Description = "The title of the widget",
        IsRequired = false,
        Order = 0 )]
    [TextField( "Subtitle",
        Description = "The subtitle of the widget",
        IsRequired = false,
        Order = 1 )]
    [CustomDropdownListField( "Column Width",
        Description = "The width of the widget.",
        ListSource = ",1,2,3,4,5,6,7,8,9,10,11,12",
        IsRequired = false,
        DefaultValue = "4",
        Order = 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES,
        Name = "Chart Style",
        Order = 3 )]
    [BooleanField( "Show Legend",
        Key = "ShowLegend",
        DefaultBooleanValue = true,
        Order = 7 )]
    [CustomDropdownListField( "Legend Position",
        Description = "Select the position of the Legend (corner)",
        ListSource = "n,ne,e,se,s,sw,w,nw",
        IsRequired = false,
        DefaultValue = "ne",
        Order = 8 )]
    [CustomDropdownListField( "Chart Type",
        ListSource = "Line,Bar,Pie",
        IsRequired = false,
        DefaultValue = "Line",
        Order = 9 )]
    [DecimalField( "Pie Inner Radius",
        Description = "If this is a pie chart, specific the inner radius to have a donut hole. For example, specify: 0.75 to have the inner radius as 75% of the outer radius.",
        IsRequired = false,
        DefaultDecimalValue = 0,
        Order = 10 )]
    [BooleanField( "Pie Show Labels",
        Key = "PieShowLabels",
        Description = "If this is a pie chart, specify if labels show be shown",
        DefaultBooleanValue = true,
        Order = 11 )]
    [Rock.SystemGuid.BlockTypeGuid( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723" )]
    public partial class DynamicChart : RockBlock
    {
        #region Fields

        private string _chartType = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Title attribute value
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                return GetAttributeValue( "Title" );
            }
        }

        /// <summary>
        /// Gets the Subtitle attribute value
        /// </summary>
        /// <value>
        /// The subtitle.
        /// </value>
        public string Subtitle
        {
            get
            {
                return GetAttributeValue( "Subtitle" );
            }
        }

        /// <summary>
        /// Gets the Column Width attribute value
        /// This will be a value from 1-12 (or null) that represents the col-md- width of this control.
        /// </summary>
        /// <value>
        /// The width of the column.
        /// </value>
        public int? ColumnWidth
        {
            get
            {
                return GetAttributeValue( "ColumnWidth" ).AsIntegerOrNull();
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            var pageReference = new Rock.Web.PageReference( this.PageCache.Id );
            pageReference.QueryString = new System.Collections.Specialized.NameValueCollection();

            // Add parameters from the URL route and query string.
            pageReference.QueryString.Add( this.CurrentPageReference.QueryString );
            pageReference.QueryString.Add( this.CurrentPageReference.Parameters.ToNameValueCollection() );

            lcLineChart.Visible = false;
            bcBarChart.Visible = false;
            pcPieChart.Visible = false;

            SetBlockConfigurationError( null );

            // Check for valid block configuration.
            var sql = this.GetAttributeValue( "SQL" );

            if ( string.IsNullOrWhiteSpace( sql ) )
            {
                SetBlockConfigurationError( "[Dynamic Chart]: SQL needs to be configured in block settings." );
                return;
            }

            ChartStyle chartStyle;
            var definedValue = DefinedValueCache.Get( this.GetAttributeValue( "ChartStyle" ).AsGuid() );
            if ( definedValue != null )
            {
                chartStyle = ChartStyle.CreateFromJson( definedValue.Value, definedValue.GetAttributeValue( "ChartStyle" ) );
            }
            else
            {
                chartStyle = new ChartStyle();
            }

            var chartHeight = GetAttributeValue( "ChartHeight" ).AsIntegerOrNull() ?? 200;
            var showLegend = GetAttributeValue( "ShowLegend" ).AsBooleanOrNull() ?? true;
            var legendPosition = GetAttributeValue( "LegendPosition" );

            // Override the style settings with the block settings.
            if ( chartStyle.Legend == null )
            {
                chartStyle.Legend = new LegendStyle();
            }
            chartStyle.Legend.Show = showLegend;
            chartStyle.Legend.Position = legendPosition;

            lcLineChart.ChartHeight = chartHeight;
            lcLineChart.ShowLegend = showLegend;
            lcLineChart.LegendPosition = legendPosition;
            // Set chart style after setting options so they are not overwritten.
            lcLineChart.SetChartStyle( chartStyle );

            bcBarChart.ChartHeight = chartHeight;
            bcBarChart.BarWidth = 0.6;
            bcBarChart.ShowLegend = showLegend;
            bcBarChart.LegendPosition = legendPosition;
            bcBarChart.SetChartStyle( chartStyle );

            pcPieChart.ChartHeight = chartHeight;
            pcPieChart.ShowLegend = showLegend;
            pcPieChart.ShowSegmentLabels = GetAttributeValue( "ShowLegend" ).AsBooleanOrNull() ?? false;
            pcPieChart.InnerRadius = this.GetAttributeValue( "PieInnerRadius" ).AsDoubleOrNull();
            pcPieChart.SetChartStyle( chartStyle );

            _chartType = this.GetAttributeValue( "ChartType" ).Trim().ToLower();

            pnlDashboardTitle.Visible = !string.IsNullOrEmpty( this.Title );
            pnlDashboardSubtitle.Visible = !string.IsNullOrEmpty( this.Subtitle );
            lDashboardTitle.Text = this.Title;
            lDashboardSubtitle.Text = this.Subtitle;
        }

        protected override void OnLoad( EventArgs e )
        {
            lcLineChart.Visible = false;
            bcBarChart.Visible = false;
            pcPieChart.Visible = false;

            // If the block has a configuration error, do not attempt to load the data.
            if ( nbConfigurationWarning.Visible )
            {
                base.OnLoad( e );
                return;
            }

            var sql = this.GetAttributeValue( "SQL" );
            DataTable dataTable = null;
            try
            {
                dataTable = GetSqlResultDataTable( sql );
            }
            catch
            {
                SetBlockConfigurationError( "[Dynamic Chart]: The data could not be retrieved." );
            }

            if ( dataTable == null )
            {
                base.OnLoad( e );
                return;
            }

            var rows = dataTable.Rows.OfType<DataRow>();
            var sampleRow = rows.FirstOrDefault();
            if ( sampleRow == null )
            {
                base.OnLoad( e );
                return;
            }

            /* Get the required fields from the dataset. */

            // A Series represents a set of related data points that are displayed on the chart.
            // A line or bar chart can display multiple series at the same time.
            var seriesFieldName = GetFirstMatchedFieldName( sampleRow, new List<string> { "SeriesName", "SeriesID" } );
            var categoryFieldName = GetFirstMatchedFieldName( sampleRow, new List<string> { "Category", "MetricTitle" } );
            var yValueFieldName = GetFirstMatchedFieldName( sampleRow, new List<string> { "Value", "YValue", "YValueTotal" } );
            var xValueFieldName = GetFirstMatchedFieldName( sampleRow, new List<string> { "XValue", "XValueTotal" } );
            var dateTimeFieldName = GetFirstMatchedFieldName( sampleRow, new List<string> { "DateTimeValue", "DateTime" } );

            if ( _chartType == "pie" )
            {
                // The Pie Chart data set requires the following columns:
                // MetricTitle (string), YValueTotal (numeric).
                // It can only be used to plot category data, not a time series.
                if ( categoryFieldName == null )
                {
                    if ( !string.IsNullOrWhiteSpace( seriesFieldName ) )
                    {
                        // Assume that each series is intended to be plotted as a pie category.
                        categoryFieldName = seriesFieldName;
                        seriesFieldName = null;
                    }
                    else if ( !string.IsNullOrWhiteSpace( xValueFieldName ) )
                    {
                        // Assume that the XValue is intended to be plotted as a pie category.
                        categoryFieldName = xValueFieldName;
                        xValueFieldName = null;
                    }
                    else
                    {
                        SetBlockConfigurationError( "[Dynamic Chart]: Pie Chart dataset must contain a category field: [Category] or [MetricTitle]" );
                    }
                }
                if ( yValueFieldName == null )
                {
                    SetBlockConfigurationError( "[Dynamic Chart]: Pie Chart dataset must contain a value field: [YValue] or [YValueTotal]" );
                }
            }
            else if ( _chartType == "bar" )
            {
                // The Bar Chart data set requires the following columns:
                // SeriesName (string), YValue (numeric).
                // It can only be used to plot category data, not a time series.
                if ( categoryFieldName == null )
                {
                    if ( !string.IsNullOrWhiteSpace( xValueFieldName ) )
                    {
                        // Assume that the X-axis values represent the bar categories.
                        categoryFieldName = xValueFieldName;
                        xValueFieldName = null;
                    }
                    else if ( !string.IsNullOrWhiteSpace( seriesFieldName ) )
                    {
                        // Assume that each series is intended to be plotted as a bar category.
                        categoryFieldName = seriesFieldName;
                        seriesFieldName = null;
                    }
                    else
                    {
                        SetBlockConfigurationError( "[Dynamic Chart]: Bar Chart dataset must contain a category field: [Category] or [MetricTitle]" );
                    }
                }
                if ( yValueFieldName == null )
                {
                    SetBlockConfigurationError( "[Dynamic Chart]: Bar Chart dataset must contain a value field: [Value], [YValue] or [DateTime]" );
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
                    SetBlockConfigurationError( "[Dynamic Chart]: Line Chart dataset must contain an X-value or datetime field: [XValue] or [DateTime]" );
                }
                if ( yValueFieldName == null )
                {
                    SetBlockConfigurationError( "[Dynamic Chart]: Line Chart dataset must contain a Y-value field: [Value] or [YValue]" );
                }
            }

            // If the block has a configuration error, do not attempt to load the data.
            if ( nbConfigurationWarning.Visible )
            {
                base.OnLoad( e );
                return;
            }

            ChartJsChart chartControl;
            bool createTimeSeries;
            if ( _chartType == "pie" )
            {
                chartControl = pcPieChart;
                createTimeSeries = false;
            }
            else if ( _chartType == "bar" )
            {
                chartControl = bcBarChart;
                createTimeSeries = false;
            }
            else
            {
                chartControl = lcLineChart;
                createTimeSeries = ( dateTimeFieldName != null );
            }

            // Create the chart data.
            if ( createTimeSeries )
            {
                // If a datetime field exists, create a time series.
                var data1 = GetChartDataForTimeSeries( dataTable, seriesFieldName, dateTimeFieldName, yValueFieldName );
                chartControl.SetChartDataItems( data1 );
            }
            else
            {
                // If a category field name is not specified, use the XValue.
                if ( string.IsNullOrWhiteSpace( categoryFieldName ) )
                {
                    categoryFieldName = xValueFieldName;
                }
                var categoryData = GetChartDataForCategorySeries( dataTable, seriesFieldName, categoryFieldName, yValueFieldName );
                chartControl.SetChartDataItems( categoryData );
            }

            chartControl.Visible = true;

            base.OnLoad( e );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( System.Web.UI.HtmlTextWriter writer )
        {
            // Create the containing panels and embed the chart content.
            var cssList = GetDivWidthCssClasses();
            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, cssList.AsDelimited( " " ) );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );

            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, "panel" );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );

            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, "panel-body" );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );

            base.RenderControl( writer );

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        #endregion

        /// <summary>
        /// Gets the div width CSS classes.
        /// </summary>
        /// <returns></returns>
        private List<string> GetDivWidthCssClasses()
        {
            int? mediumColumnWidth = this.GetAttributeValue( "ColumnWidth" ).AsIntegerOrNull();

            // add additional css to the block wrapper (if mediumColumnWidth is specified)
            List<string> widgetCssList = new List<string>();
            if ( mediumColumnWidth.HasValue )
            {
                // Table to use to derive col-xs and col-sm from the selected medium width
                /*
                XS	SM	MD
                4	2	1
                6	4	2
                6	4	3
                    6	4
            	        5
            	        6
            	        7
            	        8
            	        9
            	        10
            	        11
            	        12 */

                int? xsmallColumnWidth;
                int? smallColumnWidth;

                // logic to set reasonable col-xs- and col-sm- classes from the selected mediumColumnWidth (col-md-X)
                switch ( mediumColumnWidth.Value )
                {
                    case 1:
                        xsmallColumnWidth = 4;
                        smallColumnWidth = 2;
                        break;
                    case 2:
                    case 3:
                        xsmallColumnWidth = 6;
                        smallColumnWidth = 4;
                        break;
                    case 4:
                        xsmallColumnWidth = null;
                        smallColumnWidth = 6;
                        break;
                    default:
                        xsmallColumnWidth = null;
                        smallColumnWidth = null;
                        break;
                }

                widgetCssList.Add( string.Format( "col-md-{0}", mediumColumnWidth ) );
                if ( xsmallColumnWidth.HasValue )
                {
                    widgetCssList.Add( string.Format( "col-xs-{0}", xsmallColumnWidth ) );
                }

                if ( smallColumnWidth.HasValue )
                {
                    widgetCssList.Add( string.Format( "col-sm-{0}", smallColumnWidth ) );
                }
            }

            return widgetCssList;
        }

        private void SetBlockConfigurationError( string text )
        {
            if ( text == null )
            {
                nbConfigurationWarning.Text = "";
                nbConfigurationWarning.Visible = false;
            }
            else
            {
                nbConfigurationWarning.Text = text;
                nbConfigurationWarning.Visible = true;
            }
        }

        private DataTable GetSqlResultDataTable( string sql )
        {
            if ( string.IsNullOrWhiteSpace( sql ) )
            {
                return null;
            }

            var mergeFields = GetDynamicDataMergeFields();
            sql = sql.ResolveMergeFields( mergeFields );

            var parameters = GetParameters();
            var dataSet = DbService.GetDataSet( sql, System.Data.CommandType.Text, parameters );
            var dataTable = dataSet.Tables[0];

            return dataTable;
        }

        /// <summary>
        /// Gets the data set for a chart that represents a series of values by category.
        /// </summary>
        /// <returns></returns>
        private List<ChartJsCategorySeriesDataset> GetChartDataForCategorySeries( DataTable dataTable, string seriesFieldName, string categoryFieldName, string valueFieldName )
        {
            var rows = dataTable.Rows.OfType<DataRow>();

            var hasSeries = !string.IsNullOrEmpty( seriesFieldName );
            var datasetsMap = new Dictionary<string, ChartJsCategorySeriesDataset>( StringComparer.OrdinalIgnoreCase );
            ChartJsCategorySeriesDataset seriesDataset = null;

            if ( !hasSeries )
            {
                // Add the default series.
                seriesDataset = new ChartJsCategorySeriesDataset();
                seriesDataset.Name = "Series 1";
                seriesDataset.DataPoints = new List<IChartJsCategorySeriesDataPoint>();
                datasetsMap.Add( string.Empty, seriesDataset );
            }

            foreach ( var row in rows )
            {
                if ( hasSeries )
                {
                    var seriesName = row[seriesFieldName].ToStringOrDefault("").Trim();
                    if ( datasetsMap.ContainsKey( seriesName ) )
                    {
                        seriesDataset = datasetsMap[seriesName];
                    }
                    else
                    {
                        seriesDataset = new ChartJsCategorySeriesDataset();
                        seriesDataset.Name = seriesName;
                        seriesDataset.DataPoints = new List<IChartJsCategorySeriesDataPoint>();
                        datasetsMap.Add( seriesName, seriesDataset );
                    }
                }

                var dataPoint = new ChartJsCategorySeriesDataPoint();

                dataPoint.Category = row[categoryFieldName].ToStringOrDefault("").Trim();
                dataPoint.Value = row[valueFieldName].ToStringOrDefault("0").AsDecimal();

                seriesDataset.DataPoints.Add( dataPoint );
            }

            var datasets = datasetsMap.Values.ToList();

            return datasets;
        }

        private string GetFirstMatchedFieldName( DataRow sampleRow, List<string> fieldNames )
        {
            foreach ( var fieldName in fieldNames )
            {
                if ( sampleRow.Table.Columns.Contains( fieldName ) )
                {
                    return fieldName;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the data set for a chart that represents a series of value over time data points.
        /// </summary>
        /// <returns></returns>
        private List<ChartJsTimeSeriesDataset> GetChartDataForTimeSeries( DataTable dataTable, string seriesFieldName, string datetimeFieldName, string valueFieldName )
        {
            // SeriesName (string), DateTime (datetime), YValue (numeric).
            var datasets = new List<ChartJsTimeSeriesDataset>();
            var rows = dataTable.Rows.OfType<DataRow>();
            var chartDataPoints = new List<ChartJsTimeSeriesDataPoint>();
            var hasSeries = !string.IsNullOrEmpty( seriesFieldName );
            var datasetsMap = new Dictionary<string, ChartJsTimeSeriesDataset>( StringComparer.OrdinalIgnoreCase );
            ChartJsTimeSeriesDataset seriesDataset = null;

            if ( !hasSeries )
            {
                // Add the default series.
                seriesDataset = new ChartJsTimeSeriesDataset();
                seriesDataset.Name = "Series 1";
                seriesDataset.DataPoints = new List<IChartJsTimeSeriesDataPoint>();
                datasetsMap.Add( string.Empty, seriesDataset );
            }

            foreach ( var row in rows )
            {
                if ( hasSeries )
                {
                    var seriesName = row[seriesFieldName].ToStringOrDefault("").Trim();
                    if ( datasetsMap.ContainsKey( seriesName ) )
                    {
                        seriesDataset = datasetsMap[seriesName];
                    }
                    else
                    {
                        seriesDataset = new ChartJsTimeSeriesDataset();
                        seriesDataset.Name = seriesName;
                        seriesDataset.DataPoints = new List<IChartJsTimeSeriesDataPoint>();
                        datasetsMap.Add( seriesName, seriesDataset );
                    }
                }

                var dataPoint = new ChartJsTimeSeriesDataPoint();
                if ( datetimeFieldName == "DateTime" )
                {
                    var dateTimeValue = row["DateTime"].ToStringOrDefault( "" ).AsDateTime();
                    if ( dateTimeValue != null )
                    {
                        dataPoint.DateTimeStamp = dateTimeValue.Value.ToJavascriptMilliseconds();
                    }
                }
                else if ( datetimeFieldName == "XValue" )
                {
                    // Try to convert the field to a datestamp.
                    dataPoint.DateTimeStamp = row["XValue"].ToStringOrDefault( "0" ).AsInteger();
                }

                dataPoint.Value = row[valueFieldName].ToStringOrDefault("0").AsDecimal();

                seriesDataset.DataPoints.Add( dataPoint );
            }

            // Sort the datasets
            datasets = datasetsMap.Values.ToList();
            foreach ( var dataset in datasets )
            {
                dataset.DataPoints = dataset.DataPoints
                    .OrderBy( a => a.DateTime )
                    .ToList();
            }

            return datasets;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // reload the full page since controls are dynamically created based on block settings
            NavigateToPage( this.CurrentPageReference );
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetParameters()
        {
            string[] queryParams = GetAttributeValue( "QueryParams" ).SplitDelimitedValues();
            if ( queryParams.Length > 0 )
            {
                var parameters = new Dictionary<string, object>();
                foreach ( string queryParam in queryParams )
                {
                    string[] paramParts = queryParam.Split( '=' );
                    if ( paramParts.Length == 2 )
                    {
                        string queryParamName = paramParts[0];
                        string queryParamValue = paramParts[1];

                        // Remove leading '@' character if was included
                        if ( queryParamName.StartsWith( "@" ) )
                        {
                            queryParamName = queryParamName.Substring( 1 );
                        }

                        // If a page parameter (query or form) value matches, use it's value instead
                        string pageValue = PageParameter( queryParamName );
                        if ( !string.IsNullOrWhiteSpace( pageValue ) )
                        {
                            queryParamValue = pageValue;
                        }
                        else if ( queryParamName.ToLower() == "currentpersonid" && CurrentPerson != null )
                        {
                            // If current person id, use the value of the current person id
                            queryParamValue = CurrentPerson.Id.ToString();
                        }

                        parameters.Add( queryParamName, queryParamValue );
                    }
                }

                return parameters;
            }

            return null;
        }

        /// <summary>
        /// Gets the dynamic data merge fields.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDynamicDataMergeFields()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
            mergeFields.AddOrReplace( "CurrentPage", this.PageCache );

            return mergeFields;
        }
    }
}