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
using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Abstract class for much of the Flot Charts Logic
    /// </summary>
    public abstract class FlotChart : CompositeControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlotChart"/> class.
        /// </summary>
        public FlotChart()
        {
            Options = new FlotChartOptions();
            this.Options.xaxis.mode = AxisMode.time;
            this.Options.xaxis.timeformat = "%b %Y";
            this.Options.grid.hoverable = true;
            this.Options.grid.clickable = true;
            this.Options.grid.margin = new MarginOptions { top = 0, right = 10, bottom = 0, left = 10 };
            this.Options.grid.borderWidth = new BorderOptions  { top = 0, right = 0, bottom = 1, left = 1 };
        }

        #region Controls

        private HiddenField _hfMetricId;
        private HiddenField _hfRestUrlParams;
        private HiddenField _hfXAxisLabel;
        private Label _lblDashboardTitle;
        private Label _lblDashboardSubtitle;
        private Panel _pnlChartPlaceholder;

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
        /// Gets or sets the MetricValueType (Goal or Measure)
        /// Leave null for both
        /// </summary>
        /// <value>
        /// The type of the metric value.
        /// </value>
        public MetricValueType? MetricValueType
        {
            get
            {
                return ViewState["MetricValueType"] as MetricValueType?;
            }

            set
            {
                ViewState["MetricValueType"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
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
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId
        {
            get
            {
                return ViewState["EntityId"] as int?;
            }

            set
            {
                ViewState["EntityId"] = value;
            }
        }

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
                return _lblDashboardTitle.Text;
            }
            set
            {
                EnsureChildControls();
                _lblDashboardTitle.Text = value;
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
                return _lblDashboardSubtitle.Text;
            }
            set
            {
                EnsureChildControls();
                _lblDashboardSubtitle.Text = value;
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
        /// Gets or sets the options.
        /// Defaults to a default set of options
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public FlotChartOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the data source URL.
        /// Defaults to "~/api/MetricValues/GetByMetricId/"
        /// </summary>
        /// <value>
        /// The data source URL.
        /// </value>
        public string DataSourceUrl
        {
            get
            {
                return ViewState["DataSourceUrl"] as string ?? "~/api/MetricValues/GetByMetricId/";
            }

            set
            {
                ViewState["DataSourceUrl"] = value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( this.Page, "~/Scripts/flot/jquery.flot.js" );
            RockPage.AddScriptLink( this.Page, "~/Scripts/flot/jquery.flot.time.js" );
            RockPage.AddScriptLink( this.Page, "~/Scripts/flot/jquery.flot.resize.js" );

            EnsureChildControls();
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
            this._hfXAxisLabel.Value = metric != null ? metric.XAxisLabel : "value";

            // setup Rest URL parameters
            if ( this.MetricId.HasValue )
            {
                _hfRestUrlParams.Value = string.Format( "{0}", this.MetricId );

                List<string> filterParams = new List<string>();
                List<string> qryParams = new List<string>();
                if ( this.StartDate.HasValue )
                {
                    filterParams.Add( string.Format( "MetricValueDateTime ge DateTime'{0}'", this.StartDate.Value.ToString( "o" ) ) );
                }

                if ( this.EndDate.HasValue )
                {
                    filterParams.Add( string.Format( "MetricValueDateTime lt DateTime'{0}'", this.EndDate.Value.ToString( "o" ) ) );
                }

                if ( this.MetricValueType.HasValue )
                {
                    // MetricValueType is an enum, which isn't quite supported for $filters as of Web Api 2.1, so pass it as a regular rest param instead of as part of the odata $filter
                    qryParams.Add( string.Format( "metricValueType={0}", this.MetricValueType ) );
                }

                if ( this.EntityId.HasValue )
                {
                    filterParams.Add( string.Format( "EntityId eq {0}", this.EntityId ) );
                }

                if ( filterParams.Count > 0 )
                {
                    qryParams.Add( "$filter=" + filterParams.AsDelimited( " and " ) );
                }

                _hfRestUrlParams.Value += "?" + qryParams.AsDelimited( "&" );
            }
            else
            {
                _hfRestUrlParams.Value = string.Empty;
            }

            string scriptFormat =
@"
            var restUrl_{3} = '{0}' + $('#{1}').val();

            $.ajax({{
                url: restUrl_{3},
                dataType: 'json',
                contentType: 'application/json'
            }})
            .done( function (chartData) {{

                // setup chartPoints objects
                var chartMeasurePoints = {{
                    label: $('#{2}').val(),
                    metricValues: chartData,
                    data: []
                }}

                var chartGoalPoints = {{
                    label: $('#{2}').val() + ' goal',
                    metricValues: chartData,
                    data: []
                }}

                // populate the chartMeasurePoints data array with data from the REST result
                for (var i = 0; i < chartData.length; i++) {{
                    if (chartData[i].MetricValueType == 0) {{
                        chartMeasurePoints.data.push([chartData[i].MetricValueJavascriptTimeStamp, chartData[i].YValue]);
                    }}
                    if (chartData[i].MetricValueType == 1) {{
                        chartGoalPoints.data.push([chartData[i].MetricValueJavascriptTimeStamp, chartData[i].YValue]);
                    }}
                }}

                var chartOptions = {4};

                // plot the chart
                var chartSeriesList = [];
                if (chartMeasurePoints.data.length) {{
                    chartSeriesList.push(chartMeasurePoints);
                }}

                if (chartGoalPoints.data.length) {{
                    chartSeriesList.push(chartGoalPoints);
                }}

                $.plot('#{3}', chartSeriesList, chartOptions);

                // setup of bootstrap tooltip which we'll show on the plothover event
                $(""<div id='tooltip_{3}' class='tooltip right'><div class='tooltip-inner'></div><div class='tooltip-arrow'></div></div>"").css({{
                    position: 'absolute',
                    display: 'none',
                }}).appendTo('body');

                $('#{3}').bind('plothover', function (event, pos, item) {{
                    
                    if (item) {{
                        var tooltipText = item.series.metricValues[item.dataIndex].Note;
                        // if there isn't a note, just show the y value;
                        tooltipText = tooltipText || item.datapoint[1];
                        $('#tooltip_{3}').find('.tooltip-inner').html(tooltipText);
                        $('#tooltip_{3}').css({{ top: item.pageY + 5, left: item.pageX + 5, opacity: 1 }});
                        $('#tooltip_{3}').show();
                    }}
                    else {{
                        $('#tooltip_{3}').hide();
                    }}
                }});
                
            }})
            .fail(function (jqXHR, textStatus, errorThrown) {{
                debugger
            }});
";

            string chartOptionsJson = JsonConvert.SerializeObject( this.Options, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                } );
            

            string restUrl = this.ResolveUrl( this.DataSourceUrl );
            string script = string.Format(
                scriptFormat,
                restUrl,
                _hfRestUrlParams.ClientID,
                _hfXAxisLabel.ClientID,
                _pnlChartPlaceholder.ClientID,
                chartOptionsJson );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "flot-chart-script_" + this.ClientID, script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _hfMetricId = new HiddenField();
            _hfMetricId.ID = string.Format( "hfMetricId_{0}", this.ID );
            
            _hfXAxisLabel = new HiddenField();
            _hfXAxisLabel.ID = string.Format( "hfXAxisLabel_{0}", this.ID );
            
            _hfRestUrlParams = new HiddenField();
            _hfRestUrlParams.ID = string.Format( "hfRestUrlParams_{0}", this.ID );
            
            _lblDashboardTitle = new Label();
            _lblDashboardTitle.CssClass = "dashboard-title";
            _lblDashboardTitle.ID = string.Format( "lblDashboardTitle_{0}", this.ID );
            
            _lblDashboardSubtitle = new Label();
            _lblDashboardSubtitle.CssClass = "dashboard-subtitle";
            _lblDashboardSubtitle.ID = string.Format( "lblDashboardSubtitle_{0}", this.ID );
            
            _pnlChartPlaceholder = new Panel();
            _pnlChartPlaceholder.CssClass = "dashboard-chart-placeholder";
            _pnlChartPlaceholder.ID = string.Format( "pnlChartPlaceholder_{0}", this.ID );

            Controls.Add( _hfMetricId );
            Controls.Add( _hfXAxisLabel );
            Controls.Add( _hfRestUrlParams );
            Controls.Add( _lblDashboardTitle );
            Controls.Add( _lblDashboardSubtitle );
            Controls.Add( _pnlChartPlaceholder );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            RegisterJavaScript();

            _pnlChartPlaceholder.Width = this.ChartWidth;
            _pnlChartPlaceholder.Height = this.ChartHeight;

            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", this.GetType().Name.SplitCase().ToLower().Replace( " ", "-" ) );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _hfMetricId.RenderControl( writer );
            _hfRestUrlParams.RenderControl( writer );
            _hfXAxisLabel.RenderControl( writer );
            _lblDashboardTitle.RenderControl( writer );
            _lblDashboardSubtitle.RenderControl( writer );
            _pnlChartPlaceholder.RenderControl( writer );

            writer.RenderEndTag();
        }
    }
}
