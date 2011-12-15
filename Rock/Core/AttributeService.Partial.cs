//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Core
{
    public partial class AttributeService
    {
        /// <summary>
        /// Gets the attributes by entity qualifier and key.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Rock.Core.Attribute GetAttributesByEntityQualifierAndKey( string entity, string entityQualifierColumn, string entityQualifierValue, string key )
        {
            return Repository.FirstOrDefault( t =>
                t.Entity == entity &&
                t.EntityQualifierColumn == entityQualifierColumn &&
                t.EntityQualifierValue == entityQualifierValue &&
                t.Key == key );
        }
    }
}
