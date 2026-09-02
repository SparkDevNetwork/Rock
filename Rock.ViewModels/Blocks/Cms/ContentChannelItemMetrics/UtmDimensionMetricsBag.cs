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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemMetrics
{
    /// <summary>
    /// The ranked breakdown of captured values for a single UTM dimension.
    /// </summary>
    public class UtmDimensionMetricsBag
    {
        /// <summary>
        /// Gets or sets the UTM dimension this breakdown represents.
        /// </summary>
        public UtmDimension Dimension { get; set; }

        /// <summary>
        /// Gets or sets the captured values for the dimension, ordered highest to lowest count.
        /// </summary>
        public List<MetricSliceBag> Items { get; set; }
    }
}
