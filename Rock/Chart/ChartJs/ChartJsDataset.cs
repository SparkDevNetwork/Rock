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

namespace Rock.Chart
{
    /// <summary>
    /// A set of data points and configuration options for a dataset that can be plotted on a ChartJs chart.
    /// </summary>
    public class ChartJsDataset<TDataPoint>
    {
        /// <summary>
        /// The name of the dataset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The color of the border or outline of the region included in this dataset.
        /// </summary>
        public string BorderColor { get; set; }

        /// <summary> 
        /// The fill color of the region described by this dataset.
        /// </summary>
        public string FillColor { get; set; }

        /// <summary>
        /// The set of data points that are used to plot the chart.
        /// </summary>
        public List<TDataPoint> DataPoints { get; set; }
    }
}