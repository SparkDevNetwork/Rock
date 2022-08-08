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

using Rock.Data;

namespace Rock.Chart
{
    /// <summary>
    /// 
    /// </summary>
    public class SummaryData : IChartData, IChartJsTimeSeriesDataPoint
    {
        /// <summary>
        /// Gets the date time stamp.
        /// </summary>
        /// <value>
        /// The date time stamp.
        /// </value>
        public long DateTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        [Previewable]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets the y value.
        /// </summary>
        /// <value>
        /// The y value.
        /// </value>
        [Previewable]
        public decimal? YValue { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the series. This will be the default name of the series if MetricValuePartitionEntityIds can't be resolved
        /// </summary>
        /// <value>
        /// The name of the series.
        /// </value>
        [Previewable]
        public string SeriesName { get; set; }

        /// <summary>
        /// Gets the metric value partitions as a comma-delimited list of EntityTypeId|EntityId
        /// </summary>
        /// <value>
        /// The metric value entityTypeId,EntityId partitions
        /// </value>
        public string MetricValuePartitionEntityIds { get; set; }

        /// <summary>
        /// Gets or sets additional information about the series (i.e. for account, would be GL#)
        /// </summary>
        /// <value>
        /// The series additional information.
        /// </value>
        [Previewable]
        public string SeriesAddlInfo { get; set; }

        #region IChartJsTimeSeriesDataPoint

        DateTime IChartJsTimeSeriesDataPoint.DateTime
        {
            get => this.DateTime;
            set => this.DateTime = value;
        }

        decimal IChartJsTimeSeriesDataPoint.Value
        {
            get => this.YValue ?? 0;
            set => this.YValue = value;
        }

        #endregion
    }
}
