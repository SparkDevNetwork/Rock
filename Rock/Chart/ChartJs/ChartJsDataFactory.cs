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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Chart
{
    /// <summary>
    /// Provides base functionality for factories that build the data structures needed to render a chart with the Chart.js library.
    /// These factories produce data that describes the presentation and functionality of a chart,
    /// they do not produce the datapoints that are mapped by the chart.
    /// </summary>
    /// <remarks>
    /// Compatible with ChartJS v2.9.3.
    /// </remarks>
    public abstract class ChartJsDataFactory
    {
        /// <summary>
        /// A collection of HTML Colors that represent the default palette for datasets in the chart.
        /// Colors are selected in order from this list for each dataset that does not have a specified color.
        /// </summary>
        public List<RockColor> ChartColors { get; set; }

        /// <summary>
        /// The level of opacity for the area bounded by a dataset on a scale from 0 to 1, where 0 represents complete transparency.
        /// </summary>
        public double AreaFillOpacity { get; set; } = 0.5;

        /// <summary>
        /// Get a factory to generate a sequence of colors for the chart.
        /// </summary>
        /// <returns></returns>
        protected ChartColorPaletteGenerator GetChartColorGenerator()
        {
            if ( _chartColorGenerator == null )
            {
                _chartColorGenerator = new ChartColorPaletteGenerator( this.ChartColors );
            }

            return _chartColorGenerator;
        }

        private ChartColorPaletteGenerator _chartColorGenerator = null;

        /// <summary>
        /// Display legend?
        /// </summary>
        [RockObsolete( "1.15" )]
        [Obsolete]
        public bool DisplayLegend { get; set; } = true;

        /// <summary>
        /// Size to fit container width?
        /// </summary>
        public bool SizeToFitContainerWidth
        {
            get
            {
                return _sizeToFitContainerWidth;
            }
            set
            {

                _sizeToFitContainerWidth = value;
                SynchroniseSizingSettings();
            }
        }

        private bool _sizeToFitContainerWidth = true;

        /// <summary>
        /// Maintain aspect ratio?
        /// </summary>
        public bool MaintainAspectRatio
        {
            get
            {
                return _maintainAspectRatio;
            }
            set
            {

                _maintainAspectRatio = value;
                SynchroniseSizingSettings();
            }
        }

        private bool _maintainAspectRatio = false;

        /// <summary>
        /// Gets or sets the Javascript function that is used to create a tooltip.
        /// </summary>
        /// <example>
        /// <code>
        /// function(tooltipModel)
        /// {
        ///     var bodyText = 'This is the default body:<br/>' + tooltipModel.body[0].lines[0];
        ///     var headerText = 'This is the default title:<br/>' + tooltipModel.title;
        ///     var html = '<table><thead>';
        ///     html += '<tr><th>' + headerText + '</th></tr>';
        ///     html += '</thead><tbody>';
        ///     html += '<tr><td>' + bodyText + '</td></tr>';
        ///     html += '</tbody></table>';
        ///     return html;
        /// }
        /// </code>
        /// </example>
        /// <value>
        /// A Javascript function that returns the HTML content displayed in the tooltip.
        /// </value>
        public string CustomTooltipScript { get; set; }

        /// <summary>
        /// Gets a JavaScript data object that represents the configuration for the ChartJs Y-axis.
        /// </summary>
        /// <returns></returns>
        protected dynamic GetYAxisConfigurationObject( string valueFormatString, decimal suggestedMax, decimal? stepSize, bool isStacked )
        {
            string callbackStr;
            valueFormatString = valueFormatString ?? string.Empty;
            valueFormatString = valueFormatString.Trim().ToLower();

            if ( valueFormatString == "currency" )
            {
                var currencyCode = RockCurrencyCodeInfo.GetCurrencyCode();
                callbackStr = string.Format( @"function(label, index, labels) {{
                return Intl.NumberFormat( undefined, {{ maximumFractionDigits: 0, minimumFractionDigits: 0, style: 'currency', currency: '{0}' }}).format( label );
                }}", currencyCode );
            }
            else if ( valueFormatString == "percentage" )
            {
                callbackStr = @"function (label, index, values) {
                return label + '%';
              }";
            }
            else
            {
                callbackStr = @"function(label, index, labels) {
                return Intl.NumberFormat().format(label);
                }";
            }

            var optionsYaxis = new List<object>()
            {
                new
                {
                    ticks = new { callback = new JRaw( callbackStr ), beginAtZero = true, suggestedMax = suggestedMax, stepSize = stepSize },
                    stacked = isStacked
                }
            };

            return optionsYaxis;
        }

        /// <summary>
        /// Gets a JavaScript data object that represents the configuration for the ChartJs legend chart element.
        /// </summary>
        /// <param name="legendPosition"></param>
        /// <param name="legendAlignment"></param>
        /// <param name="displayLegend"></param>
        /// <returns></returns>
        protected dynamic GetLegendConfigurationObject( string legendPosition, string legendAlignment, bool displayLegend )
        {
            var optionsLegend = new
            {
                position = legendPosition,
                align = legendAlignment,
                display = displayLegend
            };

            return optionsLegend;
        }

        /// <summary>
        /// Gets a JavaScript data object that represents the configuration for the ChartJs tooltip chart element.
        /// </summary>
        /// <returns></returns>
        protected virtual dynamic GetTooltipsConfigurationObject( string containerControlId, string valueFormatString )
        {
            if ( containerControlId == null )
            {
                return new { enabled = true };
            }

            dynamic tooltipConfiguration;

            // Enable custom tooltips if a custom script is specified.
            var enableCustomTooltip = !string.IsNullOrEmpty( this.CustomTooltipScript );
            if ( enableCustomTooltip )
            {
                // Add a custom function to get the tooltip content.
                var tooltipScript = @"
function(tooltipModel) {
    // Tooltip Element
    var tooltipEl = document.getElementById('chartjs-tooltip');

    var canvas = document.getElementById('{containerControlId}');

    // Create element on first render.
    if (!tooltipEl) {
        tooltipEl = document.createElement('div');
        tooltipEl.id = 'chartjs-tooltip';
        tooltipEl.innerHTML = '<table></table>';
        document.body.appendChild(tooltipEl);
    }

    // Hide if no tooltip.
    if (tooltipModel.opacity === 0) {
        tooltipEl.style.opacity = 0;
        return;
    }

    // Set caret position.
    tooltipEl.classList.remove('above', 'below', 'no-transform');
    if (tooltipModel.yAlign) {
        tooltipEl.classList.add(tooltipModel.yAlign);
    } else {
        tooltipEl.classList.add('no-transform');
    }

    // Set Text
    var getTooltipHtml = {GetToolTipHtmlFunction};
    var tooltipHtml = getTooltipHtml( tooltipModel );
    if (tooltipHtml)
    {
        var tableRoot = tooltipEl.querySelector('table');
        tableRoot.outerHTML = tooltipHtml;
    }

    // Display, position, and set styles for font
    var position = canvas.getBoundingClientRect();

    tooltipEl.style.opacity = 1;
    tooltipEl.style.background = 'rgba(0,0,0,0.7)';
    tooltipEl.style.color = 'rgba(255,255,255,1)';
    tooltipEl.style.position = 'absolute';
    tooltipEl.style.left = position.left + window.pageXOffset + tooltipModel.caretX + 'px';
    tooltipEl.style.top = position.top + window.pageYOffset + tooltipModel.caretY + 'px';
    tooltipEl.style.fontFamily = tooltipModel._bodyFontFamily;
    tooltipEl.style.fontSize = tooltipModel.bodyFontSize + 'px';
    tooltipEl.style.fontStyle = tooltipModel._bodyFontStyle;
    tooltipEl.style.padding = tooltipModel.yPadding + 'px ' + tooltipModel.xPadding + 'px';
    tooltipEl.style.pointerEvents = 'none';
}
";

                tooltipScript = tooltipScript.Replace( "{GetToolTipHtmlFunction}", this.CustomTooltipScript );
                tooltipScript = tooltipScript.Replace( "{containerControlId}", containerControlId );

                tooltipConfiguration = new
                {
                    enabled = false,
                    custom = new JRaw( tooltipScript )
                };

                return tooltipConfiguration;
            }
            else
            {
                // Use the default Chart.js tooltip, with a label appropriate to the datapoint values unit of measure.
                string tooltipsCallbackStr;
                valueFormatString = valueFormatString ?? string.Empty;
                valueFormatString = valueFormatString.Trim().ToLower();
                if ( valueFormatString == "currency" )
                {
                    var currencyCode = RockCurrencyCodeInfo.GetCurrencyCode();
                    tooltipsCallbackStr = string.Format( @"function (tooltipItem, data) {{
                let label = data.datasets[tooltipItem.datasetIndex].label || '';

                if (label) {{
                    label += ': ';
                }}
                return label + Intl.NumberFormat( undefined, {{ style: 'currency', currency: '{0}' }}).format( tooltipItem.yLabel );
                }}", currencyCode );
                }
                else if ( valueFormatString == "percentage" )
                {
                    tooltipsCallbackStr = @"function (tooltipItem, data) {
                 return Chart.defaults.global.tooltips.callbacks.label(tooltipItem, data) + '%';
              }";
                }
                else
                {
                    tooltipsCallbackStr = @"function (tooltipItem, data) {
                return Chart.defaults.global.tooltips.callbacks.label(tooltipItem, data);
                }";
                }

                tooltipConfiguration = new
                {
                    enabled = true,
                    callbacks = new
                    {
                        label = new JRaw( tooltipsCallbackStr )
                    }
                };
            }

            return tooltipConfiguration;
        }

        /// <summary>
        /// Gets the appropriate color property settings for a dataset.
        /// </summary>
        /// <returns></returns>
        protected void GetDatasetColorSettings( ref string borderColorString, ref string fillColorString, out string fillStyle )
        {
            RockColor datasetBorderColor = null;
            RockColor datasetFillColor = null;

            if ( !string.IsNullOrWhiteSpace( borderColorString ) )
            {
                datasetBorderColor = new RockColor( borderColorString );
            }
            if ( !string.IsNullOrWhiteSpace( fillColorString ) )
            {
                datasetFillColor = new RockColor( fillColorString );
            }

            GetDatasetColorSettings( ref datasetBorderColor, ref datasetFillColor, out fillStyle );

            // Return the colors as RGBA values.
            borderColorString = datasetBorderColor.ToRGBA();
            fillColorString = datasetFillColor.ToRGBA();
        }

        /// <summary>
        /// Gets the appropriate color property settings for a dataset.
        /// </summary>
        /// <returns></returns>
        protected void GetDatasetColorSettings( ref RockColor datasetBorderColor, ref RockColor datasetFillColor, out string fillStyle ) //, out RockColor fillColor)
        {
            var alpha = this.AreaFillOpacity;

            if ( alpha < 0.1 )
            {
                // Make sure that the specified transparency is above the threshold of visibility.
                alpha = 0.1;
            }
            else if ( alpha > 1 )
            {
                alpha = 1;
            }

            if ( alpha > 0 )
            {
                fillStyle = "origin";
            }
            else
            {
                // Opacity is set to 0, so disable fill.
                // A backcolor must be specified to show a filled square in the legend.
                alpha = 0;
                fillStyle = "false";
            }

            // If no border color is specified, get the next available color.
            if ( datasetBorderColor == null )
            {
                datasetBorderColor = GetChartColorGenerator().GetNextColor();
            }

            if ( datasetFillColor == null )
            {
                // Calculate a new fill color.
                datasetFillColor = new RockColor( datasetBorderColor.R, datasetBorderColor.G, datasetBorderColor.B, alpha );
            }
        }

        private void SynchroniseSizingSettings()
        {
            // If "maintainAspectRatio" is enabled, responsive mode must also be enabled to avoid a Chart.js resizing bug detailed here:
            // https://github.com/chartjs/Chart.js/issues/1006
            // Until this issue is resolved, avoid this invalid combination of settings.
            if ( _maintainAspectRatio &&
                 !_sizeToFitContainerWidth )
            {
                _sizeToFitContainerWidth = true;
            }
        }

        /// <summary>
        /// Serialize the specified object to JSON.
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        protected string SerializeJsonObject( dynamic jsonObject )
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            string jsonString = JsonConvert.SerializeObject( jsonObject, Formatting.None, jsonSetting );

            return jsonString;
        }

        #region Obsolete

        /// <summary>
        /// Legend position: {top|left|bottom|right}
        /// </summary>
        [Obsolete( "Use the GetJsonArgs parameter instead." )]
        [RockObsolete( "1.15" )]
        public string LegendPosition { get; set; } = "bottom";

        /// <summary>
        /// Legend alignment: {start|center|end}
        /// </summary>
        [Obsolete( "Use the GetJsonArgs parameter instead." )]
        [RockObsolete("1.15")]
        public string LegendAlignment { get; set; } = "center";

        /// <summary>
        /// Apply a Rock Chart Style to the settings of the ChartJs factory.
        /// </summary>
        /// <param name="chartStyle">The chart style.</param>
        [Obsolete( "Use the GetJsonArgs parameters instead." )]
        [RockObsolete( "1.15" )]
        public virtual void SetChartStyle( ChartStyle chartStyle )
        {
            if ( chartStyle == null )
            {
                return;
            }

            // Set the chart Legend style.
            if ( chartStyle.Legend != null )
            {
                this.DisplayLegend = chartStyle.Legend.Show ?? true;

                SetLegendPositionAndAlignment( chartStyle.Legend.Position );
            }
        }

        [Obsolete]
        [RockObsolete( "1.15" )]
        private void SetLegendPositionAndAlignment( string rockLegendPosition )
        {
            rockLegendPosition = rockLegendPosition?.ToLower() ?? string.Empty;

            if ( rockLegendPosition == "ne" )
            {
                this.LegendPosition = "top";
                this.LegendAlignment = "end";
            }
            else if ( rockLegendPosition.StartsWith( "nw" ) )
            {
                this.LegendPosition = "top";
                this.LegendAlignment = "start";
            }
            else if ( rockLegendPosition.StartsWith( "se" ) )
            {
                this.LegendPosition = "bottom";
                this.LegendAlignment = "end";
            }
            else if ( rockLegendPosition.StartsWith( "sw" ) )
            {
                this.LegendPosition = "bottom";
                this.LegendAlignment = "start";
            }
            else
            {
                this.LegendPosition = "bottom";
                this.LegendAlignment = "center";
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Args for the GetJson method
        /// </summary>
        public class GetJsonArgs
        {
            /// <summary>
            /// A Rock chart configuration settings object.
            /// </summary>
            [RockObsolete( "1.15" )]
            [Obsolete]
            public ChartStyle ChartStyle { get; set; }

            /// <summary>
            /// Size to fit container width?
            /// </summary>
            public bool SizeToFitContainerWidth { get; set; } = true;

            /// <summary>
            /// Maintain aspect ratio?
            /// </summary>
            public bool MaintainAspectRatio { get; set; } = false;

            /// <summary>
            /// Display legend?
            /// </summary>
            public bool DisplayLegend { get; set; } = true;

            /// <summary>
            /// Legend position: {top|left|bottom|right}
            /// </summary>
            public string LegendPosition { get; set; } = "bottom";

            /// <summary>
            /// Legend alignment: {start|center|end}
            /// </summary>
            public string LegendAlignment { get; set; } = "center";

            /// <summary>
            /// Bezier curve tension of the line. Set to 0 to draw straight lines.
            /// This option is ignored if monotone cubic interpolation is used.
            /// </summary>
            public decimal LineTension { get; set; } = 0m;

            /// <summary>
            /// The format string for values plotted on the "Y-axis" of an "X vs Y" chart.
            /// These are the dependent values in the data set, which are plotted against the independent values of the X-axis.
            /// </summary>
            /// <value>
            /// {unspecified|numeric|currency|percentage}
            /// </value>
            public string YValueFormatString { get; set; }

            /// <summary>
            /// The identifier of the client control that is the container for the chart.
            /// This property is required if a custom tooltip is specified.
            /// </summary>
            public string ContainerControlId { get; set; }

            /// <summary>
            /// Gets or sets a flag to indicate if animation should be disabled when drawing the chart.
            /// </summary>
            public bool DisableAnimation { get; set; } = false;
        }

        #endregion
    }
}
