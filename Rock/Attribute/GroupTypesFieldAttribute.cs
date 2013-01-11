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
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultGroupTypeId">The default group type id.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        public GroupTypesFieldAttribute( int order, string name, bool required, string defaultGroupTypeId = "", string key = null, string category = "", string description = "" )
            : base( order, name, required, defaultGroupTypeId, key, category, description, typeof(Rock.Field.Types.GroupTypesField).FullName )
        {
        }
    }
}