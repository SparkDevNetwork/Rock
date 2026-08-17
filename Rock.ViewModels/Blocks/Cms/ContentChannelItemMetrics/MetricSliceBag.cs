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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemMetrics
{
    /// <summary>
    /// A single labeled slice of a breakdown metric (for example, one device type or status).
    /// </summary>
    public class MetricSliceBag
    {
        /// <summary>
        /// Gets or sets the display label for the slice.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the view count for the slice.
        /// </summary>
        public int Count { get; set; }
    }
}
