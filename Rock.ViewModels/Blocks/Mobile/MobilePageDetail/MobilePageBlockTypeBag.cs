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
    /// A single draggable block type shown in the block palette (left column).
    /// </summary>
    public class MobilePageBlockTypeBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the block type.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the display name of the block type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class for the block type.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the category the block type belongs to, used for grouping in
        /// the palette (the "By Category" mode and the "See All" headings).
        /// </summary>
        public string Category { get; set; }
    }
}
