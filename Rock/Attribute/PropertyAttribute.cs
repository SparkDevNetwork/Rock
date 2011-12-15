//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;

namespace Rock.Attribute
{
    /// <summary>
    /// A class Attribute that can be used by any oject that inherits from <see cref="IHasAttributes"/> to specify what attributes it needs.  The 
    /// Framework provides methods in the <see cref="Rock.Attribute.Helper"/> class to create, read, and update the attributes
    /// </summary>
    /// <remarks>
    /// If using a custom <see cref="Rock.FieldTypes.IFieldType"/> make sure that the fieldtype has been added to Rock.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PropertyAttribute : System.Attribute
    {
        // TODO: Add a way to group attributes...

        /// <summary>
        /// Gets or sets the attribute key.
        /// </summary>
        /// <remarks>
        /// The key should be unique for each <see cref="PropertyAttribute"/> defined in a <see cref="Rock.Attribute.IHasAttributes"/> object
        /// </remarks>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly name of the attribute
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the description of the attribute
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the default value of the attribute.  This is the value that will be used if a specific value has not yet been created
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the order of the attribute.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the assembly name of the <see cref="Rock.FieldTypes.IFieldType"/> to be used for the attribute
        /// </summary>
        /// <value>
        /// The field type assembly.
        /// </value>
        public string FieldTypeAssembly { get; set; }

        /// <summary>
        /// Gets or sets the class name of the <see cref="Rock.FieldTypes.IFieldType"/> to be used for the attribute.
        /// </summary>
        /// <value>
        /// The field type class.
        /// </value>
        public string FieldTypeClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PropertyAttribute"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class as a text field with no default value or description.
        /// The <see cref="Key"/> will be the same as the <see cref="Name"/> with spaces removed.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description of the attribute.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <remarks>
        ///   <see cref="Key"/> is initialized to the Name value with spaces removed
        ///   <see cref="Category"/> is blank
        ///   <see cref="DefaultValue"/> is blank
        ///   <see cref="FieldTypeAssembly"/> is initialized to <c>Rock</c>
        ///   <see cref="FieldTypeClass"/> is initialized to <c>Rock.FieldTypes.Text</c>
        /// </remarks>
        public PropertyAttribute( int order, string name, string description, bool required )
            : this( order, name, name.Replace( " ", "" ), string.Empty, description, required, string.Empty, "Rock", "Rock.FieldTypes.Text" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class as a text field with no default value or description.
        /// The <see cref="Key"/> will be the same as the <see cref="Name"/> with spaces removed.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <remarks>
        ///   <see cref="Key"/> is initialized to the Name value with spaces removed
        ///   <see cref="FieldTypeAssembly"/> is initialized to <c>Rock</c>
        ///   <see cref="FieldTypeClass"/> is initialized to <c>Rock.FieldTypes.Text</c>
        /// </remarks>
        public PropertyAttribute( int order, string name, string category, string description, bool required, string defaultValue )
            : this(order, name, name.Replace(" ", ""), category, description, required, defaultValue, "Rock", "Rock.FieldTypes.Text" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class as a text field.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <remarks>
        ///   <see cref="FieldTypeAssembly"/> is initialized to <c>Rock</c>
        ///   <see cref="FieldTypeClass"/> is initialized to <c>Rock.FieldTypes.Text</c>
        /// </remarks>
        public PropertyAttribute(int order, string name, string key, string category, string description
            , bool required, string defaultValue )
            : this(order, name, key, category, description, required, defaultValue, "Rock", "Rock.FieldTypes.Text" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
        /// The <see cref="Key"/> will be the same as the <see cref="Name"/> with spaces removed.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="fieldTypeAssembly">The field type assembly.</param>
        /// <param name="fieldTypeClass">The field type class.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <remarks>
        ///   <see cref="Key"/> is initialized to the Name value with spaces removed
        /// </remarks>
        public PropertyAttribute( int order, string name, string category, string description,
            bool required, string defaultValue, string fieldTypeAssembly, string fieldTypeClass )
            : this( order, name, name.Replace( " ", "" ), category, description, required, defaultValue, fieldTypeAssembly, fieldTypeClass )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="fieldTypeAssembly">The field type assembly.</param>
        /// <param name="fieldTypeClass">The field type class.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        public PropertyAttribute(int order, string name, string key, string category, string description, 
            bool required, string defaultValue, string fieldTypeAssembly, string fieldTypeClass)
        {
            Key = key;
            Name = name;
            Category = category;
            Description = description;
            Required = required;
            DefaultValue = defaultValue;
            Order = order;
            FieldTypeAssembly = fieldTypeAssembly;
            FieldTypeClass = fieldTypeClass;
        }

        /// <summary>
        /// Adds or Updates a <see cref="Rock.Core.Attribute"/> item for the attribute.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        internal bool UpdateAttribute( string entity, string entityQualifierColumn, string entityQualifierValue, int? currentPersonId )
        {
            bool updated = false;

            Core.AttributeService attributeService = new Core.AttributeService();
            Core.FieldTypeService fieldTypeService = new Core.FieldTypeService();

            // Look for an existing attribute record based on the entity, entityQualifierColumn and entityQualifierValue
            Core.Attribute attribute = attributeService.GetAttributesByEntityQualifierAndKey(
                entity, entityQualifierColumn, entityQualifierValue, this.Key );

            if ( attribute == null )
            {
                // If an existing attribute record doesn't exist, create a new one
                updated = true;
                attribute = new Core.Attribute();
                attribute.Entity = entity;
                attribute.EntityQualifierColumn = entityQualifierColumn;
                attribute.EntityQualifierValue = entityQualifierValue;
                attribute.Key = this.Key;
                attribute.GridColumn = false;
            }
            else
            {
                // Check to see if the existing attribute record needs to be updated
                if ( attribute.Name != this.Name ||
                    attribute.Category != this.Category ||
                    attribute.DefaultValue != this.DefaultValue ||
                    attribute.Description != this.Description ||
                    attribute.Order != this.Order ||
                    attribute.FieldType.Assembly != this.FieldTypeAssembly ||
                    attribute.FieldType.Class != this.FieldTypeClass ||
                    attribute.Required != this.Required)
                    updated = true;
            }

            if ( updated )
            {
                // Update the attribute
                attribute.Name = this.Name;
                attribute.Category = this.Category;
                attribute.Description = this.Description;
                attribute.DefaultValue = this.DefaultValue;
                attribute.Order = this.Order;
                attribute.Required = this.Required;

                // Try to set the field type by searching for an existing field type with the same assembly and class name
                if ( attribute.FieldType == null || attribute.FieldType.Assembly != this.FieldTypeAssembly ||
                    attribute.FieldType.Class != this.FieldTypeClass )
                {
                    attribute.FieldType = fieldTypeService.Queryable().FirstOrDefault( f =>
                        f.Assembly == this.FieldTypeAssembly &&
                        f.Class == this.FieldTypeClass );
                }

                // If this is a new attribute, add it, otherwise remove the exiting one from the cache
                if ( attribute.Id == 0 )
                    attributeService.Add( attribute, currentPersonId );
                else
                    Rock.Web.Cache.Attribute.Flush( attribute.Id );

                attributeService.Save( attribute, currentPersonId );

                return true;
            }
            else
                return false;
        }
    }
}