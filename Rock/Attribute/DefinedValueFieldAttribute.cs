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
    /// Field Attribute to select 0 or more Audiences
    /// </summary>
    public class DefinedValueFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute" /> class.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public DefinedValueFieldAttribute( string definedTypeGuid, string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.DefinedValueFieldType ).FullName )
        {
            var definedType = Rock.Web.Cache.DefinedTypeCache.Read( new Guid( definedTypeGuid ) );
            var configValue = new Field.ConfigurationValue( definedType.Id.ToString() );
            FieldConfigurationValues.Add( "definedtype", configValue );

            if ( string.IsNullOrWhiteSpace( Name ) )
            {
                Name = definedType.Name;
            }
        }
    }
}