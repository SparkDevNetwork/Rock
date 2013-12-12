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
    /// Field Attribute to select 0 or 1 GroupType
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class GroupRoleFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute" /> class.
        /// </summary>
        /// <param name="groupTypeGuid">The group type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public GroupRoleFieldAttribute( string groupTypeGuid = "", string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.GroupRoleFieldType ).FullName )
        {
            if ( !string.IsNullOrWhiteSpace( groupTypeGuid ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( groupTypeGuid, out guid ) )
                {
                    var groupType = Rock.Web.Cache.GroupTypeCache.Read( guid );
                    if ( groupType != null )
                    {
                        var configValue = new Field.ConfigurationValue( groupType.Id.ToString() );
                        FieldConfigurationValues.Add( "grouptype", configValue );

                        if ( string.IsNullOrWhiteSpace( Name ) )
                        {
                            Name = groupType.Name + " Role";
                            if ( string.IsNullOrWhiteSpace( Key ) )
                            {
                                Key = Name.Replace( " ", string.Empty );
                            }
                        }
                    }
                }
            }
        }
    }
}