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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Chart
{
    /// <summary>
    /// Represents a container for managing a set of data points of type TDataPoint that define one or more series in a Flot Chart.
    /// </summary>
    [RockObsolete( "1.14.0" )]
    [Obsolete( "Use the ChartJsTimeSeriesDataFactory instead." )]
    public class FlotChartDataSet<TDataPoint>
        where TDataPoint : IChartData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlotChartDataSet{TDataPoint}"/> class.
        /// </summary>
        public FlotChartDataSet()
        {
            DataPoints = new List<TDataPoint>();
        }

        private List<TDataPoint> _DataPoints = new List<TDataPoint>();

        /// <summary>
        /// A collection of data points that are displayed on the chart as one or more series of data.
        /// </summary>
        public List<TDataPoint> DataPoints
        {
            get { return _DataPoints; }
            set
            {
                _DataPoints = value ?? new List<TDataPoint>();
            }
        }

        /// <summary>
        /// Does the data set contain any data points?
        /// </summary>
        public bool HasData
        {
            get
            {
                return _DataPoints != null && _DataPoints.Any();
            }
        }

        /// <summary>
        /// Get the list of unique series names contained in this data set.
        /// </summary>
        /// <returns></returns>
        public List<string> GetSeriesNames()
        {
            var series = this.DataPoints
                .Where( x => !string.IsNullOrWhiteSpace( x.SeriesName ) )
                .Select( x => x.SeriesName )
                .Distinct()
                .ToList();

            return series;
        }

        /// <summary>
        /// Get the chart data points as JSON data that is compatible for use with the Rock Chart component.
        /// The Rock FlotChart component requires specific property names to be used for the data points, so here we ensure that our data conforms to this requirement.
        /// </summary>
        /// <returns></returns>
        public string GetRockChartJsonData()
        {
            // Serialize the data points to JSON using the IChartData interface properties only.
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.JsonInterfaceContractResolver<IChartData>()
            };

            // The chart component requires the data to be ordered by series and datestamp.
            var orderedDataPoints = this.DataPoints.OrderBy( x => x.SeriesName ).ThenBy( x => x.DateTimeStamp );

            string chartDataJson = JsonConvert.SerializeObject( orderedDataPoints, Formatting.None, jsonSetting );

            return chartDataJson;
        }
    }
}