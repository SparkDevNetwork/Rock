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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.PersonProfile;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a badge that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class BadgeCache : ModelCache<BadgeCache, Badge>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; private set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; private set; }

        /// <summary>
        /// Gets or sets the badge component entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? BadgeComponentEntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the subject entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets the Entity Type.
        /// </summary>
        public EntityTypeCache BadgeComponentEntityType
        {
            get
            {
                if ( BadgeComponentEntityTypeId.HasValue )
                {
                    return EntityTypeCache.Get( BadgeComponentEntityTypeId.Value );
                }

                return null;
            }
        }

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

        /// <summary>
        /// Gets the badge component.
        /// </summary>
        /// <value>
        /// The badge component.
        /// </value>
        public virtual BadgeComponent BadgeComponent => BadgeComponentEntityType != null ? BadgeContainer.GetComponent( BadgeComponentEntityType.Name ) : null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns all of the badges that apply to the given type (for example Person or Group).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static List<BadgeCache> All( Type type )
        {
            var entityTypeCache = EntityTypeCache.Get( type );

            if ( entityTypeCache == null )
            {
                return new List<BadgeCache>();
            }

            return All( entityTypeCache.Id );
        }

        /// <summary>
        /// Returns all of the badges that apply to the given type (for example Person or Group).
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <returns></returns>
        public static List<BadgeCache> All( int entityTypeId )
        {
            var allBadges = All();
            return allBadges.Where( b => !b.EntityTypeId.HasValue || b.EntityTypeId.Value == entityTypeId ).ToList();
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var badge = entity as Badge;
            if ( badge == null )
            {
                return;
            }

            Name = badge.Name;
            Description = badge.Description;
            BadgeComponentEntityTypeId = badge.BadgeComponentEntityTypeId;
            EntityTypeId = badge.EntityTypeId;
            Order = badge.Order;
            EntityTypeQualifierColumn = badge.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = badge.EntityTypeQualifierValue;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

    }
}