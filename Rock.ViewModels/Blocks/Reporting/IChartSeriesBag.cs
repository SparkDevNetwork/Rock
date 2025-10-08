using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Reporting
{
    /// <summary>
    /// Defines the contract for a chart series bag, which contains the label, data points, and color information for a chart series.
    /// </summary>
    public interface IChartSeriesBag
    {
        /// <summary>
        /// Gets the label for the series.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the label of the chart series.
        /// </value>
        string Label { get; set; }

        /// <summary>
        /// Gets the data points for the series.
        /// </summary>
        /// <value>
        /// A <see cref="List{T}"/> of nullable <see cref="double"/> values representing the data points in the series.
        /// </value>
        List<double> Data { get; set; }
    }
}
