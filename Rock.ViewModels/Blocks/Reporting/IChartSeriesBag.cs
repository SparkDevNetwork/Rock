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
