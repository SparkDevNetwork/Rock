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
using Rock.Field;
using Rock.Model;
using Rock.ViewModels;
using Rock.ViewModels.Utility;
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
            LoadAttributes( entities, null );
        }

        /// <summary>
        /// Loads the attributes for all entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void LoadAttributes( this IEnumerable<IHasAttributes> entities, RockContext rockContext )
        {
            Attribute.Helper.LoadAttributes( entities, rockContext, null );
        }

        /// <summary>
        /// Loads the filtered attributes.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="attributeFilter">The attribute filter.</param>
        internal static void LoadFilteredAttributes( this IEnumerable<IHasAttributes> entities, Func<AttributeCache, bool> attributeFilter )
        {
            Attribute.Helper.LoadFilteredAttributes( entities, null, attributeFilter );
        }

        /// <summary>
        /// Loads the filtered attributes.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attributeFilter">The attribute filter.</param>
        internal static void LoadFilteredAttributes( this IEnumerable<IHasAttributes> entities, RockContext rockContext, Func<AttributeCache, bool> attributeFilter )
        {
            Attribute.Helper.LoadFilteredAttributes( entities, rockContext, attributeFilter );
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
        /// Populates a view model with attributes and values from the entity for
        /// purpose of viewing the values.
        /// </summary>
        /// <param name="viewModel">The view model to be populated.</param>
        /// <param name="entity">The entity whose attributes are requested.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">If set to <c>true</c> then security will be enforced.</param>
        /// <param name="attributeFilter">If not <c>null</c> then this specifies a function to call to filter which attributes to include. This filtering will take place after the security check.</param>
        public static void LoadAttributesAndValuesForPublicView( this IViewModelWithAttributes viewModel, IHasAttributes entity, Person currentPerson, bool enforceSecurity = true, Func<AttributeCache, bool> attributeFilter = null )
        {
            viewModel.Attributes = GetPublicAttributesForView( entity, currentPerson, enforceSecurity, attributeFilter );
            viewModel.AttributeValues = GetPublicAttributeValuesForView( entity, currentPerson, enforceSecurity, attributeFilter );
        }

        /// <summary>
        /// Populates a view model with attributes and values from the entity for
        /// the purpose of editing the values.
        /// </summary>
        /// <param name="viewModel">The view model to be populated.</param>
        /// <param name="entity">The entity whose attributes are requested.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">If set to <c>true</c> then security will be enforced.</param>
        /// <param name="attributeFilter">If not <c>null</c> then this specifies a function to call to filter which attributes to include. This filtering will take place after the security check.</param>
        public static void LoadAttributesAndValuesForPublicEdit( this IViewModelWithAttributes viewModel, IHasAttributes entity, Person currentPerson, bool enforceSecurity = true, Func<AttributeCache, bool> attributeFilter = null )
        {
            viewModel.Attributes = GetPublicAttributesForEdit( entity, currentPerson, enforceSecurity, attributeFilter );
            viewModel.AttributeValues = GetPublicAttributeValuesForEdit( entity, currentPerson, enforceSecurity, attributeFilter );
        }

        /// <summary>
        /// Gets the attributes in a format that can be sent to public devices
        /// for the purpose of displaying the current value.
        /// </summary>
        /// <param name="entity">The entity whose attributes are requested.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">If set to <c>true</c> then security will be enforced.</param>
        /// <param name="attributeFilter">If not <c>null</c> then this specifies a function to call to filter which attributes to include. This filtering will take place after the security check.</param>
        /// <returns>A dictionary that represents the attribute values.</returns>
        public static Dictionary<string, PublicAttributeBag> GetPublicAttributesForView( this IHasAttributes entity, Person currentPerson, bool enforceSecurity = true, Func<AttributeCache, bool> attributeFilter = null )
        {
            if ( entity == null || entity.Attributes == null )
            {
                return new Dictionary<string, PublicAttributeBag>();
            }

            return entity.Attributes
                .Select( a => new
                {
                    Value = entity.GetAttributeValue( a.Key ),
                    Attribute = a.Value
                } )
                .Where( av => !enforceSecurity || av.Attribute.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Where( av => attributeFilter == null || attributeFilter( av.Attribute ) )
                .ToDictionary( av => av.Attribute.Key, kvp => PublicAttributeHelper.GetPublicAttributeForView( kvp.Attribute, kvp.Value ) );
        }

        /// <summary>
        /// Gets the attribute values in a format that can be sent to public
        /// devices for the purpose of displaying the value.
        /// </summary>
        /// <param name="entity">The entity whose attributes are requested.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">If set to <c>true</c> then security will be enforced.</param>
        /// <param name="attributeFilter">If not <c>null</c> then this specifies a function to call to filter which attributes to include. This filtering will take place after the security check.</param>
        /// <returns>A dictionary that represents the attribute values.</returns>
        public static Dictionary<string, string> GetPublicAttributeValuesForView( this IHasAttributes entity, Person currentPerson, bool enforceSecurity = true, Func<AttributeCache, bool> attributeFilter = null )
        {
            if ( entity == null || entity.Attributes == null )
            {
                return new Dictionary<string, string>();
            }

            return entity.Attributes
                .Select( a => new
                {
                    Value = entity.GetAttributeValue( a.Key ),
                    Attribute = a.Value
                } )
                .Where( av => !enforceSecurity || av.Attribute.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Where( av => attributeFilter == null || attributeFilter( av.Attribute ) )
                .ToDictionary( av => av.Attribute.Key, kvp => PublicAttributeHelper.GetPublicValueForView( kvp.Attribute, kvp.Value ) );
        }

        /// <summary>
        /// Gets the attributes in a format that can be sent to public devices
        /// for the purpose of editing values.
        /// </summary>
        /// <param name="entity">The entity whose attributes are requested.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">If set to <c>true</c> then security will be enforced.</param>
        /// <param name="attributeFilter">If not <c>null</c> then this specifies a function to call to filter which attributes to include. This filtering will take place after the security check.</param>
        /// <returns>A dictionary that represents the attributes.</returns>
        public static Dictionary<string, PublicAttributeBag> GetPublicAttributesForEdit( this IHasAttributes entity, Person currentPerson, bool enforceSecurity = true, Func<AttributeCache, bool> attributeFilter = null )
        {
            if ( entity == null || entity.Attributes == null )
            {
                return new Dictionary<string, PublicAttributeBag>();
            }

            return entity.Attributes
                .Select( a => a.Value )
                .Where( a => !enforceSecurity || a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Where( a => attributeFilter == null || attributeFilter( a ) )
                .ToDictionary( a => a.Key, a => PublicAttributeHelper.GetPublicAttributeForEdit( a ) );
        }

        /// <summary>
        /// Gets the attribute values in a format that can be sent to a public
        /// device for the purpose of editing the value.
        /// </summary>
        /// <param name="entity">The entity whose attributes are requested.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">If set to <c>true</c> then security will be enforced.</param>
        /// <param name="attributeFilter">If not <c>null</c> then this specifies a function to call to filter which attributes to include. This filtering will take place after the security check.</param>
        /// <returns>A dictionary that represents the attribute values.</returns>
        public static Dictionary<string, string> GetPublicAttributeValuesForEdit( this IHasAttributes entity, Person currentPerson, bool enforceSecurity = true, Func<AttributeCache, bool> attributeFilter = null )
        {
            if ( entity == null || entity.Attributes == null )
            {
                return new Dictionary<string, string>();
            }

            return entity.Attributes
                .Select( a => new
                {
                    Value = entity.GetAttributeValue( a.Key ),
                    Attribute = a.Value
                } )
                .Where( av => !enforceSecurity || av.Attribute.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Where( av => attributeFilter == null || attributeFilter( av.Attribute ) )
                .ToDictionary( av => av.Attribute.Key, av => PublicAttributeHelper.GetPublicEditValue( av.Attribute, av.Value ) );
        }

        /// <summary>
        /// <para>
        /// Sets attribute values that have been provided by a remote client.
        /// </para>
        /// <para>
        ///     This should be used to handle values the client sent back after
        ///     calling the <see cref="GetPublicAttributeValuesForEdit(IHasAttributes, Person, bool, Func{AttributeCache, bool})"/>
        ///     method. It handles conversion from custom data formats into the
        ///     proper values to be stored in the database.
        /// </para>
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="attributeValues">The attribute values.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">If set to <c>true</c> then security will be enforced.</param>
        /// <param name="attributeFilter">If not <c>null</c> then this specifies a function to call to filter which attributes to include. This filtering will take place after the security check.</param>
        public static void SetPublicAttributeValues( this IHasAttributes entity, Dictionary<string, string> attributeValues, Person currentPerson, bool enforceSecurity = true, Func<AttributeCache, bool> attributeFilter = null )
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

                if ( attributeFilter != null && !attributeFilter( attribute ) )
                {
                    continue;
                }

                var value = PublicAttributeHelper.GetPrivateValue( attribute, kvp.Value );

                entity.SetAttributeValue( kvp.Key, value );
            }
        }

        /// <summary>
        /// <para>
        /// Sets a single attribute values that have been provided by a remote client.
        /// </para>
        /// <para>
        ///     This should be used to handle values the client sent back after
        ///     calling the <see cref="GetPublicAttributeValuesForEdit(IHasAttributes, Person, bool, Func{AttributeCache, bool})"/>
        ///     method. It handles conversion from custom data formats into the
        ///     proper values to be stored in the database.
        /// </para>
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The attribute key to set.</param>
        /// <param name="value">The value provided by the remote client.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="enforceSecurity">If set to <c>true</c> then security will be enforced.</param>
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

        /// <summary>
        /// Gets the attribute value cache objects for the specified key.
        /// </summary>
        /// <param name="entity">The entity whose attributes are to be searched.</param>
        /// <param name="key">The attribute key to search for.</param>
        /// <returns>The <see cref="AttributeCache"/> and optionally the <see cref="AttributeValueCache"/> if it is valid.</returns>
        private static (AttributeCache AttributeCache, AttributeValueCache ValueCache) GetAttributeValueCache( IHasAttributes entity, string key )
        {
            if ( entity == null )
            {
                return (null, null);
            }

            AttributeValueCache valueCache = null;

            if ( entity.AttributeValues != null && entity.AttributeValues.ContainsKey( key ) )
            {
                valueCache = entity.AttributeValues[key];
            }

            if ( entity.Attributes != null && entity.Attributes.ContainsKey(key))
            {
                return (entity.Attributes[key], valueCache);
            }

            return (null, null);
        }

        /// <summary>
        /// Gets the attribute text value.
        /// </summary>
        /// <param name="entity">The entity whose attribute value is to being retrieved.</param>
        /// <param name="key">The key that identifies the attribute.</param>
        /// <param name="usePersistedOnly">if set to <c>true</c> then the persisted values will be used even if they are not valid.</param>
        /// <returns>A <see cref="string"/> that represents the attribute text value.</returns>
        public static string GetAttributeTextValue( this IHasAttributes entity, string key, bool usePersistedOnly = false )
        {
            (var attributeCache, var valueCache) = GetAttributeValueCache( entity, key );

            if ( attributeCache == null )
            {
                return null;
            }

            // Use the persisted value if it is forced or supported.
            if ( usePersistedOnly || attributeCache.IsPersistedValueSupported )
            {
                if ( valueCache != null )
                {
                    if ( usePersistedOnly || !valueCache.IsPersistedValueDirty )
                    {
                        return valueCache.PersistedTextValue;
                    }
                }
                else
                {
                    if ( usePersistedOnly || !attributeCache.IsDefaultPersistedValueDirty )
                    {
                        return attributeCache.DefaultPersistedTextValue;
                    }
                }
            }

            // No persisted value available and it isn't forced so calculate.
            var rawValue = valueCache?.Value ?? attributeCache.DefaultValue;
            var field = attributeCache.FieldType.Field;

            // If this field type wants entity context and we have it, then
            // call the appropriate method.
            if ( field is IEntityContextFieldType entityContextField )
            {
                if ( entity is IEntity fullEntity )
                {
                    return entityContextField.GetTextValue( rawValue, fullEntity, attributeCache.ConfigurationValues );
                }
                else if ( entity is IEntityCache entityCache )
                {
                    return entityContextField.GetTextValue( rawValue, entityCache.CachedEntityTypeId, entityCache.Id, attributeCache.ConfigurationValues );
                }
            }

            return field?.GetTextValue( rawValue, attributeCache.ConfigurationValues );
        }

        /// <summary>
        /// Gets the attribute HTML value.
        /// </summary>
        /// <param name="entity">The entity whose attribute value is to being retrieved.</param>
        /// <param name="key">The key that identifies the attribute.</param>
        /// <param name="usePersistedOnly">if set to <c>true</c> then the persisted values will be used even if they are not valid.</param>
        /// <returns>A <see cref="string"/> that represents the attribute HTML value.</returns>
        public static string GetAttributeHtmlValue( this IHasAttributes entity, string key, bool usePersistedOnly = false )
        {
            (var attributeCache, var valueCache) = GetAttributeValueCache( entity, key );

            if ( attributeCache == null )
            {
                return null;
            }

            // Use the persisted value if it is forced or supported.
            if ( usePersistedOnly || attributeCache.IsPersistedValueSupported )
            {
                if ( valueCache != null )
                {
                    if ( usePersistedOnly || !valueCache.IsPersistedValueDirty )
                    {
                        return valueCache.PersistedHtmlValue;
                    }
                }
                else
                {
                    if ( usePersistedOnly || !attributeCache.IsDefaultPersistedValueDirty )
                    {
                        return attributeCache.DefaultPersistedHtmlValue;
                    }
                }
            }

            // No persisted value available and it isn't forced so calculate.
            var rawValue = valueCache?.Value ?? attributeCache.DefaultValue;
            var field = attributeCache.FieldType.Field;

            // If this field type wants entity context and we have it, then
            // call the appropriate method.
            if ( field is IEntityContextFieldType entityContextField )
            {
                if ( entity is IEntity fullEntity )
                {
                    return entityContextField.GetTextValue( rawValue, fullEntity, attributeCache.ConfigurationValues );
                }
                else if ( entity is IEntityCache entityCache )
                {
                    return entityContextField.GetHtmlValue( rawValue, entityCache.CachedEntityTypeId, entityCache.Id, attributeCache.ConfigurationValues );
                }
            }

            return field?.GetHtmlValue( rawValue, attributeCache.ConfigurationValues );
        }

        /// <summary>
        /// Gets the attribute condensed text value.
        /// </summary>
        /// <remarks>
        /// The condensed text value should be used instead of the text value
        /// when space is limited where it will be displayed.
        /// </remarks>
        /// <param name="entity">The entity whose attribute value is to being retrieved.</param>
        /// <param name="key">The key that identifies the attribute.</param>
        /// <param name="usePersistedOnly">if set to <c>true</c> then the persisted values will be used even if they are not valid.</param>
        /// <returns>A <see cref="string"/> that represents the attribute condensed text value.</returns>
        public static string GetAttributeCondensedTextValue( this IHasAttributes entity, string key, bool usePersistedOnly = false )
        {
            (var attributeCache, var valueCache) = GetAttributeValueCache( entity, key );

            if ( attributeCache == null )
            {
                return null;
            }

            // Use the persisted value if it is forced or supported.
            if ( usePersistedOnly || attributeCache.IsPersistedValueSupported )
            {
                if ( valueCache != null )
                {
                    if ( usePersistedOnly || !valueCache.IsPersistedValueDirty )
                    {
                        return valueCache.PersistedCondensedTextValue;
                    }
                }
                else
                {
                    if ( usePersistedOnly || !attributeCache.IsDefaultPersistedValueDirty )
                    {
                        return attributeCache.DefaultPersistedCondensedTextValue;
                    }
                }
            }

            // No persisted value available and it isn't forced so calculate.
            var rawValue = valueCache?.Value ?? attributeCache.DefaultValue;
            var field = attributeCache.FieldType.Field;

            // If this field type wants entity context and we have it, then
            // call the appropriate method.
            if ( field is IEntityContextFieldType entityContextField )
            {
                if ( entity is IEntity fullEntity )
                {
                    return entityContextField.GetTextValue( rawValue, fullEntity, attributeCache.ConfigurationValues );
                }
                else if ( entity is IEntityCache entityCache )
                {
                    return entityContextField.GetCondensedTextValue( rawValue, entityCache.CachedEntityTypeId, entityCache.Id, attributeCache.ConfigurationValues );
                }
            }

            return field?.GetTextValue( rawValue, attributeCache.ConfigurationValues );
        }

        /// <summary>
        /// Gets the attribute condensed HTML value.
        /// </summary>
        /// <remarks>
        /// The condensed HTML value should be used instead of the HTML value
        /// when space is limited where it will be displayed.
        /// </remarks>
        /// <param name="entity">The entity whose attribute value is to being retrieved.</param>
        /// <param name="key">The key that identifies the attribute.</param>
        /// <param name="usePersistedOnly">if set to <c>true</c> then the persisted values will be used even if they are not valid.</param>
        /// <returns>A <see cref="string"/> that represents the attribute condensed HTML value.</returns>
        public static string GetAttributeCondensedHtmlValue( this IHasAttributes entity, string key, bool usePersistedOnly = false )
        {
            (var attributeCache, var valueCache) = GetAttributeValueCache( entity, key );

            if ( attributeCache == null )
            {
                return null;
            }

            // Use the persisted value if it is forced or supported.
            if ( usePersistedOnly || attributeCache.IsPersistedValueSupported )
            {
                if ( valueCache != null )
                {
                    if ( usePersistedOnly || !valueCache.IsPersistedValueDirty )
                    {
                        return valueCache.PersistedCondensedHtmlValue;
                    }
                }
                else
                {
                    if ( usePersistedOnly || !attributeCache.IsDefaultPersistedValueDirty )
                    {
                        return attributeCache.DefaultPersistedCondensedHtmlValue;
                    }
                }
            }

            // No persisted value available and it isn't forced so calculate.
            var rawValue = valueCache?.Value ?? attributeCache.DefaultValue;
            var field = attributeCache.FieldType.Field;

            // If this field type wants entity context and we have it, then
            // call the appropriate method.
            if ( field is IEntityContextFieldType entityContextField )
            {
                if ( entity is IEntity fullEntity )
                {
                    return entityContextField.GetTextValue( rawValue, fullEntity, attributeCache.ConfigurationValues );
                }
                else if ( entity is IEntityCache entityCache )
                {
                    return entityContextField.GetCondensedHtmlValue( rawValue, entityCache.CachedEntityTypeId, entityCache.Id, attributeCache.ConfigurationValues );
                }
            }

            return field?.GetTextValue( rawValue, attributeCache.ConfigurationValues );
        }

        #endregion IHasAttributes extensions
    }
}
