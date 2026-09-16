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
    /// A single day's view count for the Views Over Time chart.
    /// </summary>
    public class ViewsOverTimePointBag
    {
        /// <summary>
        /// Gets or sets the date of the point, as an ISO date string (yyyy-MM-dd).
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the number of views on this date.
        /// </summary>
        public int Count { get; set; }
    }
}
