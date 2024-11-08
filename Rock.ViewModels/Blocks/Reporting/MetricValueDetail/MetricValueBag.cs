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

using System.Collections.Generic;

using Rock.ViewModels.Utility;
using Rock.Model;
using System;

namespace Rock.ViewModels.Blocks.Reporting.MetricValueDetail
{
    /// <summary>
    /// The item details for the Metric Value Detail block.
    /// </summary>
    public class MetricValueBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the metric value partitions.
        /// </summary>
        public List<MetricValuePartitionBag> MetricValuePartitions { get; set; }

        /// <summary>
        /// Gets or sets the type of the metric value.
        /// </summary>
        public int MetricValueType { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the X axis value.
        /// Note that in Rock, graphs typically actually use the MetricValue.MetricValueDateTime as the graph's X Axis.
        /// Therefore, in most cases, Metric.XAxisLabel and MetricValue.XAxis are NOT used
        /// </summary>
        public string XValue { get; set; }

        /// <summary>
        /// Gets or sets the Y axis value.
        /// </summary>
        public decimal? YValue { get; set; }

        /// <summary>
        /// Gets or sets the metric value date time.
        /// </summary>
        /// <value>
        /// The metric value date time.
        /// </value>
        public DateTime? MetricValueDateTime { get; set; }
    }
}
