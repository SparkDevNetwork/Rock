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
namespace Rock.Chart
{
    /// <summary>
    /// 
    /// </summary>
    public interface IChartData
    {
        /// <summary>
        /// Gets the date time stamp.
        /// </summary>
        /// <value>
        /// The date time stamp.
        /// </value>
        long DateTimeStamp { get; }

        /// <summary>
        /// Gets the y value.
        /// </summary>
        /// <value>
        /// The y value.
        /// </value>
        decimal? YValue { get; }

        /// <summary>
        /// Gets or sets the name of the series. This will be the default name of the series if MetricValuePartitionEntityIds can't be resolved
        /// </summary>
        /// <value>
        /// The name of the series.
        /// </value>
        string SeriesName { get; }

        /// <summary>
        /// Gets the metric value partitions as a comma-delimited list of EntityTypeId|EntityId
        /// </summary>
        /// <value>
        /// The metric value entityTypeId,EntityId partitions
        /// </value>
        string MetricValuePartitionEntityIds { get; }
    }
}
