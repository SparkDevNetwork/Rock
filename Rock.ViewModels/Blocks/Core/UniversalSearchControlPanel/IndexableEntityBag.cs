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

namespace Rock.ViewModels.Blocks.Core.UniversalSearchControlPanel
{
    /// <summary>
    /// Contains the display information for an indexable entity type in the grid.
    /// </summary>
    public class IndexableEntityBag
    {
        /// <summary>
        /// Gets or sets the IdKey of the entity type.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indexing is currently enabled for this entity type.
        /// </summary>
        public bool IsIndexingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entity type supports interactive bulk indexing.
        /// </summary>
        public bool AllowsInteractiveBulkIndexing { get; set; }
    }
}
