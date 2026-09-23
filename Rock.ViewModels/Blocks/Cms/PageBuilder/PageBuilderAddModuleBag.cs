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
    /// Describes a module dropped onto the target page, for the Page Builder to add in a new Canvas block.
    /// </summary>
    public class PageBuilderAddModuleBag
    {
        /// <summary>
        /// Gets or sets the key of the zone the module was dropped in.
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// Gets or sets the key of the module type that was dropped.
        /// </summary>
        public string ModuleTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the block the new Canvas block is placed in front of,
        /// or <c>null</c> to place it last in the zone.
        /// </summary>
        public int? BeforeBlockId { get; set; }
    }
}
