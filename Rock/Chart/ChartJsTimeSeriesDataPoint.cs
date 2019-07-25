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

namespace Rock.Chart
{
    /// <summary>
    /// A chart data point that represents a value at a specific instant in time, suitable for use with a value-over-time chart.
    /// </summary>
    public interface IChartJsTimeSeriesDataPoint
    {
        DateTime DateTime { get; set; }
        decimal Value { get; set; }
    }

    /// <summary>
    /// A chart data point that represents a value at a specific instant in time, suitable for use with a value-over-time chart.
    /// </summary>
    public class ChartJsTimeSeriesDataPoint : IChartJsTimeSeriesDataPoint
    {
        private DateTime _DateTime = DateTime.MinValue;
        private long _DateTimeStamp = 0;

        /// <summary>
        /// The date of this data point, plotted on the X-axis.
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                return _DateTime;
            }
            set
            {
                _DateTime = value;

                // Set the DateTimeStamp property as a JavaScript datetime stamp (number of milliseconds elapsed since 1/1/1970 00:00:00 UTC)
                // This measure is required by the Chart.js component.
                if ( _DateTime == null )
                {
                    _DateTimeStamp = 0;
                }
                else
                {
                    _DateTimeStamp = _DateTime.Date.ToJavascriptMilliseconds();
                }
            }
        }

        /// <summary>
        /// The value of this data point, plotted on the Y-axis.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets an arbitrary sort key that can be used to sort this data point within the data set.
        /// </summary>
        public string SortKey { get; set; }
    }
}