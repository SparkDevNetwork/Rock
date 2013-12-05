//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select multiple MEF components
    /// </summary>
    public class ComponentsFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentFieldAttribute" /> class.
        /// </summary>
        /// <param name="mefContainerAssemblyName">Name of the mef container assembly.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public ComponentsFieldAttribute( string mefContainerAssemblyName, string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.ComponentsFieldType ).FullName )
        {
            var configValue = new Field.ConfigurationValue( mefContainerAssemblyName );
            FieldConfigurationValues.Add( "container", configValue );

            if ( string.IsNullOrWhiteSpace( Name ) )
            {
                try
                {
                    Type containerType = Type.GetType( mefContainerAssemblyName );
                    var entityType = EntityTypeCache.Read( containerType );
                    if ( entityType != null )
                    {
                        Name = entityType.FriendlyName;
                    }
                }
                catch { }
            }

            if ( string.IsNullOrWhiteSpace( Name ) )
            {
                Name = mefContainerAssemblyName;
            }

            if ( string.IsNullOrWhiteSpace( Key ) )
            {
                Key = Name.Replace( " ", string.Empty );
            }
        }
    }
}