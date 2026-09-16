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

namespace Rock.ViewModels.Blocks.Mobile.MobilePageDetail
{
    /// <summary>
    /// A custom action button contributed by a block type, shown on the block row
    /// in the builder.
    /// </summary>
    public class MobilePageBlockActionBag
    {
        /// <summary>
        /// Gets or sets the icon CSS class for the action button.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the tooltip describing the action.
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Obsidian component file that renders the action.
        /// </summary>
        public string ComponentFileUrl { get; set; }
    }
}
