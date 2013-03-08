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
    /// Field Attribute to select a single (or null) Group
    /// </summary>
    public class GroupFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultGroupId">The default group id.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public GroupFieldAttribute( string name, string description = "", bool required = true, int defaultGroupId = int.MinValue, string category = "", int order = 0, string key = null )
            : base( name, description, required, ( defaultGroupId == int.MinValue ? "" : defaultGroupId.ToString() ), category, order, key, typeof( Rock.Field.Types.GroupField ).FullName )
        {
        }
    }
}