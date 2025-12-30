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

namespace Rock.Core
{
    /// <summary>
    /// Summarizes how an entity is linked to other entities. This will be stored
    /// in metadata on the entity that this instance is linked to.
    /// </summary>
    public class LinkageSummary
    {
        /// <summary>
        /// The identifier of the <see cref="Model.EntityType"/> that represents
        /// the type of entity for <see cref="EntityId"/>.
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// The identifier of the entity that is represented by this instance.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// The value to display as the summary text of the entity. This is
        /// typically the name or title of the entity.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// If the entity represented by this instance has a parent entity that
        /// should be included when displaying the linkage summary, this will
        /// be set to an instance of <see cref="LinkageSummary"/> that represents
        /// that parent entity.
        /// </summary>
        public LinkageSummary Parent { get; set; }
    }
}
