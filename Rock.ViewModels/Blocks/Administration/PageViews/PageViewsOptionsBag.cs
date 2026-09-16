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

namespace Rock.ViewModels.Blocks.Administration.PageViews
{
    /// <summary>
    /// The additional configuration options for the Page Views block.
    /// </summary>
    public class PageViewsOptionsBag
    {
        /// <summary>
        /// Gets or sets the title displayed above the grid (for example, "Home Page Views").
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the prefix applied to the filter preference keys so that each
        /// page remembers its own filter selections. This is empty when there is no
        /// page in context.
        /// </summary>
        public string PreferenceKeyPrefix { get; set; }
    }
}
