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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Chart;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Adapter class that provides the foundation for a ChartJS implementation of a Rock chart control.
    /// </summary>
    public abstract class ChartJsChart : CompositeControl, IRockChart
    {
        #region Controls

        private HiddenFieldWithClass _hfChartData;
        private HiddenFieldWithClass _hfRestUrl;
        private HiddenFieldWithClass _hfRestUrlParams;
        private HiddenFieldWithClass _hfSeriesPartitionNameUrl;
        private HiddenFieldWithClass _hfXAxisLabel;
        private HiddenFieldWithClass _hfYAxisLabel;
        private Label _lblChartTitle;
        private Label _lblChartSubtitle;
        private Panel _pnlChartPlaceholder;
        private HelpBlock _hbChartOptions;
        private NotificationBox _nbRenderNotification;
            
        // if this chart is used for a metric
        //private HiddenField _hfMetricId;

        /// <summary>
        /// Gets the container control for the chart.
        /// </summary>
        protected Panel ChartContainerControl
        {
            get
            {
                return _pnlChartPlaceholder;
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                EnsureChildControls();
                return _lblChartTitle.Text;
            }

            set
            {
                EnsureChildControls();
                _lblChartTitle.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the subtitle.
        /// </summary>
        /// <value>
        /// The subtitle.
        /// </value>
        public string Subtitle
        {
            get
            {
                EnsureChildControls();
                return _lblChartSubtitle.Text;
            }

            set
            {
                EnsureChildControls();
                _lblChartSubtitle.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the x axis label.
        /// </summary>
        /// <value>
        /// The x axis label.
        /// </value>
        public string XAxisLabel
        {
            get
            {
                EnsureChildControls();
                return _hfXAxisLabel.Value;
            }

            set
            {
                EnsureChildControls();
                _hfXAxisLabel.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the y axis label.
        /// </summary>
        /// <value>
        /// The y axis label.
        /// </value>
        public string YAxisLabel
        {
            get
            {
                EnsureChildControls();
                return _hfYAxisLabel.Value;
            }

            set
            {
                EnsureChildControls();
                _hfYAxisLabel.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the chart.
        /// </summary>
        /// <value>
        /// The height of the chart.
        /// </value>
        public Unit ChartHeight
        {
            get
            {
                return ViewState["ChartHeight"] as Unit? ?? new Unit( 170 );
            }

            set
            {
                ViewState["ChartHeight"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of the chart.
        /// </summary>
        /// <value>
        /// The width of the chart.
        /// </value>
        public Unit ChartWidth
        {
            get
            {
                return ViewState["ChartWidth"] as Unit? ?? new Unit( "100%" );
            }

            set
            {
                ViewState["ChartWidth"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the unit of measure for the Y-value (dependent value) in a series.
        /// </summary>
        /// <value>
        /// The unit of measure, specified as "unspecified|numeric|currency|percentage".
        /// </value>
        public string YValueFormatString
        {
            get
            {
                return ViewState["YValueUom"] as string ?? "";
            }

            set
            {
                ViewState["YValueUom"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show tooltip].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show tooltip]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowTooltip
        {
            get
            {
                return ViewState["ShowTooltip"] as bool? ?? true;
            }

            set
            {
                ViewState["ShowTooltip"] = value;
            }
        }

        /// <summary>
        /// Javascript that will format the tooltip.
        /// </summary>
        /// <example>
        /// <code>
        /// function(item) {
        ///     var dateText = new Date(item.series.chartData[item.dataIndex].DateTimeStamp).toLocaleDateString();
        ///     var seriesLabel = item.series.label;
        ///     var pointValue = item.series.chartData[item.dataIndex].YValue || item.series.chartData[item.dataIndex].YValueTotal;
        ///     return dateText + '<br />' + seriesLabel + ': ' + pointValue;
        /// }
        /// </code>
        /// </example>
        /// <value>
        /// The tooltip formatter.
        /// </value>
        public string TooltipContentScript
        {
            get
            {
                return ViewState["TooltipContentScript"] as string;
            }

            set
            {
                ViewState["TooltipContentScript"] = value;
            }
        }

        /// <summary>
        /// Javascript that will format the tooltip.
        /// </summary>
        /// <example>
        /// <code>
        /// function (event, pos, item) {
        ///     var activePoints = _chart.getElementsAtEvent(event);
        ///     var chartData = activePoints[0]['_chart'].config.data;
        ///     var dataset = chartData.datasets[activePoints[0]['_datasetIndex']];
        ///     var dataItem = dataset.data[activePoints[0]['_index']];
        ///     var customData = dataItem.customData;
        ///     if (dataItem) {
        ///         postbackArg = 'SeriesId=' + customData.SeriesName
        ///             + ';DateStamp=' + customData.DateTimeStamp
        ///             + ';YValue=' + ( customData.hasOwnProperty('YValue') ? customData.YValue : customData.Value );
        ///     }
        ///     else
        ///     {
        ///         // no point was clicked
        ///         postbackArg =  'DateStamp=;YValue=;SeriesId=';
        ///     }
        ///     window.location = ""javascript:__doPostBack('{_pnlChartPlaceholder.UniqueID}', '"" +  postbackArg + ""')"";
        /// } );
        /// </code>
        /// </example>
        /// <value>
        /// The tooltip formatter.
        /// </value>
        /// <remarks>
        /// The customData variable in this script refers to a chart data item which must conform to the IChartData interface.
        /// </remarks>
        public string ChartClickScript { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show debug].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show debug]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDebug
        {
            get
            {
                return ViewState["ShowDebug"] as bool? ?? false;
            }

            set
            {
                ViewState["ShowDebug"] = value;
            }
        }

        /// <summary>
        /// The unit of measure in which the Series data is expressed.
        /// If the data is a time series, use: {day|month|year|time}
        /// where "time" specifies thatthe appropriate interval should be automatically determined from the dataset.
        /// If the data is a value series, this property is ignored.
        /// </summary>
        public string SeriesGroupIntervalType
        {
            get
            {
                return ViewState["SeriesGroupIntervalType"] as string;
            }

            set
            {
                ViewState["SeriesGroupIntervalType"] = value;
            }
        }

        /// <summary>
        /// The size of intervals into which the series data is grouped.
        /// If the data is a time series, this value represents the number of time intervals in each tick.
        /// If the data is a value series, this property is ignored.
        /// </summary>
        public string SeriesGroupIntervalSize
        {
            get
            {
                return ViewState["SeriesGroupIntervalSize"] as string;
            }

            set
            {
                ViewState["SeriesGroupIntervalSize"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the position.
        /// "ne" or "nw" or "se" or "sw"
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public string LegendPosition { get; set; }

        /// <summary>
        /// Gets the corresponding ChartJs legend location settings for the current Rock Chart settings.
        /// </summary>
        /// <param name="legendPosition"></param>
        /// <param name="legendAlignment"></param>
        protected void GetChartJsLegendLocationSettings( out string legendPosition, out string legendAlignment )
        {
            var rockLegendPosition = this.LegendPosition?.ToLower() ?? string.Empty;

            if ( rockLegendPosition == "n" )
            {
                legendPosition = "top";
                legendAlignment = "center";
            }
            else if ( rockLegendPosition == "ne" )
            {
                legendPosition = "top";
                legendAlignment = "end";
            }
            else if ( rockLegendPosition == "e" )
            {
                legendPosition = "right";
                legendAlignment = "end";
            }
            else if ( rockLegendPosition == "se" )
            {
                legendPosition = "bottom";
                legendAlignment = "end";
            }
            else if ( rockLegendPosition == "s" )
            {
                legendPosition = "bottom";
                legendAlignment = "center";
            }
            else if ( rockLegendPosition == "sw" )
            {
                legendPosition = "bottom";
                legendAlignment = "start";
            }
            else if ( rockLegendPosition ==  "w" )
            {
                legendPosition = "left";
                legendAlignment = "start";
            }
            else if ( rockLegendPosition == "nw" )
            {
                legendPosition = "top";
                legendAlignment = "start";
            }
            else
            {
                // Default to bottom-centre.
                legendPosition = "bottom";
                legendAlignment = "center";
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating if a legend is displayed on the chart.
        /// </summary>
        public bool ShowLegend { get; set; }

        /// <summary>
        /// Gets or sets the style settings applied to the chart title. 
        /// </summary>
        public ChartTextElementStyle TitleFormat { get; set; }

        /// <summary>
        /// Gets or sets the style settings applied to the chart subtitle. 
        /// </summary>
        public ChartTextElementStyle SubtitleFormat { get; set; }

        /// <inheritdoc/>
        public bool MaintainAspectRatio { get; set; }

        /// <summary>
        /// Override this method to generate the JSON model used as the data source for the chart.
        /// This object represents the "data" element of the ChartJs configuration model.
        /// For more information about the ChartJs data structure, refer to the documentation:
        /// https://www.chartjs.org/docs/latest/general/data-structures.html
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="defaultSeriesName"></param>
        /// <returns></returns>
        protected abstract string OnGenerateChartJson( object sourceData, string defaultSeriesName = null );

        private object _chartDataSource = null;
        private string _defaultSeriesName = null;

        /// <summary>
        /// Set the data items to be displayed by the chart, and generate the ChartData used to construct the ChartJS chart. 
        /// </summary>
        /// <param name="chartDataItems"></param>
        public void SetChartDataItems( IEnumerable<IChartData> chartDataItems )
        {
            this.SetChartDataItems( chartDataItems, null );
        }

        /// <summary>
        /// Set the data items to be displayed by the chart, and generate the ChartData used to construct the ChartJS chart. 
        /// </summary>
        /// <param name="chartDataItems"></param>
        /// <param name="defaultSeriesName"></param>
        public void SetChartDataItems( IEnumerable<IChartData> chartDataItems, string defaultSeriesName )
        {
            _chartDataSource = chartDataItems;
            _defaultSeriesName = defaultSeriesName;

            // Reset the chart data.
            _hfChartData.Value = null;
        }

        /// <summary>
        /// Gets or sets the datasets that will be displayed in the chart.
        /// </summary>
        public void SetChartDataItems( List<ChartJsTimeSeriesDataset> datasets )
        {
            _chartDataSource = datasets;
            _hfChartData.Value = null;
        }

        /// <summary>
        /// Gets or sets the datasets that will be displayed in the chart.
        /// </summary>
        public void SetChartDataItems( List<ChartJsCategorySeriesDataset> datasets )
        {
            _chartDataSource = datasets;
            _hfChartData.Value = null;
        }

        /// <summary>
        /// Gets or sets the dataset that will be displayed in the chart.
        /// </summary>
        public void SetChartDataItems( ChartJsCategorySeriesDataset dataset )
        {
            _chartDataSource = dataset;
            _hfChartData.Value = null;
        }

        #region Events

        /// <summary>
        /// Occurs when the chart is clicked.
        /// </summary>
        public event EventHandler<ChartClickArgs> ChartClick;

        #endregion

        #region Control overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.ChartClickScript = @"function (event, pos, item) {{
    var activePoints = _chart.getElementsAtEvent(event);
    var chartData = activePoints[0]['_chart'].config.data;
    var dataset = chartData.datasets[activePoints[0]['_datasetIndex']];
    var dataItem = dataset.data[activePoints[0]['_index']];
    var customData = dataItem.customData;
    if (dataItem) {{
        postbackArg = 'SeriesId=' + customData.SeriesName
            + ';DateStamp=' + customData.DateTimeStamp
            + ';YValue=' + ( customData.hasOwnProperty('YValue') ? customData.YValue : customData.Value );
    }}
    else
    {{
        // no point was clicked
        postbackArg =  'DateStamp=;YValue=;SeriesId=';
    }}
    window.location = ""javascript:__doPostBack('{_pnlChartPlaceholder.UniqueID}', '"" +  postbackArg + ""')"";
}}";

            RockPage.AddScriptLink( this.Page, "~/Scripts/moment.min.js" );
            RockPage.AddScriptLink( this.Page, "~/Scripts/Chartjs/Chart.min.js" );
            RockPage.AddScriptLink( this.Page, "~/Scripts/Chartjs/Chart.plugin.datalabels.min.js" );

            EnsureChildControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( this.Page.Request.Params["GetChartData"] != null )
            {
                GetChartData();
            }

            if ( this.Page.IsPostBack )
            {
                EnsureChildControls();

                if ( this.Page.Request.Params["__EVENTTARGET"] == _pnlChartPlaceholder.UniqueID )
                {
                    HandleChartClick();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            // Generate the chart script here, because we now have access to the ChartData if it has been loaded from ViewState.
            string chartData;
            try
            {
                chartData = GetChartDataJson();
            }
            catch (Exception ex)
            {
                SetRenderError( ex.Message );
                chartData = "[]";
            }

            RegisterJavaScript( chartData );
        }

        private void SetRenderError( string message )
        {
            _nbRenderNotification.Text = message;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _hfChartData = new HiddenFieldWithClass();
            _hfChartData.ID = string.Format( "hfChartData_{0}", this.ID );
            _hfChartData.CssClass = "js-chart-data";

            //_hfMetricId = new HiddenField();
            //_hfMetricId.ID = string.Format( "hfMetricId_{0}", this.ID );

            _hfXAxisLabel = new HiddenFieldWithClass();
            _hfXAxisLabel.ID = string.Format( "hfXAxisLabel_{0}", this.ID );
            _hfXAxisLabel.CssClass = "js-xaxis-value";

            _hfYAxisLabel = new HiddenFieldWithClass();
            _hfYAxisLabel.ID = string.Format( "hfYAxisLabel_{0}", this.ID );
            _hfYAxisLabel.CssClass = "js-yaxis-value";

            _hfRestUrlParams = new HiddenFieldWithClass();
            _hfRestUrlParams.ID = string.Format( "hfRestUrlParams_{0}", this.ID );
            _hfRestUrlParams.CssClass = "js-rest-url-params";

            _hfRestUrl = new HiddenFieldWithClass();
            _hfRestUrl.ID = string.Format( "hfRestUrl_{0}", this.ID );
            _hfRestUrl.CssClass = "js-rest-url";

            _hfSeriesPartitionNameUrl = new HiddenFieldWithClass();
            _hfSeriesPartitionNameUrl.ID = string.Format( "hfSeriesPartitionNameUrl_{0}", this.ID );
            _hfSeriesPartitionNameUrl.CssClass = "js-seriesname-url";

            _lblChartTitle = new Label();
            _lblChartTitle.ID = string.Format( "lblChartTitle_{0}", this.ID );

            _lblChartSubtitle = new Label();
            _lblChartSubtitle.ID = string.Format( "lblChartSubtitle_{0}", this.ID );

            _nbRenderNotification = new NotificationBox();
            _nbRenderNotification.ID = "nbRenderNotification";
            _nbRenderNotification.NotificationBoxType = NotificationBoxType.Danger;

            _pnlChartPlaceholder = new Panel();
            _pnlChartPlaceholder.CssClass = "chart-placeholder js-chart-placeholder";
            _pnlChartPlaceholder.ID = string.Format( "pnlChartPlaceholder_{0}", this.ID );

            _hbChartOptions = new HelpBlock();
            _hbChartOptions.ID = string.Format( "hbChartOptions_{0}", this.ID );

            Controls.Add( _hfChartData );
            Controls.Add( _hfXAxisLabel );
            Controls.Add( _hfYAxisLabel );
            Controls.Add( _hfRestUrlParams );
            Controls.Add( _hfRestUrl );
            Controls.Add( _hfSeriesPartitionNameUrl );
            Controls.Add( _lblChartTitle );
            Controls.Add( _lblChartSubtitle );
            Controls.Add( _pnlChartPlaceholder );
            Controls.Add( _hbChartOptions );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( !this.Visible )
            {
                return;
            }

            _pnlChartPlaceholder.Width = this.ChartWidth;
            _pnlChartPlaceholder.Height = this.ChartHeight;

            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", this.GetType().Name.SplitCase().ToLower().Replace( " ", "-" ) );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _hfChartData.RenderControl( writer );
            _hfRestUrlParams.RenderControl( writer );
            _hfRestUrl.RenderControl( writer );
            _hfSeriesPartitionNameUrl.RenderControl( writer );
            _hfXAxisLabel.RenderControl( writer );
            _hfYAxisLabel.RenderControl( writer );

            if ( !string.IsNullOrWhiteSpace( _lblChartTitle.Text ) )
            {
                writer.AddAttribute( "class", "chart-title" );
                if ( this.TitleFormat?.TextAlign != null )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.TextAlign, this.TitleFormat.TextAlign );
                }

                SetLabelStyle( _lblChartTitle, this.TitleFormat );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _lblChartTitle.RenderControl( writer );
                writer.RenderEndTag();
            }

            if ( !string.IsNullOrWhiteSpace( _lblChartSubtitle.Text ) )
            {
                writer.AddAttribute( "class", "chart-subtitle" );
                if ( this.SubtitleFormat != null )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.TextAlign, this.SubtitleFormat.TextAlign );
                }

                SetLabelStyle( _lblChartSubtitle, this.SubtitleFormat );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _lblChartSubtitle.RenderControl( writer );
                writer.RenderEndTag();
            }

            _nbRenderNotification.RenderControl( writer );
            
            if ( this.ShowDebug )
            {
                _hbChartOptions.RenderControl( writer );
            }

            var showChart = string.IsNullOrWhiteSpace( _nbRenderNotification.Text );
            _pnlChartPlaceholder.Visible = showChart;

            _pnlChartPlaceholder.RenderControl( writer );

            writer.RenderEndTag();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Gets the chart data (ajax call from Chart)
        /// </summary>
        private void GetChartData()
        {
            string chartDataJson = null;

            if ( GetChartDataCallback != null )
            {
                chartDataJson = GetChartDataCallback();
            }

            try
            {
                if ( chartDataJson != null )
                {
                    var response = this.Page.Response;

                    response.Clear();
                    response.Write( chartDataJson );
                    response.End();
                }
            }
            catch ( System.Threading.ThreadAbortException )
            {
                // ignore the ThreadAbort exception from Response.End();
            }
            catch ( Exception ex )
            {
                throw new Exception( "GetChartData action failed.", ex );
            }
        }

        /// <summary>
        /// Handles the chart click event.
        /// </summary>
        private void HandleChartClick()
        {
            if ( ChartClick == null )
            {
                return;
            }

            var eventArgs = this.Page.Request.Params["__EVENTARGUMENT"].Split( ';' );
            var chartClickArgs = new ChartClickArgs();
            foreach ( var parts in eventArgs )
            {
                var param = parts.Split( '=' );
                if ( param[0] == "DateStamp" )
                {
                    if ( long.TryParse( param[1], out long dateStamp ) )
                    {
                        chartClickArgs.DateTimeValue = new DateTime( 1970, 1, 1 ).AddMilliseconds( dateStamp );
                    }
                }
                else if ( param[0] == "YValue" )
                {
                    chartClickArgs.YValue = param[1].AsInteger();
                }
            }

            ChartClick( this, chartClickArgs );
        }

        /// <summary>
        /// Get the Javascript function used to plot the chart.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetPlotChartJavaScript()
        {
            var script = @"
function plotChart (chartData, plotSelector, yaxisLabelText)
{
    if (chartData && chartData.data && chartData.data.datasets && chartData.data.datasets.length > 0)
    {
        var ctx = $('<canvas />').appendTo( plotSelector);
        var chart = new Chart(ctx, chartData);
        return chart;
    }
    else
    {
        $(plotSelector).html('<div class=""alert alert-info"">No Data Found</div>');
    }
}";

            return script;
        }

        /// <summary>
        /// Gets the chart data as a JSON string.
        /// </summary>
        /// <returns></returns>
        protected string GetChartDataJson()
        {
            string json;
            if ( string.IsNullOrWhiteSpace( _hfChartData.Value )
                 && _chartDataSource != null )
            {
                json = OnGenerateChartJson( _chartDataSource, _defaultSeriesName );

                // Store the data as Base64 encoded in a hidden field on the page.
                _hfChartData.Value = Convert.ToBase64String( Encoding.UTF8.GetBytes( json ) );
            }
            else
            {
                json = Encoding.UTF8.GetString( Convert.FromBase64String( _hfChartData.Value ) );
            }

            if ( string.IsNullOrWhiteSpace( json ) )
            {
                json = "[]";
            }

            return json;
        }

        /// <summary>
        /// Get a JSON string representation of the ChartData object.
        /// </summary>
        protected Func<string> GetChartDataCallback { get; set; }

        /// <summary>
        /// Registers the java script used to produce the chart.
        /// </summary>
        protected virtual void RegisterJavaScript( string chartData = "[]" )
        {
            var scriptFormat = new StringBuilder();

            // Create the function to return the chart data.
            var getDataWithCallback = ( GetChartDataCallback != null );

            if ( getDataWithCallback )
            {
                // If the chart data is loaded using a callback mechanism, add the client script for the callback here.
                var rockBlock = this.RockBlock();
                var pageReference = new Rock.Web.PageReference( rockBlock.PageCache.Id );
                pageReference.QueryString = new System.Collections.Specialized.NameValueCollection();
                pageReference.QueryString.Add( this.Page.Request.QueryString );
                pageReference.QueryString.Add( "GetChartData", "true" );
                pageReference.QueryString.Add( "GetChartDataBlockId", rockBlock.BlockId.ToString() );
                pageReference.QueryString.Add( "TimeStamp", RockDateTime.Now.ToJavascriptMilliseconds().ToString() );

                var callbackUrl = pageReference.BuildUrl();

                string restUrl = this.ResolveUrl( callbackUrl );
                _hfRestUrl.Value = restUrl;

                scriptFormat.Append( @"
// Retrieve the chart data from the specified REST service URL.
var restUrl_{0} = $('#{0} .js-rest-url').val() + $('#{0} .js-rest-url-params').val();

$.ajax({
    url: restUrl_{0},
    dataType: 'json',
    contentType: 'application/json'
})
.done( function (chartData) {
" );
            }
            else
            {
                scriptFormat.Append( @"
$(function() {
var _chartData = <chartData>;
" )
                    .Replace( "<chartData>", chartData );
            }

            scriptFormat.Append( @"
var chartOptions = [];
var plotSelector = '#{0} .js-chart-placeholder';
var yaxisLabelText = $('#{0} .js-yaxis-value').val();
" );

            var plotScript = this.GetPlotChartJavaScript();

            scriptFormat.Append( $@"
// Create the Chart
var _chart = plotChart(_chartData, plotSelector, yaxisLabelText);
{ plotScript }
" );

            scriptFormat.Append( @"
// Chart datapoint click script
<clickScript>
" );
            if ( getDataWithCallback )
            {
                scriptFormat.Append( @"
})
.fail(function (jqXHR, textStatus, errorThrown) {
    //debugger
" );
            }

            scriptFormat.Append( @"
        });" );

            var chartClickScript = GetChartClickScript();

            string script = scriptFormat.ToString()
                .Replace( "{0}", this.ClientID )
                .Replace( "<clickScript>", chartClickScript );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "flot-chart-script_" + this.ClientID, script, true );
        }

        #endregion

        /// <summary>
        /// Create the client script to generate a click event for the chart.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetChartClickScript()
       {
            var chartClickScript = string.Empty;

            if ( ChartClick != null && !string.IsNullOrWhiteSpace( this.ChartClickScript ) )
            {
                chartClickScript = $@"$('#{this.ClientID}').find('.js-chart-placeholder').bind('click',{ this.ChartClickScript });";
            }

            return chartClickScript;
        }

        private void SetLabelStyle( Label label, ChartTextElementStyle format )
        {
            if ( format == null )
            {
                return;
            }
            if ( !string.IsNullOrWhiteSpace( format.ForeColor ) )
            {
                _lblChartTitle.ForeColor = ColorTranslator.FromHtml( format.ForeColor );
            }

            if ( !string.IsNullOrWhiteSpace( format.FontName ) )
            {
                _lblChartTitle.Font.Name = format.FontName;
            }

            if ( format.FontSize != null )
            {
                _lblChartTitle.Font.Size = new FontUnit( format.FontSize.Value );
            }

        }

        /// <summary>
        /// Sets properties of this chart from the properties of a ChartStyle object.
        /// </summary>
        /// <param name="chartStyle">The chart style.</param>
        public void SetChartStyle( ChartStyle chartStyle )
        {
            if ( chartStyle.Legend != null )
            {
                this.ShowLegend = chartStyle.Legend.Show ?? true;
                this.LegendPosition = chartStyle.Legend.Position;
            }

            if ( chartStyle.Title != null )
            {
                this.TitleFormat = new ChartTextElementStyle
                {
                    ForeColor = chartStyle.Title.Font.Color,
                    FontName = chartStyle.Title.Font.Family,
                    FontSize = chartStyle.Title.Font.Size,
                    TextAlign = chartStyle.Title.Align
                };
            }

            if ( chartStyle.Subtitle?.Font != null )
            {
                this.SubtitleFormat = new ChartTextElementStyle
                {
                    ForeColor = chartStyle.Subtitle.Font.Color,
                    FontName = chartStyle.Subtitle.Font.Family,
                    FontSize = chartStyle.Subtitle.Font.Size,
                    TextAlign = chartStyle.Subtitle.Align
                };
            }
        }

        internal DateTime GetDateTimeFromJavascriptMilliseconds( long millisecondsAfterEpoch )
        {
            return new DateTime( 1970, 1, 1 ).AddTicks( millisecondsAfterEpoch * 10000 );
        }

        #region Obsolete

        /// <summary>
        /// Get a ChartStyle object that encapsulates the settings for this chart.
        /// </summary>
        /// <returns></returns>
        [Obsolete( "The ChartStyle is no longer used to format charts. Reference the equivalent properties on derived controls instead." )]
        [RockObsolete( "1.15" )]
        public ChartStyle GetChartStyle()
        {
            var chartStyle = new ChartStyle();

            chartStyle.Legend = new LegendStyle();
            chartStyle.Legend.Show = this.ShowLegend;
            chartStyle.Legend.Position = this.LegendPosition;

            return chartStyle;
        }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [Obsolete( "This property has no effect. Use equivalent properties on derived controls instead." )]
        [RockObsolete( "1.14" )]
        public DateTime? StartDate
        {
            get
            {
                return ViewState["StartDate"] as DateTime?;
            }

            set
            {
                ViewState["StartDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [Obsolete( "This property has no effect. Use equivalent properties on derived controls instead." )]
        [RockObsolete( "1.14" )]
        public DateTime? EndDate
        {
            get
            {
                return ViewState["EndDate"] as DateTime?;
            }

            set
            {
                ViewState["EndDate"] = value;
            }
        }

        /// <summary>
        /// Create a category series of data values from a collection of Rock Chart Data Items.
        /// </summary>
        /// <param name="chartDataItems"></param>
        /// <returns></returns>
        [Obsolete( "Use the GetChartDataJson() method instead." )]
        [RockObsolete( "1.14" )]
        public List<ChartJsCategorySeriesDataset> GetCategorySeriesFromChartData( IEnumerable<IChartData> chartDataItems )
        {
            // Show a bar chart to summarize the data for a single date.
            var exceptionsByCategory = chartDataItems.GroupBy( k => k.SeriesName, v => v );

            var datasets = new List<ChartJsCategorySeriesDataset>();
            foreach ( var exceptionCategory in exceptionsByCategory )
            {
                var categoryName = exceptionCategory.Key;
                if ( string.IsNullOrWhiteSpace( categoryName ) )
                {
                    categoryName = "(unknown)";
                }

                datasets.Add( new ChartJsCategorySeriesDataset
                {
                    Name = exceptionCategory.Key,
                    DataPoints = new List<IChartJsCategorySeriesDataPoint>()
                        {
                            new ChartJsCategorySeriesDataPoint
                            {
                                Category = categoryName,
                                Value = exceptionCategory.Sum(x => x.YValue ?? 0)
                            }
                        }
                } );
            }

            return datasets;
        }

        /// <summary>
        /// Create a time series of data values from a collection of Rock Chart Data Items.
        /// </summary>
        /// <param name="chartDataItems"></param>
        /// <returns></returns>
        [Obsolete( "Use the GetChartDataJson() method instead." )]
        [RockObsolete( "1.14" )]
        public List<ChartJsTimeSeriesDataset> GetTimeSeriesFromChartData( IEnumerable<IChartData> chartDataItems )
        {
            var itemsBySeries = chartDataItems.GroupBy( k => k.SeriesName, v => v );
            var timeDatasets = new List<ChartJsTimeSeriesDataset>();
            foreach ( var series in itemsBySeries )
            {
                timeDatasets.Add( new ChartJsTimeSeriesDataset
                {
                    Name = series.Key,
                    DataPoints = chartDataItems.Where( x => x.SeriesName == series.Key )
                        .Select( x => ( IChartJsTimeSeriesDataPoint ) new ChartJsTimeSeriesDataPoint
                        {
                            DateTime = GetDateTimeFromJavascriptMilliseconds( x.DateTimeStamp ),
                            Value = x.YValue ?? 0,
                        } ).ToList()
                } );
            }

            return timeDatasets;
        }

        #endregion
    }

    #region Support classes

    /// <summary>
    /// A collection of settings that define the style of a text element on the chart.
    /// </summary>
    public class ChartTextElementStyle
    {
        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public string TextAlign { get; set; }

        /// <summary>
        /// Gets or sets the font name.
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public int? FontSize { get; set; }

        /// <summary>
        /// Gets or sets the forecolor of the text.
        /// </summary>
        public string ForeColor { get; set; }
    }

    #endregion
}
