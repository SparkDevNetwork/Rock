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
using System.Collections.Generic;

namespace Rock.Model
{
    /// <summary>
    /// Parameters for the AddEntitySet action.
    /// This action adds an Entity Set to the current data store with the specified properties.
    /// </summary>
    public class AddEntitySetActionOptions
    {
        /// <summary>
        /// The name of the entity set.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The identifier for the Entity Type stored in this Entity Set.
        /// All of the items added  to this set represent entities of this type.
        /// </summary>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Sets the expiry time of the entity set in minutes.
        /// If not set, a default expiry will be set.
        /// </summary>
        public int? ExpiryInMinutes { get; set; }

        /// <summary>
        /// Specifies the purpose for which this entity set is created.
        /// </summary>
        public int? PurposeValueId { get; set; }

        /// <summary>
        /// The optional identifier of a parent entity set.
        /// </summary>
        public int? ParentEntitySetId { get; set; }

        /// <summary>
        /// A note describing the entity set.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// The identifiers of the entities that form part of this set.
        /// </summary>
        public IEnumerable<int> EntityIdList { get; set; }
    }
}
