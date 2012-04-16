//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Linq;
using System.ComponentModel.DataAnnotations;

using Rock.Data;

namespace Rock.Core
{
    public partial class Attribute 
    {
        /// <summary>
        /// Gets the value of an attribute
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public string GetValue (int entityId)
        {
            AttributeValue attributeValue = this.AttributeValues.Where( v => v.EntityId == entityId ).FirstOrDefault();
            if ( attributeValue != null )
                return attributeValue.Value;
            return DefaultValue;
        }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get { return new Security.GenericEntity( "Global" ); }
        }
    }
}
