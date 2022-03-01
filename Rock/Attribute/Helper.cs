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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.ViewModel.NonEntities;
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

            bool customGridColumnsBlock = typeof( Rock.Web.UI.ICustomGridColumns ).IsAssignableFrom( type );
            if ( customGridColumnsBlock )
            {
                entityProperties.Add( new TextFieldAttribute( CustomGridColumnsConfig.AttributeKey, category: "CustomSetting" ) );
            }

            bool customGridOptionsBlock = typeof( Rock.Web.UI.ICustomGridOptions ).IsAssignableFrom( type );
            if ( customGridOptionsBlock )
            {
                entityProperties.Add( new BooleanFieldAttribute( CustomGridOptionsConfig.EnableStickyHeadersAttributeKey, category: "CustomSetting" ) );
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
            List<int> altEntityIds = null;

            //
            // If this entity can provide inherited attribute information then
            // load that data now. If they don't provide any then generate empty lists.
            //
            if ( entity is Rock.Attribute.IHasInheritedAttributes )
            {
                rockContext = rockContext ?? new RockContext();
                allAttributes = ( ( Rock.Attribute.IHasInheritedAttributes ) entity ).GetInheritedAttributes( rockContext );
                altEntityIds = ( ( Rock.Attribute.IHasInheritedAttributes ) entity ).GetAlternateEntityIds( rockContext );
            }

            allAttributes = allAttributes ?? new List<AttributeCache>();
            altEntityIds = altEntityIds ?? new List<int>();

            //
            // Get all the attributes that apply to this entity type and this entity's
            // properties match any attribute qualifiers.
            //
            if ( entityTypeCache != null )
            {
                int entityTypeId = entityTypeCache.Id;
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
                            propertyValues.AddOrIgnore( propertyName, propertyInfo.GetValue( entity, null ) );
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
                    attributeValues.AddOrIgnore( attribute.Key, null );
                }

                // If loading attributes for a saved item, read the item's value(s) for each attribute 
                if ( !entityTypeCache.IsEntity || entity.Id != 0 )
                {
                    rockContext = rockContext ?? new RockContext();
                    var attributeValueService = new Rock.Model.AttributeValueService( rockContext );

                    List<int> attributeIds = allAttributes.Select( a => a.Id ).ToList();
                    IQueryable<AttributeValue> attributeValueQuery;

                    if ( altEntityIds.Any() )
                    {
                        attributeValueQuery = attributeValueService.Queryable().AsNoTracking()
                            .Where( v =>
                                v.EntityId.HasValue &&
                                ( v.EntityId.Value == entity.Id || altEntityIds.Contains( v.EntityId.Value ) ) );
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
                            a.Value
                        } );

                    foreach ( var attributeValueSelect in attributeValueSelectQuery )
                    {
                        var attributeKey = AttributeCache.Get( attributeValueSelect.AttributeId ).Key;
                        var attributeValueCache = new AttributeValueCache( attributeValueSelect.AttributeId, attributeValueSelect.EntityId, attributeValueSelect.Value );
                        attributeValues[attributeKey] = attributeValueCache;
                    }
                }

                // Look for any attributes that don't have a value and create a default value entry
                foreach ( var attribute in allAttributes )
                {
                    if ( attributeValues[attribute.Key] == null )
                    {
                        var attributeValue = new AttributeValueCache();
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.EntityId = entity?.Id;

                        var attributeValueDefaults = entity.AttributeValueDefaults;
                        if ( attributeValueDefaults != null && attributeValueDefaults.ContainsKey( attribute.Key ) )
                        {
                            attributeValue.Value = attributeValueDefaults[attribute.Key];
                        }
                        else
                        {
                            attributeValue.Value = attribute.DefaultValue;
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
            allAttributes.ForEach( a => entity.Attributes.AddOrIgnore( a.Key, a ) );

            entity.AttributeValues = attributeValues;
        }

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
        /// Gets the publically editable attribute model. This contains all the
        /// information required for the individual to make changes to the attribute.
        /// </summary>
        /// <param name="attribute">The attribute that will be represented.</param>
        /// <returns>A <see cref="PublicEditableAttributeViewModel"/> that represents the attribute.</returns>
        internal static PublicEditableAttributeViewModel GetPublicEditableAttributeViewModel( Rock.Model.Attribute attribute )
        {
            var fieldTypeCache = FieldTypeCache.Get( attribute.FieldTypeId );
            var configurationValues = attribute.AttributeQualifiers.ToDictionary( q => q.Key, q => q.Value );

            return new PublicEditableAttributeViewModel
            {
                Guid = attribute.Guid,
                Name = attribute.Name,
                Key = attribute.Key,
                AbbreviatedName = attribute.AbbreviatedName,
                Description = attribute.Description,
                IsActive = attribute.IsActive,
                IsAnalytic = attribute.IsAnalytic,
                IsAnalyticHistory = attribute.IsAnalyticHistory,
                PreHtml = attribute.PreHtml,
                PostHtml = attribute.PostHtml,
                IsAllowSearch = attribute.AllowSearch,
                IsEnableHistory = attribute.EnableHistory,
                IsIndexEnabled = attribute.IsIndexEnabled,
                IsPublic = attribute.IsPublic,
                IsRequired = attribute.IsRequired,
                IsSystem = attribute.IsSystem,
                IsShowInGrid = attribute.IsGridColumn,
                IsShowOnBulk = attribute.ShowOnBulk,
                FieldTypeGuid = fieldTypeCache.Guid,
                Categories = attribute.Categories
                    .Select( c => new ListItemViewModel
                    {
                        Value = c.Guid.ToString(),
                        Text = c.Name
                    } )
                    .ToList(),
                ConfigurationOptions = fieldTypeCache.Field?.GetPublicConfigurationOptions( configurationValues ) ?? new Dictionary<string, string>(),
                DefaultValue = fieldTypeCache.Field?.GetPublicEditValue( attribute.DefaultValue, configurationValues ) ?? string.Empty
            };
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
        internal static Rock.Model.Attribute SaveAttributeEdits( PublicEditableAttributeViewModel attribute, int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue, RockContext rockContext = null )
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

            var fieldTypeCache = FieldTypeCache.Get( attribute.FieldTypeGuid ?? Guid.Empty );

            // For now, if they try to make changes to an attribute that is using
            // an unknown field type we just can't do it. Even if they only change
            // the name, just don't allow it.
            if ( fieldTypeCache?.Field == null )
            {
                throw new Exception( "Unable to save attribute referencing unknown field type." );
            }

            var configurationValues = fieldTypeCache.Field.GetPrivateConfigurationOptions( attribute.ConfigurationOptions );

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
            }
            else
            {
                // If it did exist, set the new attribute ID and GUID since we're copying all properties in the next step
                newAttribute.Id = attribute.Id;
                newAttribute.Guid = attribute.Guid;
            }

            // Copy all the properties from the new attribute to the attribute model
            attribute.CopyPropertiesFrom( newAttribute );

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

            rockContext.SaveChanges();

            return attribute;
        }

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
                bool changesMade = false;

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
                                changesMade = true;
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
                                changesMade = true;
                            }
                        }
                    }
                }

                if ( changesMade )
                {
                    rockContext.SaveChanges();
                }
            }
        }

        /*
            02/18/2022 - KA

            SaveAttributeValue() checks if the Attribute Field Type is an EncryptedTextFieldType
            or an SSNFieldType and encrypts the value before saving it to the database, there is
            no need to encrypt the value before calling SaveAttributeValue().
        */

        /// <summary>
        /// Saves an attribute value. If the attribute field type is an <see cref="Field.Types.EncryptedTextFieldType"/> or an <see cref="Field.Types.SSNFieldType"/> the value will be encrypted.
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
                    if ( attribute.FieldType.Field is Field.Types.EncryptedTextFieldType || attribute.FieldType.Field is Field.Types.SSNFieldType )
                    {
                        newValue = Security.Encryption.EncryptString( newValue );
                    }

                    attributeValue.Value = newValue;
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

                attributeValue.Value = newValue;

                rockContext.SaveChanges();
            }
        }

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
                            var attributeValue = new AttributeValueCache();
                            attributeValue.AttributeId = value.AttributeId;
                            attributeValue.Value = fieldType.GetCopyValue( value.Value, rockContext );
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
                    Control control = parentControl.FindControl( string.Format( "attribute_field_{0}", attributeKeyValue.Value.Id ) );
                    if ( control != null )
                    {
                        result.AddOrIgnore( attributeKeyValue.Value, control );
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
