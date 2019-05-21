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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.Tag"/> entity objects.
    /// </summary>
    public partial class TagService
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Tag">Tags</see> by EntityType, Qualifier Column, Qualifier Value and Owner.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete("Use one of the other Gets", true )]
        public IQueryable<Tag> Get( int entityTypeId, string entityQualifierColumn, string entityQualifierValue, int? ownerId )
        {
            return this.Get( entityTypeId, entityQualifierColumn, entityQualifierValue, ownerId, null, null );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Tag">Tags</see> by EntityType, Qualifier Column, Qualifier Value and Owner.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32" /> representing the EntityTypeID of the <see cref="Rock.Model.EntityType" /> of the entities that are eligible for the <see cref="Rock.Model.Tag" />.</param>
        /// <param name="entityQualifierColumn">A <see cref="System.String" /> that represents the EntityQualifierColumn of the <see cref="Rock.Model.Tag" />. This value can be null.</param>
        /// <param name="entityQualifierValue">A <see cref="System.String" /> that represents the EntityQualifierValue of the <see cref="Rock.Model.Tag" />. This value can be null.</param>
        /// <param name="ownerId">A <see cref="System.Int32" /> representing the <see cref="Rock.Model.Tag" /> owner's PersonId. If the <see cref="Rock.Model.Tag" /> is public this value can be null.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="includeInactive">The include inactive.</param>
        /// <returns>
        /// A queryable collection of <see cref="Rock.Model.Tag">Tags</see> that match the provided criteria.
        /// </returns>
        public IQueryable<Tag> Get( int entityTypeId, string entityQualifierColumn, string entityQualifierValue, int? ownerId, Guid? categoryGuid, bool? includeInactive )
        {
            var qry = Queryable()
                .Where( t =>
                    ( !t.EntityTypeId.HasValue || t.EntityTypeId == entityTypeId ) &&
                    ( t.OwnerPersonAlias == null || ( ownerId.HasValue && t.OwnerPersonAlias.PersonId == ownerId ) ) );


            if ( entityQualifierColumn.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( t => t.EntityTypeQualifierColumn == entityQualifierColumn );
            }

            if ( entityQualifierValue.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( t => t.EntityTypeQualifierValue == entityQualifierValue );
            }

            if ( categoryGuid.HasValue )
            {
                qry = qry.Where( t => t.Category.Guid == categoryGuid.Value );
            }

            if ( !includeInactive.HasValue || !includeInactive.Value )
            {
                qry = qry.Where( t => t.IsActive );
            }

            return qry.OrderBy( t => t.Name );
        }

        /// <summary>
        /// Returns an <see cref="Rock.Model.Tag" /> by EntityType, Qualifier Column, Qualifier Value, Owner and Tag Name.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32" /> representing the EntityTypeID of the <see cref="Rock.Model.EntityType" /> of entities that are eligible for the <see cref="Rock.Model.Tag" />.</param>
        /// <param name="entityQualifierColumn">A <see cref="System.String" /> representing the EntityQualifierColumn of the <see cref="Rock.Model.Tag" />.
        /// If the <see cref="Rock.Model.Tag" /> does not have a EntityQualifierColumn associated with it, this value can be null.</param>
        /// <param name="entityQualifierValue">A <see cref="System.String" /> representing the EntityQualifierValue of the <see cref="Rock.Model.Tag" />.
        /// If the <see cref="Rock.Model.Tag" /> does not have a EntityQualifierValue associated with it, this value can be null.</param>
        /// <param name="ownerId">A <see cref="System.Int32" /> representing the owner's PersonId.</param>
        /// <param name="name">A <see cref="System.String" /> representing the Name of the <see cref="Rock.Model.Tag" />.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Tag" /> that matches the provided criteria. If a match is not found, null will be returned.
        /// </returns>
        public Tag Get( int entityTypeId, string entityQualifierColumn, string entityQualifierValue, int? ownerId, string name )
        {
            return Get( entityTypeId, entityQualifierColumn, entityQualifierValue, ownerId, name, null, false );
        }

        /// <summary>
        /// Returns an <see cref="Rock.Model.Tag" /> by EntityType, Qualifier Column, Qualifier Value, Owner and Tag Name.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32" /> representing the EntityTypeID of the <see cref="Rock.Model.EntityType" /> of entities that are eligible for the <see cref="Rock.Model.Tag" />.</param>
        /// <param name="entityQualifierColumn">A <see cref="System.String" /> representing the EntityQualifierColumn of the <see cref="Rock.Model.Tag" />.
        /// If the <see cref="Rock.Model.Tag" /> does not have a EntityQualifierColumn associated with it, this value can be null.</param>
        /// <param name="entityQualifierValue">A <see cref="System.String" /> representing the EntityQualifierValue of the <see cref="Rock.Model.Tag" />.
        /// If the <see cref="Rock.Model.Tag" /> does not have a EntityQualifierValue associated with it, this value can be null.</param>
        /// <param name="ownerId">A <see cref="System.Int32" /> representing the owner's PersonId.</param>
        /// <param name="name">A <see cref="System.String" /> representing the Name of the <see cref="Rock.Model.Tag" />.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="includeInactive">The include inactive.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Tag" /> that matches the provided criteria. If a match is not found, null will be returned.
        /// </returns>
        public Tag Get( int entityTypeId, string entityQualifierColumn, string entityQualifierValue, int? ownerId, string name, Guid? categoryGuid, bool? includeInactive )
        {
            var tags = Get( entityTypeId, entityQualifierColumn, entityQualifierValue, ownerId, categoryGuid, includeInactive )
                .Where( t => t.Name == name );

            // First look for personal tag
            if ( ownerId.HasValue )
            {
                var personalTag = tags.Where( t => t.OwnerPersonAlias != null && t.OwnerPersonAlias.PersonId == ownerId.Value ).FirstOrDefault();
                if ( personalTag != null )
                {
                    return personalTag;
                }
            }

            // Then look for first org tag that they are authorized to see
            var orgTags = tags.Where( t => t.OwnerPersonAlias == null ).ToList();
            if ( orgTags.Any() )
            {
                Person person = ownerId.HasValue ? new PersonService( (RockContext)this.Context ).Get( ownerId.Value ) : null;
                foreach ( var tag in orgTags )
                {
                    if ( tag.IsAuthorized( Security.Authorization.VIEW, person ) )
                    {
                        return tag;
                    }
                }
            }

            return null;
        }
    }
}
