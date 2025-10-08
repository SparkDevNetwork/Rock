using System.Collections.Generic;

using Rock.ViewModels.Blocks.Reporting;

namespace Rock.ViewModels.Reporting
{
    /// <summary>
    /// Defines the contract for a chart bag, which contains the labels, series data, and chart options for a chart.
    /// </summary>
    public class ChartBag
    {
        /// <summary>
        /// Gets or sets the labels for the chart.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of <see cref="string"/> values representing the labels for the chart.
        /// </value>
        public List<string> Labels { get; set; }

        /// <summary>
        /// Gets or sets the series bags for the chart.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of <see cref="IChartSeriesBag"/> objects representing the data series for the chart.
        /// </value>
        public List<IChartSeriesBag> SeriesBags { get; set; }

        /// <summary>
        /// Gets or sets the chart options bag.
        /// </summary>
        /// <value>
        /// A <see cref="ChartOptionsBag"/> object containing configuration options for the chart.
        /// </value>
        public ChartOptionsBag ChartOptionsBag { get; set; }
    }
}
