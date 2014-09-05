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
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityHelper
    {
        /// <summary>
        /// the list of fields and attributes for the EntityType. 
        /// ThreadStatic so that the fields don't get refetched more than once per page load, but also don't become stale
        /// </summary>
        [ThreadStatic]
        private static Dictionary<Type, List<EntityField>> _entityFields = null;

        /// <summary>
        /// Gets the entity fields.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="includeOnlyReportingFields">if set to <c>true</c> [include only reporting fields].</param>
        /// <returns></returns>
        public static List<EntityField> GetEntityFields( Type entityType, bool includeOnlyReportingFields = true )
        {
            if ( _entityFields == null )
            {
                _entityFields = new Dictionary<Type, List<EntityField>>();
            }

            if ( _entityFields.ContainsKey( entityType ) && _entityFields[entityType] != null )
            {
                return _entityFields[entityType];
            }

            var entityFields = new List<EntityField>();

            var entityProperties = entityType.GetProperties().ToList();
            var filteredEntityProperties = entityProperties.Where( p => !p.GetGetMethod().IsVirtual || p.Name == "Id" || p.Name == "Guid" || p.Name == "Order" ).ToList();

            // Get Properties
            foreach ( var property in filteredEntityProperties )
            {
                EntityField entityProperty = null;

                // Enum Properties
                if ( property.PropertyType.IsEnum )
                {
                    entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 1 );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                }

                // Boolean properties
                else if ( property.PropertyType == typeof( bool ) || property.PropertyType == typeof( bool? ) )
                {
                    entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 1 );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.SINGLE_SELECT;
                }

                // Date properties
                else if ( property.PropertyType == typeof( DateTime ) || property.PropertyType == typeof( DateTime? ) )
                {
                    entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.DATE;
                }

                // Decimal properties
                else if ( property.PropertyType == typeof( decimal ) || property.PropertyType == typeof( decimal? ) )
                {
                    entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.DECIMAL;
                }

                // Guid properties
                else if ( property.PropertyType == typeof( Guid ) || property.PropertyType == typeof( Guid? ) )
                {
                    entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.TEXT;
                }

                // Text Properties
                else if ( property.PropertyType == typeof( string ) )
                {
                    entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.TEXT;
                }

                // Integer Properties
                else if ( property.PropertyType == typeof( int ) || property.PropertyType == typeof( int? ) )
                {
                    var definedValueAttribute = property.GetCustomAttributes( typeof( Rock.Data.DefinedValueAttribute ), true ).FirstOrDefault();

                    if ( definedValueAttribute != null )
                    {
                        // Defined Value Properties
                        entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 1 );
                        var definedType = DefinedTypeCache.Read( ( (Rock.Data.DefinedValueAttribute)definedValueAttribute ).DefinedTypeGuid );
                        entityProperty.Title = definedType != null ? definedType.Name : property.Name.Replace( "ValueId", string.Empty ).SplitCase();
                        entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                        entityProperty.DefinedTypeGuid = definedType.Guid;
                    }
                    else
                    {
                        entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.INTEGER;
                    }
                }

                else
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "Unreported Entity PropertyType {0} for {1}", property.PropertyType.ToString(), property.Name ) );
                }

                if ( entityProperty != null )
                {
                    entityProperty.IsPreviewable = property.GetCustomAttributes( typeof( PreviewableAttribute ), true ).Any();
                    if ( includeOnlyReportingFields )
                    {
                        bool isReportable = !property.GetCustomAttributes( typeof( HideFromReportingAttribute ), true ).Any();
                        if ( isReportable )
                        {
                            entityFields.Add( entityProperty );
                        }
                    }
                    else
                    {
                        entityFields.Add( entityProperty );
                    }
                }
            }

            // Get Attributes
            int entityTypeId = EntityTypeCache.Read( entityType ).Id;
            var rockContext = new RockContext();
            var qryAttributes = new AttributeService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeId );
            if ( entityType == typeof( Group ) )
            {
                // in the case of Group, show attributes that are entity global, but also ones that are qualified by GroupTypeId
                qryAttributes = qryAttributes.Where( a => a.EntityTypeQualifierColumn == string.Empty || a.EntityTypeQualifierColumn == "GroupTypeId" );
            }
            else
            {
                qryAttributes = qryAttributes.Where( a => a.EntityTypeQualifierColumn == string.Empty && a.EntityTypeQualifierValue == string.Empty );
            }

            var attributeList = qryAttributes.ToList();

            foreach ( var attribute in attributeList )
            {
                AddEntityFieldForAttribute( entityFields, AttributeCache.Read( attribute.Id ) );
            }

            int index = 1;
            _entityFields[entityType] = new List<EntityField>();
            foreach ( var entityProperty in entityFields.OrderBy( p => p.Title ).ThenBy( p => p.Name ) )
            {
                entityProperty.Index = index;
                index += entityProperty.ControlCount;
                _entityFields[entityType].Add( entityProperty );
            }

            return _entityFields[entityType];
        }

        /// <summary>
        /// Adds the entity field for attribute.
        /// </summary>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="attribute">The attribute.</param>
        public static void AddEntityFieldForAttribute( List<EntityField> entityFields, AttributeCache attribute )
        {
            // Ensure prop name is unique
            string propName = attribute.Name;
            int i = 1;
            while ( entityFields.Any( p => p.Name.Equals( propName, StringComparison.CurrentCultureIgnoreCase ) ) )
            {
                propName = attribute.Name + ( i++ ).ToString();
            }

            EntityField entityProperty = null;

            var fieldType = FieldTypeCache.Read( attribute.FieldTypeId );
            string fieldTypeUpperGuid = fieldType.Guid.ToString().ToUpper();

            switch ( fieldTypeUpperGuid )
            {
                case SystemGuid.FieldType.BOOLEAN:
                    entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Guid );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.SINGLE_SELECT;
                    break;

                case SystemGuid.FieldType.DATE:
                    entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Guid );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.DATE;
                    break;

                case SystemGuid.FieldType.TIME:
                    entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Guid );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.TIME;
                    break;

                case SystemGuid.FieldType.INTEGER:
                    entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Guid );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.INTEGER;
                    break;

                case SystemGuid.FieldType.DECIMAL:
                    entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Guid );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.DECIMAL;
                    break;

                case SystemGuid.FieldType.DAY_OF_WEEK:
                    entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Guid );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.DAY_OF_WEEK;
                    break;

                case SystemGuid.FieldType.MULTI_SELECT:
                    entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Guid );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                    break;

                case SystemGuid.FieldType.SINGLE_SELECT:
                    entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Guid );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                    break;

                case SystemGuid.FieldType.TEXT:
                    entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Guid );
                    entityProperty.FilterFieldType = SystemGuid.FieldType.TEXT;
                    break;
            }

            if ( entityProperty != null )
            {
                if ( attribute.EntityTypeId == EntityTypeCache.GetId( typeof( Group ) ) && attribute.EntityTypeQualifierColumn == "GroupTypeId" )
                {
                    var groupType = new GroupTypeService( new RockContext() ).Get( attribute.EntityTypeQualifierValue.AsInteger() );
                    if ( groupType != null )
                    {
                        entityProperty.Title = string.Format( "{0} ({1})", attribute.Name.SplitCase(), groupType.Name );
                    }
                }

                entityFields.Add( entityProperty );
            }
        }
    }

    #region Helper Classes

    /// <summary>
    /// Helper class for saving information about each property and attribute of an entity
    /// Note: the type of a field or attribute does not neccesarily determine the ui rendered for filtering.   For example, a Single-Select attribute
    /// will use a multi-select ui so that user can filter on one or more values.  The FilterFieldType property determines the UI rendered for filtering
    /// and not the type of field.
    /// 
    /// Entity Property Types and their renderd filter field type
    ///     string              ->  TEXT
    ///     bool or bool?       ->  SINGLE_SELECT
    ///     date or date?       ->  DATE
    ///     int or int?
    ///         Defined Values  ->  MULTI_SELECT
    ///         otherwise       ->  INTEGER
    ///     enumeration         ->  MULTI_SELECT
    /// 
    /// Attribute types and their rendered filter field type
    ///     MULTI_SELECT        ->  MULTI_SELECT
    ///     SINGLE_SELECT       ->  MULTI_SELECT
    ///     BOOLEAN             ->  SINGLE_SELECT (True or False)
    ///     DATE                ->  DATE
    ///     INTEGER             ->  INTEGER
    ///     TEXT                ->  TEXT
    ///     
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
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the control count.
        /// </summary>
        /// <value>
        /// The control count.
        /// </value>
        public int ControlCount { get; set; }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        public Guid? AttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the type of the filter field.
        /// </summary>
        /// <value>
        /// The type of the filter field.
        /// </value>
        public string FilterFieldType { get; set; }

        /// <summary>
        /// Gets or sets the defined type identifier.
        /// </summary>
        /// <value>
        /// The defined type identifier.
        /// </value>
        public Guid? DefinedTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is previewable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is previewable]; otherwise, <c>false</c>.
        /// </value>
        public bool IsPreviewable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityField" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fieldKind">Kind of the field.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="controlCount">The control count.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        public EntityField( string name, FieldKind fieldKind, Type propertyType, int controlCount, Guid? attributeGuid = null )
        {
            Name = name;
            Title = name.SplitCase();
            FieldKind = fieldKind;
            PropertyType = propertyType;
            ControlCount = controlCount;
            AttributeGuid = attributeGuid;
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
