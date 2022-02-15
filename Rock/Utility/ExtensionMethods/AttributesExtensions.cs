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
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.NonEntities;
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
        /// Gets the attribute values in a format that can be sent to remote
        /// clients in a compact and secure manner.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="entity">The entity whose attributes are requested.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">if set to <c>true</c> then security will be enforced.</param>
        /// <returns>A collection of <see cref="PublicAttributeValueViewModel" /> objects.</returns>
        [RockInternal]
        public static List<PublicAttributeValueViewModel> GetPublicAttributeValues( this IHasAttributes entity, Person currentPerson, bool enforceSecurity = true )
        {
            if ( entity == null )
            {
                return new List<PublicAttributeValueViewModel>();
            }

            return entity.AttributeValues
                .Select( av => new
                {
                    av.Value,
                    Attribute = AttributeCache.Get( av.Value.AttributeId )
                } )
                .Where( av => !enforceSecurity || av.Attribute.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Select( kvp => PublicAttributeHelper.ToPublicAttributeValue( kvp.Value ) )
                .ToList();
        }

        /// <summary>
        /// Gets the attribute values in a format that can be sent to remote
        /// clients in a compact and secure manner. This includes additional
        /// details to allow for editing the value.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="entity">The entity whose attributes are requested.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">if set to <c>true</c> then security will be enforced.</param>
        /// <returns>A collection of <see cref="PublicEditableAttributeValueViewModel" /> objects.</returns>
        [RockInternal]
        public static List<PublicEditableAttributeValueViewModel> GetPublicEditableAttributeValues( this IHasAttributes entity, Person currentPerson, bool enforceSecurity = true )
        {
            if ( entity == null )
            {
                return new List<PublicEditableAttributeValueViewModel>();
            }

            return entity.AttributeValues
                .Select( av => new
                {
                    av.Value,
                    Attribute = AttributeCache.Get( av.Value.AttributeId )
                } )
                .Where( av => !enforceSecurity || av.Attribute.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Select( kvp => PublicAttributeHelper.ToPublicEditableAttributeValue( kvp.Value ) )
                .ToList();
        }

        /// <summary>
        /// Sets attribute values that have been provided by a remote client.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This should be used to handle values the client sent back after
        ///         calling the <see cref="GetPublicEditableAttributeValues(IHasAttributes, Person, bool)"/>
        ///         method. It handles conversion from custom data formats into the
        ///         proper values to be stored in the database.
        ///     </para>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="entity">The entity.</param>
        /// <param name="attributeValues">The attribute values.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">if set to <c>true</c> then security will be enforced.</param>
        [RockInternal]
        public static void SetPublicAttributeValues( this IHasAttributes entity, Dictionary<string, string> attributeValues, Person currentPerson, bool enforceSecurity = true )
        {
            if ( entity == null || entity.Attributes == null || entity.AttributeValues == null )
            {
                return;
            }

            foreach ( var kvp in attributeValues )
            {
                if ( !entity.Attributes.ContainsKey( kvp.Key ) || !entity.AttributeValues.ContainsKey( kvp.Key ) )
                {
                    continue;
                }

                var attribute = entity.Attributes[kvp.Key];

                if ( enforceSecurity && !attribute.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson ) )
                {
                    continue;
                }

                var value = PublicAttributeHelper.GetPrivateValue( attribute, kvp.Value );

                entity.SetAttributeValue( kvp.Key, value );
            }
        }

        /// <summary>
        /// Sets a single attribute values that have been provided by a remote client.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This should be used to handle values the client sent back after
        ///         calling the <see cref="GetPublicEditableAttributeValues(IHasAttributes, Person, bool)"/>
        ///         method. It handles conversion from custom data formats into the
        ///         proper values to be stored in the database.
        ///     </para>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The value provided by the remote client.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">if set to <c>true</c> then security will be enforced.</param>
        [RockInternal]
        public static void SetPublicAttributeValue( this IHasAttributes entity, string key, string value, Person currentPerson, bool enforceSecurity = true )
        {
            if ( entity == null || entity.Attributes == null || entity.AttributeValues == null )
            {
                return;
            }

            if ( !entity.Attributes.ContainsKey( key ) || !entity.AttributeValues.ContainsKey( key ) )
            {
                return;
            }

            var attribute = entity.Attributes[key];

            if ( enforceSecurity && !attribute.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson ) )
            {
                return;
            }

            var databaseValue = PublicAttributeHelper.GetPrivateValue( attribute, value );

            entity.SetAttributeValue( key, databaseValue );
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
            {
                return authorizedAttributes;
            }

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
        [Obsolete( "Use ToAttributeCacheList instead", true )]
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

        /// <summary>
        /// Query by AttributeIds. This is optimized to execute about 10-15ms more quickly than doing a Contains statement.
        /// </summary>
        /// <param name="attributeValueQuery">The attribute value query.</param>
        /// <param name="attributeIds">The attribute ids.</param>
        /// <returns>IQueryable&lt;AttributeValue&gt;.</returns>
        public static IQueryable<AttributeValue> WhereAttributeIds( this IQueryable<Rock.Model.AttributeValue> attributeValueQuery, List<int> attributeIds )
        {
            if ( attributeIds.Count != 1 )
            {
                if ( attributeIds.Count >= 1000 )
                {
                    // the linq Expression.Or tree gets too big if there is more than 1000 attributes, so just do a contains instead
                    attributeValueQuery = attributeValueQuery.Where( v => attributeIds.Contains( v.AttributeId ) );
                }
                else
                {
                    // a Linq query that uses 'Contains' can't be cached in the EF Plan Cache, so instead of doing a Contains, build a List of OR conditions. This can save 15-20ms per call (and still ends up with the exact same SQL)
                    var parameterExpression = Expression.Parameter( typeof( AttributeValue ), "p" );
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, "AttributeId" );
                    Expression expression = null;

                    foreach ( var attributeId in attributeIds )
                    {
                        Expression attributeIdValue = Expression.Constant( attributeId );
                        if ( expression != null )
                        {
                            expression = Expression.Or( expression, Expression.Equal( propertyExpression, attributeIdValue ) );
                        }
                        else
                        {
                            expression = Expression.Equal( propertyExpression, attributeIdValue );
                        }
                    }

                    attributeValueQuery = attributeValueQuery.Where( parameterExpression, expression );
                }
            }
            else
            {
                int attributeId = attributeIds[0];
                attributeValueQuery = attributeValueQuery.Where( v => v.AttributeId == attributeId );
            }

            return attributeValueQuery;
        }

        #endregion IHasAttributes extensions
    }
}
