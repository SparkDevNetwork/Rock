using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public class BooleanFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFieldAttribute" /> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <param name="key">The key. (null means derive from name)</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        public BooleanFieldAttribute( int order, string name, bool defaultValue, string key, string category = "", string description = "" )
            : base( order, name, false, defaultValue.ToTrueFalse(), key, category, description, typeof(Rock.Field.Types.Boolean).FullName)
        {
        }
    }
}