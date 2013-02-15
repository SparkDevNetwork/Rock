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
    /// Attribute used to specify an EntityType
    /// </summary>
    public class EntityTypeAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public EntityTypeAttribute(string name, string description)
            : base( 0, name, false, "", null, string.Empty, description, typeof( Rock.Field.Types.EntityType).FullName )
        {
        }
    }
}