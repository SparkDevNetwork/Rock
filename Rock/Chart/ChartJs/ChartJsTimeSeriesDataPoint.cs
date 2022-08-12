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
    /// A chart data point that represents a value at a specific instant in time, and allows sorting by a specified key.
    /// </summary>
    public class ChartJsTimeSeriesDataPoint : IChartJsTimeSeriesDataPoint
    {
        private long _DateTimeStamp = 0;

        /// <summary>
        /// The date of this data point, plotted on the X-axis.
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                return GetDateTimeFromJavascriptMilliseconds( _DateTimeStamp );
            }
            set
            {
                // Set the DateTimeStamp property as a JavaScript datetime stamp (number of milliseconds elapsed since 1/1/1970 00:00:00 UTC)
                // This measure is required by the Chart.js component.
                if ( value == null )
                {
                    _DateTimeStamp = 0;
                }
                else
                {
                    _DateTimeStamp = value.Date.ToJavascriptMilliseconds();
                }
            }
        }

        /// <summary>
        /// A JavaScript datetime stamp, representing the number of milliseconds elapsed since the Unix epoch (1/1/1970 00:00:00 UTC).
        /// This measure is required by the ChartJS component.
        /// </summary>
        public long DateTimeStamp
        {
            get
            {
                return _DateTimeStamp;
            }
            set
            {
                _DateTimeStamp = value;
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

        private static DateTime GetDateTimeFromJavascriptMilliseconds( long millisecondsAfterEpoch )
        {
            return new DateTime( 1970, 1, 1 ).AddTicks( millisecondsAfterEpoch * 10000 );
        }
    }
}