using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Services.Core
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
        public Rock.Models.Core.Attribute GetAttributesByEntityQualifierAndKey( string entity, string entityQualifierColumn, string entityQualifierValue, string key )
        {
            return Repository.FirstOrDefault( t =>
                t.Entity == entity &&
                t.EntityQualifierColumn == entityQualifierColumn &&
                t.EntityQualifierValue == entityQualifierValue &&
                t.Key == key );
        }
    }
}
