//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Field attribute for selecting a Time.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class TimeFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public TimeFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "",
            int order = 0, string key = null)
            : base( name, description, required, defaultValue, category, order, key, typeof( TimeFieldType ).FullName )
        {
        }
    }
}
