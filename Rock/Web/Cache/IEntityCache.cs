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

using Rock.Data;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Interface for EntityCache items
    /// </summary>
    public interface IEntityCache : IItemCache
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int Id { get; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        Guid Guid { get; }

        /// <summary>
        /// Gets or sets the foreign identifier.
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        int? ForeignId { get; }

        /// <summary>
        /// Gets or sets the foreign unique identifier.
        /// </summary>
        /// <value>
        /// The foreign unique identifier.
        /// </value>
        Guid? ForeignGuid { get; }

        /// <summary>
        /// Gets or sets the foreign key.
        /// </summary>
        /// <value>
        /// The foreign key.
        /// </value>
        string ForeignKey { get; }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void SetFromEntity( IEntity entity );

        /// <summary>
        /// The EntityType of the cached entity
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        int CachedEntityTypeId { get; }
    }
}
