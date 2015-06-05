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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityHelper
    {
        /// <summary>
        /// Gets the entity fields.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="includeOnlyReportingFields">if set to <c>true</c> [include only reporting fields].</param>
        /// <param name="limitToFilterableFields">if set to <c>true</c> [limit to filterable fields].</param>
        /// <returns></returns>
        public static List<EntityField> GetEntityFields( Type entityType, bool includeOnlyReportingFields = true, bool limitToFilterableFields = true )
        {
            List<EntityField> entityFields = null;

            if ( HttpContext.Current != null )
            {
                entityFields = HttpContext.Current.Items[string.Format( "EntityHelper:GetEntityFields:{0}", entityType.FullName )] as List<EntityField>;
                if ( entityFields != null )
                {
                    return entityFields;
                }
            }

            if ( entityFields == null )
            {
                entityFields = new List<EntityField>();
            }

            // Find all non-virtual properties or properties that have the [IncludeForReporting] attribute
            var entityProperties = entityType.GetProperties().ToList();
            var filteredEntityProperties = entityProperties
                .Where( p =>
                    !p.GetGetMethod().IsVirtual ||
                    p.GetCustomAttributes( typeof( IncludeForReportingAttribute ), true ).Any() ||
                    p.Name == "Order" )
                .ToList();

            // Get Properties
            foreach ( var property in filteredEntityProperties )
            {
                bool isReportable = !property.GetCustomAttributes( typeof( HideFromReportingAttribute ), true ).Any();
                if ( !includeOnlyReportingFields || isReportable )
                {

                    EntityField entityField = new EntityField( property.Name, FieldKind.Property, property );
                    entityField.IsPreviewable = property.GetCustomAttributes( typeof( PreviewableAttribute ), true ).Any();
                    var fieldTypeAttribute = property.GetCustomAttribute<Rock.Data.FieldTypeAttribute>();

                    // check if we can set it from the fieldTypeAttribute
                    if ( ( fieldTypeAttribute != null ) && SetEntityFieldFromFieldTypeAttribute( entityField, fieldTypeAttribute ) )
                    {
                        // intentially blank, entity field is already setup
                    }

                    // Enum Properties
                    else if ( property.PropertyType.IsEnum )
                    {
                        entityField.FieldType = FieldTypeCache.Read( SystemGuid.FieldType.SINGLE_SELECT.AsGuid() );

                        var list = new List<string>();
                        foreach ( var value in Enum.GetValues( property.PropertyType ) )
                        {
                            list.Add( string.Format( "{0}^{1}", value, value.ToString().SplitCase() ) );
                        }

                        var listSource = string.Join( ",", list );
                        entityField.FieldConfig.Add( "values", new Field.ConfigurationValue( listSource ) );
                        entityField.FieldConfig.Add( "fieldtype", new Field.ConfigurationValue( "rb" ) );
                    }

                    // Boolean properties
                    else if ( property.PropertyType == typeof( bool ) || property.PropertyType == typeof( bool? ) )
                    {
                        entityField.FieldType = FieldTypeCache.Read( SystemGuid.FieldType.BOOLEAN.AsGuid() );
                    }

                    // Datetime properties
                    else if ( property.PropertyType == typeof( DateTime ) || property.PropertyType == typeof( DateTime? ) )
                    {
                        var colAttr = property.GetCustomAttributes( typeof( ColumnAttribute ), true ).FirstOrDefault();
                        if ( colAttr != null && ( (ColumnAttribute)colAttr ).TypeName == "Date" )
                        {
                            entityField.FieldType = FieldTypeCache.Read( SystemGuid.FieldType.DATE.AsGuid() );
                        }
                        else
                        {
                            entityField.FieldType = FieldTypeCache.Read( SystemGuid.FieldType.DATE_TIME.AsGuid() );
                        }
                    }

                    // Decimal properties
                    else if ( property.PropertyType == typeof( decimal ) || property.PropertyType == typeof( decimal? ) )
                    {
                        entityField.FieldType = FieldTypeCache.Read( SystemGuid.FieldType.DECIMAL.AsGuid() );
                    }

                    // Text Properties
                    else if ( property.PropertyType == typeof( string ) )
                    {
                        entityField.FieldType = FieldTypeCache.Read( SystemGuid.FieldType.TEXT.AsGuid() );
                    }

                    // Integer Properties
                    else if ( property.PropertyType == typeof( int ) || property.PropertyType == typeof( int? ) )
                    {
                        entityField.FieldType = FieldTypeCache.Read( SystemGuid.FieldType.INTEGER.AsGuid() );

                        var definedValueAttribute = property.GetCustomAttribute<Rock.Data.DefinedValueAttribute>();
                        if ( definedValueAttribute != null )
                        {
                            // Defined Value Properties
                            Guid? definedTypeGuid = ( (Rock.Data.DefinedValueAttribute)definedValueAttribute ).DefinedTypeGuid;
                            if ( definedTypeGuid.HasValue )
                            {
                                var definedType = DefinedTypeCache.Read( definedTypeGuid.Value );
                                entityField.Title = definedType != null ? definedType.Name : property.Name.Replace( "ValueId", string.Empty ).SplitCase();
                                if ( definedType != null )
                                {
                                    entityField.FieldType = FieldTypeCache.Read( SystemGuid.FieldType.DEFINED_VALUE.AsGuid() );
                                    entityField.FieldConfig.Add( "definedtype", new Field.ConfigurationValue( definedType.Id.ToString() ) );
                                }
                            }
                        }
                    }

                    if ( entityField != null && entityField.FieldType != null )
                    {
                        entityFields.Add( entityField );
                    }
                }
            }

            // Get Attributes
            var entityTypeCache = EntityTypeCache.Read( entityType, true );
            if ( entityTypeCache != null )
            {
                int entityTypeId = entityTypeCache.Id;
                using ( var rockContext = new RockContext() )
                {
                    var qryAttributes = new AttributeService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeId );
                    if ( entityType == typeof( Group ) )
                    {
                        // in the case of Group, show attributes that are entity global, but also ones that are qualified by GroupTypeId
                        qryAttributes = qryAttributes
                            .Where( a =>
                                a.EntityTypeQualifierColumn == null ||
                                a.EntityTypeQualifierColumn == string.Empty ||
                                a.EntityTypeQualifierColumn == "GroupTypeId" );
                    }
                    else
                    {
                        qryAttributes = qryAttributes.Where( a => a.EntityTypeQualifierColumn == string.Empty && a.EntityTypeQualifierValue == string.Empty );
                    }

                    var attributeIdList = qryAttributes.Select( a => a.Id ).ToList();

                    foreach ( var attributeId in attributeIdList )
                    {
                        AddEntityFieldForAttribute( entityFields, AttributeCache.Read( attributeId ), limitToFilterableFields );
                    }
                }
            }

            // Order the fields by title, name
            int index = 0;
            var sortedFields = new List<EntityField>();
            foreach ( var entityField in entityFields.OrderBy( p => p.Title ).ThenBy( p => p.Name ) )
            {
                entityField.Index = index;
                index++;
                sortedFields.Add( entityField );
            }

            if ( HttpContext.Current != null )
            {
                HttpContext.Current.Items[string.Format( "EntityHelper:GetEntityFields:{0}", entityType.FullName )] = sortedFields;
            }

            return sortedFields;
        }

        /// <summary>
        /// Sets the entity field from field type attribute.
        /// </summary>
        /// <param name="entityField">The entity field.</param>
        /// <param name="fieldTypeAttribute">The field type attribute.</param>
        /// <returns></returns>
        private static bool SetEntityFieldFromFieldTypeAttribute( EntityField entityField, FieldTypeAttribute fieldTypeAttribute )
        {
            if ( fieldTypeAttribute != null )
            {
                var fieldTypeCache = FieldTypeCache.Read( fieldTypeAttribute.FieldTypeGuid );
                if ( fieldTypeCache != null && fieldTypeCache.Field != null )
                {
                    if ( fieldTypeCache.Field.HasFilterControl() )
                    {
                        if ( entityField.Title.EndsWith( " Id" ) )
                        {
                            entityField.Title = entityField.Title.ReplaceLastOccurrence( " Id", string.Empty );
                        }

                        entityField.FieldType = fieldTypeCache;
                        if ( fieldTypeAttribute.ConfigurationKey != null && fieldTypeAttribute.ConfigurationValue != null )
                        {
                            entityField.FieldConfig.Add( fieldTypeAttribute.ConfigurationKey, new ConfigurationValue( fieldTypeAttribute.ConfigurationValue ) );
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the entity field for attribute.
        /// </summary>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="limitToFilterableAttributes">if set to <c>true</c> [limit to filterable attributes].</param>
        public static void AddEntityFieldForAttribute( List<EntityField> entityFields, AttributeCache attribute, bool limitToFilterableAttributes = true )
        {
            // Ensure prop name only has Alpha, Numeric and underscore chars
            string propName = attribute.Key.RemoveSpecialCharacters().Replace( ".", "" );

            // Ensure prop name is unique
            int i = 1;
            while ( entityFields.Any( p => p.Name.Equals( propName, StringComparison.CurrentCultureIgnoreCase ) ) )
            {
                propName = attribute.Key + ( i++ ).ToString();
            }

            // Make sure that the attributes field type actually renders a filter control if limitToFilterableAttributes
            var fieldType = FieldTypeCache.Read( attribute.FieldTypeId );
            if ( fieldType != null && ( !limitToFilterableAttributes || fieldType.Field.HasFilterControl() ) )
            {
                var entityField = new EntityField( propName, FieldKind.Attribute, typeof( string ), attribute.Guid, fieldType );
                entityField.Title = attribute.Name.SplitCase();

                foreach ( var config in attribute.QualifierValues )
                {
                    entityField.FieldConfig.Add( config.Key, config.Value );
                }

                if ( attribute.EntityTypeId == EntityTypeCache.GetId( typeof( Group ) ) && attribute.EntityTypeQualifierColumn == "GroupTypeId" )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var groupType = new GroupTypeService( rockContext ).Get( attribute.EntityTypeQualifierValue.AsInteger() );
                        if ( groupType != null )
                        {
                            entityField.Title = string.Format( "{0} ({1})", attribute.Name, groupType.Name );
                        }
                    }
                }

                entityFields.Add( entityField );
            }
        }
    }

    #region Helper Classes

    /// <summary>
    /// Helper class for saving information about each property and attribute of an entity
    /// </summary>
    public class EntityField
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the kind of the field.
        /// </summary>
        /// <value>
        /// The kind of the field.
        /// </value>
        public FieldKind FieldKind { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the property information (if this is FieldKind.Property and PropertyInfo is known)
        /// </summary>
        /// <value>
        /// The property information.
        /// </value>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        public Guid? AttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the type of the filter field.
        /// </summary>
        /// <value>
        /// The type of the filter field.
        /// </value>
        public FieldTypeCache FieldType { get; set; }

        /// <summary>
        /// Gets the type of the bound field.
        /// </summary>
        /// <returns></returns>
        public System.Web.UI.WebControls.BoundField GetBoundFieldType()
        {
            if ( this.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() ) )
            {
                return new DefinedValueField();
            }
            else if ( this.PropertyInfo != null )
            {
                return Grid.GetGridField( this.PropertyInfo );
            }
            else
            {
                return Grid.GetGridField( this.PropertyType );
            }
        }

        /// <summary>
        /// Gets or sets the field configuration.
        /// </summary>
        /// <value>
        /// The field configuration.
        /// </value>
        public Dictionary<string, ConfigurationValue> FieldConfig { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is previewable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is previewable]; otherwise, <c>false</c>.
        /// </value>
        public bool IsPreviewable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityField"/> class.
        /// </summary>
        [Obsolete( "Use one of the other EntityField constructors instead" )]
        public EntityField()
        {
            FieldConfig = new Dictionary<string, ConfigurationValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityField"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fieldKind">Kind of the field.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        [Obsolete( "Use one of the other EntityField constructors instead" )]
        public EntityField( string name, FieldKind fieldKind, Type propertyType, Guid? attributeGuid = null )
            : this( name, fieldKind, propertyType, null, attributeGuid )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityField" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fieldKind">Kind of the field.</param>
        /// <param name="propertyInfo">The property information.</param>
        public EntityField( string name, FieldKind fieldKind, PropertyInfo propertyInfo )
            : this( name, fieldKind, propertyInfo.PropertyType, propertyInfo, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityField" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fieldKind">Kind of the field.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="fieldType">Type of the field.</param>
        public EntityField( string name, FieldKind fieldKind, Type propertyType, Guid attributeGuid, FieldTypeCache fieldType )
            : this( name, fieldKind, propertyType, null, attributeGuid )
        {
            this.FieldType = fieldType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityField"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fieldKind">Kind of the field.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private EntityField( string name, FieldKind fieldKind, Type propertyType, PropertyInfo propertyInfo, Guid? attributeGuid )
        {
            FieldConfig = new Dictionary<string, ConfigurationValue>();
            Name = name;
            Title = name.SplitCase();
            FieldKind = fieldKind;
            PropertyType = propertyType;
            PropertyInfo = propertyInfo;
            AttributeGuid = attributeGuid;
        }

        /// <summary>
        /// Formatteds the filter.
        /// </summary>
        /// <param name="filterValues">The filter values.</param>
        /// <returns></returns>
        public string FormattedFilterDescription( List<string> filterValues )
        {
            if ( this.FieldType != null && this.FieldType.Field != null )
            {
                return string.Format( "{0} {1}", this.Title, this.FieldType.Field.FormatFilterValues( this.FieldConfig, filterValues ) );
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( this.FieldKind == Reporting.FieldKind.Attribute )
            {
                return string.Format( "Attribute:{0} (Guid:{1})", this.Name, this.AttributeGuid );
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( this.Name ) )
                {
                    return string.Format( "Property:{0}", this.Name );
                }
            }

            return base.ToString();
        }
    }

    #endregion

    #region Private Enumerations

    /// <summary>
    /// 
    /// </summary>
    public enum FieldKind
    {
        /// <summary>
        /// Property Field
        /// </summary>
        Property,

        /// <summary>
        /// Attribute Field
        /// </summary>
        Attribute,
    }

    #endregion
}
