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

namespace Rock.ViewModels.Blocks.Administration.ZoneBlocks
{
    /// <summary>
    /// The instance information needed to render the Zone Blocks block.
    /// </summary>
    public class ZoneBlocksBag
    {
        /// <summary>
        /// Gets or sets the name of the zone whose blocks are being managed.
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// Gets or sets the name of the layout used by the page being edited. Shown on the Layout tab.
        /// </summary>
        public string LayoutName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person may administrate the page being
        /// edited. When <c>false</c> the block renders a not-authorized notice and no editing UI.
        /// </summary>
        public bool CanAdministrate { get; set; }

        /// <summary>
        /// Gets or sets the number of blocks in the Page scope of the zone, used to size the grid's
        /// loading skeleton so it does not flash the full modal height before the rows load.
        /// </summary>
        public int PageBlockCount { get; set; }

        /// <summary>
        /// Gets or sets the number of blocks in the Layout scope of the zone, used to size the grid's
        /// loading skeleton so it does not flash the full modal height before the rows load.
        /// </summary>
        public int LayoutBlockCount { get; set; }

        /// <summary>
        /// Gets or sets the number of blocks in the Site scope of the zone, used to size the grid's
        /// loading skeleton so it does not flash the full modal height before the rows load.
        /// </summary>
        public int SiteBlockCount { get; set; }
    }
}
