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
using System;

namespace Rock.ViewModels.Blocks.Core.TaggedItemList
{
    /// <summary>
    /// Represents a tagged item list bag.
    /// </summary>
    public class TaggedItemListBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity's globally unique identifier.
        /// </summary>
        public Guid EntityGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }
    }
}
