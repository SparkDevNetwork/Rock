using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        /// The key should be unique for each <see cref="PropertyAttribute"/> defined in a <see cref="IHasAttribute"/> object
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
        /// Gets or sets the assembly name of the <see cref="Rock.FieldType.IFieldType"/> to be used for the attribute
        /// </summary>
        /// <value>
        /// The field type assembly.
        /// </value>
        public string FieldTypeAssembly { get; set; }

        /// <summary>
        /// Gets or sets the class name of the <see cref="Rock.FieldType.IFieldType"/> to be used for the attribute.
        /// </summary>
        /// <value>
        /// The field type class.
        /// </value>
        public string FieldTypeClass { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class as a text field with no default value or description.  
        /// The <see cref="Key"/> will be the same as the <see cref="Name"/> with spaces removed.
        /// </summary>
        /// <remarks>
        /// <see cref="Key"/> is initialized to the <see cref="Name"/> with spaces removed.
        /// <see cref="Description"/> is initialized as an empty string.
        /// <see cref="DefaultValue"/> is initialized as an empty string.
        /// <see cref="FieldTypeAssembly"/> is initialized to <c>Rock.Framework</c>
        /// <see cref="FieldTypeClass"/> is initialized to <c>Rock.FieldTypes.Text</c>
        /// </remarks>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        public PropertyAttribute(int order, string name )
            : this(order, name, name.Replace(" ", ""), string.Empty, string.Empty, "Rock.Framework", "Rock.FieldTypes.Text" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class as a text field with no default value.  
        /// The <see cref="Key"/> will be the same as the <see cref="Name"/> with spaces removed.
        /// </summary>
        /// <remarks>
        /// <see cref="Key"/> is initialized to the <see cref="Name"/> with spaces removed.
        /// <see cref="DefaultValue"/> is initialized as an empty string.
        /// <see cref="FieldTypeAssembly"/> is initialized to <c>Rock.Framework</c>
        /// <see cref="FieldTypeClass"/> is initialized to <c>Rock.FieldTypes.Text</c>
        /// </remarks>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public PropertyAttribute(int order, string name, string description )
            : this(order, name, name.Replace( " ", "" ), description, string.Empty, "Rock.Framework", "Rock.FieldTypes.Text" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class as a text field.
        /// The <see cref="Key"/> will be the same as the <see cref="Name"/> with spaces removed.
        /// </summary>
        /// <remarks>
        /// <see cref="Key"/> is initialized to the <see cref="Name"/> with spaces removed.
        /// <see cref="FieldTypeAssembly"/> is initialized to <c>Rock.Framework</c>
        /// <see cref="FieldTypeClass"/> is initialized to <c>Rock.FieldTypes.Text</c>
        /// </remarks>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">The default value.</param>
        public PropertyAttribute(int order, string name, string description, string defaultValue )
            : this(order, name, name.Replace( " ", "" ), description, defaultValue, "Rock.Framework", "Rock.FieldTypes.Text" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class as a text field.
        /// </summary>
        /// <remarks>
        /// <see cref="FieldTypeAssembly"/> is initialized to <c>Rock.Framework</c>
        /// <see cref="FieldTypeClass"/> is initialized to <c>Rock.FieldTypes.Text</c>
        /// </remarks>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">The default value.</param>
        public PropertyAttribute(int order, string name, string key, string description, string defaultValue )
            : this(order, name, key, description, defaultValue, "Rock.Framework", "Rock.FieldTypes.Text" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="fieldTypeAssembly">The field type assembly.</param>
        /// <param name="fieldTypeClass">The field type class.</param>
        public PropertyAttribute(int order, string name, string key, string description, string defaultValue, 
            string fieldTypeAssembly, string fieldTypeClass)
        {
            Key = key;
            Name = name;
            Description = description;
            DefaultValue = defaultValue;
            Order = order;
            FieldTypeAssembly = fieldTypeAssembly;
            FieldTypeClass = fieldTypeClass;
        }

        /// <summary>
        /// Adds or Updates a <see cref="Rock.Model.Core.Attribute"/> item for the attribute.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        internal bool UpdateAttribute( string entity, string entityQualifierColumn, string entityQualifierValue, int? currentPersonId )
        {
            bool updated = false;

            Services.Core.AttributeService attributeService = new Services.Core.AttributeService();
            Services.Core.FieldTypeService fieldTypeService = new Services.Core.FieldTypeService();

            // Look for an existing attribute record based on the entity, entityQualifierColumn and entityQualifierValue
            Models.Core.Attribute attribute = attributeService.GetAttributesByEntityQualifierAndKey(
                entity, entityQualifierColumn, entityQualifierValue, this.Key );

            if ( attribute == null )
            {
                // If an existing attribute record doesn't exist, create a new one
                updated = true;
                attribute = new Models.Core.Attribute();
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
                    attribute.DefaultValue != this.DefaultValue ||
                    attribute.Description != this.Description ||
                    attribute.Order != this.Order ||
                    attribute.FieldType.Assembly != this.FieldTypeAssembly ||
                    attribute.FieldType.Class != this.FieldTypeClass )
                    updated = true;
            }

            if ( updated )
            {
                // Update the attribute
                attribute.Name = this.Name;
                attribute.Description = this.Description;
                attribute.DefaultValue = this.DefaultValue;
                attribute.Order = this.Order;

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
                    Rock.Cms.Cached.Attribute.Flush( attribute.Id );

                attributeService.Save( attribute, currentPersonId );

                return true;
            }
            else
                return false;
        }
    }
}