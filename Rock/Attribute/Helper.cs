// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Attribute
{
    /// <summary>
    /// Static Helper class for creating, saving, and reading attributes and attribute values of any <see cref="IHasAttributes"/> class
    /// </summary>
    public static class Helper
    {

        /// <summary>
        /// Updates the attributes.
        /// </summary>
        /// <param name="type">The type (should be a <see cref="IHasAttributes" /> object.</param>
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

            List<string> existingKeys = new List<string>();

            var blockProperties = new List<FieldAttribute>();

            // If a ContextAwareAttribute exists without an EntityType defined, add a property attribute to specify the type
            int properties = 0;
            foreach ( var customAttribute in type.GetCustomAttributes( typeof( ContextAwareAttribute ), true ) )
            {
                var contextAttribute = (ContextAwareAttribute)customAttribute;
                if ( contextAttribute != null && contextAttribute.EntityType == null )
                {
                    if ( contextAttribute.IsConfigurable )
                    {
                        string propertyKeyName = string.Format( "ContextEntityType{0}", properties > 0 ? properties.ToString() : "" );
                        properties++;

                        blockProperties.Add( new EntityTypeFieldAttribute( "Entity Type", false, "The type of entity that will provide context for this block", false, "Context", 0, propertyKeyName ) );
                    }
                }
            }

            // Add any property attributes that were defined for the block
            foreach ( var customAttribute in type.GetCustomAttributes( typeof( FieldAttribute ), true ) )
            {
                blockProperties.Add( (FieldAttribute)customAttribute );
            }

            rockContext = rockContext ?? new RockContext();

            // Create any attributes that need to be created
            foreach ( var blockProperty in blockProperties )
            {
                try
                {
                    attributesUpdated = UpdateAttribute( blockProperty, entityTypeId, entityQualifierColumn, entityQualifierValue, rockContext ) || attributesUpdated;
                    existingKeys.Add( blockProperty.Key );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( string.Format( "Could not update a block attribute ( Entity Type Id: {0}; Property Name: {1} ). ", entityTypeId, blockProperty.Name ), ex ), null );
                }
            }

            // Remove any old attributes
            try
            {
                var attributeService = new Model.AttributeService( rockContext );
                foreach ( var a in attributeService.Get( entityTypeId, entityQualifierColumn, entityQualifierValue ).ToList() )
                {
                    if ( !existingKeys.Contains( a.Key ) )
                    {
                        attributeService.Delete( a );
                        attributesDeleted = true;
                    }
                }
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Could not delete one or more old attributes.", ex ), null );
            }

            if ( attributesUpdated || attributesDeleted )
            {
                AttributeCache.FlushEntityAttributes();
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
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <remarks>
        /// If a rockContext value is included, this method will save any previous changes made to the context
        /// </remarks>
        private static bool UpdateAttribute( FieldAttribute property, int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, RockContext rockContext = null )
        {
            bool updated = false;

            rockContext = rockContext ?? new RockContext();

            var attributeService = new AttributeService( rockContext );
            var attributeQualifierService = new AttributeQualifierService( rockContext );
            var fieldTypeService = new FieldTypeService(rockContext);
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
                    attribute.Order != property.Order ||
                    attribute.FieldType.Assembly != property.FieldTypeAssembly ||
                    attribute.FieldType.Class != property.FieldTypeClass ||
                    attribute.IsRequired != property.IsRequired )
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

            if ( updated )
            {
                // Update the attribute
                attribute.Name = property.Name;
                attribute.Description = property.Description;
                attribute.DefaultValue = property.DefaultValue;
                attribute.Order = property.Order;
                attribute.IsRequired = property.IsRequired;

                attribute.Categories.Clear();
                if ( propertyCategories.Any() )
                {
                    foreach ( string propertyCategory in propertyCategories )
                    {
                        int attributeEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
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
                    attributeService.Add( attribute );
                }
                else
                {
                    AttributeCache.Flush( attribute.Id );
                }

                rockContext.SaveChanges();

                return true;
            }
            else
            {
                return false;
            }
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
        /// Loads the <see cref="P:IHasAttributes.Attributes" /> and <see cref="P:IHasAttributes.AttributeValues" /> of any <see cref="IHasAttributes" /> object
        /// </summary>
        /// <param name="entity">The item.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void LoadAttributes( Rock.Attribute.IHasAttributes entity, RockContext rockContext )
        {
            if ( entity != null )
            {
                Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

                Type entityType = entity.GetType();
                if ( entityType.IsDynamicProxyType() )
                {
                    entityType = entityType.BaseType;
                }

                rockContext = rockContext ?? new RockContext();

                // Check for group type attributes
                var groupTypeIds = new List<int>();
                if ( entity is GroupMember || entity is Group || entity is GroupType )
                {
                    // Can't use GroupTypeCache here since it loads attributes and would result in a recursive stack overflow situation
                    var groupTypeService = new GroupTypeService( rockContext );
                    GroupType groupType = null;

                    if ( entity is GroupMember )
                    {
                        var group = ( (GroupMember)entity ).Group ?? new GroupService( rockContext )
                            .Queryable().AsNoTracking().FirstOrDefault(g => g.Id == ( (GroupMember)entity ).GroupId );
                        if ( group != null )
                        {
                            groupType = group.GroupType ?? groupTypeService
                                .Queryable().AsNoTracking().FirstOrDefault( t => t.Id == group.GroupTypeId );
                        }
                    }
                    else if ( entity is Group )
                    {
                        groupType = ( (Group)entity ).GroupType ?? groupTypeService
                            .Queryable().AsNoTracking().FirstOrDefault( t => t.Id == ( (Group)entity ).GroupTypeId );
                    }
                    else
                    {
                        groupType = ( (GroupType)entity );
                    }

                    while ( groupType != null )
                    {
                        groupTypeIds.Insert( 0, groupType.Id );

                        // Check for inherited group type id's
                        if ( groupType.InheritedGroupTypeId.HasValue )
                        {
                            groupType = groupType.InheritedGroupType ?? groupTypeService
                                .Queryable().AsNoTracking().FirstOrDefault( t => t.Id == ( groupType.InheritedGroupTypeId ?? 0 ) );
                        }
                        else
                        {
                            groupType = null;
                        }
                    }

                }

                // Check for registration template type attributes
                int? registrationTemplateId = null;
                if ( entity is RegistrationRegistrant )
                {
                    RegistrationInstance registrationInstance = null;
                    var registration = ( (RegistrationRegistrant)entity ).Registration ?? new RegistrationService( rockContext )
                        .Queryable().AsNoTracking().FirstOrDefault( r => r.Id == ( (RegistrationRegistrant)entity ).RegistrationId );
                    if ( registration != null )
                    {
                        registrationInstance = registration.RegistrationInstance ?? new RegistrationInstanceService( rockContext )
                            .Queryable().AsNoTracking().FirstOrDefault( r => r.Id == registration.RegistrationInstanceId );
                        if ( registrationInstance != null )
                        {
                            registrationTemplateId = registrationInstance.RegistrationTemplateId;
                        }
                    }
                }

                foreach ( PropertyInfo propertyInfo in entityType.GetProperties() )
                    properties.Add( propertyInfo.Name.ToLower(), propertyInfo );

                Rock.Model.AttributeService attributeService = new Rock.Model.AttributeService( rockContext );
                Rock.Model.AttributeValueService attributeValueService = new Rock.Model.AttributeValueService( rockContext );

                var inheritedAttributes = new Dictionary<int, List<Rock.Web.Cache.AttributeCache>>();
                if ( groupTypeIds.Any() )
                {
                    groupTypeIds.ForEach( g => inheritedAttributes.Add( g, new List<Rock.Web.Cache.AttributeCache>() ) );
                }
                else
                {
                    inheritedAttributes.Add( 0, new List<Rock.Web.Cache.AttributeCache>() );
                }

                var attributes = new List<Rock.Web.Cache.AttributeCache>();

                // Get all the attributes that apply to this entity type and this entity's properties match any attribute qualifiers
                var entityTypeCache = Rock.Web.Cache.EntityTypeCache.Read( entityType);
                if ( entityTypeCache != null )
                {
                    int entityTypeId = entityTypeCache.Id;
                    foreach ( var entityAttributes in AttributeCache.GetByEntity( entityTypeCache.Id ) )
                    {
                        // group type ids exist (entity is either GroupMember, Group, or GroupType) and qualifier is for a group type id
                        if ( groupTypeIds.Any() && (
                                ( entity is GroupMember && string.Compare( entityAttributes.EntityTypeQualifierColumn, "GroupTypeId", true ) == 0 ) ||
                                ( entity is Group && string.Compare( entityAttributes.EntityTypeQualifierColumn, "GroupTypeId", true ) == 0 ) ||
                                ( entity is GroupType && string.Compare( entityAttributes.EntityTypeQualifierColumn, "Id", true ) == 0 ) ) )
                        {
                            int groupTypeIdValue = int.MinValue;
                            if ( int.TryParse( entityAttributes.EntityTypeQualifierValue, out groupTypeIdValue ) && groupTypeIds.Contains( groupTypeIdValue ) )
                            {
                                foreach( int attributeId in entityAttributes.AttributeIds )
                                {
                                    inheritedAttributes[groupTypeIdValue].Add( Rock.Web.Cache.AttributeCache.Read( attributeId ) );
                                }
                            }
                        }

                        // Registrant attribute ( by RegistrationTemplateId )
                        else if ( entity is RegistrationRegistrant && 
                            registrationTemplateId.HasValue &&
                            entityAttributes.EntityTypeQualifierValue.AsInteger() == registrationTemplateId.Value )
                        {
                            foreach ( int attributeId in entityAttributes.AttributeIds )
                            {
                                attributes.Add( Rock.Web.Cache.AttributeCache.Read( attributeId ) );
                            }
                        }

                        else if ( string.IsNullOrEmpty( entityAttributes.EntityTypeQualifierColumn ) ||
                            ( properties.ContainsKey( entityAttributes.EntityTypeQualifierColumn.ToLower() ) &&
                            ( string.IsNullOrEmpty( entityAttributes.EntityTypeQualifierValue ) ||
                            ( properties[entityAttributes.EntityTypeQualifierColumn.ToLower()].GetValue( entity, null ) ?? "" ).ToString() == entityAttributes.EntityTypeQualifierValue ) ) )
                        {
                            foreach( int attributeId in entityAttributes.AttributeIds )
                            {
                                attributes.Add( Rock.Web.Cache.AttributeCache.Read( attributeId ) );
                            }
                        }
                    }
                }

                var allAttributes = new List<Rock.Web.Cache.AttributeCache>();

                foreach ( var attributeGroup in inheritedAttributes )
                {
                    foreach ( var attribute in attributeGroup.Value )
                    {
                        allAttributes.Add( attribute );
                    }
                }
                foreach ( var attribute in attributes )
                {
                    allAttributes.Add( attribute );
                }

                var attributeValues = new Dictionary<string, Rock.Model.AttributeValue>();

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
                        List<int> attributeIds = allAttributes.Select( a => a.Id ).ToList();
                        foreach ( var attributeValue in attributeValueService.Queryable().AsNoTracking()
                            .Where( v => v.EntityId == entity.Id && attributeIds.Contains( v.AttributeId ) ) )
                        {
                            var attributeKey = AttributeCache.Read( attributeValue.AttributeId ).Key;
                            attributeValues[attributeKey] = attributeValue.Clone( false ) as Rock.Model.AttributeValue;
                        }
                    }

                    // Look for any attributes that don't have a value and create a default value entry
                    foreach ( var attribute in allAttributes )
                    {
                        if ( attributeValues[attribute.Key] == null )
                        {
                            var attributeValue = new Rock.Model.AttributeValue();
                            attributeValue.AttributeId = attribute.Id;
                            if ( entity.AttributeValueDefaults != null && entity.AttributeValueDefaults.ContainsKey( attribute.Name ) )
                            {
                                attributeValue.Value = entity.AttributeValueDefaults[attribute.Name];
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

                entity.Attributes = new Dictionary<string, Web.Cache.AttributeCache>();
                allAttributes.ForEach( a => entity.Attributes.AddOrIgnore( a.Key, a ) );

                entity.AttributeValues = attributeValues;
            }
        }

        /// <summary>
        /// Gets the attribute categories.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="onlyIncludeGridColumns">if set to <c>true</c> will only include those attributes with the option to display in grid set to true</param>
        /// <param name="allowMultiple">if set to <c>true</c> returns the attribute in each of its categories, if false, only returns attribut in first category.</param>
        /// <param name="supressOrdering">if set to <c>true</c> supresses reording (LoadAttributes() may perform custom ordering as is the case for group member attributes).</param>
        /// <returns></returns>
        public static List<AttributeCategory> GetAttributeCategories( IHasAttributes entity, bool onlyIncludeGridColumns = false, bool allowMultiple = false, bool supressOrdering = false)
        {
            if ( entity != null )
            {
                var attributes = entity.Attributes.Select( a => a.Value );
                if ( !supressOrdering )
                {
                    attributes = attributes.OrderBy( t => t.Order ).ThenBy( t => t.Name );
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
        /// <param name="allowMultiple">if set to <c>true</c> returns the attribute in each of its categories, if false, only returns attribut in first category.</param>
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

        private static void AddAttributeCategory( List<AttributeCategory> attributeCategories, Rock.Web.Cache.CategoryCache category, Rock.Web.Cache.AttributeCache attribute )
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
                attributeCategory.Attributes = new List<Web.Cache.AttributeCache>();
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
            var newAttribute = new Rock.Model.Attribute();
            edtAttribute.GetAttributeProperties( newAttribute );

            rockContext = rockContext ?? new RockContext();
            var internalAttributeService = new AttributeService( rockContext );
            Rock.Model.Attribute attribute = null;

            if ( newAttribute.Id > 0 )
            {
                attribute = internalAttributeService.Get( newAttribute.Id );
            }

            if ( attribute == null )
            {
                newAttribute.Order = internalAttributeService.Queryable().Max( a => a.Order ) + 1;
            }
            else
            {
                newAttribute.Order = attribute.Order;
            } 
            
            return SaveAttributeEdits( newAttribute, entityTypeId, entityTypeQualifierColumn, entityTypeQualifierValue, rockContext );
        }

        /// <summary>
        /// Saves any attribute edits made to an attribute. Note: any attributes will be flushed from the cache. Be sure to either reload the cache or the individual attribute when you are done.
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
            if (!newAttribute.IsValid)
            {
                return null;
            }

            // Create a attribute model that will be saved
            Rock.Model.Attribute attribute = null;

            // Check to see if this was an existing or new attribute
            if (newAttribute.Id > 0)
            {
                // If editing an existing attribute, remove all the old qualifiers in case they were changed
                foreach ( var oldQualifier in attributeQualifierService.GetByAttributeId( newAttribute.Id ).ToList() )
                {
                    attributeQualifierService.Delete( oldQualifier );
                }
                rockContext.SaveChanges();

                // Then re-load the existing attribute 
                attribute = internalAttributeService.Get( newAttribute.Id );
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

            // Add any qualifiers
            foreach ( var qualifier in newAttribute.AttributeQualifiers )
            {
                attribute.AttributeQualifiers.Add( new AttributeQualifier { Key = qualifier.Key, Value = qualifier.Value, IsSystem = qualifier.IsSystem } );
            }

            // Add any categories
            attribute.Categories.Clear();
            foreach ( var category in newAttribute.Categories )
            {
                attribute.Categories.Add( categoryService.Get( category.Id ) );
            }

            attribute.EntityTypeId = entityTypeId;
            attribute.EntityTypeQualifierColumn = entityTypeQualifierColumn;
            attribute.EntityTypeQualifierValue = entityTypeQualifierValue;

            rockContext.SaveChanges();

            if ( attribute != null )
            {
                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );

                // If this is a global attribute, flush all global attributes
                if ( !entityTypeId.HasValue && entityTypeQualifierColumn == string.Empty && entityTypeQualifierValue == string.Empty )
                {
                    Rock.Web.Cache.GlobalAttributesCache.Flush();
                }
            }

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
        public static void SaveAttributeValues( IHasAttributes model, RockContext rockContext = null )
        {
            if ( model != null && model.Attributes != null && model.AttributeValues != null )
            {
                foreach ( var attribute in model.Attributes )
                {
                    if ( model.AttributeValues.ContainsKey( attribute.Key ) )
                    {
                        SaveAttributeValue( model, attribute.Value, model.AttributeValues[attribute.Key].Value, rockContext );
                    }
                }
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
        public static void SaveAttributeValue( IHasAttributes model, Rock.Web.Cache.AttributeCache attribute, string newValue, RockContext rockContext = null )
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

                attributeValue.Value = newValue;

                rockContext.SaveChanges();

                if ( model.AttributeValues != null && model.AttributeValues.ContainsKey( attribute.Key ) )
                {
                    model.AttributeValues[attribute.Key] = attributeValue.Clone( false ) as Rock.Model.AttributeValue;
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
        public static void CopyAttributes( IHasAttributes source, IHasAttributes target )
        {
            if ( source != null && target != null )
            {
                // Copy Attributes
                if ( source.Attributes != null )
                {
                    target.Attributes = new Dictionary<string, Web.Cache.AttributeCache>();
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
                    target.AttributeValues = new Dictionary<string, Model.AttributeValue>();
                    foreach ( var item in source.AttributeValues )
                    {
                        var value = item.Value;
                        if ( value != null )
                        {
                            var attributeValue = new Model.AttributeValue();
                            attributeValue.IsSystem = value.IsSystem;
                            attributeValue.AttributeId = value.AttributeId;
                            attributeValue.EntityId = value.EntityId;
                            attributeValue.Value = value.Value;
                            attributeValue.Id = value.Id;
                            attributeValue.Guid = value.Guid;
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
        /// Adds the edit controls.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        [Obsolete("Use the other AddEditControls (which requires ValidationGroup)")]
        public static void AddEditControls( IHasAttributes item, Control parentControl, bool setValue)
        {
            AddEditControls( item, parentControl, setValue, "", new List<string>(), false );
        }
        
        /// <summary>
        /// Adds edit controls for each of the item's attributes
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="supressOrdering">if set to <c>true</c> supresses reording (LoadAttributes() may perform custom ordering as is the case for group member attributes).</param>
        public static void AddEditControls( IHasAttributes item, Control parentControl, bool setValue, string validationGroup = "", bool supressOrdering = false )
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
        /// <param name="exclude">List of attribute names not to render</param>
        /// <param name="supressOrdering">if set to <c>true</c> supresses reording (LoadAttributes() may perform custom ordering as is the case for group member attributes).</param>
        public static void AddEditControls( IHasAttributes item, Control parentControl, bool setValue, string validationGroup, List<string> exclude, bool supressOrdering = false )
        {
            if ( item.Attributes != null )
            {
                foreach ( var attributeCategory in GetAttributeCategories( item, false, false, supressOrdering ) )
                {
                    AddEditControls(
                        attributeCategory.Category != null ? attributeCategory.Category.Name : string.Empty,
                        attributeCategory.Attributes.Select( a => a.Key ).ToList(),
                        item, parentControl, validationGroup, setValue, exclude );
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
        public static void AddEditControls( string category, List<string> attributeKeys, IHasAttributes item, Control parentControl, string validationGroup, bool setValue, List<string> exclude )
        {
            HtmlGenericControl fieldSet;
            if ( parentControl is DynamicControlsPanel )
            {
                fieldSet = new DynamicControlsHtmlGenericControl( "fieldset" );
            }
            else
            {
                fieldSet = new HtmlGenericControl( "fieldset" );
            }

            parentControl.Controls.Add( fieldSet );
            fieldSet.Controls.Clear();

            if ( !string.IsNullOrEmpty( category ) )
            {
                HtmlGenericControl legend = new HtmlGenericControl( "h4" );
                fieldSet.Controls.Add( legend );
                legend.Controls.Clear();
                legend.InnerText = category.Trim();
            }

            foreach ( string key in attributeKeys )
            {
                var attribute = item.Attributes[key];

                if ( !exclude.Contains( attribute.Name ) )
                {
                    // Add the control for editing the attribute value
                    attribute.AddControl( fieldSet.Controls, item.AttributeValues[attribute.Key].Value, validationGroup, setValue, true );
                }
            }
        }

        /// <summary>
        /// Gets the display HTML.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="exclude">The exclude.</param>
        /// <param name="supressOrdering">if set to <c>true</c> supresses reording (LoadAttributes() may perform custom ordering as is the case for group member attributes).</param>
        public static void AddDisplayControls( IHasAttributes item, Control parentControl, List<string> exclude = null, bool supressOrdering = false )
        {
            exclude = exclude ?? new List<string>();
            string result = string.Empty;

            if ( item.Attributes != null )
            {
                AddDisplayControls(item, GetAttributeCategories(item, false, supressOrdering), parentControl, exclude);
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
        public static void AddDisplayControls( IHasAttributes item, List<AttributeCategory> attributeCategories, Control parentControl, List<string> exclude = null, bool showHeading = true )
        {
            foreach ( var attributeCategory in attributeCategories )
            {

                if ( showHeading )
                {
                    HtmlGenericControl header = new HtmlGenericControl( "h4" );

                    string categoryName = attributeCategory.Category != null ? attributeCategory.Category.Name : string.Empty;

                    header.InnerText = string.IsNullOrWhiteSpace( categoryName ) ? item.GetType().GetFriendlyTypeName() + " Attributes" : categoryName.Trim();
                    parentControl.Controls.Add( header );
                }

                HtmlGenericControl dl = new HtmlGenericControl( "dl" );
                parentControl.Controls.Add( dl );

                foreach ( var attribute in attributeCategory.Attributes )
                {
                    if ( exclude == null || !exclude.Contains( attribute.Name ) )
                    {
                        // Get the Attribute Value formatted for display.
                        string value = attribute.DefaultValue;
                        if ( item.AttributeValues.ContainsKey( attribute.Key ) && item.AttributeValues[attribute.Key] != null )
                        {
                            value = item.AttributeValues[attribute.Key].Value;
                        }

                        string controlHtml = attribute.FieldType.Field.FormatValueAsHtml( parentControl, value, attribute.QualifierValues );

                        // If the Attribute Value has some content, display it.
                        if (!string.IsNullOrWhiteSpace(controlHtml))
                        {
                            HtmlGenericControl dt = new HtmlGenericControl("dt");
                            dt.InnerText = attribute.Name;
                            dl.Controls.Add(dt);

                            HtmlGenericControl dd = new HtmlGenericControl("dd");

                            dd.InnerHtml = controlHtml;
                            dl.Controls.Add(dd);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the edit values.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="item">The item.</param>
        public static void GetEditValues( Control parentControl, IHasAttributes item )
        {
            if ( item.Attributes != null )
            {
                foreach ( var attribute in item.Attributes )
                {
                    Control control = parentControl.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        var value = new AttributeValue();

                        // Creating a brand new AttributeValue and setting its Value property.
                        // The Value prop's setter then queries the AttributeCache passing in the AttributeId, which is 0
                        // The AttributeCache.Read method returns null
                        value.Value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );
                        item.AttributeValues[attribute.Key] = value;
                    }
                }
            }
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

}