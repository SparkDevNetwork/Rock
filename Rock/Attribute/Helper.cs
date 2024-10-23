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
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Attribute
{
    /// <summary>
    /// Static Helper class for creating, saving, and reading attributes and attribute values of any <see cref="Rock.Attribute.IHasAttributes"/> class
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Updates the attributes.
        /// </summary>
        /// <param name="type">The type (should be a <see cref="Rock.Attribute.IHasAttributes" /> object.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        public static bool UpdateAttributes( Type type, int? entityTypeId, RockContext rockContext = null )
        {
            return UpdateAttributes( type, entityTypeId, String.Empty, String.Empty, rockContext );
        }

        /// <summary>
        /// Uses reflection to find any <see cref="FieldAttribute" /> attributes for the specified type and will create and/or update
        /// a <see cref="Rock.Model.Attribute" /> record for each attribute defined.
        /// </summary>
        /// <param name="type">The type (should be a <see cref="IHasAttributes" /> object.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        public static bool UpdateAttributes( Type type, int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, RockContext rockContext = null )
        {
            bool attributesUpdated = false;
            bool attributesDeleted = false;

            if ( type == null )
            {
                return false;
            }

            var entityProperties = new List<FieldAttribute>();

            // If a ContextAwareAttribute exists without an EntityType defined, add a property attribute to specify the type
            int properties = 0;
            foreach ( var customAttribute in type.GetCustomAttributes( typeof( ContextAwareAttribute ), true ) )
            {
                var contextAttribute = ( ContextAwareAttribute ) customAttribute;
                if ( contextAttribute != null && contextAttribute.IsConfigurable )
                {
                    string propertyKeyName = string.Format( "ContextEntityType{0}", properties > 0 ? properties.ToString() : "" );
                    properties++;

                    entityProperties.Add( new EntityTypeFieldAttribute( "Entity Type", false, "The type of entity that will provide context for this block", false, "Context", 0, propertyKeyName ) );
                }
            }

            // Add any property attributes that were defined for the entity
            foreach ( var customAttribute in type.GetCustomAttributes( typeof( FieldAttribute ), true ) )
            {
                entityProperties.Add( ( FieldAttribute ) customAttribute );
            }

            rockContext = rockContext ?? new RockContext();

            var customizedGrid = type.GetCustomAttribute<Blocks.CustomizedGridAttribute>();

            bool customGridColumnsBlock = typeof( Rock.Web.UI.ICustomGridColumns ).IsAssignableFrom( type );
            if ( customGridColumnsBlock || customizedGrid?.IsCustomColumnsSupported == true )
            {
                entityProperties.Add( new TextFieldAttribute( CustomGridColumnsConfig.AttributeKey, category: "CustomSetting" ) );
            }

            bool customGridOptionsBlock = typeof( Rock.Web.UI.ICustomGridOptions ).IsAssignableFrom( type );

            if ( customGridOptionsBlock || customizedGrid?.IsStickyHeaderSupported == true )
            {
                entityProperties.Add( new BooleanFieldAttribute( CustomGridOptionsConfig.EnableStickyHeadersAttributeKey, category: "CustomSetting" ) );
            }

            if ( customGridOptionsBlock || customizedGrid?.IsCustomActionsSupported == true )
            {
                entityProperties.Add( new TextFieldAttribute( CustomGridOptionsConfig.CustomActionsConfigsAttributeKey, category: "CustomSetting" ) );
                entityProperties.Add( new BooleanFieldAttribute( CustomGridOptionsConfig.EnableDefaultWorkflowLauncherAttributeKey, category: "CustomSetting", defaultValue: true ) );
            }

            bool dynamicAttributesBlock = typeof( Rock.Web.UI.IDynamicAttributesBlock ).IsAssignableFrom( type );

            // Create any attributes that need to be created
            foreach ( var entityProperty in entityProperties )
            {
                try
                {
                    attributesUpdated = UpdateAttribute( entityProperty, entityTypeId, entityQualifierColumn, entityQualifierValue, dynamicAttributesBlock, rockContext ) || attributesUpdated;
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( string.Format( "Could not update an entity attribute ( Entity Type Id: {0}; Property Name: {1} ). ", entityTypeId, entityProperty.Name ), ex ), null );
                }
            }

            // Remove any old attributes
            try
            {
                var attributeService = new Model.AttributeService( rockContext );

                // if the entity is a block that implements IDynamicAttributesBlock, don't delete the attribute
                if ( !dynamicAttributesBlock )
                {
                    var existingKeys = entityProperties.Select( a => a.Key ).ToList();
                    foreach ( var a in attributeService.GetByEntityTypeQualifier( entityTypeId, entityQualifierColumn, entityQualifierValue, true ).ToList() )
                    {
                        if ( !existingKeys.Contains( a.Key ) )
                        {
                            attributeService.Delete( a );
                            attributesDeleted = true;
                        }
                    }
                }

                if ( attributesDeleted )
                {
                    rockContext.SaveChanges();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Could not delete one or more old attributes.", ex ), null );
            }

            return attributesUpdated;
        }

        /// <summary>
        /// Adds or updates a <see cref="Rock.Model.Attribute" /> item for the field attribute.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the attribute was created or updated; <c>false</c> otherwise.</returns>
        /// <remarks>
        /// If a <paramref name="rockContext"/> value is included, this method will save any previous changes made to the context
        /// </remarks>

        internal static bool UpdateAttribute( FieldAttribute property, int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, RockContext rockContext = null )
        {
            return UpdateAttribute( property, entityTypeId, entityQualifierColumn, entityQualifierValue, false, rockContext );
        }

        /// <summary>
        /// Adds or Updates a <see cref="Rock.Model.Attribute" /> item for the attribute.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="dynamicAttributesBlock">if set to <c>true</c> [dynamic attributes block].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        private static bool UpdateAttribute( FieldAttribute property, int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, bool dynamicAttributesBlock, RockContext rockContext = null )
        {
            bool updated = false;

            rockContext = rockContext ?? new RockContext();

            var attributeService = new AttributeService( rockContext );
            var attributeQualifierService = new AttributeQualifierService( rockContext );
            var fieldTypeService = new FieldTypeService( rockContext );
            var categoryService = new CategoryService( rockContext );

            var propertyCategories = property.Category.SplitDelimitedValues( false ).ToList();

            // Look for an existing attribute record based on the entity, entityQualifierColumn and entityQualifierValue
            Model.Attribute attribute = attributeService.Get( entityTypeId, entityQualifierColumn, entityQualifierValue, property.Key );

            if ( attribute == null )
            {
                // If an existing attribute record doesn't exist, create a new one
                updated = true;

                attribute = new Model.Attribute();
                attribute.EntityTypeId = entityTypeId;
                attribute.EntityTypeQualifierColumn = entityQualifierColumn;
                attribute.EntityTypeQualifierValue = entityQualifierValue;
                attribute.Key = property.Key;
                attribute.IconCssClass = string.Empty;
                attribute.IsGridColumn = false;
            }
            else
            {
                // Check to see if the existing attribute record needs to be updated
                if ( attribute.Name != property.Name ||
                    attribute.DefaultValue != property.DefaultValue ||
                    attribute.Description != property.Description ||
                    attribute.FieldType.Assembly != property.FieldTypeAssembly ||
                    attribute.FieldType.Class != property.FieldTypeClass ||
                    attribute.IsRequired != property.IsRequired )
                {
                    updated = true;
                }

                if ( attribute.Order != property.Order && !dynamicAttributesBlock )
                {
                    updated = true;
                }

                // Check category
                else if ( attribute.Categories.Select( c => c.Name ).Except( propertyCategories ).Any() ||
                    propertyCategories.Except( attribute.Categories.Select( c => c.Name ) ).Any() )
                {
                    updated = true;
                }

                // Check the qualifier values
                else if ( attribute.AttributeQualifiers.Select( q => q.Key ).Except( property.FieldConfigurationValues.Select( c => c.Key ) ).Any() ||
                    property.FieldConfigurationValues.Select( c => c.Key ).Except( attribute.AttributeQualifiers.Select( q => q.Key ) ).Any() )
                {
                    updated = true;
                }
                else
                {
                    foreach ( var attributeQualifier in attribute.AttributeQualifiers )
                    {
                        if ( !property.FieldConfigurationValues.ContainsKey( attributeQualifier.Key ) ||
                            property.FieldConfigurationValues[attributeQualifier.Key].Value != attributeQualifier.Value )
                        {
                            updated = true;
                            break;
                        }
                    }
                }

            }

            if ( !updated )
            {
                return false;
            }

            // Update the attribute
            attribute.Name = property.Name;
            attribute.Description = property.Description;
            attribute.DefaultValue = property.DefaultValue;

            // if the block is IDynamicAttributesBlock, only update the attribute.Order if this is a new attribute 
            if ( !dynamicAttributesBlock || attribute.Id == 0 )
            {
                attribute.Order = property.Order;
            }

            attribute.IsRequired = property.IsRequired;

            attribute.Categories.Clear();
            if ( propertyCategories.Any() )
            {
                foreach ( string propertyCategory in propertyCategories )
                {
                    int attributeEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;
                    var category = categoryService.Get( propertyCategory, attributeEntityTypeId, "EntityTypeId", entityTypeId.ToString() ).FirstOrDefault();
                    if ( category == null )
                    {
                        category = new Category();
                        category.Name = propertyCategory;
                        category.EntityTypeId = attributeEntityTypeId;
                        category.EntityTypeQualifierColumn = "EntityTypeId";
                        category.EntityTypeQualifierValue = entityTypeId.ToString();
                        category.Order = 0;
                    }
                    attribute.Categories.Add( category );

                }

                // Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime just in case they were changed
                attribute.ModifiedDateTime = RockDateTime.Now;
            }

            foreach ( var qualifier in attribute.AttributeQualifiers.ToList() )
            {
                attributeQualifierService.Delete( qualifier );
            }
            attribute.AttributeQualifiers.Clear();

            foreach ( var configValue in property.FieldConfigurationValues )
            {
                var qualifier = new Model.AttributeQualifier();
                qualifier.Key = configValue.Key;
                qualifier.Value = configValue.Value.Value;
                attribute.AttributeQualifiers.Add( qualifier );
            }

            // Try to set the field type by searching for an existing field type with the same assembly and class name
            if ( attribute.FieldType == null || attribute.FieldType.Assembly != property.FieldTypeAssembly ||
                attribute.FieldType.Class != property.FieldTypeClass )
            {
                attribute.FieldType = fieldTypeService.Queryable().FirstOrDefault( f =>
                    f.Assembly == property.FieldTypeAssembly &&
                    f.Class == property.FieldTypeClass );
            }

            // If this is a new attribute, add it, otherwise remove the exiting one from the cache
            if ( attribute.Id == 0 )
            {
                // double check that another thread didn't add this attribute
                if ( !attributeService.AlreadyExists( entityTypeId, entityQualifierColumn, entityQualifierValue, property.Key ) )
                {
                    attributeService.Add( attribute );
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine( $@"Tried to add {attribute}, but it already exists.
This can be due to multiple threads updating the same attribute at the same time." );
                }
            }

            rockContext.SaveChanges();

            return true;
        }

        /// <summary>
        /// Ensures the attributes from the component are configured for this
        /// entity. This handles the situation where the attributes are defined
        /// on a component but need to be applied to an entity that is using
        /// that component.
        /// </summary>
        /// <example>
        /// <code>
        /// Helper.EnsureComponentInstanceAttributes( financialGateway, fg =&gt; fg.EntityTypeId, rockContext );
        /// </code>
        /// </example>
        /// <param name="entity">The entity the component attributes should be setup for.</param>
        /// <param name="navigationExpression">The expression to use to find the component entity type identifier.</param>
        /// <param name="rockContext">The rock context to use when updating the database.</param>
        internal static void EnsureComponentInstanceAttributes<TEntity>( TEntity entity, Expression<Func<TEntity, int?>> navigationExpression, RockContext rockContext )
            where TEntity : IEntity
        {
            if ( !( navigationExpression.Body is MemberExpression memberExpression ) || !( memberExpression.Member is PropertyInfo ) )
            {
                throw new ArgumentException( "Expression must evaluate to a valid property.", nameof( navigationExpression ) );
            }

            var componentEntityTypeId = navigationExpression.Compile().Invoke( entity );

            if ( !componentEntityTypeId.HasValue )
            {
                return;
            }

            var componentEntityType = EntityTypeCache.Get( componentEntityTypeId.Value );

            if ( componentEntityType == null )
            {
                return;
            }

            UpdateAttributes(
                componentEntityType.GetEntityType(),
                entity.TypeId,
                memberExpression.Member.Name,
                componentEntityType.Id.ToString(),
                rockContext );
        }

        /// <summary>
        /// Ensures the attributes from the component are configured for this
        /// entity. This handles the situation where the attributes are defined
        /// on a component but need to be applied to an entity that is using
        /// that component.
        /// </summary>
        /// <param name="entity">The entity the component attributes should be setup for.</param>
        /// <param name="navigationExpression">The expression to use to find the component entity type identifier.</param>
        /// <param name="rockContext">The rock context to use when updating the database.</param>
        internal static void EnsureComponentInstanceAttributes<TEntity>( TEntity entity, Expression<Func<TEntity, int>> navigationExpression, RockContext rockContext )
            where TEntity : IEntity
        {
            if ( !( navigationExpression.Body is MemberExpression memberExpression ) || !( memberExpression.Member is PropertyInfo ) )
            {
                throw new ArgumentException( "Expression must evaluate to a valid property.", nameof( navigationExpression ) );
            }

            var componentEntityTypeId = navigationExpression.Compile().Invoke( entity );

            if ( componentEntityTypeId == 0 )
            {
                return;
            }

            var componentEntityType = EntityTypeCache.Get( componentEntityTypeId );

            if ( componentEntityType == null )
            {
                return;
            }

            UpdateAttributes(
                componentEntityType.GetEntityType(),
                entity.TypeId,
                memberExpression.Member.Name,
                componentEntityType.Id.ToString(),
                rockContext );
        }

        #region Load Attributes and Values

        /// <summary>
        /// Loads the <see cref="P:IHasAttributes.Attributes" /> and <see cref="P:IHasAttributes.AttributeValues" /> of any <see cref="IHasAttributes" /> object
        /// </summary>
        /// <param name="entity">The item.</param>
        public static void LoadAttributes( Rock.Attribute.IHasAttributes entity )
        {
            using ( var rockContext = new RockContext() )
            {
                LoadAttributes( entity, rockContext );
            }
        }

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="limitToAttributes">The limit to attributes.</param>
        public static void LoadAttributes( Rock.Attribute.IHasAttributes entity, List<AttributeCache> limitToAttributes )
        {
            using ( var rockContext = new RockContext() )
            {
                LoadAttributes( entity, rockContext, limitToAttributes );
            }
        }

        /// <summary>
        /// Loads the <see cref="P:IHasAttributes.Attributes" /> and <see cref="P:IHasAttributes.AttributeValues" /> of any <see cref="IHasAttributes" /> object
        /// </summary>
        /// <param name="entity">The item.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void LoadAttributes( Rock.Attribute.IHasAttributes entity, RockContext rockContext )
        {
            LoadAttributes( entity, rockContext, null );
        }

        /// <summary>
        /// Loads the <see cref="P:IHasAttributes.Attributes" /> and <see cref="P:IHasAttributes.AttributeValues" /> of any <see cref="IHasAttributes" /> object with an option to limit to specific attributes
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="limitToAttributes">The limit to attributes.</param>
        public static void LoadAttributes( Rock.Attribute.IHasAttributes entity, RockContext rockContext, List<AttributeCache> limitToAttributes )
        {
            if ( entity == null )
            {
                return;
            }

            if ( entity is Rock.Web.Cache.IEntityCache )
            {
                // Don't let this LoadAttributes get called on a IEntityCache (or ModelCache<,>)
                // It'll just end up removing the attributes since this LoadAttributes is looking up Attributes based on entity.GetType(), which wouldn't be the entity type of the underlying model
                // CacheObjects manage attributes themselves
                return;
            }

            Type entityType = entity.GetType();
            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

            var attributes = new List<Rock.Web.Cache.AttributeCache>();

            var entityTypeCache = EntityTypeCache.Get( entityType );

            List<Rock.Web.Cache.AttributeCache> allAttributes = null;
            Dictionary<int, List<int>> inheritedAttributes = null;

            //
            // If this entity can provide inherited attribute information then
            // load that data now. If they don't provide any then generate empty lists.
            //
            if ( entity is Rock.Attribute.IHasInheritedAttributes entityWithInheritedAttributes )
            {
                rockContext = rockContext ?? new RockContext();
                allAttributes = entityWithInheritedAttributes.GetInheritedAttributes( rockContext );
                inheritedAttributes = entityWithInheritedAttributes.GetAlternateEntityIdsByType( rockContext );
            }

            allAttributes = allAttributes ?? new List<AttributeCache>();
            inheritedAttributes = inheritedAttributes ?? new Dictionary<int, List<int>>();

            //
            // Get all the attributes that apply to this entity type and this entity's
            // properties match any attribute qualifiers.
            //
            var entityTypeId = entityTypeCache?.Id;

            if ( entityTypeCache != null )
            {
                var entityTypeAttributesList = AttributeCache.GetByEntityType( entityTypeCache.Id );
                if ( entityTypeAttributesList.Any() )
                {
                    var entityTypeQualifierColumnPropertyNames = entityTypeAttributesList.Select( a => a.EntityTypeQualifierColumn ).Distinct().Where( a => !string.IsNullOrWhiteSpace( a ) ).ToList();
                    Dictionary<string, object> propertyValues = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );
                    foreach ( var propertyName in entityTypeQualifierColumnPropertyNames )
                    {
                        PropertyInfo propertyInfo = entityType.GetProperty( propertyName ) ?? entityType.GetProperties().Where( a => a.Name.Equals( propertyName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
                        if ( propertyInfo != null )
                        {
                            propertyValues.TryAdd( propertyName, propertyInfo.GetValue( entity, null ) );
                        }
                    }

                    var entityTypeAttributesForQualifier = entityTypeAttributesList.Where( x =>
                      string.IsNullOrEmpty( x.EntityTypeQualifierColumn ) ||
                             ( propertyValues.ContainsKey( x.EntityTypeQualifierColumn ) &&
                             ( string.IsNullOrEmpty( x.EntityTypeQualifierValue ) ||
                             ( propertyValues[x.EntityTypeQualifierColumn] ?? "" ).ToString() == x.EntityTypeQualifierValue ) ) );

                    attributes.AddRange( entityTypeAttributesForQualifier );
                }
            }

            //
            // Append these attributes to our inherited attributes, in order.
            //
            foreach ( var attribute in attributes.OrderBy( a => a.Order ) )
            {
                allAttributes.Add( attribute );
            }

            /*
            2/14/2020 - SK 
            Only "active" attributes should be included here because otherwise
            we are passing the responsibility to filter them out (and the
            AttributeValues) to other areas of code where it is sometimes
            not easily done. See issue #3915 for example.

            Reason: Lava AttributeValues would otherwise contain inactive attributes.
            */

            allAttributes = allAttributes.Where( a => a.IsActive ).ToList();

            if ( limitToAttributes?.Any() == true )
            {
                allAttributes = allAttributes.Where( a => limitToAttributes.Any( l => l.Id == a.Id ) ).ToList();
            }

            var attributeValues = new Dictionary<string, AttributeValueCache>();

            if ( allAttributes.Any() )
            {
                foreach ( var attribute in allAttributes )
                {
                    // Add a placeholder for this item's value for each attribute
                    attributeValues.TryAdd( attribute.Key, null );
                }

                // If loading attributes for a saved item, read the item's value(s) for each attribute 
                if ( !entityTypeCache.IsEntity || entity.Id != 0 )
                {
                    rockContext = rockContext ?? new RockContext();
                    var attributeValueService = new Rock.Model.AttributeValueService( rockContext );

                    List<int> attributeIds = allAttributes.Select( a => a.Id ).ToList();
                    IQueryable<AttributeValue> attributeValueQuery;

                    if ( inheritedAttributes.Any() )
                    {
                        // Add the current entity to the set of target items.
                        inheritedAttributes.Add( entityTypeId.GetValueOrDefault(), new List<int> { entity.Id } );

                        // Create the set of conditions for filtering the target entities.
                        // Attribute Values are filtered by entity identifiers, grouped by entity type and
                        // combined with a logical OR.
                        var predicate = LinqPredicateBuilder.False<AttributeValue>();
                        foreach ( var inheritedAttribute in inheritedAttributes )
                        {
                            // a Linq query that uses 'Contains' can't be cached
                            // in the EF Plan Cache, so instead of doing a
                            // Contains, build a List of OR conditions. This can
                            // save 15-20ms per call (and still ends up with the
                            // exact same SQL).
                            // https://learn.microsoft.com/en-us/ef/ef6/fundamentals/performance/perf-whitepaper?redirectedfrom=MSDN#41-using-ienumerabletcontainstt-value
                            var attributeValueParameterExpression = Expression.Parameter( typeof( AttributeValue ), "v" );
                            var entityIdPropertyExpression = Expression.Property( attributeValueParameterExpression, "EntityId" );
                            entityIdPropertyExpression = Expression.Property( entityIdPropertyExpression, "Value" );
                            Expression<Func<AttributeValue, bool>> inheritedEntityIdExpression = null;

                            foreach ( var alternateEntityId in inheritedAttribute.Value )
                            {
                                var alternateEntityIdConstant = Expression.Constant( alternateEntityId );
                                var equalityExpression = Expression.Equal( entityIdPropertyExpression, alternateEntityIdConstant );
                                var expr = Expression.Lambda<Func<AttributeValue, bool>>( equalityExpression, attributeValueParameterExpression );

                                if ( inheritedEntityIdExpression != null )
                                {
                                    inheritedEntityIdExpression = inheritedEntityIdExpression.Or( expr );
                                }
                                else
                                {
                                    inheritedEntityIdExpression = expr;
                                }
                            }

                            // Build the expression for this attribute. This is effectively:
                            // .Where( v => v.Attribute.EntityTypeId == inheritedAttribute.Key
                            //    && v.EntityId.HasValue
                            //    && inheritedAttribute.Value.Contains( v.EntityId.Value ) );
                            var expression = LinqPredicateBuilder.Create<AttributeValue>( v => v.Attribute.EntityTypeId == inheritedAttribute.Key
                                     && v.EntityId.HasValue );
                            expression = expression.And( inheritedEntityIdExpression );

                            predicate = predicate.Or( expression );
                        }

                        attributeValueQuery = attributeValueService.Queryable().AsNoTracking().Where( predicate );
                    }
                    else
                    {
                        attributeValueQuery = attributeValueService.Queryable().AsNoTracking()
                            .Where( v => v.EntityId.HasValue && v.EntityId.Value == entity.Id );
                    }

                    attributeValueQuery = attributeValueQuery.WhereAttributeIds( attributeIds );

                    /* 2020-07-29 MDP
                     Select just AttributeId, EntityId, and Value. That way the AttributeId_EntityId_Value index is the
                     only thing that SQL needs to touch. This improves query performance around 10% since SQL doesn't
                     have to look at the full row after finding the records in the index.
                     */
                    var attributeValueSelectQuery = attributeValueQuery.Select( a =>
                        new
                        {
                            a.AttributeId,
                            a.EntityId,
                            a.Value,
                            a.PersistedTextValue,
                            a.PersistedHtmlValue,
                            a.PersistedCondensedTextValue,
                            a.PersistedCondensedHtmlValue,
                            a.IsPersistedValueDirty
                        } );

                    foreach ( var attributeValueSelect in attributeValueSelectQuery )
                    {
                        var attributeKey = AttributeCache.Get( attributeValueSelect.AttributeId ).Key;
                        var attributeValueCache = new AttributeValueCache( attributeValueSelect.AttributeId,
                            attributeValueSelect.EntityId,
                            attributeValueSelect.Value,
                            attributeValueSelect.PersistedTextValue,
                            attributeValueSelect.PersistedHtmlValue,
                            attributeValueSelect.PersistedCondensedTextValue,
                            attributeValueSelect.PersistedCondensedHtmlValue,
                            attributeValueSelect.IsPersistedValueDirty );
                        attributeValues[attributeKey] = attributeValueCache;
                    }
                }

                // Look for any attributes that don't have a value and create a default value entry
                foreach ( var attribute in allAttributes )
                {
                    if ( attributeValues[attribute.Key] == null )
                    {
                        AttributeValueCache attributeValue;

                        var attributeValueDefaults = entity.AttributeValueDefaults;
                        if ( attributeValueDefaults != null && attributeValueDefaults.ContainsKey( attribute.Key ) )
                        {
                            attributeValue = new AttributeValueCache( attribute.Id,
                                entity?.Id,
                                attributeValueDefaults[attribute.Key] );
                        }
                        else
                        {
                            attributeValue = new AttributeValueCache( attribute.Id,
                                entity?.Id,
                                attribute.DefaultValue,
                                attribute.DefaultPersistedTextValue,
                                attribute.DefaultPersistedHtmlValue,
                                attribute.DefaultPersistedCondensedTextValue,
                                attribute.DefaultPersistedCondensedHtmlValue,
                                attribute.IsDefaultPersistedValueDirty );
                        }

                        attributeValues[attribute.Key] = attributeValue;
                    }
                    else
                    {
                        if ( !String.IsNullOrWhiteSpace( attribute.DefaultValue ) &&
                            String.IsNullOrWhiteSpace( attributeValues[attribute.Key].Value ) )
                        {
                            attributeValues[attribute.Key].Value = attribute.DefaultValue;
                        }
                    }
                }
            }

            entity.Attributes = new Dictionary<string, Rock.Web.Cache.AttributeCache>();
            allAttributes.ForEach( a => entity.Attributes.TryAdd( a.Key, a ) );

            entity.AttributeValues = attributeValues;
        }

        /// <summary>
        /// Loads the <see cref="IHasAttributes.Attributes" /> and <see cref="IHasAttributes.AttributeValues" /> of any <see cref="IHasAttributes" /> object with an option to limit to specific attributes
        /// </summary>
        /// <param name="entities">The entities whose attributes are to be loaded.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="limitToAttributes">The limit to attributes.</param>
        internal static void LoadAttributes( IEnumerable<IHasAttributes> entities, RockContext rockContext, List<AttributeCache> limitToAttributes )
        {
            if ( limitToAttributes != null )
            {
                LoadFilteredAttributes( entities, rockContext, attribute => limitToAttributes.Any( a => a.Id == attribute.Id ) );
            }
            else
            {
                LoadFilteredAttributes( entities, rockContext, null );
            }
        }

        /// <summary>
        /// Loads the <see cref="IHasAttributes.Attributes" /> and <see cref="IHasAttributes.AttributeValues" /> of any <see cref="IHasAttributes" /> object with an option to limit to specific attributes
        /// </summary>
        /// <param name="entities">The entities whose attributes are to be loaded.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attributeFilter">The expression to use when filtering which attributes to load.</param>
        internal static void LoadFilteredAttributes( IEnumerable<IHasAttributes> entities, RockContext rockContext, Func<AttributeCache, bool> attributeFilter )
        {
            if ( entities == null || !entities.Any() )
            {
                return;
            }

            // Get the entity type from the first entity. In the future, we might
            // throw an exception if any entities do not inherit from this type, or
            // possibly group by the type and load in chunks.
            var entityType = entities.First().GetType();

            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

            // We can only operate on IEntity objects, but using both in the
            // constraints confuses the compiler a bit.
            if ( !typeof( IEntity ).IsAssignableFrom( entityType ) )
            {
                return;
            }

            var entityTypeCache = EntityTypeCache.Get( entityType );

            // This shouldn't happen.
            if ( entityTypeCache == null )
            {
                return;
            }

            // Get all the attributes that apply to this entity type.
            var entityTypeId = entityTypeCache.Id;
            var entityTypeAttributesList = AttributeCache.AllForEntityType( entityTypeId )
                .Where( a => a.IsActive && ( attributeFilter == null || attributeFilter( a ) ) )
                .ToList();

            // Filter the attribute list by qualifiers.
            entityTypeAttributesList = FilterAttributesByQualifiers( entities, entityTypeAttributesList );

            // Initialize the list of attributes, more will be added later.
            var allAttributes = new List<AttributeCache>( entityTypeAttributesList );

            // Begin processing inherited attributes.
            var allInheritedAttributes = new List<AttributeCache>();
            var inheritedAttributes = new Dictionary<int, InheritedAttributeLookup>();

            // If this entity can provide inherited attribute information then
            // load that data now. If they don't provide any then generate empty lists.
            if ( typeof( Rock.Attribute.IHasInheritedAttributes ).IsAssignableFrom( entityType ) )
            {
                rockContext = rockContext ?? new RockContext();

                foreach ( var entity in entities )
                {
                    var entityWithInheritedAttributes = entity as Rock.Attribute.IHasInheritedAttributes;

                    // Add in all inherited attributes we don't already know about.
                    var entityInheritedAttributes = entityWithInheritedAttributes.GetInheritedAttributes( rockContext );

                    if ( entityInheritedAttributes != null )
                    {
                        entityInheritedAttributes = entityInheritedAttributes
                            .Where( a => a.IsActive && ( attributeFilter == null || attributeFilter( a ) ) )
                            .ToList();

                        allInheritedAttributes.AddRange( entityInheritedAttributes.Where( ia => !allInheritedAttributes.Any( a => a.Id == ia.Id ) ) );
                    }

                    var alternateIds = entityWithInheritedAttributes.GetAlternateEntityIdsByType( rockContext );

                    // Set the alternate identifiers for the inherited attributes.
                    // Since this information could be different for each entity we need
                    // to track it per entity.
                    if ( entityInheritedAttributes?.Any() == true || alternateIds?.Any() == true )
                    {
                        inheritedAttributes.Add( entity.Id, new InheritedAttributeLookup
                        {
                            Attributes = entityInheritedAttributes,
                            AlternateIds = alternateIds
                        } );
                    }
                }
            }

            // Add any inherited attributes to those that need to be loaded.
            allAttributes.AddRange( allInheritedAttributes.Where( ia => !allAttributes.Any( a => a.Id == ia.Id ) ) );

            // If we don't have any attributes then just initialize the entities
            // with empty values and stop.
            if ( !allAttributes.Any() )
            {
                foreach ( var entity in entities )
                {
                    entity.Attributes = new Dictionary<string, Rock.Web.Cache.AttributeCache>();
                    entity.AttributeValues = new Dictionary<string, Rock.Web.Cache.AttributeValueCache>();
                }

                return;
            }

            rockContext = rockContext ?? new RockContext();

            // Build the list of primary entity ids to load values for.
            var valueEntityKeys = entities
                .Select( e => new LoadAttributesKey
                {
                    EntityTypeId = entityTypeId,
                    EntityId = e.Id,
                    RealEntityId = e.Id
                } )
                .ToList();

            // Add all the alternate identifiers to look for.
            foreach ( var inheritedAttribute in inheritedAttributes )
            {
                if ( inheritedAttribute.Value.AlternateIds != null )
                {
                    valueEntityKeys.AddRange( inheritedAttribute.Value.AlternateIds.SelectMany( ai => ai.Value.Select( id => new LoadAttributesKey
                    {
                        EntityTypeId = ai.Key,
                        EntityId = id,
                        RealEntityId = inheritedAttribute.Key
                    } ) ) );
                }
            }

            // Load all the values from the database in one big query.
            var allAttributeValues = LoadAttributeValues( valueEntityKeys, allAttributes, rockContext );

            // Break up the entity attributes into those with and without qualifiers.
            var unqualifiedEntityAttributes = entityTypeAttributesList.Where( a => a.EntityTypeQualifierColumn.IsNullOrWhiteSpace() ).ToList();
            var qualifiedEntityAttributes = entityTypeAttributesList.Where( a => !a.EntityTypeQualifierColumn.IsNullOrWhiteSpace() ).ToList();

            // Get all the property names we are interested in for those
            // attributes that are qualified.
            var entityTypeQualifierColumnPropertyNames = qualifiedEntityAttributes.Select( a => a.EntityTypeQualifierColumn )
                .Distinct()
                .Where( a => !string.IsNullOrWhiteSpace( a ) )
                .ToList();

            // Group the qualified attributes by the property they are checking.
            var groupedQualifiedEntityAttributes = qualifiedEntityAttributes
                .GroupBy( a => new Tuple<string, string>( a.EntityTypeQualifierColumn.ToLower(), a.EntityTypeQualifierValue ) )
                .ToList();

            // Loop over all the entities and populate their attributes and values.
            foreach ( var entity in entities )
            {
                var attributeValues = new Dictionary<string, AttributeValueCache>();
                var entityAttributes = unqualifiedEntityAttributes.ToList();
                var inheritedAttributeLookup = inheritedAttributes.GetValueOrNull( entity.Id );

                // Filter any entity attributes that have qualifiers.
                if ( qualifiedEntityAttributes.Any() )
                {
                    var propertyValues = new Dictionary<string, string>();

                    // Get all the qualified property values for this entity.
                    foreach ( var propertyName in entityTypeQualifierColumnPropertyNames )
                    {
                        var propertyInfo = entityType.GetProperty( propertyName ) ?? entityType.GetProperties().Where( a => a.Name.Equals( propertyName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
                        if ( propertyInfo != null )
                        {
                            propertyValues.TryAdd( propertyName.ToLower(), propertyInfo.GetValue( entity, null ).ToStringSafe() );
                        }
                    }

                    // Loop over all the qualified attributes and see which ones match.
                    for ( int gai = 0; gai < groupedQualifiedEntityAttributes.Count; gai++ )
                    {
                        var qualification = groupedQualifiedEntityAttributes[gai];
                        var qualifierColumn = qualification.Key.Item1;
                        var qualifierValue = qualification.Key.Item2;

                        // If the entity doesn't contain the property being
                        // qualified, then the attribute does not match.
                        if ( !propertyValues.TryGetValue( qualifierColumn, out var value ) )
                        {
                            continue;
                        }

                        // If there is no qualifier value, then the check is to
                        // ensure that the property exists, not any value check.
                        if ( string.IsNullOrEmpty( qualifierValue ) )
                        {
                            entityAttributes.AddRange( qualification );
                        }

                        // If the entity property value matches the qualifier value
                        // then this attribute should be included on the entity.
                        if ( value == qualifierValue )
                        {
                            entityAttributes.AddRange( qualification );
                        }
                    }
                }

                // Add in any inherited attributes, these do not get the qualifier
                // checks applied to them.
                if ( inheritedAttributeLookup?.Attributes != null )
                {
                    entityAttributes.AddRange( inheritedAttributeLookup.Attributes.Where( ia => !entityAttributes.Any( a => a.Id == ia.Id ) ) );
                }

                // Add the value for each attribute defined on the entity type.
                foreach ( var attribute in entityAttributes )
                {
                    if ( allAttributeValues.TryGetValue( ( entity.Id, attribute.Id ), out var value ) )
                    {
                        var attributeValueCache = new AttributeValueCache( attribute.Id, value.EntityId, value.Value, value.PersistedTextValue, value.PersistedHtmlValue, value.PersistedCondensedTextValue, value.PersistedCondensedHtmlValue, value.IsPersistedValueDirty );

                        attributeValues[attribute.Key] = attributeValueCache;
                    }
                }

                // Look for any attributes that don't have a value and create a
                // default value entry.
                foreach ( var attribute in entityAttributes )
                {
                    if ( !attributeValues.ContainsKey( attribute.Key ) )
                    {
                        AttributeValueCache attributeValue = new AttributeValueCache
                        {
                            AttributeId = attribute.Id,
                            EntityId = entity?.Id
                        };

                        var attributeValueDefaults = entity.AttributeValueDefaults;
                        if ( attributeValueDefaults != null && attributeValueDefaults.ContainsKey( attribute.Key ) )
                        {
                            attributeValue = new AttributeValueCache( attribute.Id,
                                entity?.Id,
                                attributeValueDefaults[attribute.Key] );
                        }
                        else
                        {
                            attributeValue = new AttributeValueCache( attribute.Id,
                                entity?.Id,
                                attribute.DefaultValue,
                                attribute.DefaultPersistedTextValue,
                                attribute.DefaultPersistedHtmlValue,
                                attribute.DefaultPersistedCondensedTextValue,
                                attribute.DefaultPersistedCondensedHtmlValue,
                                attribute.IsDefaultPersistedValueDirty );
                        }

                        attributeValues[attribute.Key] = attributeValue;
                    }
                    else if ( attributeValues[attribute.Key].Value.IsNullOrWhiteSpace() && !attribute.DefaultValue.IsNullOrWhiteSpace() )
                    {
                        attributeValues[attribute.Key].Value = attribute.DefaultValue;
                    }
                }

                entity.Attributes = new Dictionary<string, Rock.Web.Cache.AttributeCache>();
                entityAttributes.ForEach( a => entity.Attributes.TryAdd( a.Key, a ) );

                entity.AttributeValues = attributeValues;
            }
        }

        /// <summary>
        /// Filters the attributes by qualifiers. This is used to filter out
        /// any attributes that could not possibly match any of the entities.
        /// So if an attribute has a qualifier for EntityTypeId=3 but none of
        /// the entities have that value, then the attribute is excluded.
        /// </summary>
        /// <param name="entities">The entities whose values will be loaded.</param>
        /// <param name="attributes">The attributes that are up for consideration.</param>
        /// <returns>A list of attributes that match or have no qualifications.</returns>
        private static List<AttributeCache> FilterAttributesByQualifiers( IEnumerable<IHasAttributes> entities, List<AttributeCache> attributes )
        {
            var entityTypeQualifierColumnPropertyNames = attributes.Select( a => a.EntityTypeQualifierColumn )
                .Distinct()
                .Where( a => !string.IsNullOrWhiteSpace( a ) )
                .ToList();

            // If none of the attributes have any qualifiers then we have
            // nothing to filter.
            if ( !entityTypeQualifierColumnPropertyNames.Any() )
            {
                return attributes;
            }

            // Make sure it's a list since we will be materializing it many times.
            var entityList = entities.ToList();
            var entityQualifications = new HashSet<string>();
            var innerType = entityList[0].GetType();
            var entityType = innerType.IsDynamicProxyType() ? innerType.BaseType : innerType;

            // Populate the entityQualifications hash set with all the combinations
            // of qualifier columns and values. Use an actual for loop for
            // performance reasons.
            for ( int pi = 0; pi < entityTypeQualifierColumnPropertyNames.Count; pi++ )
            {
                var propertyName = entityTypeQualifierColumnPropertyNames[pi];
                var propertyNameLower = propertyName.ToLower();
                var propertyInfo = entityType.GetProperty( propertyName )
                    ?? entityType.GetProperties()
                        .Where( p => p.Name.Equals( propertyName, StringComparison.OrdinalIgnoreCase ) )
                        .FirstOrDefault();

                if ( propertyInfo != null )
                {
                    // Add a record saying "this property exists".
                    entityQualifications.Add( propertyNameLower );

                    var values = entityList.Select( e => propertyInfo.GetValue( e, null )?.ToString() ?? "" )
                        .Distinct();

                    // Add a record for each unique value.
                    foreach ( var value in values )
                    {
                        entityQualifications.Add( string.Format( "{0}|{1}", propertyNameLower, value ) );
                    }
                }
            }

            return attributes
                .Where( a =>
                {
                    if ( string.IsNullOrEmpty( a.EntityTypeQualifierColumn ) )
                    {
                        return true;
                    }

                    // If the qualifier value is blank, that means the qualification
                    // is simply "does this property exist".
                    var needle = string.IsNullOrEmpty( a.EntityTypeQualifierValue )
                        ? a.EntityTypeQualifierColumn.ToLower()
                        : string.Format( "{0}|{1}", a.EntityTypeQualifierColumn.ToLower(), a.EntityTypeQualifierValue );

                    return entityQualifications.Contains( needle );
                } )
                .ToList();
        }

        /// <summary>
        /// Loads the attribute values from the database with a direct SQL query
        /// to get optimal performance.
        /// </summary>
        /// <param name="entityKeys">The entity keys that identify all the entities whose values should be loaded.</param>
        /// <param name="attributes">The attributes whose values we are interested in.</param>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>A dictionary of all values loaded.</returns>
        private static Dictionary<(int RealEntityId, int AttributeId), AttributeItemValue> LoadAttributeValues( List<LoadAttributesKey> entityKeys, List<AttributeCache> attributes, RockContext rockContext )
        {
            // Initialize the EntityKey SQL parameter.
            var entityIdsTable = new DataTable();
            entityIdsTable.Columns.Add( "EntityTypeId", typeof( int ) );
            entityIdsTable.Columns.Add( "EntityId", typeof( int ) );
            entityIdsTable.Columns.Add( "RealEntityId", typeof( int ) );

            for ( int i = 0; i < entityKeys.Count; i++ )
            {
                var key = entityKeys[i];

                entityIdsTable.Rows.Add( key.EntityTypeId, key.EntityId, key.RealEntityId );
            }

            var entityIdsParameter = new SqlParameter( "@EntityKey", SqlDbType.Structured )
            {
                TypeName = "dbo.LoadAttributesKeyList",
                Value = entityIdsTable
            };

            List<AttributeItemValue> items;

            // When dealing with a lot of attributes, it actually slows down the query
            // to specify the specific attributes. I think it's confusing the query
            // plan optimizer. So it's actually about 8x faster to just get all the
            // attribute values and then filter out what we don't want. If we are getting
            // more than 25 attributes anyway then it probably means we are getting
            // pretty much all attributes in the first place.
            if ( attributes.Count > 25 )
            {
                items = rockContext.Database.SqlQuery<AttributeItemValue>(
                        @"
SELECT
    A.[EntityTypeId],
    AV.[EntityId],
    entityKey.[RealEntityId],
    AV.[AttributeId],
    AV.[Value],
    AV.[PersistedTextValue],
    AV.[PersistedHtmlValue],
    AV.[PersistedCondensedTextValue],
    AV.[PersistedCondensedHtmlValue],
    AV.[IsPersistedValueDirty]
FROM [AttributeValue] AV
INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
INNER JOIN @EntityKey entityKey ON entityKey.[EntityTypeId] = A.[EntityTypeId] AND entityKey.[EntityId] = AV.[EntityId]",
                        entityIdsParameter )
                    .ToList();
            }
            else
            {
                // Initialize the AttributeId SQL parameter.
                var attributeIdsTable = new DataTable();
                attributeIdsTable.Columns.Add( "Id", typeof( int ) );

                for ( int i = 0; i < attributes.Count; i++ )
                {
                    attributeIdsTable.Rows.Add( attributes[i].Id );
                }

                var attributeIdsParameter = new SqlParameter( "@AttributeId", SqlDbType.Structured )
                {
                    TypeName = "dbo.EntityIdList",
                    Value = attributeIdsTable
                };

                items = rockContext.Database.SqlQuery<AttributeItemValue>(
                        @"
SELECT
    A.[EntityTypeId],
    AV.[EntityId],
    entityKey.[RealEntityId],
    AV.[AttributeId],
    AV.[Value],
    AV.[PersistedTextValue],
    AV.[PersistedHtmlValue],
    AV.[PersistedCondensedTextValue],
    AV.[PersistedCondensedHtmlValue],
    AV.[IsPersistedValueDirty]
FROM [AttributeValue] AV
INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
INNER JOIN @EntityKey entityKey ON entityKey.[EntityTypeId] = A.[EntityTypeId] AND entityKey.[EntityId] = AV.[EntityId]
INNER JOIN @AttributeId attributeId ON attributeId.[Id] = AV.[AttributeId]",
                        entityIdsParameter, attributeIdsParameter )
                .ToList();
            }

            return items.ToDictionary( i => ( i.RealEntityId, i.AttributeId ), i => i );
        }

        #endregion

        /// <summary>
        /// Gets the attribute categories.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="onlyIncludeGridColumns">if set to <c>true</c> will only include those attributes with the option to display in grid set to true</param>
        /// <param name="allowMultiple">if set to <c>true</c> returns the attribute in each of its categories, if false, only returns attribute in first category.</param>
        /// <param name="supressOrdering">if set to <c>true</c> suppresses reordering of the attributes within each Category. (LoadAttributes() may perform custom ordering as is the case for group member attributes).</param>
        /// <returns></returns>
        public static List<AttributeCategory> GetAttributeCategories( Rock.Attribute.IHasAttributes entity, bool onlyIncludeGridColumns = false, bool allowMultiple = false, bool supressOrdering = false )
        {
            if ( entity != null )
            {
                var attributes = entity.Attributes.Select( a => a.Value ).Where( a => a.IsActive );
                if ( !supressOrdering )
                {
                    attributes = attributes.OrderBy( t => t.EntityTypeQualifierValue ).ThenBy( t => t.Order ).ThenBy( t => t.Name );
                }

                return GetAttributeCategories( attributes.ToList(), onlyIncludeGridColumns, allowMultiple );
            }

            return null;
        }

        /// <summary>
        /// Gets attributes grouped by category
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="onlyIncludeGridColumns">if set to <c>true</c> will only include those attributes with the option to display in grid set to true</param>
        /// <param name="allowMultiple">if set to <c>true</c> returns the attribute in each of its categories, if false, only returns attribute in first category.</param>
        /// <returns></returns>
        public static List<AttributeCategory> GetAttributeCategories( List<Rock.Web.Cache.AttributeCache> attributes, bool onlyIncludeGridColumns = false, bool allowMultiple = false )
        {
            var attributeCategories = new List<AttributeCategory>();

            if ( onlyIncludeGridColumns )
            {
                attributes = attributes.Where( a => a.IsGridColumn ).ToList();
            }

            foreach ( var attribute in attributes )
            {
                if ( attribute.Categories.Any() )
                {
                    foreach ( var category in attribute.Categories.OrderBy( c => c.Name ) )
                    {
                        AddAttributeCategory( attributeCategories, category, attribute );
                        if ( !allowMultiple )
                        {
                            break;
                        }
                    }
                }
                else
                {
                    AddAttributeCategory( attributeCategories, null, attribute );
                }
            }

            return attributeCategories.OrderBy( c => c.CategoryName ).ToList();
        }

        private static void AddAttributeCategory( List<AttributeCategory> attributeCategories, CategoryCache category, Rock.Web.Cache.AttributeCache attribute )
        {
            AttributeCategory attributeCategory = null;
            if ( category != null )
            {
                attributeCategory = attributeCategories.Where( g => g.Category != null && g.Category.Id == category.Id ).FirstOrDefault();
            }
            else
            {
                attributeCategory = attributeCategories.Where( g => g.Category == null ).FirstOrDefault();
            }

            if ( attributeCategory == null )
            {
                attributeCategory = new AttributeCategory();
                attributeCategory.Category = category;
                attributeCategory.Attributes = new List<Rock.Web.Cache.AttributeCache>();
                attributeCategories.Add( attributeCategory );
            }

            attributeCategory.Attributes.Add( attribute );
        }

        #region Save Attributes

        /// <summary>
        /// Saves any attribute edits made using an Attribute Editor control
        /// </summary>
        /// <param name="edtAttribute">The edt attribute.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        public static Rock.Model.Attribute SaveAttributeEdits( AttributeEditor edtAttribute, int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue, RockContext rockContext = null )
        {
            // Create and update a new attribute object with new values
            rockContext = rockContext ?? new RockContext();
            var internalAttributeService = new AttributeService( rockContext );

            Rock.Model.Attribute attribute = null;
            var newAttribute = new Rock.Model.Attribute();

            if ( edtAttribute.AttributeId.HasValue )
            {
                attribute = internalAttributeService.Get( edtAttribute.AttributeId.Value );
            }

            if ( attribute != null )
            {
                newAttribute.CopyPropertiesFrom( attribute );
            }
            else
            {
                newAttribute.Order = internalAttributeService.Queryable().Max( a => a.Order ) + 1;
            }

            edtAttribute.GetAttributeProperties( newAttribute );

            return SaveAttributeEdits( newAttribute, entityTypeId, entityTypeQualifierColumn, entityTypeQualifierValue, rockContext );
        }

        /// <summary>
        /// Saves any attribute edits made using a view model.
        /// </summary>
        /// <param name="attribute">The attribute values that were edited.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Rock.Model.Attribute"/> that was saved to the database.</returns>
        /// <remarks>
        /// If a <paramref name="rockContext"/> is included, this method will save any previous changes made to the context.
        /// </remarks>
        internal static Rock.Model.Attribute SaveAttributeEdits( PublicEditableAttributeBag attribute, int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var attributeService = new AttributeService( rockContext );
            Rock.Model.Attribute existingAttribute = null;

            var newAttribute = new Rock.Model.Attribute();

            // Try to load the existing attribute if we have one.
            if ( attribute.Guid.HasValue )
            {
                existingAttribute = attributeService.Get( attribute.Guid ?? Guid.Empty );
            }

            // If we found an existing attribute, copy its values over to our new
            // attribute. Otherwise set the initial Order value on the new attribute.
            if ( existingAttribute != null )
            {
                newAttribute.CopyPropertiesFrom( existingAttribute );
            }
            else
            {
                newAttribute.Order = attributeService.Queryable().Max( a => a.Order ) + 1;
            }

            var fieldTypeCache = FieldTypeCache.Get( attribute.RealFieldTypeGuid ?? attribute.FieldTypeGuid ?? Guid.Empty );

            // For now, if they try to make changes to an attribute that is using
            // an unknown field type we just can't do it. Even if they only change
            // the name, just don't allow it.
            if ( fieldTypeCache?.Field == null )
            {
                throw new Exception( "Unable to save attribute referencing unknown field type." );
            }

            var configurationValues = fieldTypeCache.Field.GetPrivateConfigurationValues( attribute.ConfigurationValues );

            // Note: We intentionally ignore IsSystem, that cannot be changed by the user.
            newAttribute.Name = attribute.Name;
            newAttribute.AbbreviatedName = attribute.AbbreviatedName;
            newAttribute.Key = attribute.Key;
            newAttribute.Description = attribute.Description;
            newAttribute.IsActive = attribute.IsActive;
            newAttribute.IsPublic = attribute.IsPublic;
            newAttribute.IsRequired = attribute.IsRequired;
            newAttribute.ShowOnBulk = attribute.IsShowOnBulk;
            newAttribute.IsGridColumn = attribute.IsShowInGrid;
            newAttribute.IsAnalytic = attribute.IsAnalytic;
            newAttribute.IsAnalyticHistory = attribute.IsAnalyticHistory;
            newAttribute.AllowSearch = attribute.IsAllowSearch;
            newAttribute.EnableHistory = attribute.IsEnableHistory;
            newAttribute.IsIndexEnabled = attribute.IsIndexEnabled;
            newAttribute.PreHtml = attribute.PreHtml;
            newAttribute.PostHtml = attribute.PostHtml;
            newAttribute.FieldTypeId = fieldTypeCache.Id;
            newAttribute.DefaultValue = fieldTypeCache.Field.GetPrivateEditValue( attribute.DefaultValue, configurationValues );

            var categoryGuids = attribute.Categories?.Select( c => c.Value.AsGuid() ).ToList();
            newAttribute.Categories.Clear();
            if ( categoryGuids != null && categoryGuids.Any() )
            {
                new CategoryService( rockContext ).Queryable()
                    .Where( c => categoryGuids.Contains( c.Guid ) )
                    .ToList()
                    .ForEach( c => newAttribute.Categories.Add( c ) );
            }

            // Since changes to Categories isn't tracked by ChangeTracker,
            // set the ModifiedDateTime just in case Categories is the only
            // actual change.
            newAttribute.ModifiedDateTime = RockDateTime.Now;

            // Clear out any old qualifier values and then set
            newAttribute.AttributeQualifiers.Clear();
            foreach ( var qualifier in configurationValues )
            {
                AttributeQualifier attributeQualifier = new AttributeQualifier
                {
                    IsSystem = false,
                    Key = qualifier.Key,
                    Value = qualifier.Value ?? string.Empty
                };

                newAttribute.AttributeQualifiers.Add( attributeQualifier );
            }

            // Merge in any old qualifiers if they were not provided by the client.
            if ( existingAttribute != null )
            {
                foreach ( var qualifier in existingAttribute.AttributeQualifiers )
                {
                    var aq = newAttribute.AttributeQualifiers.FirstOrDefault( q => q.Key == qualifier.Key );

                    if ( aq == null )
                    {
                        AttributeQualifier attributeQualifier = new AttributeQualifier
                        {
                            IsSystem = false,
                            Key = qualifier.Key,
                            Value = qualifier.Value ?? string.Empty
                        };

                        newAttribute.AttributeQualifiers.Add( attributeQualifier );
                    }
                }
            }

            return SaveAttributeEdits( newAttribute, entityTypeId, entityTypeQualifierColumn, entityTypeQualifierValue, rockContext );
        }

        /// <summary>
        /// Saves the attribute edits.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveAttributeEdits( List<Rock.Model.Attribute> attributes, int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue, RockContext rockContext = null )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, entityTypeQualifierColumn, entityTypeQualifierValue, true );

            // Delete any of those attributes that don't exist in the specified attributes
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attribute in attributes )
            {
                Helper.SaveAttributeEdits( attribute, entityTypeId, entityTypeQualifierColumn, entityTypeQualifierValue, rockContext );
            }
        }

        /// <summary>
        /// Saves any attribute edits made to an attribute.
        /// </summary>
        /// <param name="newAttribute">The new attribute.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        public static Rock.Model.Attribute SaveAttributeEdits( Rock.Model.Attribute newAttribute, int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var internalAttributeService = new AttributeService( rockContext );
            var attributeQualifierService = new AttributeQualifierService( rockContext );
            var categoryService = new CategoryService( rockContext );
            bool isNew;

            // If attribute is not valid, return null
            if ( !newAttribute.IsValid )
            {
                return null;
            }

            // Create a attribute model that will be saved
            Rock.Model.Attribute attribute = null;

            List<AttributeQualifier> existingQualifiers;

            // Check to see if this was an existing or new attribute
            if ( newAttribute.Id > 0 )
            {
                existingQualifiers = attributeQualifierService.GetByAttributeId( newAttribute.Id ).ToList();

                rockContext.SaveChanges();

                // Then re-load the existing attribute 
                attribute = internalAttributeService.Get( newAttribute.Id );
            }
            else
            {
                existingQualifiers = new List<AttributeQualifier>();
            }

            if ( attribute == null )
            {
                // If the attribute didn't exist, create it
                attribute = new Rock.Model.Attribute();
                internalAttributeService.Add( attribute );
                isNew = true;
            }
            else
            {
                // If it did exist, set the new attribute ID and GUID since we're copying all properties in the next step
                newAttribute.Id = attribute.Id;
                newAttribute.Guid = attribute.Guid;
                isNew = false;
            }

            // Copy all the properties from the new attribute to the attribute model
            attribute.CopyPropertiesFrom( newAttribute );

            var oldConfigurationValues = existingQualifiers.ToDictionary( eq => eq.Key, eq => eq.Value );
            var newConfigurationValues = newAttribute.AttributeQualifiers.ToDictionary( aq => aq.Key, aq => aq.Value );

            var addedQualifiers = newAttribute.AttributeQualifiers.Where( a => !existingQualifiers.Any( x => x.Key == a.Key ) );
            var deletedQualifiers = existingQualifiers.Where( a => !newAttribute.AttributeQualifiers.Any( x => x.Key == a.Key ) );
            var modifiedQualifiers = newAttribute.AttributeQualifiers.Where( a => existingQualifiers.Any( x => x.Key == a.Key && a.Value != x.Value ) );

            // Add any new qualifiers
            foreach ( var addedQualifier in addedQualifiers )
            {
                attribute.AttributeQualifiers.Add( new AttributeQualifier { Key = addedQualifier.Key, Value = addedQualifier.Value, IsSystem = addedQualifier.IsSystem } );
            }

            // Delete any deleted qualifiers
            attributeQualifierService.DeleteRange( deletedQualifiers );

            foreach ( var deletedQualifier in deletedQualifiers )
            {
                attribute.AttributeQualifiers.Remove( deletedQualifier );
            }

            // Update any modified qualifiers
            foreach ( var modifiedQualifier in modifiedQualifiers )
            {
                var existingQualifier = attribute.AttributeQualifiers.Where( a => a.Key == modifiedQualifier.Key ).FirstOrDefault();
                if ( existingQualifier != null )
                {
                    existingQualifier.Value = modifiedQualifier.Value;
                }
            }

            // Add any categories
            attribute.Categories.Clear();
            foreach ( var category in newAttribute.Categories )
            {
                attribute.Categories.Add( categoryService.Get( category.Id ) );
            }

            // Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime just in case Categories changed
            attribute.ModifiedDateTime = RockDateTime.Now;

            attribute.EntityTypeId = entityTypeId;
            attribute.EntityTypeQualifierColumn = entityTypeQualifierColumn;
            attribute.EntityTypeQualifierValue = entityTypeQualifierValue;

            rockContext.ExecuteAfterCommit( () =>
            {
                using ( var innerContext = new RockContext() )
                {
                    // Don't use the cache because we aren't 100% confident it is
                    // safe to hit the cache right now.
                    var innerAttribute = new AttributeService( innerContext ).Get( attribute.Id );

                    if ( innerAttribute == null )
                    {
                        return;
                    }

                    UpdateAttributeDefaultPersistedValues( innerAttribute );
                    UpdateAttributeEntityReferences( innerAttribute, innerContext );

                    // Disable pre-post processing because this is a special update.
                    // We don't want to touch anything or trigger any special logic.
                    innerContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
                }

                // If we are updating an existing attribute then check if we need
                // to recalculate all existing values.
                if ( !isNew )
                {
                    var field = FieldTypeCache.Get( attribute.FieldTypeId )?.Field;

                    if ( field != null && field.IsPersistedValueInvalidated( oldConfigurationValues, newConfigurationValues ) )
                    {
                        // Run on a task because this operation could take a while.
                        Task.Run( () => BulkUpdateInvalidatedPersistedValues( attribute, newConfigurationValues ) );
                    }
                }
            } );

            rockContext.SaveChanges();

            return attribute;
        }

        #endregion

        #region Save Attribute Values

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        public static void SaveAttributeValues( Rock.Attribute.IHasAttributes model, RockContext rockContext = null )
        {
            if ( model != null && model.Attributes != null && model.AttributeValues != null && model.Attributes.Any() && model.AttributeValues.Any() )
            {
                rockContext = rockContext ?? new RockContext();
                var attributeValueService = new Model.AttributeValueService( rockContext );

                var attributeIds = model.Attributes.Select( y => y.Value.Id ).ToList();
                var valueQuery = attributeValueService.Queryable().WhereAttributeIds( attributeIds ).Where( x => x.EntityId == model.Id );
                var attributeValuesThatWereChanged = new List<AttributeValue>();

                var attributeValues = valueQuery.ToDictionary( x => x.AttributeKey );
                foreach ( var attribute in model.Attributes.Values )
                {
                    if ( model.AttributeValues.ContainsKey( attribute.Key ) )
                    {
                        if ( attributeValues.ContainsKey( attribute.Key ) )
                        {
                            if ( attributeValues[attribute.Key].Value != model.AttributeValues[attribute.Key].Value )
                            {
                                attributeValues[attribute.Key].Value = model.AttributeValues[attribute.Key].Value;

                                attributeValuesThatWereChanged.Add( attributeValues[attribute.Key] );
                            }
                        }
                        else
                        {
                            // only save a new AttributeValue if the value has a nonempty value
                            var value = model.AttributeValues[attribute.Key].Value;
                            if ( value.IsNotNullOrWhiteSpace() )
                            {
                                var attributeValue = new AttributeValue();
                                attributeValue.AttributeId = attribute.Id;
                                attributeValue.EntityId = model.Id;
                                attributeValue.Value = value;

                                attributeValueService.Add( attributeValue );
                                attributeValuesThatWereChanged.Add( attributeValue );
                            }
                        }
                    }
                }

                // If nothing changed, we don't need to save anything.
                if ( !attributeValuesThatWereChanged.Any() )
                {
                    return;
                }

                // Execute after the commit since getting the persisted values
                // could cause deadlock inside a transaction.
                rockContext.ExecuteAfterCommit( () =>
                {
                    var changedValueIds = attributeValuesThatWereChanged.Select( av => av.Id ).ToList();
                    var attributeValueReferenceValues = new List<AttributeValue>();

                    using ( var innerContext = new RockContext() )
                    {
                        var changedAttributeValues = new AttributeValueService( innerContext )
                            .Queryable()
                            .Where( av => changedValueIds.Contains( av.Id ) )
                            .ToList();

                        foreach ( var changedAttributeValue in changedAttributeValues )
                        {
                            var attributeCache = AttributeCache.Get( changedAttributeValue.AttributeId );

                            if ( attributeCache != null )
                            {
                                UpdateAttributeValuePersistedValues( changedAttributeValue, attributeCache );

                                if ( attributeCache.IsReferencedEntityFieldType )
                                {
                                    attributeValueReferenceValues.Add( changedAttributeValue );
                                }
                            }
                        }

                        // Disable processing because this is a special update
                        // that is only used by the persisted value system and should
                        // not trigger any other logic.
                        innerContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );

                        BulkUpdateAttributeValueEntityReferences( attributeValueReferenceValues, innerContext );
                    }
                } );

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Saves an attribute value.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        public static void SaveAttributeValue( Rock.Attribute.IHasAttributes model, Rock.Web.Cache.AttributeCache attribute, string newValue, RockContext rockContext = null )
        {
            if ( model != null && attribute != null )
            {
                rockContext = rockContext ?? new RockContext();
                var attributeValueService = new Model.AttributeValueService( rockContext );

                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, model.Id );
                if ( attributeValue == null )
                {
                    if ( newValue == null )
                    {
                        return;
                    }

                    attributeValue = new Rock.Model.AttributeValue();
                    attributeValue.AttributeId = attribute.Id;
                    attributeValue.EntityId = model.Id;
                    attributeValueService.Add( attributeValue );
                }

                if ( attributeValue.Value != newValue )
                {
                    attributeValue.Value = newValue;

                    // Execute after the commit since getting the persisted values
                    // could cause deadlock inside a transaction.
                    rockContext.ExecuteAfterCommit( () =>
                    {
                        using ( var innerContext = new RockContext() )
                        {
                            var innerAttributeValue = new AttributeValueService( innerContext ).Get( attributeValue.Id );

                            if ( innerAttributeValue == null )
                            {
                                return;
                            }

                            UpdateAttributeValuePersistedValues( innerAttributeValue, attribute );

                            if ( attribute.IsReferencedEntityFieldType )
                            {
                                UpdateAttributeValueEntityReferences( innerAttributeValue, innerContext );
                            }

                            // Disable processing because this is a special update
                            // that is only used by the persisted value system and should
                            // not trigger any other logic.
                            innerContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
                        }
                    } );

                    rockContext.SaveChanges();
                }

                if ( model.AttributeValues != null && model.AttributeValues.ContainsKey( attribute.Key ) )
                {
                    model.AttributeValues[attribute.Key] = new AttributeValueCache( attributeValue );
                }
            }
        }

        /// <summary>
        /// Saves the attribute value.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        public static void SaveAttributeValue( int entityId, Rock.Web.Cache.AttributeCache attribute, string newValue, RockContext rockContext = null )
        {
            if ( attribute != null )
            {
                rockContext = rockContext ?? new RockContext();
                var attributeValueService = new Model.AttributeValueService( rockContext );

                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, entityId );
                if ( attributeValue == null )
                {
                    if ( newValue == null )
                    {
                        return;
                    }

                    attributeValue = new Rock.Model.AttributeValue();
                    attributeValue.AttributeId = attribute.Id;
                    attributeValue.EntityId = entityId;
                    attributeValueService.Add( attributeValue );
                }

                if ( attributeValue.Value != newValue )
                {
                    attributeValue.Value = newValue;

                    // Execute after the commit since getting the persisted values
                    // could cause deadlock inside a transaction.
                    rockContext.ExecuteAfterCommit( () =>
                    {
                        using ( var innerContext = new RockContext() )
                        {
                            var innerAttributeValue = new AttributeValueService( innerContext ).Get( attributeValue.Id );

                            if ( innerAttributeValue == null )
                            {
                                return;
                            }

                            UpdateAttributeValuePersistedValues( innerAttributeValue, attribute );

                            if ( attribute.IsReferencedEntityFieldType )
                            {
                                UpdateAttributeValueEntityReferences( innerAttributeValue, innerContext );
                            }

                            // Disable processing because this is a special update
                            // that is only used by the persisted value system and should
                            // not trigger any other logic.
                            innerContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
                        }
                    } );
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the computed columns (ValueAs...) of all attribute values
        /// belonging to the specified attribute identifier that match the
        /// given value.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method immediately updates the database, no SaveChanges()
        ///         call is required.
        ///     </para>
        /// </remarks>
        /// <param name="attributeId">The attribute identifier of the values to be updated.</param>
        /// <param name="value">The current value of those attribute values.</param>
        /// <param name="rockContext">The database context to use when performing the update.</param>
        /// <returns>The number of rows that were updated.</returns>
        internal static int BulkUpdateAttributeValueComputedColumns( int attributeId, string value, RockContext rockContext )
        {
            var attributeValue = new AttributeValue
            {
                AttributeId = attributeId,
                Value = value
            };

            attributeValue.UpdateValueAsProperties( rockContext );

            var valueAsBooleanParameter = new SqlParameter( "@ValueAsBoolean", (object) attributeValue.ValueAsBoolean ?? DBNull.Value );
            var valueAsDateTimeParameter = new SqlParameter( "@ValueAsDateTime", ( object ) attributeValue.ValueAsDateTime ?? DBNull.Value );
            var valueAsNumericParameter = new SqlParameter( "@ValueAsNumeric", ( object ) attributeValue.ValueAsNumeric ?? DBNull.Value );
            var valueAsPersonIdParameter = new SqlParameter( "@ValueAsPersonId", ( object ) attributeValue.ValueAsPersonId ?? DBNull.Value );
            var attributeIdParameter = new SqlParameter( "@AttributeId", attributeId );
            var valueParameter = new SqlParameter( "@Value", ( object ) value ?? DBNull.Value );

            return rockContext.Database.ExecuteSqlCommand( @"
UPDATE [AttributeValue]
SET [ValueAsBoolean] = @ValueAsBoolean,
    [ValueAsDateTime] = @ValueAsDateTime,
    [ValueAsNumeric] = @ValueAsNumeric,
    [ValueAsPersonId] = @ValueAsPersonId
WHERE [AttributeId] = @AttributeId
  AND [ValueChecksum] = CHECKSUM(@Value)
  AND [Value] = @Value",
                valueAsBooleanParameter,
                valueAsDateTimeParameter,
                valueAsNumericParameter,
                valueAsPersonIdParameter,
                attributeIdParameter,
                valueParameter );
        }

        /// <summary>
        /// Updates the computed columns (ValueAs...) of all attribute values
        /// that match the specified <paramref name="valueIds"/>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method immediately updates the database, no SaveChanges()
        ///         call is required.
        ///     </para>
        /// </remarks>
        /// <param name="attributeId">The attribute identifier of the values to be updated.</param>
        /// <param name="valueIds">The identifiers of the attribute values to be updated.</param>
        /// <param name="value">The current value of those attribute values.</param>
        /// <param name="rockContext">The database context to use when performing the update.</param>
        /// <returns>The number of rows that were updated.</returns>
        internal static int BulkUpdateAttributeValueComputedColumns( int attributeId, IEnumerable<int> valueIds, string value, RockContext rockContext )
        {
            var attributeValue = new AttributeValue
            {
                AttributeId = attributeId,
                Value = value
            };

            attributeValue.UpdateValueAsProperties( rockContext );

            var valueAsBooleanParameter = new SqlParameter( "@ValueAsBoolean", ( object ) attributeValue.ValueAsBoolean ?? DBNull.Value );
            var valueAsDateTimeParameter = new SqlParameter( "@ValueAsDateTime", ( object ) attributeValue.ValueAsDateTime ?? DBNull.Value );
            var valueAsNumericParameter = new SqlParameter( "@ValueAsNumeric", ( object ) attributeValue.ValueAsNumeric ?? DBNull.Value );
            var valueAsPersonIdParameter = new SqlParameter( "@ValueAsPersonId", ( object ) attributeValue.ValueAsPersonId ?? DBNull.Value );

            // Initialize the ValueId SQL parameter.
            var attributeIdsTable = new DataTable();
            attributeIdsTable.Columns.Add( "Id", typeof( int ) );

            foreach ( var valueId in valueIds )
            {
                attributeIdsTable.Rows.Add( valueId );
            }

            var valueIdParameter = new SqlParameter( "@ValueId", SqlDbType.Structured )
            {
                TypeName = "dbo.EntityIdList",
                Value = attributeIdsTable
            };

            return rockContext.Database.ExecuteSqlCommand( @"
UPDATE AV
SET [AV].[ValueAsBoolean] = @ValueAsBoolean,
    [AV].[ValueAsDateTime] = @ValueAsDateTime,
    [AV].[ValueAsNumeric] = @ValueAsNumeric,
    [AV].[ValueAsPersonId] = @ValueAsPersonId
FROM [AttributeValue] AS [AV]
INNER JOIN @ValueId AS [valueId] ON  [valueId].[Id] = [AV].[Id]",
                valueAsBooleanParameter,
                valueAsDateTimeParameter,
                valueAsNumericParameter,
                valueAsPersonIdParameter,
                valueIdParameter );
        }

        #endregion

        #region Persisted Values

        /// <summary>
        /// Gets the persisted values from the field type. If an error occurs then
        /// the placeholder value will be used instead. If even that fails then the
        /// standard not supported message will be used.
        /// </summary>
        /// <remarks>
        /// <strong>Note:</strong> This does not check if the field supports
        /// persisted values, only if an error occurs while getting them.
        /// </remarks>
        /// <param name="field">The field type to use when formatting the values.</param>
        /// <param name="rawValue">The raw value to be formatted.</param>
        /// <param name="configuration">The configuration of the field type.</param>
        /// <param name="cache">A cache dictionary that can be used by the field type to store and retrieve data that would otherwise need a database hit.</param>
        /// <returns>An instance of <see cref="Field.PersistedValues"/> that specifies all the values to be persisted.</returns>
        internal static Field.PersistedValues GetPersistedValuesOrPlaceholder( Field.IFieldType field, string rawValue, Dictionary<string, string> configuration, IDictionary<string, object> cache )
        {
            try
            {
                return field.GetPersistedValues( rawValue, configuration, cache );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Unable to retrieve persisted values from field.", ex ) );

                return GetPersistedValuePlaceholderOrDefault( field, configuration );
            }
        }

        /// <summary>
        /// Gets the persisted value placeholder from the field type. If an error
        /// occurs then the standard not supported message will be used.
        /// </summary>
        /// <remarks>
        /// <strong>Note:</strong> This does not check if the field supports
        /// persisted values, only if an error occurs while getting the placeholder.
        /// </remarks>
        /// <param name="field">The field type to use when formatting the values.</param>
        /// <param name="configuration">The configuration of the field type.</param>
        /// <returns>An instance of <see cref="Field.PersistedValues"/> that specifies all the placeholder values to be used.</returns>
        internal static Field.PersistedValues GetPersistedValuePlaceholderOrDefault( Field.IFieldType field, Dictionary<string, string> configuration )
        {
            try
            {
                var placeholder = field.GetPersistedValuePlaceholder( configuration ) ?? Rock.Constants.DisplayStrings.PersistedValuesAreNotSupported;

                return new Field.PersistedValues
                {
                    TextValue = placeholder,
                    CondensedTextValue = placeholder,
                    HtmlValue = placeholder,
                    CondensedHtmlValue = placeholder
                };
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Unable to retrieve placeholder values from field.", ex ) );

                return new Field.PersistedValues
                {
                    TextValue = Rock.Constants.DisplayStrings.PersistedValuesAreNotSupported,
                    CondensedTextValue = Rock.Constants.DisplayStrings.PersistedValuesAreNotSupported,
                    HtmlValue = Rock.Constants.DisplayStrings.PersistedValuesAreNotSupported,
                    CondensedHtmlValue = Rock.Constants.DisplayStrings.PersistedValuesAreNotSupported
                };
            }
        }

        /// <summary>
        /// Updates the attribute default persisted values to match the <see cref="Rock.Model.Attribute.DefaultValue"/>.
        /// </summary>
        /// <param name="attribute">The attribute whose persisted values need to be updated.</param>
        internal static void UpdateAttributeDefaultPersistedValues( Rock.Model.Attribute attribute )
        {
            var field = FieldTypeCache.Get( attribute.FieldTypeId )?.Field;
            var configuration = attribute.AttributeQualifiers.ToDictionary( aq => aq.Key, aq => aq.Value );

            if ( field != null && field.IsPersistedValueSupported( configuration ) )
            {
                var persistedValues = GetPersistedValuesOrPlaceholder( field, attribute.DefaultValue, configuration, null );

                attribute.DefaultPersistedTextValue = persistedValues.TextValue;
                attribute.DefaultPersistedHtmlValue = persistedValues.HtmlValue;
                attribute.DefaultPersistedCondensedTextValue = persistedValues.CondensedTextValue;
                attribute.DefaultPersistedCondensedHtmlValue = persistedValues.CondensedHtmlValue;
                attribute.IsDefaultPersistedValueDirty = false;
            }
            else
            {
                var placeholder = field?.GetPersistedValuePlaceholder( configuration ) ?? Rock.Constants.DisplayStrings.PersistedValuesAreNotSupported;

                attribute.DefaultPersistedTextValue = placeholder;
                attribute.DefaultPersistedHtmlValue = placeholder;
                attribute.DefaultPersistedCondensedTextValue = placeholder;
                attribute.DefaultPersistedCondensedHtmlValue = placeholder;
                attribute.IsDefaultPersistedValueDirty = false;
            }
        }

        /// <summary>
        /// Bulk updates all the values for an attribute. This should be called
        /// any time the configuration values have changed in a way that would
        /// cause the persisted values to no longer be valid.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method immediately updates the database, no SaveChanges()
        ///         call is required.
        ///     </para>
        ///     <para>
        ///         This can be a very expensive method call, so it should not be called
        ///         on a UI related thread.
        ///     </para>
        /// </remarks>
        /// <param name="attribute">The attribute that needs to be updated.</param>
        /// <param name="configurationValues">The configuration values for the attribute.</param>
        private static int BulkUpdateInvalidatedPersistedValues( Rock.Model.Attribute attribute, Dictionary<string, string> configurationValues )
        {
            var field = FieldTypeCache.Get( attribute.FieldType )?.Field;
            var count = 0;

            // Field is kind of required...
            if ( field == null )
            {
                return 0;
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    // Make this 5 minutes because on large data sets this could take a while.
                    // And by large I mean like 300,000 attribute values.
                    rockContext.Database.CommandTimeout = 300;

                    var distinctValues = new AttributeValueService( rockContext )
                        .Queryable()
                        .Where( av => av.AttributeId == attribute.Id )
                        .Select( av => av.Value )
                        .Distinct()
                        .ToList();

                    var cache = new Dictionary<string, object>();

                    foreach ( var value in distinctValues )
                    {
                        if ( field.IsPersistedValueSupported( configurationValues ) )
                        {
                            var persistedValues = GetPersistedValuesOrPlaceholder( field, value, configurationValues, cache );

                            count += BulkUpdateAttributeValuePersistedValues( attribute.Id, value, persistedValues, rockContext );
                        }
                        else
                        {
                            var persistedValues = GetPersistedValuePlaceholderOrDefault( field, configurationValues );

                            count += BulkUpdateAttributeValuePersistedValues( attribute.Id, value, persistedValues, rockContext );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            return count;
        }

        /// <summary>
        /// Bulk updates the persisted values for all the values of an attribute
        /// that have the specified value. This updates whether they are marked
        /// as <see cref="AttributeValue.IsPersistedValueDirty"/> or not.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method immediately updates the database, no SaveChanges()
        ///         call is required.
        ///     </para>
        /// </remarks>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="value">The value that matches the <see cref="AttributeValue.Value"/> property.</param>
        /// <param name="persistedValues">The persisted values to use during the update.</param>
        /// <param name="rockContext">The database context to use when updating.</param>
        /// <returns>The number of attribute value rows that were updated.</returns>
        internal static int BulkUpdateAttributeValuePersistedValues( int attributeId, string value, Rock.Field.PersistedValues persistedValues, RockContext rockContext )
        {
            var textValueParameter = new SqlParameter( "@TextValue", ( object ) persistedValues.TextValue ?? DBNull.Value );
            var htmlValueParameter = new SqlParameter( "@HtmlValue", ( object ) persistedValues.HtmlValue ?? DBNull.Value );
            var condensedTextValueParameter = new SqlParameter( "@CondensedTextValue", ( object ) persistedValues.CondensedTextValue ?? DBNull.Value );
            var condensedHtmlValueParameter = new SqlParameter( "@CondensedHtmlValue", ( object ) persistedValues.CondensedHtmlValue ?? DBNull.Value );
            var attributeIdParameter = new SqlParameter( "@AttributeId", attributeId );
            var valueParameter = new SqlParameter( "@Value", ( object ) value ?? DBNull.Value );

            return rockContext.Database.ExecuteSqlCommand( @"
UPDATE [AttributeValue]
SET [PersistedTextValue] = @TextValue,
    [PersistedHtmlValue] = @HtmlValue,
    [PersistedCondensedTextValue] = @CondensedTextValue,
    [PersistedCondensedHtmlValue] = @CondensedHtmlValue,
    [IsPersistedValueDirty] = 0
WHERE [AttributeId] = @AttributeId
  AND [ValueChecksum] = CHECKSUM(@Value)
  AND [Value] = @Value
  AND [IsPersistedValueDirty] = 1",
                textValueParameter,
                htmlValueParameter,
                condensedTextValueParameter,
                condensedHtmlValueParameter,
                attributeIdParameter,
                valueParameter );
        }

        /// <summary>
        /// Bulk updates the persisted values for the specified values of an attribute.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method immediately updates the database, no SaveChanges()
        ///         call is required.
        ///     </para>
        /// </remarks>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="valueIds">The value identifiers that should be updated.</param>
        /// <param name="persistedValues">The persisted values to use during the update.</param>
        /// <param name="onlyDirty">Only update the <see cref="AttributeValue"/> objects if they marked dirty.</param>
        /// <param name="rockContext">The database context to use when updating.</param>
        /// <returns>The number of attribute value rows that were updated.</returns>
        internal static int BulkUpdateAttributeValuePersistedValues( int attributeId, IEnumerable<int> valueIds, Rock.Field.PersistedValues persistedValues, bool onlyDirty, RockContext rockContext )
        {
            var textValueParameter = new SqlParameter( "@TextValue", ( object ) persistedValues.TextValue ?? DBNull.Value );
            var htmlValueParameter = new SqlParameter( "@HtmlValue", ( object ) persistedValues.HtmlValue ?? DBNull.Value );
            var condensedTextValueParameter = new SqlParameter( "@CondensedTextValue", ( object ) persistedValues.CondensedTextValue ?? DBNull.Value );
            var condensedHtmlValueParameter = new SqlParameter( "@CondensedHtmlValue", ( object ) persistedValues.CondensedHtmlValue ?? DBNull.Value );
            var attributeIdParameter = new SqlParameter( "@AttributeId", attributeId );
            var onlyDirtyParameter = new SqlParameter( "@OnlyDirty", onlyDirty );

            // Initialize the ValueId SQL parameter.
            var attributeIdsTable = new DataTable();
            attributeIdsTable.Columns.Add( "Id", typeof( int ) );

            foreach ( var valueId in valueIds )
            {
                attributeIdsTable.Rows.Add( valueId );
            }

            var valueIdParameter = new SqlParameter( "@ValueId", SqlDbType.Structured )
            {
                TypeName = "dbo.EntityIdList",
                Value = attributeIdsTable
            };

            return rockContext.Database.ExecuteSqlCommand( @"
UPDATE AV
SET [AV].[PersistedTextValue] = @TextValue,
    [AV].[PersistedHtmlValue] = @HtmlValue,
    [AV].[PersistedCondensedTextValue] = @CondensedTextValue,
    [AV].[PersistedCondensedHtmlValue] = @CondensedHtmlValue,
    [AV].[IsPersistedValueDirty] = 0
FROM [AttributeValue] AS [AV]
INNER JOIN @ValueId AS [valueId] ON  [valueId].[Id] = [AV].[Id]
WHERE [AV].[AttributeId] = @AttributeId
  AND (@OnlyDirty = 0 OR [AV].[IsPersistedValueDirty] = 1)",
                textValueParameter,
                htmlValueParameter,
                condensedTextValueParameter,
                condensedHtmlValueParameter,
                attributeIdParameter,
                onlyDirtyParameter,
                valueIdParameter );
        }

        /// <summary>
        /// Updates all entity references for the given attribute value.
        /// </summary>
        /// <param name="attributeValue">The attribute value that needs its references updated.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.14", true )]
        public static void UpdateAttributeValueEntityReferences( AttributeValue attributeValue, RockContext rockContext )
        {
            var referencedEntitySet = rockContext.Set<AttributeValueReferencedEntity>();
            var attributeCache = AttributeCache.Get( attributeValue.AttributeId );

            if ( attributeCache?.IsReferencedEntityFieldType != true )
            {
                return;
            }

            // Get all the existing referenced entities for those modified attribute values.
            var previousReferencedEntities = referencedEntitySet
                .Where( re => re.AttributeValueId == attributeValue.Id )
                .ToList();

            var field = ( Rock.Field.IEntityReferenceFieldType ) attributeCache.FieldType.Field;
            var referencedEntities = field.GetReferencedEntities( attributeValue.Value, attributeCache.ConfigurationValues ) ?? new List<Field.ReferencedEntity>();

            // Add references that don't already exist.
            foreach ( var referencedEntity in referencedEntities )
            {
                if ( !previousReferencedEntities.Any( re => re.EntityTypeId == referencedEntity.EntityTypeId && re.EntityId == referencedEntity.EntityId ) )
                {
                    referencedEntitySet.Add( new AttributeValueReferencedEntity
                    {
                        AttributeValueId = attributeValue.Id,
                        EntityTypeId = referencedEntity.EntityTypeId,
                        EntityId = referencedEntity.EntityId
                    } );
                }
            }

            // Remove references that are no longer needed.
            foreach ( var previousReferencedEntity in previousReferencedEntities )
            {
                if ( !referencedEntities.Any( re => re.EntityTypeId == previousReferencedEntity.EntityTypeId && re.EntityId == previousReferencedEntity.EntityId ) )
                {
                    referencedEntitySet.Remove( previousReferencedEntity );
                }
            }
        }

        /// <summary>
        /// Updates all entity references for the given attribute values.
        /// </summary>
        /// <param name="attributeValues">The list of attribute values.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <remarks>
        ///     <para>
        ///         This method immediately updates the database, no SaveChanges()
        ///         call is required.
        ///     </para>
        /// </remarks>
        private static void BulkUpdateAttributeValueEntityReferences( List<AttributeValue> attributeValues, RockContext rockContext )
        {
            var referenceDictionary = new Dictionary<int, List<Field.ReferencedEntity>>();

            foreach ( var attributeValue in attributeValues )
            {
                var attributeCache = AttributeCache.Get( attributeValue.AttributeId );

                if ( !attributeCache.IsReferencedEntityFieldType )
                {
                    continue;
                }

                var field = ( Rock.Field.IEntityReferenceFieldType ) attributeCache.FieldType.Field;
                var referencedEntities = field.GetReferencedEntities( attributeValue.Value, attributeCache.ConfigurationValues ) ?? new List<Field.ReferencedEntity>();

                referenceDictionary.Add( attributeValue.Id, referencedEntities );
            }

            BulkUpdateAttributeValueEntityReferences( referenceDictionary, rockContext );
        }

        /// <summary>
        /// Updates all entity references for the given attribute values.
        /// </summary>
        /// <param name="referenceDictionary">A dictionary whose keys identify the attribute value and corresponding value is the list of referenced entities.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <remarks>
        ///     <para>
        ///         This method immediately updates the database, no SaveChanges()
        ///         call is required.
        ///     </para>
        /// </remarks>
        internal static void BulkUpdateAttributeValueEntityReferences( Dictionary<int, List<Field.ReferencedEntity>> referenceDictionary, RockContext rockContext )
        {
            // Initialize the EntityKey SQL parameter.
            var dataTable = new DataTable();
            dataTable.Columns.Add( "ReferencedEntityTypeId", typeof( int ) );
            dataTable.Columns.Add( "ReferencedEntityId", typeof( int ) );
            dataTable.Columns.Add( "EntityId", typeof( int ) );

            // We only need to add rows if we have any referenced entities.
            if ( referenceDictionary.Any() )
            {
                foreach (var kvpReference in referenceDictionary )
                {
                    var valueId = kvpReference.Key;
                    var referencedEntities = kvpReference.Value;

                    for ( int referencedIndex = 0; referencedIndex < referencedEntities.Count; referencedIndex++ )
                    {
                        dataTable.Rows.Add( referencedEntities[referencedIndex].EntityTypeId, referencedEntities[referencedIndex].EntityId, valueId );
                    }
                }
            }

            var dataParameter = new SqlParameter( "@Data", SqlDbType.Structured )
            {
                TypeName = "dbo.AttributeReferencedEntityList",
                Value = dataTable
            };

            // Doing direct SQL like this is nearly 4x faster than using EF. Since
            // these are special use tables and not standard models that should be
            // fine.

            // Execute the raw SQL query to perform the update.
            // This first deletes any references that are no longer valid for
            // the attribute value. It then creates any new references
            // that are missing from the database.
            rockContext.Database.ExecuteSqlCommand( @"
DELETE [AVRE]
FROM [AttributeValueReferencedEntity] AS [AVRE]
LEFT OUTER JOIN @Data AS [D] ON [D].[EntityId] = [AVRE].[AttributeValueId] AND [D].[ReferencedEntityTypeId] = [AVRE].[EntityTypeId] AND [D].[ReferencedEntityId] = [AVRE].[EntityId]
WHERE [AVRE].[AttributeValueId] IN (SELECT [EntityId] FROM @Data)
  AND [D].[EntityId] IS NULL

INSERT INTO [AttributeValueReferencedEntity] ([AttributeValueId], [EntityTypeId], [EntityId])
	SELECT [D].[EntityId], [D].[ReferencedEntityTypeId], [D].[ReferencedEntityId]
	FROM @Data AS [D]
	LEFT OUTER JOIN [AttributeValueReferencedEntity] AS [AVRE] ON [AVRE].[AttributeValueId] = [D].[EntityId] AND [AVRE].[EntityTypeId] = [D].[ReferencedEntityTypeId] AND [AVRE].[EntityId] = [D].[ReferencedEntityId]
	WHERE [AVRE].[AttributeValueId] IS NULL",
                dataParameter );
        }

        /// <summary>
        /// Updates all entity references for the given attribute values.
        /// </summary>
        /// <param name="attribute">The attribute that needs its references updated for the default value.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.14", true )]
        public static void UpdateAttributeEntityReferences( Rock.Model.Attribute attribute, RockContext rockContext )
        {
            var referencedEntitySet = rockContext.Set<AttributeReferencedEntity>();

            // Get all the existing referenced entities for those modified attribute values.
            var previousReferencedEntities = referencedEntitySet
                .Where( re => re.AttributeId == attribute.Id )
                .ToList();

            if ( !( FieldTypeCache.Get( attribute.FieldTypeId ).Field is Rock.Field.IEntityReferenceFieldType field ) )
            {
                return;
            }

            // Get the configuration values from the attribute instead of
            // cache because it might not be saved yet.
            var configurationValues = new Dictionary<string, string>();
            foreach ( var qualifier in attribute.AttributeQualifiers )
            {
                configurationValues.AddOrReplace( qualifier.Key, qualifier.Value );
            }

            var referencedEntities = field.GetReferencedEntities( attribute.DefaultValue, configurationValues ) ?? new List<Field.ReferencedEntity>();

            // Add references that don't already exist.
            foreach ( var referencedEntity in referencedEntities )
            {
                if ( !previousReferencedEntities.Any( re => re.EntityTypeId == referencedEntity.EntityTypeId && re.EntityId == referencedEntity.EntityId ) )
                {
                    referencedEntitySet.Add( new AttributeReferencedEntity
                    {
                        AttributeId = attribute.Id,
                        EntityTypeId = referencedEntity.EntityTypeId,
                        EntityId = referencedEntity.EntityId
                    } );
                }
            }

            // Remove references that are no longer needed.
            foreach ( var previousReferencedEntity in previousReferencedEntities )
            {
                if ( !referencedEntities.Any( re => re.EntityTypeId == previousReferencedEntity.EntityTypeId && re.EntityId == previousReferencedEntity.EntityId ) )
                {
                    referencedEntitySet.Remove( previousReferencedEntity );
                }
            }
        }

        /// <summary>
        /// Updates the attribute value persisted values to match <see cref="AttributeValue.Value"/>.
        /// </summary>
        /// <param name="attributeValue">The attribute value to be updated.</param>
        /// <param name="attribute">The attribute that contains the configuration.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.14", true )]
        public static void UpdateAttributeValuePersistedValues( Rock.Model.AttributeValue attributeValue, AttributeCache attribute )
        {
            var field = attribute?.FieldType?.Field;

            if ( attribute.IsPersistedValueSupported && field != null )
            {
                var persistedValues = GetPersistedValuesOrPlaceholder( field, attributeValue.Value, attribute.ConfigurationValues, null );

                attributeValue.PersistedTextValue = persistedValues.TextValue;
                attributeValue.PersistedHtmlValue = persistedValues.HtmlValue;
                attributeValue.PersistedCondensedTextValue = persistedValues.CondensedTextValue;
                attributeValue.PersistedCondensedHtmlValue = persistedValues.CondensedHtmlValue;
                attributeValue.IsPersistedValueDirty = false;
            }
            else
            {
                var placeholder = field.GetPersistedValuePlaceholder( attribute.ConfigurationValues )
                    ?? Rock.Constants.DisplayStrings.PersistedValuesAreNotSupported;

                attributeValue.PersistedTextValue = placeholder;
                attributeValue.PersistedHtmlValue = placeholder;
                attributeValue.PersistedCondensedTextValue = placeholder;
                attributeValue.PersistedCondensedHtmlValue = placeholder;
                attributeValue.IsPersistedValueDirty = false;
            }
        }

        /// <summary>
        /// Update the attributes that depend on the changes made to a specific entity.
        /// </summary>
        /// <param name="attributeIds">The attribute identifiers whose values should be updated; should be <c>null</c> or empty to update all attributes.</param>
        /// <param name="entityTypeId">The identifier of the type of entity that was changed.</param>
        /// <param name="entityId">The identifier of the entity that was changed.</param>
        /// <param name="rockContext">The database context to use when accessing the database.</param>
        internal static void UpdateDependantAttributesAndValues( IReadOnlyList<int> attributeIds, int entityTypeId, int entityId, RockContext rockContext )
        {
            var qry = rockContext.Set<AttributeReferencedEntity>()
                .Where( re => re.EntityTypeId == entityTypeId && re.EntityId == entityId );

            if ( attributeIds != null && attributeIds.Any() )
            {
                qry = qry.Where( re => attributeIds.Contains( re.AttributeId ) );
            }

            // Get all attributes that reference this entity.
            var referencingAttributes = qry
                .Select( re => re.Attribute )
                .ToList();

            // Loop through each reference attribute and update.
            foreach ( var attribute in referencingAttributes )
            {
                var attributeCache = AttributeCache.Get( attribute.Id );
                var field = attributeCache.FieldType.Field;
                Field.PersistedValues persistedValues;

                if ( field.IsPersistedValueSupported( attributeCache.ConfigurationValues ) )
                {
                    persistedValues = GetPersistedValuesOrPlaceholder( field, attribute.DefaultValue, attributeCache.ConfigurationValues, null );
                }
                else
                {
                    var placeholderValue = field.GetPersistedValuePlaceholder( attributeCache.ConfigurationValues );

                    persistedValues = new Field.PersistedValues
                    {
                        TextValue = placeholderValue,
                        CondensedTextValue = placeholderValue,
                        HtmlValue = placeholderValue,
                        CondensedHtmlValue = placeholderValue
                    };
                }

                attribute.DefaultPersistedTextValue = persistedValues.TextValue;
                attribute.DefaultPersistedHtmlValue = persistedValues.HtmlValue;
                attribute.DefaultPersistedCondensedTextValue = persistedValues.CondensedTextValue;
                attribute.DefaultPersistedCondensedHtmlValue = persistedValues.CondensedHtmlValue;
                attribute.IsDefaultPersistedValueDirty = false;
            }

            rockContext.SaveChanges();

            // Now update all the values as well.
            UpdateDependantAttributeValues( attributeIds, entityTypeId, entityId, rockContext );
        }

        /// <summary>
        /// Update the attribute values that depend on the changes made to a specific entity.
        /// </summary>
        /// <param name="attributeIds">The attribute identifiers whose values should be updated; should be <c>null</c> or empty to update all attributes.</param>
        /// <param name="entityTypeId">The identifier of the type of entity that was changed.</param>
        /// <param name="entityId">The identifier of the entity that was changed.</param>
        /// <param name="rockContext">The database context to use when accessing the database.</param>
        private static void UpdateDependantAttributeValues( IReadOnlyList<int> attributeIds, int entityTypeId, int entityId, RockContext rockContext )
        {
            var qry = rockContext.Set<AttributeValueReferencedEntity>()
                .AsNoTracking()
                .Where( re => re.EntityTypeId == entityTypeId && re.EntityId == entityId );

            if ( attributeIds != null && attributeIds.Any() )
            {
                qry = qry.Where( re => attributeIds.Contains( re.AttributeValue.AttributeId ) );
            }

            // Find all attribute values that reference this entity.
            var referencingValues = qry
                .Select( re => new
                {
                    re.AttributeValue.AttributeId,
                    AttributeValueId = re.AttributeValueId,
                    re.AttributeValue.Value
                } )
                .ToList()
                .GroupBy( re => re.AttributeId );

            // Loop through each reference value group, which is an attribute Id,
            // and process all the values.
            foreach ( var attributeGroup in referencingValues )
            {
                var attributeId = attributeGroup.Key;
                var valueGroups = attributeGroup.GroupBy( ag => ag.Value );
                var attributeCache = AttributeCache.Get( attributeId );
                var field = attributeCache.FieldType.Field;
                var cache = new Dictionary<string, object>();

                foreach ( var valueGroup in valueGroups )
                {
                    var value = valueGroup.Key;
                    var attributeValueIds = valueGroup.Select( vg => vg.AttributeValueId );
                    Field.PersistedValues persistedValues;

                    if ( field.IsPersistedValueSupported( attributeCache.ConfigurationValues ) )
                    {
                        persistedValues = GetPersistedValuesOrPlaceholder( field, value, attributeCache.ConfigurationValues, cache );
                    }
                    else
                    {
                        var placeholderValue = field.GetPersistedValuePlaceholder( attributeCache.ConfigurationValues );

                        persistedValues = new Field.PersistedValues
                        {
                            TextValue = placeholderValue,
                            CondensedTextValue = placeholderValue,
                            HtmlValue = placeholderValue,
                            CondensedHtmlValue = placeholderValue
                        };
                    }

                    BulkUpdateAttributeValuePersistedValues( attributeId, attributeValueIds, persistedValues, false, rockContext );
                }
            }
        }

        #endregion

        /// <summary>
        /// Copies the attributes from one entity to another
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public static void CopyAttributes( Rock.Attribute.IHasAttributes source, Rock.Attribute.IHasAttributes target )
        {
            CopyAttributes( source, target, null );
        }

        /// <summary>
        /// Copies the attributes from one entity to another
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void CopyAttributes( Rock.Attribute.IHasAttributes source, Rock.Attribute.IHasAttributes target, RockContext rockContext )
        {
            if ( source != null && target != null )
            {
                // Copy Attributes
                if ( source.Attributes != null )
                {
                    target.Attributes = new Dictionary<string, AttributeCache>();
                    foreach ( var item in source.Attributes )
                    {
                        target.Attributes.Add( item.Key, item.Value );
                    }
                }
                else
                {
                    target.Attributes = null;
                }

                // Copy Attribute Values
                if ( source.AttributeValues != null )
                {
                    target.AttributeValues = new Dictionary<string, AttributeValueCache>();
                    foreach ( var item in source.AttributeValues )
                    {
                        var attribute = source.Attributes[item.Key];
                        var fieldType = attribute.FieldType.Field as Field.FieldType;

                        var value = item.Value;
                        if ( fieldType != null && value != null )
                        {
                            var copyValue = fieldType.GetCopyValue( value.Value, rockContext );

                            var attributeValue = new AttributeValueCache( value.AttributeId,
                                null,
                                copyValue,
                                value.PersistedTextValue,
                                value.PersistedHtmlValue,
                                value.PersistedCondensedTextValue,
                                value.PersistedCondensedHtmlValue,
                                value.IsPersistedValueDirty );

                            target.AttributeValues.Add( item.Key, attributeValue );
                        }
                        else
                        {
                            target.AttributeValues.Add( item.Key, null );
                        }
                    }
                }
                else
                {
                    target.AttributeValues = null;
                }
            }
        }

        /// <summary>
        /// Adds edit controls for each of the item's attributes
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="supressOrdering">if set to <c>true</c> suppresses reordering of the attributes within each Category. (LoadAttributes() may perform custom ordering as is the case for group member attributes).</param>
        public static void AddEditControls( Rock.Attribute.IHasAttributes item, Control parentControl, bool setValue, string validationGroup = "", bool supressOrdering = false )
        {
            AddEditControls( item, parentControl, setValue, validationGroup, new List<string>(), supressOrdering );
        }

        /// <summary>
        /// Adds edit controls for each of the item's attributes
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="exclude">List of attributes not to render. Attributes with a Key or Name in the exclude list will not be shown.</param>
        /// <param name="supressOrdering">if set to <c>true</c> suppresses reordering of the attributes within each Category. (LoadAttributes() may perform custom ordering as is the case for group member attributes).</param>
        public static void AddEditControls( Rock.Attribute.IHasAttributes item, Control parentControl, bool setValue, string validationGroup, List<string> exclude, bool supressOrdering = false )
        {
            AddEditControls( item, parentControl, setValue, validationGroup, exclude, supressOrdering, null );
        }

        /// <summary>
        /// Adds the edit controls.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="numberOfColumns">The number of columns.</param>
        public static void AddEditControls( Rock.Attribute.IHasAttributes item, Control parentControl, bool setValue, string validationGroup, int? numberOfColumns )
        {
            AddEditControls( item, parentControl, setValue, validationGroup, new List<string>(), false, numberOfColumns );
        }

        /// <summary>
        /// Adds the edit controls.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="numberOfColumns">The number of columns.</param>
        /// <param name="exclude">List of attributes not to render. Attributes with a Key or Name in the exclude list will not be shown.</param>
        /// <param name="supressOrdering">if set to <c>true</c> suppresses reordering of the attributes within each Category. (LoadAttributes() may perform custom ordering as is the case for group member attributes).</param>
        public static void AddEditControls( Rock.Attribute.IHasAttributes item, Control parentControl, bool setValue, string validationGroup, List<string> exclude, bool supressOrdering, int? numberOfColumns = null )
        {
            if ( item != null && item.Attributes != null )
            {
                exclude = exclude ?? new List<string>();
                foreach ( var attributeCategory in GetAttributeCategories( item, false, false, supressOrdering ) )
                {
                    if ( attributeCategory.Attributes.Where( a => a.IsActive ).Where( a => !exclude.Contains( a.Name ) && !exclude.Contains( a.Key ) ).Select( a => a.Key ).Count() > 0 )
                    {
                        AddEditControls(
                            attributeCategory.Category != null ? attributeCategory.Category.Name : string.Empty,
                            attributeCategory.Attributes.Where( a => a.IsActive ).Select( a => a.Key ).ToList(),
                            item, parentControl, validationGroup, setValue, exclude, numberOfColumns );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the edit controls.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="attributeKeys">The attribute keys.</param>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="exclude">The exclude.</param>
        public static void AddEditControls( string category, List<string> attributeKeys, Rock.Attribute.IHasAttributes item, Control parentControl, string validationGroup, bool setValue, List<string> exclude )
        {
            AddEditControls( category, attributeKeys, item, parentControl, validationGroup, setValue, exclude, null );
        }

        /// <summary>
        /// Adds the edit controls.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="attributeKeys">The attribute keys.</param>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="exclude">List of attributes not to render. Attributes with a Key or Name in the exclude list will not be shown.</param>
        /// <param name="numberOfColumns">The number of columns.</param>
        public static void AddEditControls( string category, List<string> attributeKeys, Rock.Attribute.IHasAttributes item, Control parentControl, string validationGroup, bool setValue, List<string> exclude, int? numberOfColumns )
        {
            AttributeAddEditControlsOptions attributeAddEditControlsOptions = new AttributeAddEditControlsOptions
            {
                NumberOfColumns = numberOfColumns
            };

            attributeAddEditControlsOptions.IncludedAttributes = attributeKeys != null ? item?.Attributes.Select( a => a.Value ).Where( a => attributeKeys.Contains( a.Key ) ).ToList() : null;
            attributeAddEditControlsOptions.ExcludedAttributes = exclude != null ? item?.Attributes.Select( a => a.Value ).Where( a => exclude.Contains( a.Key ) || exclude.Contains( a.Name ) ).ToList() : null;

            AddEditControlsForCategory( category, item, parentControl, validationGroup, setValue, attributeAddEditControlsOptions );
        }

        /// <summary>
        /// Adds the edit controls for category.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="addEditControlsOptions">The add edit controls options.</param>
        public static void AddEditControlsForCategory( string categoryName, IHasAttributes item, Control parentControl, string validationGroup, bool setValue, AttributeAddEditControlsOptions addEditControlsOptions )
        {
            int? numberOfColumns = addEditControlsOptions?.NumberOfColumns;
            List<AttributeCache> excludedAttributes = addEditControlsOptions?.ExcludedAttributes ?? new List<AttributeCache>();
            List<AttributeCache> attributes = addEditControlsOptions?.IncludedAttributes ?? item.Attributes.Select( a => a.Value ).Where( a => a.Categories.Any( ( CategoryCache c ) => c.Name == categoryName ) ).ToList();
            bool showCategoryLabel = addEditControlsOptions?.ShowCategoryLabel ?? true;

            // ensure valid number of columns
            if ( numberOfColumns.HasValue )
            {
                if ( numberOfColumns.Value > 12 )
                {

                }
                else if ( numberOfColumns < 1 )
                {
                    numberOfColumns = 1;
                }
            }

            bool parentIsDynamic = parentControl is DynamicControlsPanel || parentControl is DynamicPlaceholder;
            HtmlGenericControl fieldSet = parentIsDynamic ? new DynamicControlsHtmlGenericControl( "fieldset" ) : new HtmlGenericControl( "fieldset" );

            parentControl.Controls.Add( fieldSet );
            fieldSet.Controls.Clear();
            if ( showCategoryLabel && !string.IsNullOrEmpty( categoryName ) )
            {
                HtmlGenericControl legend = new HtmlGenericControl( "h4" );

                if ( numberOfColumns.HasValue )
                {
                    HtmlGenericControl row = new HtmlGenericControl( "div" );
                    row.AddCssClass( "row" );
                    fieldSet.Controls.Add( row );

                    HtmlGenericControl col = new HtmlGenericControl( "div" );
                    col.AddCssClass( "col-md-12" );
                    row.Controls.Add( col );

                    col.Controls.Add( legend );
                }
                else
                {
                    fieldSet.Controls.Add( legend );
                }

                legend.Controls.Clear();
                legend.InnerText = categoryName.Trim();
            }

            HtmlGenericControl attributeRow = parentIsDynamic ? new DynamicControlsHtmlGenericControl( "div" ) : new HtmlGenericControl( "div" );
            if ( numberOfColumns.HasValue )
            {
                fieldSet.Controls.Add( attributeRow );
                attributeRow.AddCssClass( "row" );
            }

            foreach ( AttributeCache attribute in attributes )
            {
                if ( attribute.IsActive && !excludedAttributes.Contains( attribute ) )
                {
                    // Add the control for editing the attribute value
                    AttributeControlOptions attributeControlOptions = new AttributeControlOptions
                    {
                        SetValue = setValue,
                        SetId = true,
                        ValidationGroup = validationGroup,
                        Value = setValue ? item.AttributeValues?[attribute.Key]?.Value : null,
                        ShowPrePostHtml = ( addEditControlsOptions?.ShowPrePostHtml ?? true )
                    };

                    if ( addEditControlsOptions.RequiredAttributes != null )
                    {
                        attributeControlOptions.Required = addEditControlsOptions.RequiredAttributes.Any( a => a.Id == attribute.Id );
                    }

                    if ( numberOfColumns.HasValue )
                    {
                        int colSize = ( int ) Math.Ceiling( 12.0 / ( double ) numberOfColumns.Value );
                        HtmlGenericControl attributeCol = parentIsDynamic ? new DynamicControlsHtmlGenericControl( "div" ) : new HtmlGenericControl( "div" );

                        /*
                            6/8/2022 - PA

                            The attributeCol controls helps add the Attributes to the page.
                            But, not having the attributeCol.ID set causes the Page to throw View State Exception as DynamicControlsHtmlGenericControl instances are required to have an Id.
                            
                            Reason: https://github.com/SparkDevNetwork/Rock/issues/3867
                         */

                        attributeCol.ID = "attributeCol_" + attribute.Key;

                        attributeRow.Controls.Add( attributeCol );
                        attributeCol.AddCssClass( $"col-md-{colSize}" );
                        attribute.AddControl( attributeCol.Controls, attributeControlOptions );
                    }
                    else
                    {
                        attribute.AddControl( fieldSet.Controls, attributeControlOptions );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the display HTML.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="exclude">The exclude.</param>
        /// <param name="supressOrdering">if set to <c>true</c> suppresses reordering of the attributes within each Category. (LoadAttributes() may perform custom ordering as is the case for group member attributes).</param>
        /// <param name="showHeading">if set to <c>true</c> [show heading].</param>
        public static void AddDisplayControls( Rock.Attribute.IHasAttributes item, Control parentControl, List<string> exclude = null, bool supressOrdering = false, bool showHeading = true )
        {
            exclude = exclude ?? new List<string>();

            if ( item?.Attributes != null )
            {
                AddDisplayControls( item, GetAttributeCategories( item, false, supressOrdering ), parentControl, exclude, showHeading );
            }
        }

        /// <summary>
        /// Adds the display controls.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="attributeCategories">The attribute categories.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="exclude">The exclude.</param>
        /// <param name="showHeading">if set to <c>true</c> [show heading].</param>
        public static void AddDisplayControls( Rock.Attribute.IHasAttributes item, List<AttributeCategory> attributeCategories, Control parentControl, List<string> exclude = null, bool showHeading = true )
        {
            AttributeAddDisplayControlsOptions attributeAddDisplayControlsOptions = new AttributeAddDisplayControlsOptions
            {
                ShowCategoryLabel = showHeading,
            };

            attributeAddDisplayControlsOptions.ExcludedAttributes = exclude != null ? item?.Attributes.Select( a => a.Value ).Where( a => exclude.Contains( a.Key ) || exclude.Contains( a.Name ) ).ToList() : null;


            AddDisplayControls( item, attributeCategories, parentControl, attributeAddDisplayControlsOptions );
        }

        /// <summary>
        /// Adds the display controls.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="attributeCategories">The attribute categories.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="attributeAddDisplayControlsOptions">The attribute add display controls options.</param>
        public static void AddDisplayControls( Rock.Attribute.IHasAttributes item, List<AttributeCategory> attributeCategories, Control parentControl, AttributeAddDisplayControlsOptions attributeAddDisplayControlsOptions )
        {
            if ( item == null )
            {
                return;
            }

            attributeAddDisplayControlsOptions = attributeAddDisplayControlsOptions ?? new AttributeAddDisplayControlsOptions();
            List<AttributeCache> excludedAttributes = attributeAddDisplayControlsOptions?.ExcludedAttributes ?? new List<AttributeCache>();
            int? numberOfColumns = attributeAddDisplayControlsOptions?.NumberOfColumns;

            // ensure valid number of columns
            if ( numberOfColumns.HasValue )
            {
                if ( numberOfColumns.Value > 12 )
                {

                }
                else if ( numberOfColumns < 1 )
                {
                    numberOfColumns = 1;
                }
            }

            foreach ( var attributeCategory in attributeCategories )
            {
                if ( attributeAddDisplayControlsOptions.ShowCategoryLabel )
                {
                    HtmlGenericControl header = new HtmlGenericControl( "h4" );

                    string categoryName = attributeCategory.Category != null ? attributeCategory.Category.Name : string.Empty;

                    header.InnerText = string.IsNullOrWhiteSpace( categoryName ) ? item.GetType().GetFriendlyTypeName() + " Attributes" : categoryName.Trim();
                    parentControl.Controls.Add( header );
                }

                HtmlGenericControl dl = new HtmlGenericControl( "dl" );
                parentControl.Controls.Add( dl );
                dl.AddCssClass( "attribute-value-container-display" );
                if ( numberOfColumns.HasValue )
                {
                    dl.AddCssClass( "row" );
                }

                foreach ( var attribute in attributeCategory.Attributes.Where( a => AttributeCache.Get( a.Id ).IsActive && !excludedAttributes.Contains( a ) ) )
                {
                    // Get the Attribute Value formatted for display.
                    string value = attribute.DefaultValue;
                    if ( item.AttributeValues.ContainsKey( attribute.Key ) && item.AttributeValues[attribute.Key] != null )
                    {
                        value = item.AttributeValues[attribute.Key].Value;
                    }

                    string controlHtml = attribute.FieldType.Field.FormatValueAsHtml( parentControl, attribute.EntityTypeId, item.Id, value, attribute.QualifierValues );

                    // If the Attribute Value has some content, display it.
                    if ( !string.IsNullOrWhiteSpace( controlHtml ) )
                    {
                        HtmlGenericControl dtDdParent;
                        if ( numberOfColumns.HasValue )
                        {
                            int colSize = ( int ) Math.Ceiling( 12.0 / ( double ) numberOfColumns.Value );
                            dtDdParent = new HtmlGenericControl( "div" );
                            dtDdParent.AddCssClass( $"col-md-{colSize}" );
                            dl.Controls.Add( dtDdParent );
                        }
                        else
                        {
                            dtDdParent = dl;
                        }

                        dtDdParent.AddCssClass( "js-attribute-display-wrapper" );
                        dtDdParent.Attributes["data-attribute-id"] = attribute.Id.ToString();

                        HtmlGenericControl dt = new HtmlGenericControl( "dt" );
                        dt.InnerText = attribute.Name;
                        dtDdParent.Controls.Add( dt );

                        HtmlGenericControl dd = new HtmlGenericControl( "dd" );

                        dd.InnerHtml = controlHtml;
                        dtDdParent.Controls.Add( dd );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the attributes that ended up getting displayed as a result of AddDisplayControls
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <returns></returns>
        public static List<AttributeCache> GetDisplayedAttributes( Control parentControl )
        {
            List<AttributeCache> displayedAttributes = new List<AttributeCache>();
            var attributeWrappers = parentControl.ControlsOfTypeRecursive<HtmlGenericControl>().Where( a => a.Attributes["class"]?.Contains( "js-attribute-display-wrapper" ) == true ).ToList();
            foreach ( var attributeWrapper in attributeWrappers )
            {
                var attributeId = attributeWrapper.Attributes?["data-attribute-id"]?.AsIntegerOrNull();
                if ( attributeId.HasValue )
                {
                    displayedAttributes.Add( AttributeCache.Get( attributeId.Value ) );
                }
            }

            return displayedAttributes.Where( a => a != null ).ToList();
        }

        /// <summary>
        /// Gets the edit values from edit controls contained within the parentControl for the specified item
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="item">The item.</param>
        public static void GetEditValues( Control parentControl, Rock.Attribute.IHasAttributes item )
        {
            var attributeEditControls = GetAttributeEditControls( parentControl, item );
            if ( attributeEditControls != null )
            {
                foreach ( var attributeEditControl in attributeEditControls )
                {
                    var attribute = attributeEditControl.Key;
                    var control = attributeEditControl.Value;
                    if ( control != null )
                    {
                        var editValue = attribute.FieldType.Field.GetEditValue( control, attribute.QualifierValues );
                        item.AttributeValues[attribute.Key] = new AttributeValueCache { AttributeId = attribute.Id, EntityId = item.Id, Value = editValue };
                    }
                }
            }
        }

        /// <summary>
        /// Gets a dictionary of each edit control (where Key is AttributeCache) contained within the parentControl for the specified item
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static Dictionary<AttributeCache, Control> GetAttributeEditControls( Control parentControl, Rock.Attribute.IHasAttributes item )
        {
            Dictionary<AttributeCache, Control> result = new Dictionary<AttributeCache, Control>();
            if ( item?.Attributes != null )
            {
                foreach ( var attributeKeyValue in item.Attributes )
                {
                    Control control = parentControl?.FindControl( string.Format( "attribute_field_{0}", attributeKeyValue.Value.Id ) );
                    if ( control != null )
                    {
                        result.TryAdd( attributeKeyValue.Value, control );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the attribute value expression.
        /// </summary>
        /// <param name="attributeValues">The attribute values.</param>
        /// <param name="attributeValueParameter">The attribute value parameter.</param>
        /// <param name="parentIdProperty">The parent identifier property.</param>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <returns></returns>
        public static Expression GetAttributeValueExpression( IQueryable<AttributeValue> attributeValues, ParameterExpression attributeValueParameter, Expression parentIdProperty, int attributeId )
        {
            MemberExpression attributeIdProperty = Expression.Property( attributeValueParameter, "AttributeId" );
            MemberExpression entityIdProperty = Expression.Property( attributeValueParameter, "EntityId" );
            Expression attributeIdConstant = Expression.Constant( attributeId );

            Expression attributeIdCompare = Expression.Equal( attributeIdProperty, attributeIdConstant );
            Expression entityIdCompre = Expression.Equal( entityIdProperty, Expression.Convert( parentIdProperty, typeof( int? ) ) );
            Expression andExpression = Expression.And( attributeIdCompare, entityIdCompre );

            var match = new Expression[] {
                Expression.Constant(attributeValues),
                Expression.Lambda<Func<AttributeValue, bool>>( andExpression, new ParameterExpression[] { attributeValueParameter })
            };

            Expression whereExpression = Expression.Call( typeof( Queryable ), "Where", new Type[] { typeof( AttributeValue ) }, match );

            var attributeCache = AttributeCache.Get( attributeId );
            var attributeValueFieldName = "Value";
            Type attributeValueFieldType = typeof( string );
            if ( attributeCache != null )
            {
                attributeValueFieldName = attributeCache.FieldType.Field.AttributeValueFieldName;
                attributeValueFieldType = attributeCache.FieldType.Field.AttributeValueFieldType;
            }

            MemberExpression valueProperty = Expression.Property( attributeValueParameter, attributeValueFieldName );

            Expression valueLambda = Expression.Lambda( valueProperty, new ParameterExpression[] { attributeValueParameter } );

            Expression selectValue = Expression.Call( typeof( Queryable ), "Select", new Type[] { typeof( AttributeValue ), attributeValueFieldType }, whereExpression, valueLambda );

            Expression firstOrDefault = Expression.Call( typeof( Queryable ), "FirstOrDefault", new Type[] { attributeValueFieldType }, selectValue );

            return firstOrDefault;
        }

        #region Support Classes

        /// <summary>
        /// Used by the attribute loading code to identify all the possible
        /// entity types and identifier values to use when loading values
        /// from the database.
        /// </summary>
        private class LoadAttributesKey
        {
            /// <summary>
            /// Gets or sets the entity type identifier to match to the Attribute.
            /// </summary>
            /// <value>The entity type identifier to match to the Attribute.</value>
            public int EntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the entity identifier to match to the AttributeValue.
            /// </summary>
            /// <value>The entity identifier to match to the AttributeValue.</value>
            public int EntityId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the original entity whose values are being loaded.
            /// </summary>
            /// <value>The identifier of the original entity whose values are being loaded.</value>
            public int RealEntityId { get; set; }
        }

        /// <summary>
        /// Used by the attribute loading code to identify all the inherited
        /// attributes and the alternate identifiers used to find the values.
        /// One instance is associated with each entity whose attributes are
        /// being loaded.
        /// </summary>
        private class InheritedAttributeLookup
        {
            /// <summary>
            /// Gets or sets the attributes that are valid for the entity.
            /// </summary>
            /// <value>The attributes that are valid for the entity.</value>
            public List<AttributeCache> Attributes { get; set; }

            /// <summary>
            /// Gets or sets the alternate identifiers that will be used to
            /// load values. This is a dictionary of entity type identifiers and
            /// the entity identifiers for that type.
            /// </summary>
            /// <value>The alternate identifiers that will be used to load values.</value>
            public Dictionary<int, List<int>> AlternateIds { get; set; }
        }

        /// <summary>
        /// Used by the attribute loading code to identify a single value that
        /// has been loaded from the database by custom query.
        /// </summary>
        private class AttributeItemValue
        {
            /// <summary>
            /// Gets or sets the entity type identifier of the matched Attribute.
            /// </summary>
            /// <value>The entity type identifier of the matched Attribute.</value>
            public int EntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the entity identifier of the matched AttributeValue.
            /// </summary>
            /// <value>The entity identifier of the matched AttributeValue.</value>
            public int EntityId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the entity this value will be provided to.
            /// </summary>
            /// <value>The identifier of the entity this value will be provided to.</value>
            public int RealEntityId { get; set; }

            /// <summary>
            /// Gets or sets the attribute identifier.
            /// </summary>
            /// <value>The attribute identifier.</value>
            public int AttributeId { get; set; }

            /// <summary>
            /// Gets or sets the value from AttributeValue.
            /// </summary>
            /// <value>The value from AttributeValue.</value>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the persisted text value.
            /// </summary>
            /// <value>The persisted text value.</value>
            public string PersistedTextValue { get; set; }

            /// <summary>
            /// Gets or sets the persisted HTML value.
            /// </summary>
            /// <value>The persisted HTML value.</value>
            public string PersistedHtmlValue { get; set; }

            /// <summary>
            /// Gets or sets the persisted condensed text value.
            /// </summary>
            /// <value>The persisted condensed text value.</value>
            public string PersistedCondensedTextValue { get; set; }

            /// <summary>
            /// Gets or sets the persisted condensed HTML value.
            /// </summary>
            /// <value>The persisted condensed HTML value.</value>
            public string PersistedCondensedHtmlValue { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this persisted value is dirty.
            /// </summary>
            /// <value><c>true</c> if this this persisted value is dirty; otherwise, <c>false</c>.</value>
            public bool IsPersistedValueDirty { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// Attributes group by category
    /// </summary>
    public class AttributeCategory
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public Rock.Web.Cache.CategoryCache Category { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public List<Rock.Web.Cache.AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        /// <value>
        /// The name of the category.
        /// </value>
        public string CategoryName
        {
            get { return Category != null ? Category.Name : string.Empty; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Attribute.AttributeAddControlsOptions" />
    public class AttributeAddEditControlsOptions : AttributeAddControlsOptions
    {
        /// <summary>
        /// Gets or sets the included attributes.
        /// </summary>
        /// <value>
        /// The included attributes.
        /// </value>
        public List<AttributeCache> IncludedAttributes { get; set; }

        /// <summary>
        /// Overrides which Attributes are required. Leave null to use normal IsRequired for the attribute
        /// </summary>
        /// <value>
        /// The required attributes.
        /// </value>
        public List<AttributeCache> RequiredAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show pre post HTML] (if EntityType supports it)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show pre post HTML]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPrePostHtml { get; set; } = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public class AttributeAddDisplayControlsOptions : AttributeAddControlsOptions
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class AttributeAddControlsOptions
    {
        /// <summary>
        /// Gets or sets the excluded attributes.
        /// </summary>
        /// <value>
        /// The excluded attributes.
        /// </value>
        public List<AttributeCache> ExcludedAttributes { get; set; }

        /// <summary>
        /// Gets or sets the number of columns.
        /// </summary>
        /// <value>
        /// The number of columns.
        /// </value>
        public int? NumberOfColumns { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show category label].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show category label]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCategoryLabel { get; set; } = true;
    }
}
