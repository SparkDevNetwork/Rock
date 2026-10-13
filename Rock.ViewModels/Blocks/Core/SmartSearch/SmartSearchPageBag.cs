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

namespace Rock.ViewModels.Blocks.Core.SmartSearch
{
    /// <summary>
    /// Represents a page the current person can navigate to from the Smart
    /// Search command palette.
    /// </summary>
    public class SmartSearchPageBag
    {
        /// <summary>
        /// Gets or sets the title of the page.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the titles of the page's ancestors, joined with a
        /// separator, such as "Finance &gt; Analytics". This is empty for a
        /// top-level page.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the URL to navigate to when the page is chosen.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the CSS class of the page's icon.
        /// </summary>
        public string IconCssClass { get; set; }
    }
}
