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
        /// Gets the entity fields.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static List<EntityField> GetEntityFields( Type entityType )
        {
            var entityFields = new List<EntityField>();

            // Get Properties
            foreach ( var property in entityType.GetProperties() )
            {
                if ( !property.GetGetMethod().IsVirtual || property.Name == "Id" || property.Name == "Guid" || property.Name == "Order" )
                {
                    EntityField entityProperty = null;

                    // Enum Properties
                    if ( property.PropertyType.IsEnum )
                    {
                        entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 1 );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                    }

                    // Boolean properties
                    if ( property.PropertyType == typeof( bool ) || property.PropertyType == typeof( bool? ) )
                    {
                        entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 1 );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.SINGLE_SELECT;
                    }

                    // Date properties
                    if ( property.PropertyType == typeof( DateTime ) || property.PropertyType == typeof( DateTime? ) )
                    {
                        entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.DATE;
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
                            entityProperty.DefinedTypeId = definedType.Id;
                        }
                        else
                        {
                            entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.INTEGER;
                        }
                    }

                    if ( entityProperty != null )
                    {
                        entityProperty.IsPreviewable = property.GetCustomAttributes( typeof( PreviewableAttribute ), true ).Any();
                        entityFields.Add( entityProperty );
                    }
                }
            }

            // Get Attributes
            int entityTypeId = EntityTypeCache.Read( entityType ).Id;
            foreach ( var attribute in new AttributeService().Get( entityTypeId, string.Empty, string.Empty ) )
            {
                // Ensure prop name is unique
                string propName = attribute.Name;
                int i = 1;
                while ( entityFields.Any( p => p.Name.Equals( propName, StringComparison.CurrentCultureIgnoreCase ) ) )
                {
                    propName = attribute.Name + ( i++ ).ToString();
                }

                EntityField entityProperty = null;

                switch ( attribute.FieldType.Guid.ToString().ToUpper() )
                {
                    case SystemGuid.FieldType.BOOLEAN:
                        entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Id );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.SINGLE_SELECT;
                        break;

                    case SystemGuid.FieldType.DATE:
                        entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Id );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.DATE;
                        break;

                    case SystemGuid.FieldType.INTEGER:
                        entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Id );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.INTEGER;
                        break;

                    case SystemGuid.FieldType.MULTI_SELECT:
                        entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Id );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                        break;

                    case SystemGuid.FieldType.SINGLE_SELECT:
                        entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Id );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                        break;

                    case SystemGuid.FieldType.TEXT:
                        entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Id );
                        entityProperty.FilterFieldType = SystemGuid.FieldType.TEXT;
                        break;
                }

                if ( entityProperty != null )
                {
                    entityFields.Add( entityProperty );
                }
            }

            int index = 1;
            List<EntityField> result = new List<EntityField>();
            foreach ( var entityProperty in entityFields.OrderBy( p => p.Title ).ThenBy( p => p.Name ) )
            {
                entityProperty.Index = index;
                index += entityProperty.ControlCount;
                result.Add( entityProperty );
            }

            return result;
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
        public int? AttributeId { get; set; }

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
        public int? DefinedTypeId { get; set; }

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
        /// <param name="name">The name.</param>
        /// <param name="fieldKind">Kind of the field.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="controlCount">The control count.</param>
        /// <param name="attributeId">The attribute identifier.</param>
        public EntityField( string name, FieldKind fieldKind, Type propertyType, int controlCount, int? attributeId = null )
        {
            Name = name;
            Title = name.SplitCase();
            FieldKind = fieldKind;
            PropertyType = propertyType;
            ControlCount = controlCount;
            AttributeId = attributeId;
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
