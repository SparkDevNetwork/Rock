//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Linq;

namespace Rock.Core
{
    public partial class AttributeService
    {
        /// <summary>
        /// Gets the attributes by entity qualifier.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <returns></returns>
        public IQueryable<Rock.Core.Attribute> GetAttributesByEntityQualifier( string entity, string entityQualifierColumn, string entityQualifierValue )
        {
            return Repository.AsQueryable().Where( t =>
                t.Entity == entity &&
                t.EntityQualifierColumn == entityQualifierColumn &&
                t.EntityQualifierValue == entityQualifierValue );
        }

        /// <summary>
        /// Gets a global attribute.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Rock.Core.Attribute GetGlobalAttribute( string key )
        {
            return this.GetByEntityAndEntityQualifierColumnAndEntityQualifierValueAndKey( string.Empty, string.Empty, string.Empty, key );
        }
    }
}
