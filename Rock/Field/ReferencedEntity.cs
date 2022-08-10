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

namespace Rock.Field
{
    /// <summary>
    /// Identifies a single entity that is referenced by a field type value.
    /// </summary>
    internal class ReferencedEntity
    {
        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>The entity type identifier.</value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>The entity identifier.</value>
        public int EntityId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencedEntity"/> class.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        public ReferencedEntity( int entityTypeId, int entityId )
        {
            EntityTypeId = entityTypeId;
            EntityId = entityId;
        }
    }
}
