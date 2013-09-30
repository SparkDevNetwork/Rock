//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute for selecting either true or false.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class BooleanFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key. (null means derive from name)</param>
        public BooleanFieldAttribute( string name, string description = "", bool defaultValue = false, string category = "", int order = 0, string key = null )
            : base( name, description, false, defaultValue.ToTrueFalse(), category, order, key, typeof( Rock.Field.Types.BooleanFieldType ).FullName )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="trueText">The true text.</param>
        /// <param name="falseText">The false text.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public BooleanFieldAttribute( string name, string trueText, string falseText, string description = "", bool defaultValue = false, string category = "", int order = 0, string key = null )
            : base( name, description, false, defaultValue.ToTrueFalse(), category, order, key, typeof( Rock.Field.Types.BooleanFieldType ).FullName )
        {
            FieldConfigurationValues.Add( "truetext", new Field.ConfigurationValue( trueText ) );
            FieldConfigurationValues.Add( "falsetext", new Field.ConfigurationValue( falseText ) );
        }
    }
}