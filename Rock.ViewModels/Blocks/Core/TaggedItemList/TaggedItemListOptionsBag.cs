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

namespace Rock.ViewModels.Blocks.Core.TaggedItemList
{
    /// <summary>
    /// Represents options for a tagged item list.
    /// </summary>
    public class TaggedItemListOptionsBag
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity type.
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the globally unique identifier of the entity type.
        /// </summary>
        public string EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a person tag.
        /// </summary>
        public bool IsPersonTag { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is hidden.
        /// </summary>
        public bool IsBlockHidden { get; set; }

        /// <summary>
        /// Gets or sets the tag identifier.
        /// </summary>
        public int TagId { get; set; }
    }
}
