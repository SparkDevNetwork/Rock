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
using System.Runtime.Serialization;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a badge that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [RockObsolete( "1.10" )]
    [Obsolete( "Use BadgeCache instead.", false )]
    [Serializable]
    [DataContract]
    public class PersonBadgeCache
    {
        /// <summary>
        /// Construct a badge cache
        /// </summary>
        /// <param name="badgeCache"></param>
        public PersonBadgeCache( BadgeCache badgeCache )
        {
            Id = badgeCache.Id;
            Guid = badgeCache.Guid;
            Name = badgeCache.Name;
            Description = badgeCache.Description;
            EntityTypeId = badgeCache.BadgeComponentEntityTypeId;
            Order = badgeCache.Order;
        }

        /// <summary>
        /// Gets or sets the guid.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public Guid Guid { get; private set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the subject entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int? EntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the Entity Type.
        /// </summary>
        public EntityTypeCache EntityType
        {
            get
            {
                if ( EntityTypeId.HasValue )
                {
                    return EntityTypeCache.Get( EntityTypeId.Value );
                }

                return null;
            }
        }
    }
}