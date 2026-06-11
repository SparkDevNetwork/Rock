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

namespace Rock.ViewModels.Blocks.Communication.SnippetList
{
    /// <summary>
    /// The additional configuration options for the Snippet List block.
    /// </summary>
    public class SnippetListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Personal column
        /// should be displayed in the grid.
        /// </summary>
        public bool IsShowPersonalColumnEnabled { get; set; }

        /// <summary>
        /// Gets or sets the title to display for the grid, derived from
        /// the configured snippet type name (e.g., "Email Snippets").
        /// </summary>
        public string Title { get; set; }
    }
}
