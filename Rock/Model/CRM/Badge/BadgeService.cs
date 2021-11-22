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
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class BadgeService
    {
        #region Methods

        /// <summary>
        /// Gets the Guid for the Badge that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = BadgeCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }

        /// <summary>
        /// Determines if the badge applies to the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="badge"></param>
        public static bool DoesBadgeApplyToEntity( BadgeCache badge, IEntity entity )
        {
            if ( entity == null || badge == null )
            {
                return false;
            }

            // Determine the type of the entity
            var typeOfEntity = entity.GetType();

            if ( typeOfEntity.IsDynamicProxyType() )
            {
                typeOfEntity = typeOfEntity.BaseType;
            }

            var entityTypeCache = EntityTypeCache.Get( typeOfEntity );

            // Check that the type matches
            if ( entityTypeCache == null || ( badge.EntityTypeId.HasValue && entityTypeCache.Id != badge.EntityTypeId ) )
            {
                return false;
            }

            if ( badge.EntityTypeQualifierColumn.IsNullOrWhiteSpace() || badge.EntityTypeQualifierValue.IsNullOrWhiteSpace() )
            {
                // If the qualifier column or value are omitted, then the badge is not filtered
                return true;
            }

            // Get the property to which the qualifier column refers
            var qualifierProperty = typeOfEntity.GetProperties().FirstOrDefault( p => p.Name == badge.EntityTypeQualifierColumn );

            if ( qualifierProperty == null )
            {
                return false;
            }

            // Make sure the qualifier value matches the badge type specifications
            var qualifierValue = qualifierProperty.GetValue( entity );

            if ( qualifierValue == null || qualifierValue.ToString() != badge.EntityTypeQualifierValue )
            {
                return false;
            }

            // Qualifiers match
            return true;
        }

        #endregion Methods
    }
}