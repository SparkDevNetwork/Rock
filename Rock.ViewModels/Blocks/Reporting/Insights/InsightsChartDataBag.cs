using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.Reporting.Insights
{
    /// <summary>
    /// Data Bag for the Insights block charts.
    /// </summary>
    public class InsightsChartDataBag
    {
        /// <summary>
        /// Gets or sets the category of the insight.
        /// </summary>
        public string InsightCategory { get; set; }

        /// <summary>
        /// Gets or sets the subcategory of the insight.
        /// </summary>
        public string InsightSubcategory { get; set; }

        /// <summary>
        /// Gets or sets the chart bag containing the chart's data and configuration.
        /// </summary>
        public ChartBag ChartBag { get; set; }
    }
}
