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
    /// Field Attribute to select 0 or more GroupTypes
    /// </summary>
    public class GroupTypesFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypesFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultGroupTypeId">The default group type id.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public GroupTypesFieldAttribute( string name, string description = "", bool required = true, string defaultGroupTypeIds = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultGroupTypeIds, category, order, key, typeof( Rock.Field.Types.GroupTypesFieldType ).FullName )
        {
        }
    }
}