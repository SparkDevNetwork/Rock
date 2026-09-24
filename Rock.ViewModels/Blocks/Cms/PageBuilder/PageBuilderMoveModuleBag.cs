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

namespace Rock.ViewModels.Blocks.Cms.PageBuilder
{
    /// <summary>
    /// Describes a Canvas block dragged by its handle to a new position on the target page, for the Page Builder to move.
    /// </summary>
    public class PageBuilderMoveModuleBag
    {
        /// <summary>
        /// Gets or sets the identifier of the Canvas block being moved.
        /// </summary>
        public int BlockId { get; set; }

        /// <summary>
        /// Gets or sets the key of the zone the block was dropped in.
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the block the moved block is placed in front of,
        /// or <c>null</c> to place it last in the zone.
        /// </summary>
        public int? BeforeBlockId { get; set; }
    }
}
