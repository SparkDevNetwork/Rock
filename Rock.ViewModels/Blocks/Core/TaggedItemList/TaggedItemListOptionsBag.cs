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
    public class TaggedItemListOptionsBag
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; set; }


        /// <summary>
        /// Gets the tag Name.
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Gets the Entity Type Name.
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// Gets the Entity Type GUID.
        /// </summary>
        public string EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets if the Tag is a Person Tag.
        /// </summary>
        public bool IsPersonTag { get; internal set; }

        /// <summary>
        /// Gets if the block needs to be hidden.
        /// </summary>
        public bool IsBlockHidden { get; set; }

        /// <summary>
        /// Gets the Tag Id.
        /// </summary>
        public int TagId { get; set; }
    }
}
