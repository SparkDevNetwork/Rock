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
    /// If using a custom <see cref="Rock.Field.IFieldType"/> make sure that the fieldtype has been added to Rock.
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
        /// Gets or sets the assembly name of the <see cref="Rock.Field.IFieldType"/> to be used for the attribute
        /// </summary>
        /// <value>
        /// The field type assembly.
        /// </value>
        public string FieldTypeAssembly { get; set; }

        /// <summary>
        /// Gets or sets the class name of the <see cref="Rock.Field.IFieldType"/> to be used for the attribute.
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
        public bool IsRequired { get; set; }

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
        ///   <see cref="FieldTypeClass"/> is initialized to <c>Rock.Field.Types.Text</c>
        /// </remarks>
        public PropertyAttribute( int order, string name, string description, bool required )
            : this( order, name, name.Replace( " ", "" ), string.Empty, description, required, string.Empty, "Rock", "Rock.Field.Types.Text" )
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
        ///   <see cref="FieldTypeClass"/> is initialized to <c>Rock.Field.Types.Text</c>
        /// </remarks>
        public PropertyAttribute( int order, string name, string category, string description, bool required, string defaultValue )
            : this(order, name, name.Replace(" ", ""), category, description, required, defaultValue, "Rock", "Rock.Field.Types.Text" )
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
        ///   <see cref="FieldTypeClass"/> is initialized to <c>Rock.Field.Types.Text</c>
        /// </remarks>
        public PropertyAttribute(int order, string name, string key, string category, string description
            , bool required, string defaultValue )
            : this(order, name, key, category, description, required, defaultValue, "Rock", "Rock.Field.Types.Text" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
        /// The <see cref="Key"/> will be the same as the <see cref="Name"/> with spaces removed.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
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
            IsRequired = required;
            DefaultValue = defaultValue;
            Order = order;
            FieldTypeAssembly = fieldTypeAssembly;
            FieldTypeClass = fieldTypeClass;
        }
    }
}