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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ChartClickArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the date time value.
        /// </summary>
        /// <value>
        /// The date time value.
        /// </value>
        public DateTime DateTimeValue { get; set; }

        /// <summary>
        /// Gets or sets the y value.
        /// </summary>
        /// <value>
        /// The y value.
        /// </value>
        public decimal? YValue { get; set; }

        /// <summary>
        /// Gets or sets the series identifier.
        /// </summary>
        /// <value>
        /// The series identifier.
        /// </value>
        public string SeriesId { get; set; }

        /// <summary>
        /// Gets or sets the metric value identifier.
        /// </summary>
        /// <value>
        /// The metric value identifier.
        /// </value>
        public int? MetricValueId { get; set; }
    }
}
