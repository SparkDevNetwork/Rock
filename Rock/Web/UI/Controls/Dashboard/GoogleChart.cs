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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Reporting.Dashboard;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Abstract class for much of the Google Charts Logic
    /// </summary>
    public abstract class GoogleChart : Panel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleChart"/> class.
        /// </summary>
        public GoogleChart()
        {
            Options = ChartOptions.Default;
        }

        #region Controls

        private HiddenField _hfMetricId;
        private HiddenField _hfChartTypeName;
        private HiddenField _hfColumns;
        private HiddenField _hfRestUrlParams;
        private HiddenField _hfOptions;

        #endregion

        /// <summary>
        /// Gets or sets the metric identifier.
        /// </summary>
        /// <value>
        /// The metric identifier.
        /// </value>
        public int? MetricId
        {
            get
            {
                EnsureChildControls();
                return _hfMetricId.Value.AsInteger( false );
            }

            set
            {
                EnsureChildControls();
                _hfMetricId.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the type of the metric value.
        /// </summary>
        /// <value>
        /// The type of the metric value.
        /// </value>
        public MetricValueType? MetricValueType { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the type of the chart.
        /// </summary>
        /// <value>
        /// The type of the chart.
        /// </value>
        public abstract GoogleChartType? ChartType { get; }

        /// <summary>
        /// Gets or sets the options.
        /// Defaults to a default set of options
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public ChartOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the column definitions.
        /// Defaults to 3 columns: X (date), Y (decimal) and Tooltip
        /// </summary>
        /// <value>
        /// The column definitions.
        /// </value>
        public List<ColumnDefinition> ColumnDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the data source URL.
        /// Defaults to "~/api/MetricValues/GetChartData/"
        /// </summary>
        /// <value>
        /// The data source URL.
        /// </value>
        public string DataSourceUrl { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( this.Page, "https://www.google.com/jsapi", false );
            RockPage.AddScriptLink( this.Page, "~/Scripts/jquery.smartresize.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            var metric = new Rock.Model.MetricService( new Rock.Data.RockContext() ).Get( this.MetricId ?? 0 );

            // setup ChartOptions (unless caller set them up manually)
            if ( this.Options == null )
            {
                this.Options = ChartOptions.Default;

                if ( metric != null )
                {
                    this.Options.vAxis.title = metric.Title;
                    if ( !string.IsNullOrWhiteSpace( metric.Subtitle ) )
                    {
                        this.Options.vAxis.title += Environment.NewLine + metric.Subtitle;
                    }
                }
            }

            _hfOptions.Value = this.Options.ToJson();

            // setup ColumnDefinitions (unless caller set them up manually)
            if ( this.ColumnDefinitions == null )
            {
                this.ColumnDefinitions = new List<ColumnDefinition>();
                string valueLabel = "Value";
                string dateLabel = "Date";
                if ( metric != null )
                {
                    dateLabel = metric.XAxisLabel ?? dateLabel;
                    valueLabel = metric.YAxisLabel ?? valueLabel;
                }

                this.ColumnDefinitions.Add( new ColumnDefinition( dateLabel, ColumnDataType.date ) );
                this.ColumnDefinitions.Add( new ColumnDefinition( valueLabel, ColumnDataType.number ) );
                this.ColumnDefinitions.Add( new ChartTooltip() );
            }

            _hfColumns.Value = this.ColumnDefinitions.ToJson();

            // setup Rest URL parameters
            if ( this.MetricId.HasValue )
            {
                _hfRestUrlParams.Value = string.Format( "{0}?startDate={1}&endDate={2}", this.MetricId, this.StartDate ?? DateTime.MinValue, this.EndDate ?? DateTime.MaxValue );
                if ( this.MetricValueType.HasValue )
                {
                    _hfRestUrlParams.Value += string.Format( "&metricValueType={0}", this.MetricValueType );
                }

                if ( this.EntityId.HasValue )
                {
                    _hfRestUrlParams.Value += string.Format( "&entityId={0}", this.EntityId );
                }
            }
            else
            {
                _hfRestUrlParams.Value = string.Empty;
            }

            string loadScript = @"
            // Load the Visualization API and the corechart package.
            google.load('visualization', '1.0', { 'packages': ['corechart'] });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "google-chart-script-load", loadScript, true );

            string scriptFormat =
@"
            // Set a callback to run when the Google Visualization API is loaded.
            google.setOnLoadCallback(function () {{

                var restUrl = '{0}';

                $.ajax({{
                    url: restUrl + $('#{1}').val(),
                    dataType: 'json',
                    contentType: 'application/json'
                }})
                .done( function (chartDataJS) {{

                    // define chart
                    var columnsText = $('#{2}').val();
                    var columnsData = $.parseJSON(columnsText);

                    var dataTable = new google.visualization.DataTable(
                        {{
                            cols: columnsData
                        }});

                    // data for chart is in JS object literal notation, so we need to eval it first
                    var chartDataArray = eval(chartDataJS);

                    dataTable.addRows(chartDataArray);

                    // options for chart
                    var optionsText = $('#{3}').val();
                    var options = $.parseJSON(optionsText);

                    // create and draw chart
                    var chartDiv = document.getElementById('{4}');
                    var chart = new google.visualization.{5}(chartDiv);
                    chart.draw(dataTable, options);

                    $(window).smartresize(function () {{
                        {{
                            chart.draw(dataTable, options);
                        }}
                    }});
                }})
                .fail(function (jqXHR, textStatus, errorThrown) {{
                    debugger
                }});
            }});
";

            string restUrl = this.ResolveUrl( this.DataSourceUrl ?? "~/api/MetricValues/GetChartData/" );
            string script = string.Format(
                scriptFormat,
                restUrl,
                _hfRestUrlParams.ClientID,
                _hfColumns.ClientID,
                _hfOptions.ClientID,
                this.ClientID,
                this.ChartType.ConvertToString( false ) );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "google-chart-script_" + this.ClientID, script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _hfMetricId = new HiddenField();
            _hfMetricId.ClientIDMode = ClientIDMode.Static;
            _hfMetricId.ID = string.Format( "hfMetricId_{0}", this.ID );
            _hfChartTypeName = new HiddenField();
            _hfChartTypeName.ClientIDMode = ClientIDMode.Static;
            _hfChartTypeName.ID = string.Format( "hfChartTypeName_{0}", this.ID );
            _hfColumns = new HiddenField();
            _hfColumns.ClientIDMode = ClientIDMode.Static;
            _hfColumns.ID = string.Format( "hfColumns_{0}", this.ID );
            _hfRestUrlParams = new HiddenField();
            _hfRestUrlParams.ClientIDMode = ClientIDMode.Static;
            _hfRestUrlParams.ID = string.Format( "hfRestUrlParams_{0}", this.ID );
            _hfOptions = new HiddenField();
            _hfOptions.ClientIDMode = ClientIDMode.Static;
            _hfOptions.ID = string.Format( "hfOptions_{0}", this.ID );

            Controls.Add( _hfMetricId );
            Controls.Add( _hfChartTypeName );
            Controls.Add( _hfColumns );
            Controls.Add( _hfRestUrlParams );
            Controls.Add( _hfOptions );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            RegisterJavaScript();

            base.RenderControl( writer );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum GoogleChartType
    {
        LineChart,
        ColumnChart
    }
}
