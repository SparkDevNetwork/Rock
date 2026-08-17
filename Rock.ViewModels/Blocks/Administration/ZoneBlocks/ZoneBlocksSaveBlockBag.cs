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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Administration.ZoneBlocks
{
    /// <summary>
    /// The information required to create or update a block within a zone.
    /// </summary>
    public class ZoneBlocksSaveBlockBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the block being edited, or <c>null</c> / empty to add a new block.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the scope (Page, Layout, or Site) the new block belongs to. Only used when adding.
        /// </summary>
        public BlockLocation Location { get; set; }

        /// <summary>
        /// Gets or sets the user-defined name of the block.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the block type the block implements.
        /// </summary>
        public string BlockTypeValue { get; set; }
    }
}
