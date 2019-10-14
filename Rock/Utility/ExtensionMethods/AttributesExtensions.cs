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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Rock.Attribute Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region IHasAttributes extensions

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public static void LoadAttributes( this IHasAttributes entity )
        {
            Attribute.Helper.LoadAttributes( entity );
        }

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void LoadAttributes( this IHasAttributes entity, RockContext rockContext )
        {
            Attribute.Helper.LoadAttributes( entity, rockContext );
        }

        /// <summary>
        /// Loads the attributes for all entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public static void LoadAttributes( this IEnumerable<IHasAttributes> entities )
        {
            foreach ( var entity in entities )
            {
                Attribute.Helper.LoadAttributes( entity );
            }
        }

        /// <summary>
        /// Loads the attributes for all entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void LoadAttributes( this IEnumerable<IHasAttributes> entities, RockContext rockContext )
        {
            foreach ( var entity in entities )
            {
                Attribute.Helper.LoadAttributes( entity, rockContext );
            }
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveAttributeValues( this IHasAttributes entity, RockContext rockContext = null )
        {
            Attribute.Helper.SaveAttributeValues( entity, rockContext );
        }

        /// <summary>
        /// Saves the specified attribute values to the database.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="keys">The attribute keys.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveAttributeValues( this IHasAttributes entity, IEnumerable<string> keys, RockContext rockContext = null )
        {
            foreach ( var key in keys )
            {
                if ( entity.AttributeValues.ContainsKey( key ) )
                {
                    Attribute.Helper.SaveAttributeValue( entity, entity.Attributes[key], entity.AttributeValues[key].Value, rockContext );
                }
            }
        }

        /// <summary>
        /// Saves an attribute value to the database.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The attribute key.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveAttributeValue( this IHasAttributes entity, string key, RockContext rockContext = null )
        {
            if ( entity.AttributeValues.ContainsKey( key ) )
            {
                Attribute.Helper.SaveAttributeValue( entity, entity.Attributes[key], entity.AttributeValues[key].Value, rockContext );
            }
        }

        /// <summary>
        /// Copies the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="source">The source.</param>
        public static void CopyAttributesFrom( this IHasAttributes entity, IHasAttributes source )
        {
            Attribute.Helper.CopyAttributes( source, entity );
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetAttributeValue( this IHasAttributes entity, string key, int? value )
        {
            entity.SetAttributeValue( key, value.ToString() );
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetAttributeValue( this IHasAttributes entity, string key, decimal? value )
        {
            entity.SetAttributeValue( key, value.ToString() );
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetAttributeValue( this IHasAttributes entity, string key, Guid? value )
        {
            entity.SetAttributeValue( key, value.ToString() );
        }

        /// <summary>
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetAttributeValue( this IHasAttributes entity, string key, DateTime? value )
        {
            entity.SetAttributeValue( key, value?.ToString( "o" ) ?? string.Empty );
        }

        /// <summary>
        /// Gets the authorized attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static Dictionary<string, AttributeCache> GetAuthorizedAttributes( this IHasAttributes entity, string action, Person person )
        {
            var authorizedAttributes = new Dictionary<string, AttributeCache>();

            if ( entity == null )
                return authorizedAttributes;

            foreach ( var item in entity.Attributes )
            {
                if ( item.Value.IsAuthorized( action, person ) )
                {
                    authorizedAttributes.Add( item.Key, item.Value );
                }
            }

            return authorizedAttributes;
        }

        /// <summary>
        /// Selects just the Id from the Attribute Query and reads the Ids into a list of AttributeCache
        /// </summary>
        /// <param name="attributeQuery">The attribute query.</param>
        /// <returns></returns>
        [Obsolete( "Use ToAttributeCacheList instead" )]
        [RockObsolete( "1.9" )]
        public static List<AttributeCache> ToCacheAttributeList( this IQueryable<Rock.Model.Attribute> attributeQuery )
        {
            return attributeQuery.ToAttributeCacheList();
        }

        /// <summary>
        /// Selects just the Id from the Attribute Query and reads the Ids into a list of AttributeCache
        /// </summary>
        /// <param name="attributeQuery">The attribute query.</param>
        /// <returns></returns>
        public static List<AttributeCache> ToAttributeCacheList( this IQueryable<Rock.Model.Attribute> attributeQuery )
        {
            return attributeQuery.AsNoTracking().Select( a => a.Id ).ToList().Select( a => AttributeCache.Get( a ) ).ToList().Where( a => a != null ).ToList();
        }

        #endregion IHasAttributes extensions

    }
}
