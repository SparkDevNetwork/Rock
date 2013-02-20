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
    /// 
    /// </summary>
    public class CustomRadioListFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudiencesPrimarySecondaryFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="values">The values to display.  Format is 'value1,value2,value3,...' or 'value1:text1,value2:text2,value3:text3,...'.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public CustomRadioListFieldAttribute( string name, string description, string values, bool required = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.SelectSingle ).FullName )
        {
            FieldConfigurationValues.Add( "values", new Field.ConfigurationValue( values ) );
            FieldConfigurationValues.Add( "fieldtype", new Field.ConfigurationValue( "rb" ) );
        }
    }
}